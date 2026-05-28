using System;
using System.Collections.Generic;

namespace SysBot.Pokemon.SV.BotRaid.Helpers;

/// <summary>
/// Cross-thread snapshot of the currently-hosting raid + bot stats. RotatingRaidBotSV
/// writes to this on state transitions; the WinForms Dashboard polls it on a 1-second
/// UI timer and updates controls.
///
/// All access is via the static getters/setters — no locking needed because individual
/// field writes are atomic at the CLR level (string, bool, int, byte). Multi-field
/// updates may briefly appear inconsistent across reads, but the UI tolerates that.
/// </summary>
public static class DashboardState
{
    public static bool HasActiveRaid     { get; set; }
    public static string CurrentSpecies  { get; set; } = string.Empty;  // "Charizard"
    public static int    CurrentForm     { get; set; }                  // 0 = base, 1+ = alt
    public static int    Stars           { get; set; }                  // 1..7
    public static bool   IsShiny         { get; set; }
    public static string TeraTypeEnglish { get; set; } = string.Empty;  // "Fire", "Poison", etc.
    public static string CurrentCode     { get; set; } = "—";
    public static string SpriteUrl       { get; set; } = string.Empty;
    public static string MoveList        { get; set; } = string.Empty;  // newline-separated
    public static string MetaLine        { get; set; } = string.Empty;  // "7-star Black Crystal · Mightiest Mark · Lv 100"

    /// <summary>Seconds remaining until the raid times out / starts. -1 = unknown.</summary>
    public static int TimerSecondsRemaining { get; set; } = -1;
    public static int TimerSecondsTotal     { get; set; } = -1;
    public static int JoinedCount           { get; set; }
    public static int JoinedCapacity        { get; set; } = 4;

    // Stats
    public static int  CompletedTotal { get; set; }
    public static int  WinCount       { get; set; }
    public static int  LossCount      { get; set; }
    public static long UptimeMinutes  { get; set; }

    // Connection
    public static bool   SwitchConnected   { get; set; }
    public static string SwitchIp          { get; set; } = string.Empty;
    public static bool   DiscordConnected  { get; set; }
    public static int    DiscordLatencyMs  { get; set; } = -1;
    public static bool   SysBotBaseReady   { get; set; }

    // Queue snapshot — list of "name (stars)" entries, top first.
    private static readonly object _queueLock = new();
    private static List<string> _queueSnapshot = new();
    public static IReadOnlyList<string> QueueSnapshot
    {
        get { lock (_queueLock) return _queueSnapshot.ToArray(); }
    }
    public static void SetQueueSnapshot(IEnumerable<string> entries)
    {
        lock (_queueLock) _queueSnapshot = new List<string>(entries);
    }

    /// <summary>
    /// Fired by RotatingRaidBotSV on notable events (raid started, trainer joined,
    /// raid completed, error). The Dashboard's Live Feed rail listens.
    /// (icon, body, timeLabel) — icon is one of: "+", "✓", "✕", "!", "★"
    /// </summary>
    public static event Action<string, string, string>? LiveEventOccurred;

    public static void Emit(string icon, string body)
    {
        try { LiveEventOccurred?.Invoke(icon, body, "just now"); }
        catch { /* listener errors shouldn't break the bot */ }
    }
}
