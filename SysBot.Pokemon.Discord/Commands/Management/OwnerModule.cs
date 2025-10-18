using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PKHeX.Core;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    public class OwnerModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        private readonly ExtraCommandUtil<T> Util = new();

        [Command("addSudo")]
        [Summary("Adds mentioned user to global sudo")]
        [RequireOwner]
        public async Task SudoUsers([Remainder] string _)
        {
            var users = Context.Message.MentionedUsers;
            var objects = users.Select(GetReference);
            SysCordSettings.Settings.GlobalSudoList.AddIfNew(objects);
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("removeSudo")]
        [Summary("Removes mentioned user from global sudo")]
        [RequireOwner]
        public async Task RemoveSudoUsers([Remainder] string _)
        {
            var users = Context.Message.MentionedUsers;
            var objects = users.Select(GetReference);
            SysCordSettings.Settings.GlobalSudoList.RemoveAll(z => objects.Any(o => o.ID == z.ID));
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("addChannel")]
        [Summary("Adds a channel to the list of channels that are accepting commands.")]
        [RequireOwner]
        public async Task AddChannel()
        {
            var obj = GetReference(Context.Message.Channel);
            SysCordSettings.Settings.ChannelWhitelist.AddIfNew(new[] { obj });
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("removeChannel")]
        [Summary("Removes a channel from the list of channels that are accepting commands.")]
        [RequireOwner]
        public async Task RemoveChannel()
        {
            var obj = GetReference(Context.Message.Channel);
            SysCordSettings.Settings.ChannelWhitelist.RemoveAll(z => z.ID == obj.ID);
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        [Command("leave")]
        [Alias("bye")]
        [Summary("Leaves the current server.")]
        [RequireOwner]
        public async Task Leave()
        {
            await ReplyAsync("Goodbye.").ConfigureAwait(false);
            await Context.Guild.LeaveAsync().ConfigureAwait(false);
        }

        [Command("leaveguild")]
        [Alias("lg")]
        [Summary("Leaves guild based on supplied ID.")]
        [RequireOwner]
        public async Task LeaveGuild(string userInput)
        {
            if (!ulong.TryParse(userInput, out ulong id))
            {
                await ReplyAsync("Please provide a valid Guild ID.").ConfigureAwait(false);
                return;
            }
            var guild = Context.Client.Guilds.FirstOrDefault(x => x.Id == id);
            if (guild is null)
            {
                await ReplyAsync($"Provided input ({userInput}) is not a valid guild ID or the bot is not in the specified guild.").ConfigureAwait(false);
                return;
            }

            await ReplyAsync($"Leaving {guild}.").ConfigureAwait(false);
            await guild.LeaveAsync().ConfigureAwait(false);
        }

        [Command("leaveall")]
        [Summary("Leaves all servers the bot is currently in.")]
        [RequireOwner]
        public async Task LeaveAll()
        {
            await ReplyAsync("Leaving all servers.").ConfigureAwait(false);
            foreach (var guild in Context.Client.Guilds)
            {
                await guild.LeaveAsync().ConfigureAwait(false);
            }
        }

        [Command("listguilds")]
        [Alias("guildlist", "gl")]
        [Summary("Lists all the servers the bot is in.")]
        [RequireOwner]
        public async Task ListGuilds()
        {
            var guilds = Context.Client.Guilds.OrderBy(guild => guild.Name);
            var guildList = new StringBuilder();
            guildList.AppendLine("\n");
            foreach (var guild in guilds)
            {
                guildList.AppendLine($"{Format.Bold($"{guild.Name}")}\nID: {guild.Id}\n");
            }
            await Util.ListUtil(Context, "Here is a list of all servers this bot is currently in", guildList.ToString()).ConfigureAwait(false);
        }

        [Command("sudoku")]
        [Alias("kill", "shutdown")]
        [Summary("Causes the entire process to end itself!")]
        [RequireOwner]
        public async Task ExitProgram()
        {
            await Context.Channel.EchoAndReply("Shutting down... goodbye! **Bot services are going offline.**").ConfigureAwait(false);
            Environment.Exit(0);
        }

        private RemoteControlAccess GetReference(IUser channel) => new()
        {
            ID = channel.Id,
            Name = channel.Username,
            Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
        };

        private RemoteControlAccess GetReference(IChannel channel) => new()
        {
            ID = channel.Id,
            Name = channel.Name,
            Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
        };

        [Command("dme")]
        [Summary("Sends a DM to a user with an embedded message and optional attachments. With embed")]
        [RequireSudo]
        public async Task DMEUserAsync(string userIdentifier, [Remainder] string? message = null)
        {
            SocketUser? user = null;
            if (MentionUtils.TryParseUser(userIdentifier, out ulong userId))
            {
                user = Context.Client.GetUser(userId);
            }
            else if (ulong.TryParse(userIdentifier, out userId))
            {
                user = Context.Client.GetUser(userId);
            }

            if (user == null)
            {
                var error = await ReplyAsync("Invalid user identifier. Please mention the user or provide their ID.").ConfigureAwait(false);
                await Task.Delay(15000);
                await error.DeleteAsync().ConfigureAwait(false);
                return;
            }

            var attachments = Context.Message.Attachments;
            var hasAttachments = attachments.Count > 0;

            try
            {
                var dmChannel = await user.CreateDMChannelAsync().ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    var embedBuilder = new EmbedBuilder()
                        .WithTitle("Private Message from SV-Raid Bot")
                        .WithDescription(message)
                        .WithColor(new Color(0, 255, 255))
                        .WithThumbnailUrl("https://i.imgur.com/hQhaPph.jpg");

                    await dmChannel.SendMessageAsync(embed: embedBuilder.Build()).ConfigureAwait(false);
                }

                if (hasAttachments)
                {
                    foreach (var attachment in attachments)
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            var stream = await httpClient.GetStreamAsync(attachment.Url).ConfigureAwait(false);
                            await dmChannel.SendFileAsync(stream, attachment.Filename).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            await ReplyAsync($"Error processing attachment: {attachment.Filename}. Exception: {ex.Message}").ConfigureAwait(false);
                        }
                    }
                }

                var confirmation = await ReplyAsync($"Message successfully sent to {user.Username}.").ConfigureAwait(false);
                await Task.Delay(15000);
                await confirmation.DeleteAsync().ConfigureAwait(false);
                await Context.Message.DeleteAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var error = await ReplyAsync($"Failed to send a message. Error: {ex.Message}").ConfigureAwait(false);
                await Task.Delay(15000);
                await error.DeleteAsync().ConfigureAwait(false);
            }
        }

        [Command("dm")]
        [Summary("Sends a DM to a user with an optional message and attachments. Without embed")]
        [RequireSudo]
        public async Task DMUserAsync(string userIdentifier, [Remainder] string? message = null)
        {
            SocketUser? user = null;
            if (MentionUtils.TryParseUser(userIdentifier, out ulong userId))
            {
                user = Context.Client.GetUser(userId);
            }
            else if (ulong.TryParse(userIdentifier, out userId))
            {
                user = Context.Client.GetUser(userId);
            }

            if (user == null)
            {
                var error = await ReplyAsync("Invalid user identifier. Please mention the user or provide their ID.").ConfigureAwait(false);
                await Task.Delay(15000);
                await error.DeleteAsync().ConfigureAwait(false);
                return;
            }

            var attachments = Context.Message.Attachments;
            var hasAttachments = attachments.Count > 0;

            try
            {
                var dmChannel = await user.CreateDMChannelAsync().ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    await dmChannel.SendMessageAsync(message).ConfigureAwait(false);
                }

                if (hasAttachments)
                {
                    foreach (var attachment in attachments)
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            var stream = await httpClient.GetStreamAsync(attachment.Url).ConfigureAwait(false);
                            await dmChannel.SendFileAsync(stream, attachment.Filename).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            await ReplyAsync($"Error processing attachment: {attachment.Filename}. Exception: {ex.Message}").ConfigureAwait(false);
                        }
                    }
                }

                var confirmation = await ReplyAsync($"Message successfully sent to {user.Username}.").ConfigureAwait(false);
                await Task.Delay(15000);
                await confirmation.DeleteAsync().ConfigureAwait(false);
                await Context.Message.DeleteAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var error = await ReplyAsync($"Failed to send a message. Error: {ex.Message}").ConfigureAwait(false);
                await Task.Delay(15000);
                await error.DeleteAsync().ConfigureAwait(false);
            }
        }

        [Command("say")]
        [Summary("Sends a message to a mentioned channel with optional attachments.")]
        [RequireSudo]
        public async Task SayAsync(ITextChannel channel, [Remainder] string? content = null)
        {
            var attachments = Context.Message.Attachments;
            var hasAttachments = attachments.Count > 0;

            try
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    await channel.SendMessageAsync(content).ConfigureAwait(false);
                }

                if (hasAttachments)
                {
                    foreach (var attachment in attachments)
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            var stream = await httpClient.GetStreamAsync(attachment.Url).ConfigureAwait(false);
                            await channel.SendFileAsync(stream, attachment.Filename).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            await ReplyAsync($"Error processing attachment `{attachment.Filename}`: {ex.Message}").ConfigureAwait(false);
                        }
                    }
                }

                var confirmation = await ReplyAsync($"Message successfully sent to {channel.Mention}.").ConfigureAwait(false);
                await Task.Delay(15000).ConfigureAwait(false);
                await confirmation.DeleteAsync().ConfigureAwait(false);
                await Context.Message.DeleteAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var error = await ReplyAsync($"Failed to send message to {channel.Mention}. Error: {ex.Message}").ConfigureAwait(false);
                await Task.Delay(15000).ConfigureAwait(false);
                await error.DeleteAsync().ConfigureAwait(false);
            }
        }
    }
}