using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    public class LogModule : ModuleBase<SocketCommandContext>
    {
        private class LogAction : ChannelAction<string, string>
        {
            public LogAction(ulong id, Action<string, string> messager, string channel) : base(id, messager, channel)
            {
            }
        }

        private static readonly Dictionary<ulong, LogAction> Channels = new();

        public static void RestoreLogging(DiscordSocketClient discord, DiscordSettings settings)
        {
            foreach (var ch in settings.LoggingChannels)
            {
                if (discord.GetChannel(ch.ID) is ISocketMessageChannel c)
                    AddLogChannel(c, ch.ID);
            }

            LogUtil.LogInfo("Added logging to Discord channel(s) on Bot startup.", "Discord");
        }

        [Command("logHere")]
        [Summary("Makes the bot log to the channel.")]
        [RequireSudo]
        public async Task AddLogAsync()
        {
            var c = Context.Channel;
            var cid = c.Id;
            if (Channels.TryGetValue(cid, out _))
            {
                await ReplyAsync("Already logging here.").ConfigureAwait(false);
                return;
            }

            AddLogChannel(c, cid);

            // Add to discord global loggers (saves on program close)
            SysCordSettings.Settings.LoggingChannels.AddIfNew(new[] { GetReference(Context.Channel) });
            await ReplyAsync("Added logging output to this channel!").ConfigureAwait(false);
        }

        private static readonly Dictionary<ulong, DiscordLogRelay> Relays = new();

        private static void AddLogChannel(ISocketMessageChannel c, ulong cid)
        {
            var relay = new DiscordLogRelay(c);
            Action<string, string> l = relay.Post;
            LogUtil.Forwarders.Add(l);

            var entry = new LogAction(cid, l, c.Name);
            Channels.Add(cid, entry);
            Relays[cid] = relay;
        }

        /// <summary>
        /// Stops a channel's relay and flushes whatever it still holds, so `logClear` never
        /// strands buffered lines or leaves a timer running.
        /// </summary>
        private static void DisposeRelay(ulong cid)
        {
            if (!Relays.TryGetValue(cid, out var relay))
                return;
            relay.Dispose();
            Relays.Remove(cid);
        }

        /// <summary>
        /// Relays bot logs to a Discord channel WITHOUT one REST call per log line.
        ///
        /// The old forwarder fired an un-awaited SendMessageAsync for every single LogUtil line.
        /// On a raid bot that is ~13,000 Discord messages/day, delivered in bursts (a raid cycle
        /// emits ~10 lines at once), which blows straight through Discord's per-channel bucket of
        /// 5 messages / 5 seconds. Measured 2026-08-30: the three raid bots were 88% of the whole
        /// fleet's Discord REST traffic and 93% of its 429s — and the rate-limit warnings were
        /// themselves being echoed, feeding the loop.
        ///
        /// Three fixes, all tunable from config (Hub.Discord.LogEcho) so tuning never needs a rebuild:
        ///   1. FILTER  — routine raid-loop chatter is not relayed. Warnings/errors always are.
        ///   2. DEDUPE  — a line repeating inside a window collapses to one "(+N suppressed)" note.
        ///   3. BATCH   — survivors are joined and sent as one message every N seconds, which
        ///                smooths the bursts that actually trigger the 429s.
        /// Disk logging is untouched: logs\SysBotLog.txt still receives every line.
        /// </summary>
        private sealed class DiscordLogRelay : IDisposable
        {
            // Discord's hard cap is 2000 chars; leave headroom for the code fence and a long tail line.
            private const int ChunkLimit = 1900;

            /// <summary>
            /// Never relayed: echoing these to Discord is what CAUSES more of them. A rate-limit
            /// warning turned into a Discord message is a feedback loop, so it is dropped even
            /// though the keep-warnings rule would otherwise catch it.
            /// </summary>
            private static readonly string[] AlwaysDrop =
            {
                "Rate limit triggered",
            };

            /// <summary>
            /// Always relayed (unless AlwaysDrop or the user's ExtraSkipPatterns matched first).
            /// This is the guarantee that filtering can never hide a real problem.
            /// </summary>
            private static readonly string[] KeepMarkers =
            {
                "error", "exception", "fail", "warning", "unable", "denied", "invalid",
                "timeout", "timed out", "disconnect", "crash", "not found", "refused",
            };

            /// <summary>
            /// Routine raid-loop mechanics — the ~90% of log volume that is only useful when you
            /// are reading the on-disk log to reconstruct a specific raid. Derived from a full-day
            /// profile of Greninja's 2026-08-30 log (13,087 lines).
            /// </summary>
            private static readonly string[] RoutinePatterns =
            {
                "active raids in",
                "Caching session offsets",
                "Caching offsets complete",
                "Getting Raid data",
                "Reading event raid status",
                "Raids...",
                "Successfully processed",
                "Waiting for Player",
                "Player is near den",
                "Player location:",
                "Player is at the nearest active den",
                "Seed at nearest den:",
                "Nearest active den:",
                "Distance to nearest den:",
                "Preparing lobby",
                "Connecting to lobby",
                "Attempting to remake lobby",
                "Nobody joined the raid",
                "Lobby Method:",
                "SkipRaid mode:",
                "Reached SkipRaidLimit",
                "lost/empty raids",
                "Clearing stored OTs",
                "Already connected online",
                "Generating raid info with parameters",
                "reaction to message",
                "Next raid in the list",
                "Closed out of the game",
                "Restarting the game",
                "Back in the overworld",
                // A failing connection still reports through an error/"Failed" line, which the
                // keep-markers catch — so silencing the routine attempt counter is safe.
                "Connection attempt",
                "Successfully injected seed",
                "Successfully teleported to den",
                "Attempting to override seed",
                "Seed override completed",
                "Today Seed",
                "Preparing parameter for",
                "Rotation for",
                "found for",
                "Checking Current Game Progress",
                "Game progress level is already",
                "Overworld Spawns",
                "Successfully reached overworld",
                "Preparing PartyPK",
                "PartyPK switch successful",
            };

            private sealed class RepeatState
            {
                public DateTime LastEmitted;
                public int Suppressed;
            }

            private readonly ISocketMessageChannel _channel;
            private readonly List<string> _buffer = new();
            private readonly Dictionary<string, RepeatState> _recent = new();
            private readonly object _sync = new();
            private readonly Timer? _timer;
            private bool _disposed;

            public DiscordLogRelay(ISocketMessageChannel channel)
            {
                _channel = channel;
                var seconds = Config.BatchSeconds;
                if (seconds > 0)
                {
                    var period = TimeSpan.FromSeconds(seconds);
                    _timer = new Timer(_ => Flush(), null, period, period);
                }
            }

            /// <summary>
            /// Settings are read live on every call, so `logHere` after a settings change picks up
            /// the new values. Falls back to defaults if the hub settings aren't wired up yet.
            /// </summary>
            private static DiscordSettings.LogEchoSettingsCategory Config
            {
                get
                {
                    try { return SysCordSettings.Settings?.LogEcho ?? Defaults; }
                    catch { return Defaults; }
                }
            }

            private static readonly DiscordSettings.LogEchoSettingsCategory Defaults = new();

            public void Post(string msg, string identity)
            {
                try
                {
                    if (_disposed || msg is null)
                        return;

                    var cfg = Config;
                    if (!ShouldRelay(msg, cfg))
                        return;

                    var line = $"> [{DateTime.Now:hh:mm:ss}] - {identity}: {msg}";

                    lock (_sync)
                    {
                        if (!Accept(msg, ref line, cfg))
                            return;
                        _buffer.Add(line);
                    }

                    if (cfg.BatchSeconds <= 0)
                        Flush();
                }
                catch
                {
                    // Swallow. Never call LogUtil from here: that would re-enter this forwarder
                    // and can spiral into an infinite log->send->fail->log loop.
                }
            }

            private static bool ShouldRelay(string msg, DiscordSettings.LogEchoSettingsCategory cfg)
            {
                if (ContainsAny(msg, AlwaysDrop))
                    return false;

                // User-supplied skips win over the keep-warnings rule, so a noisy warning can be silenced.
                var extra = cfg.ExtraSkipPatterns;
                if (!string.IsNullOrWhiteSpace(extra))
                {
                    foreach (var p in extra.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        if (msg.Contains(p, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                }

                if (ContainsAny(msg, KeepMarkers))
                    return true;

                if (!cfg.SkipRoutine)
                    return true;

                return !ContainsAny(msg, RoutinePatterns);
            }

            private static bool ContainsAny(string msg, string[] patterns)
            {
                foreach (var p in patterns)
                {
                    if (msg.Contains(p, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Dedupe on the message shape (digits collapsed), so "Found 25 active raids" and
            /// "Found 19 active raids" count as the same repeating line. Returns false to suppress.
            /// Caller must hold the lock.
            /// </summary>
            private bool Accept(string msg, ref string line, DiscordSettings.LogEchoSettingsCategory cfg)
            {
                var window = cfg.DedupeSeconds;
                if (window <= 0)
                    return true;

                var key = Shape(msg);
                var now = DateTime.Now;

                if (_recent.TryGetValue(key, out var state))
                {
                    if ((now - state.LastEmitted).TotalSeconds < window)
                    {
                        state.Suppressed++;
                        return false;
                    }

                    if (state.Suppressed > 0)
                        line += $"  _(+{state.Suppressed} repeats suppressed)_";
                    state.Suppressed = 0;
                    state.LastEmitted = now;
                    return true;
                }

                // Bound the map: raid logs have a small set of shapes, so this only trips if a
                // message embeds something unbounded (a name, a path) that Shape() can't collapse.
                if (_recent.Count > 500)
                    _recent.Clear();

                _recent[key] = new RepeatState { LastEmitted = now };
                return true;
            }

            private static string Shape(string msg)
            {
                var sb = new StringBuilder(msg.Length);
                var inNumber = false;
                foreach (var ch in msg)
                {
                    if (char.IsDigit(ch))
                    {
                        if (!inNumber)
                        {
                            sb.Append('#');
                            inNumber = true;
                        }
                        continue;
                    }
                    inNumber = false;
                    sb.Append(ch);
                }
                return sb.ToString();
            }

            private void Flush()
            {
                List<string> pending;
                lock (_sync)
                {
                    if (_buffer.Count == 0)
                        return;
                    pending = new List<string>(_buffer);
                    _buffer.Clear();
                }

                var sb = new StringBuilder();
                foreach (var line in pending)
                {
                    var text = line.Length > ChunkLimit ? line[..ChunkLimit] : line;
                    if (sb.Length > 0 && sb.Length + text.Length + 1 > ChunkLimit)
                    {
                        Send(sb.ToString());
                        sb.Clear();
                    }
                    if (sb.Length > 0)
                        sb.Append('\n');
                    sb.Append(text);
                }

                if (sb.Length > 0)
                    Send(sb.ToString());
            }

            private void Send(string text)
            {
                try
                {
                    // Fire-and-forget, same as the original forwarder, but faults are observed and
                    // dropped here rather than surfacing as unobserved task exceptions.
                    _ = _channel.SendMessageAsync(text).ContinueWith(
                        t => { _ = t.Exception; },
                        TaskContinuationOptions.OnlyOnFaulted);
                }
                catch
                {
                    // Never log from the log relay.
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;
                _disposed = true;
                _timer?.Dispose();
                Flush();
            }
        }

        [Command("logInfo")]
        [Summary("Dumps the logging settings.")]
        [RequireSudo]
        public async Task DumpLogInfoAsync()
        {
            foreach (var c in Channels)
                await ReplyAsync($"{c.Key} - {c.Value}").ConfigureAwait(false);
        }

        [Command("logClear")]
        [Summary("Clears the logging settings in that specific channel.")]
        [RequireSudo]
        public async Task ClearLogsAsync()
        {
            var id = Context.Channel.Id;
            if (!Channels.TryGetValue(id, out var log))
            {
                await ReplyAsync("Not echoing in this channel.").ConfigureAwait(false);
                return;
            }
            LogUtil.Forwarders.Remove(log.Action);
            DisposeRelay(Context.Channel.Id);
            Channels.Remove(Context.Channel.Id);
            SysCordSettings.Settings.LoggingChannels.RemoveAll(z => z.ID == id);
            await ReplyAsync($"Logging cleared from channel: {Context.Channel.Name}").ConfigureAwait(false);
        }

        [Command("logClearAll")]
        [Summary("Clears all the logging settings.")]
        [RequireSudo]
        public async Task ClearLogsAllAsync()
        {
            foreach (var l in Channels)
            {
                var entry = l.Value;
                await ReplyAsync($"Logging cleared from {entry.ChannelName} ({entry.ChannelID}!").ConfigureAwait(false);
                LogUtil.Forwarders.Remove(entry.Action);
            }

            LogUtil.Forwarders.RemoveAll(y => Channels.Select(x => x.Value.Action).Contains(y));
            foreach (var cid in Relays.Keys.ToList())
                DisposeRelay(cid);
            Channels.Clear();
            SysCordSettings.Settings.LoggingChannels.Clear();
            await ReplyAsync("Logging cleared from all channels!").ConfigureAwait(false);
        }

        private RemoteControlAccess GetReference(IChannel channel) => new()
        {
            ID = channel.Id,
            Name = channel.Name,
            Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
        };
    }
}