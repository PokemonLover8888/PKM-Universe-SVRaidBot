using Discord;
using Discord.WebSocket;
using SysBot.Base;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Helpers;

/// <summary>
/// Background scheduler that posts a pinned weekly leaderboard to the configured channel
/// every Sunday at 00:00 UTC. Idempotent — if the bot restarts mid-week, it won't double-post
/// (it tracks the last-post week-of-year in a tiny state file).
/// </summary>
public static class WeeklyLeaderboardScheduler
{
    private static Timer? _timer;
    private static DiscordSocketClient? _client;
    private static Func<ulong>? _getChannelId;
    private static readonly string _stateFile = System.IO.Path.Combine(AppContext.BaseDirectory, "leaderboard_state.json");

    public static void Start(DiscordSocketClient client, Func<ulong> getChannelId)
    {
        _client = client;
        _getChannelId = getChannelId;

        // Tick every hour — cheap, and hourly granularity is fine for "post some time Sunday morning".
        _timer = new Timer(_ => _ = TickAsync(), null, TimeSpan.FromMinutes(2), TimeSpan.FromHours(1));
        LogUtil.LogInfo("Weekly leaderboard scheduler started (hourly tick).", nameof(WeeklyLeaderboardScheduler));
    }

    private static async Task TickAsync()
    {
        try
        {
            var nowUtc = DateTime.UtcNow;
            // Only post on Sunday between 00:00 and 02:00 UTC.
            if (nowUtc.DayOfWeek != DayOfWeek.Sunday) return;
            if (nowUtc.Hour >= 2) return;

            // Idempotency check — if we've already posted this week, skip.
            string thisWeekKey = $"{nowUtc.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(nowUtc):D2}";
            string lastPosted = TryReadLastWeek();
            if (string.Equals(lastPosted, thisWeekKey, StringComparison.Ordinal)) return;

            ulong channelId = _getChannelId?.Invoke() ?? 0;
            if (channelId == 0)
            {
                LogUtil.LogInfo("Weekly leaderboard: no channel configured (LeaderboardChannelId=0); skipping.", nameof(WeeklyLeaderboardScheduler));
                return;
            }
            if (_client == null) return;

            var channel = await _client.GetChannelAsync(channelId) as IMessageChannel;
            if (channel == null)
            {
                LogUtil.LogError($"Weekly leaderboard: channel {channelId} not found / not accessible.", nameof(WeeklyLeaderboardScheduler));
                return;
            }

            await PostAndPinAsync(channel, nowUtc).ConfigureAwait(false);
            WriteLastWeek(thisWeekKey);
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Weekly leaderboard tick failed: {ex.Message}", nameof(WeeklyLeaderboardScheduler));
        }
    }

    private static async Task PostAndPinAsync(IMessageChannel channel, DateTime now)
    {
        var since = now.AddDays(-7);
        var rows = RaidHistoryStore.Leaderboard(top: 10, sinceUtc: since);

        var embed = new EmbedBuilder
        {
            Title = $"★ PKM Universe Weekly Leaderboard — Week of {since:MMM d}",
            Color = new Color(0xD4, 0xAF, 0x37),
        };
        if (rows.Count == 0)
        {
            embed.Description = "No raid claims this week. Make some noise — click 🔑 Get Code on the next raid embed.";
        }
        else
        {
            var sb = new StringBuilder();
            int rank = 1;
            foreach (var (userId, total) in rows)
            {
                string medal = rank switch
                {
                    1 => "🥇",
                    2 => "🥈",
                    3 => "🥉",
                    _ => $"`#{rank,2}`",
                };
                var user = await _client!.GetUserAsync(userId);
                string name = user?.Username ?? $"<@{userId}>";
                sb.AppendLine($"{medal}  **{name}**  —  {total} raid{(total == 1 ? "" : "s")}");
                rank++;
            }
            embed.Description = $"Top 10 raid joiners of the week · **{rows.Sum(r => r.Total)}** total claims";
            embed.AddField("Standings", sb.ToString().TrimEnd(), false);
        }
        embed.WithFooter("pkm-universe.com · auto-posts every Sunday at 00:00 UTC");

        var posted = await channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        try { await posted.PinAsync().ConfigureAwait(false); }
        catch (Exception ex)
        {
            // Pinning may fail if the bot lacks Manage Messages — log but don't error the whole job.
            LogUtil.LogInfo($"Weekly leaderboard pin failed ({ex.Message}); message still posted.", nameof(WeeklyLeaderboardScheduler));
        }
    }

    private static string TryReadLastWeek()
    {
        try { return System.IO.File.Exists(_stateFile) ? System.IO.File.ReadAllText(_stateFile).Trim() : string.Empty; }
        catch { return string.Empty; }
    }

    private static void WriteLastWeek(string key)
    {
        try { System.IO.File.WriteAllText(_stateFile, key); }
        catch (Exception ex) { LogUtil.LogError($"Failed to persist leaderboard week key: {ex.Message}", nameof(WeeklyLeaderboardScheduler)); }
    }
}
