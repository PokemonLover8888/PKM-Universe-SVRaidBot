using Discord;
using Discord.Commands;
using SysBot.Base;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

/// <summary>
/// Weekly leaderboard. Live ranking on-demand via !leaderboard, plus an auto-posted
/// pinned weekly recap driven by <see cref="WeeklyLeaderboardScheduler"/>.
/// </summary>
public class LeaderboardModule : ModuleBase<SocketCommandContext>
{
    [Command("leaderboard")]
    [Alias("lb", "top")]
    [Summary("Shows the top 10 raid joiners over the current week (or this month with `!leaderboard month`).")]
    public async Task LeaderboardAsync(string scope = "week")
    {
        DateTime since;
        string label;
        switch (scope.ToLowerInvariant())
        {
            case "month": since = DateTime.UtcNow.AddDays(-30); label = "Last 30 days"; break;
            case "all":
            case "alltime": since = DateTime.MinValue; label = "All time"; break;
            default: since = DateTime.UtcNow.AddDays(-7); label = "This week"; break;
        }

        var rows = RaidHistoryStore.Leaderboard(top: 10, sinceUtc: since);

        var embed = new EmbedBuilder
        {
            Title = "★ PKM Universe Raid Leaderboard",
            Color = new Color(0xD4, 0xAF, 0x37),
            Description = rows.Count == 0
                ? $"No raid claims yet for **{label}**. Be the first — click **🔑 Get Code** on the next raid embed."
                : $"**{label}** · {rows.Sum(r => r.Total)} total claims by {rows.Count} trainer{(rows.Count == 1 ? "" : "s")}",
        };

        if (rows.Count > 0)
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
                var user = await Context.Client.GetUserAsync(userId);
                string name = user?.Username ?? $"<@{userId}>";
                sb.AppendLine($"{medal}  **{name}**  —  {total} raid{(total == 1 ? "" : "s")}");
                rank++;
            }
            embed.AddField("Top 10", sb.ToString().TrimEnd(), false);
        }

        embed.WithFooter("pkm-universe.com · use !leaderboard month or !leaderboard all for different windows");
        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
    }
}
