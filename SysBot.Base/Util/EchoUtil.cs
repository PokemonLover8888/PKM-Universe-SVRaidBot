using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SysBot.Base
{
    public static class EchoUtil
    {
        public static readonly List<Action<string>> Forwarders = new();
        public static readonly List<Action<string, Embed>> EmbedForwarders = new();
        public static readonly List<Func<byte[], string, EmbedBuilder, Task<IUserMessage>>> RaidForwarders = new();
        // V2: same as RaidForwarders but also accepts a MessageComponent (buttons).
        public static readonly List<Func<byte[], string, EmbedBuilder, MessageComponent?, Task<IUserMessage>>> RaidForwardersV2 = new();

        public static void Echo(string message)
        {
            foreach (var fwd in Forwarders)
            {
                try
                {
                    fwd(message);
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo($"Exception: {ex} occurred while trying to echo: {message} to the forwarder: {fwd}", "Echo");
                    LogUtil.LogSafe(ex, "Echo");
                }
            }
            LogUtil.LogInfo(message, "Echo");
        }

        public static void EchoEmbed(string ping, string message, string url, string markurl, bool result)
        {
            foreach (var fwd in EmbedForwarders)
            {
                try
                {
                    if (!result)
                    {
                        ping = string.Empty;
                        if (string.IsNullOrEmpty(markurl))
                            markurl = $"https://i.imgur.com/t2M8qF4.png";
                        else
                            markurl = $"https://i.imgur.com/t2M8qF4.png";
                    }
                    else if (result)
                    {
                        if (string.IsNullOrEmpty(markurl))
                            markurl = "https://i.imgur.com/T8vEiIk.jpg";
                    }
                    var author = new EmbedAuthorBuilder { IconUrl = markurl, Name = result ? "Match found!" : "Unwanted match..." };
                    var embed = new EmbedBuilder
                    {
                        Color = result ? Color.Teal : Color.Red,
                        ThumbnailUrl = url
                    }.WithAuthor(author).WithDescription(message);
                    fwd(ping, embed.Build());
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo($"Exception: {ex} occurred while trying to echo: {message} to the forwarder: {fwd}", "Echo");
                    LogUtil.LogSafe(ex, "Echo");
                }
            }
            LogUtil.LogInfo(message, "Echo");
        }

        public static async Task<List<IUserMessage>> RaidEmbed(byte[] bytes, string fileName, EmbedBuilder embeds)
        {
            List<IUserMessage> sentMessages = new List<IUserMessage>();
            foreach (var fwd in RaidForwarders)
            {
                try
                {
                    var message = await fwd(bytes, fileName, embeds);
                    if (message != null)
                        sentMessages.Add(message);
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo($"Exception: {ex} occurred while trying to echo: RaidEmbed to the forwarder: {fwd}", "Echo");
                    LogUtil.LogSafe(ex, "Echo");
                }
            }
            return sentMessages;
        }

        /// <summary>
        /// V2 dispatch: same as <see cref="RaidEmbed"/> but also passes a MessageComponent (button row)
        /// to channel forwarders. Falls back to plain RaidForwarders if V2 forwarders aren't registered yet.
        /// </summary>
        public static async Task<List<IUserMessage>> RaidEmbedWithComponents(byte[] bytes, string fileName, EmbedBuilder embeds, MessageComponent? components)
        {
            var sent = new List<IUserMessage>();
            if (RaidForwardersV2.Count == 0)
            {
                // No V2 forwarder registered — silently fall back so callers can opt in without breaking older deployments.
                return await RaidEmbed(bytes, fileName, embeds).ConfigureAwait(false);
            }
            foreach (var fwd in RaidForwardersV2)
            {
                try
                {
                    var message = await fwd(bytes, fileName, embeds, components);
                    if (message != null)
                        sent.Add(message);
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo($"Exception {ex.GetType().Name} in RaidEmbedWithComponents forwarder", "Echo");
                    LogUtil.LogSafe(ex, "Echo");
                }
            }
            return sent;
        }
    }
}