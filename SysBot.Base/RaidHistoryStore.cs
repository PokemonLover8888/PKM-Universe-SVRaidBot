using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace SysBot.Base;

/// <summary>
/// JSON-backed per-trainer raid history. Tracks who claimed a raid code, what raid it was,
/// and whether the raid ultimately won or lost.
///
/// Lifecycle:
///   1. User clicks Get Code → RecordClaim() adds a PENDING entry keyed on (userId, messageId).
///   2. Raid finishes → MarkResult(messageId, won) updates every entry with that messageId
///      from Pending to Won/Lost.
///   3. User runs !myraids → GetRecent(userId, 20) returns their last entries newest-first.
/// </summary>
public static class RaidHistoryStore
{
    public enum RaidResult { Pending, Won, Lost }

    public sealed class Entry
    {
        public ulong UserId { get; set; }
        public ulong MessageId { get; set; }
        public string RaidTitle { get; set; } = string.Empty;
        public int Stars { get; set; }
        public bool IsShiny { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RaidResult Result { get; set; } = RaidResult.Pending;
        public DateTime ClaimedUtc { get; set; } = DateTime.UtcNow;
    }

    private static readonly object _lock = new();
    private static List<Entry> _entries = new();
    private static string _filePath = string.Empty;
    private static bool _initialized = false;

    /// <summary>One-time init — pass the directory to store the JSON file in.</summary>
    public static void Init(string directory)
    {
        if (_initialized) return;
        _filePath = Path.Combine(directory, "raid_history.json");
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var loaded = JsonSerializer.Deserialize<List<Entry>>(json);
                if (loaded != null) _entries = loaded;
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Failed to load raid history: {ex.Message}", nameof(RaidHistoryStore));
        }
        _initialized = true;
    }

    public static void RecordClaim(ulong userId, ulong messageId, string raidTitle, int stars, bool isShiny)
    {
        if (!_initialized) return;
        lock (_lock)
        {
            // De-dupe: one entry per (user, message) pair.
            if (_entries.Any(e => e.UserId == userId && e.MessageId == messageId)) return;
            _entries.Add(new Entry
            {
                UserId = userId,
                MessageId = messageId,
                RaidTitle = raidTitle,
                Stars = stars,
                IsShiny = isShiny,
                Result = RaidResult.Pending,
                ClaimedUtc = DateTime.UtcNow,
            });
            // Cap total entries to prevent unbounded growth — keep most recent 50,000
            if (_entries.Count > 50000)
                _entries = _entries.OrderByDescending(e => e.ClaimedUtc).Take(50000).ToList();
            SaveUnlocked();
        }
    }

    /// <summary>Returns the number of entries updated.</summary>
    public static int MarkResult(ulong messageId, bool won)
    {
        if (!_initialized) return 0;
        int count = 0;
        lock (_lock)
        {
            foreach (var e in _entries.Where(e => e.MessageId == messageId && e.Result == RaidResult.Pending))
            {
                e.Result = won ? RaidResult.Won : RaidResult.Lost;
                count++;
            }
            if (count > 0) SaveUnlocked();
        }
        return count;
    }

    /// <summary>Returns the list of user IDs that claimed the code for a given raid message.</summary>
    public static IReadOnlyList<ulong> GetClaimantsForMessage(ulong messageId)
    {
        if (!_initialized) return Array.Empty<ulong>();
        lock (_lock)
        {
            return _entries
                .Where(e => e.MessageId == messageId)
                .Select(e => e.UserId)
                .Distinct()
                .ToList();
        }
    }

    public static IReadOnlyList<Entry> GetRecent(ulong userId, int count = 20)
    {
        if (!_initialized) return Array.Empty<Entry>();
        lock (_lock)
        {
            return _entries
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.ClaimedUtc)
                .Take(count)
                .ToList();
        }
    }

    public static (int total, int won, int lost) GetUserStats(ulong userId)
    {
        if (!_initialized) return (0, 0, 0);
        lock (_lock)
        {
            var u = _entries.Where(e => e.UserId == userId).ToList();
            return (u.Count, u.Count(e => e.Result == RaidResult.Won), u.Count(e => e.Result == RaidResult.Lost));
        }
    }

    /// <summary>Top N users by total claims. Used by the weekly leaderboard task.</summary>
    public static IReadOnlyList<(ulong UserId, int Total)> Leaderboard(int top = 10, DateTime? sinceUtc = null)
    {
        if (!_initialized) return Array.Empty<(ulong, int)>();
        lock (_lock)
        {
            var since = sinceUtc ?? DateTime.MinValue;
            return _entries
                .Where(e => e.ClaimedUtc >= since)
                .GroupBy(e => e.UserId)
                .Select(g => (g.Key, g.Count()))
                .OrderByDescending(t => t.Item2)
                .Take(top)
                .ToList();
        }
    }

    private static void SaveUnlocked()
    {
        try
        {
            var json = JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = false });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Failed to save raid history: {ex.Message}", nameof(RaidHistoryStore));
        }
    }
}
