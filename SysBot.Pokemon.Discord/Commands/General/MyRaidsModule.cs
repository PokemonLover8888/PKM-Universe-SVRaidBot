using Discord;
using Discord.Commands;
using SysBot.Base;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

/// <summary>
/// Per-trainer raid history. Lets users see their own last 20 raids and overall W/L.
/// </summary>
public class MyRaidsModule : ModuleBase<SocketCommandContext>
{
    private const int RecentLimit = 20;

    [Command("myraids")]
    [Alias("raidhistory", "mystats")]
    [Summary("Shows your most recent claimed raids and your overall win/loss.")]
    public async Task MyRaidsAsync()
    {
        var userId = Context.User.Id;
        var recent = RaidHistoryStore.GetRecent(userId, RecentLimit);
        var (total, won, lost) = RaidHistoryStore.GetUserStats(userId);

        var embed = new EmbedBuilder
        {
            Title = $"Your Raid History",
            Color = new Color(0xD4, 0xAF, 0x37),
            Description = total == 0
                ? "You haven't claimed any raid codes yet. Click **🔑 Get Code** on a raid embed to start your history."
                : $"**{total}** total claims · **{won}** wins · **{lost}** losses · " +
                  $"{(total > 0 ? Math.Round((double)won / total * 100, 1) : 0)}% win rate",
        };
        embed.WithAuthor(Context.User.Username, Context.User.GetAvatarUrl());
        embed.WithFooter($"Showing your last {Math.Min(recent.Count, RecentLimit)} of {total} · pkm-universe.com");

        if (recent.Count > 0)
        {
            var sb = new StringBuilder();
            foreach (var e in recent)
            {
                string mark = e.Result switch
                {
                    RaidHistoryStore.RaidResult.Won => "✅",
                    RaidHistoryStore.RaidResult.Lost => "❌",
                    _ => "⌛",
                };
                string shiny = e.IsShiny ? "★ " : string.Empty;
                string ago = HumanizeAgo(e.ClaimedUtc);
                // Truncate raid title to avoid embed-field-overflow
                string title = e.RaidTitle.Length > 50 ? e.RaidTitle.Substring(0, 47) + "…" : e.RaidTitle;
                sb.AppendLine($"{mark}  {shiny}{title}  *· {ago}*");
            }
            embed.AddField("Recent", sb.ToString().TrimEnd(), false);
        }

        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
    }

    private static string HumanizeAgo(System.DateTime utc)
    {
        var span = System.DateTime.UtcNow - utc;
        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        if (span.TotalDays < 30) return $"{(int)span.TotalDays}d ago";
        return utc.ToString("yyyy-MM-dd");
    }
}
