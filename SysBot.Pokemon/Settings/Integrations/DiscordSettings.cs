using System.ComponentModel;
using System.Linq;
using static SysBot.Pokemon.RotatingRaidSettingsSV;

namespace SysBot.Pokemon
{
    public class DiscordSettings
    {
        private const string Startup = nameof(Startup);
        private const string Operation = nameof(Operation);
        private const string Channels = nameof(Channels);
        private const string Roles = nameof(Roles);
        private const string Users = nameof(Users);

        public override string ToString() => "Discord Integration Settings";

        // Startup

        [Category(Startup), Description("Bot login token.")]
        public string Token { get; set; } = string.Empty;

        [Category(Startup), Description("List of bot command prefixes")]
        public System.Collections.Generic.List<string> CommandPrefix { get; set; } = new System.Collections.Generic.List<string> { "!" };

        public void EnsureCommandPrefix()
        {
            if (CommandPrefix.Count == 0)
            {
                CommandPrefix.Add("!"); // PKM Universe Reborn — raid bot uses ! to avoid clashing with trade-bot $
            }

            // If CommandPrefix is a single string, make it a list
            if (CommandPrefix.Count == 1 && CommandPrefix[0].Contains(","))
            {
                CommandPrefix = CommandPrefix[0].Split(",").ToList();
            }
        }

        [Category(Startup), Description("Toggle to handle commands asynchronously or synchronously.")]
        public bool AsyncCommands { get; set; }

        [Category(Startup), Description("Custom Status for playing a game.")]
        public string BotGameStatus { get; set; } = "PKM Universe Raids · pkm-universe.com";

        [Category(Operation), Description("Custom message the bot will reply with when a user says hello to it. Use string formatting to mention the user in the reply.")]
        public string HelloResponse { get; set; } = "Hello, {0}!  I'm online!";

        // Whitelists
        [Category(Roles), Description("Users with this role are allowed to enter the Raid queue.")]
        public RemoteControlAccessList RoleRaidRequest { get; set; } = new() { AllowIfEmpty = false };

        [Browsable(false)]
        [Category(Roles), Description("Users with this role are allowed to remotely control the console (if running as Remote Control Bot.")]
        public RemoteControlAccessList RoleRemoteControl { get; set; } = new() { AllowIfEmpty = false };

        [Category(Roles), Description("Users with this role are allowed to bypass command restrictions.")]
        public RemoteControlAccessList RoleSudo { get; set; } = new() { AllowIfEmpty = false };

        // Operation
        [Category(Users), Description("Users with these user IDs cannot use the bot.")]
        public RemoteControlAccessList UserBlacklist { get; set; } = new();

        [Category(Channels), Description("Channels with these IDs are the only channels where the bot acknowledges commands.")]
        public RemoteControlAccessList ChannelWhitelist { get; set; } = new();

        [Category(Users), Description("Comma separated Discord user IDs that will have sudo access to the Bot Hub.")]
        public RemoteControlAccessList GlobalSudoList { get; set; } = new();

        [Category(Users), Description("Disabling this will remove global sudo support.")]
        public bool AllowGlobalSudo { get; set; } = true;

        [Category(Channels), Description("Channel IDs that will echo the log bot data.")]
        public RemoteControlAccessList LoggingChannels { get; set; } = new();

        [Category(Channels), Description("Raid Embed Channels.")]
        public RemoteControlAccessList EchoChannels { get; set; } = new();

        [Category(Channels), Description("Channel ID where the weekly leaderboard auto-post lands every Sunday at midnight UTC. 0 = disabled.")]
        [DisplayName("Weekly Leaderboard Channel ID")]
        public ulong LeaderboardChannelId { get; set; } = 0;

        [Category(Channels), Description("PKM Universe region-routing: raids hosted in PALDEA post here. 0 = use the default Echo Channels instead.")]
        [DisplayName("Paldea Raid Channel ID")]
        public ulong PaldeaRaidChannelId { get; set; } = 0;

        [Category(Channels), Description("PKM Universe region-routing: raids hosted in KITAKAMI (Teal Mask) post here. 0 = use the default Echo Channels instead.")]
        [DisplayName("Kitakami Raid Channel ID")]
        public ulong KitakamiRaidChannelId { get; set; } = 0;

        [Category(Channels), Description("PKM Universe region-routing: raids hosted in BLUEBERRY (Indigo Disk) post here. 0 = use the default Echo Channels instead.")]
        [DisplayName("Blueberry Raid Channel ID")]
        public ulong BlueberryRaidChannelId { get; set; } = 0;

        public LogEchoSettingsCategory LogEcho { get; set; } = new();

        /// <summary>
        /// Controls how bot logs reach the Discord logging channels. The relay used to send ONE
        /// Discord message per log line, which on a raid bot is ~13,000 messages/day and was
        /// responsible for ~93% of the fleet's Discord 429s (measured 2026-08-30). Nothing here
        /// affects disk logging — logs\SysBotLog.txt always gets every line.
        /// </summary>
        [Category(Operation), TypeConverter(typeof(CategoryConverter<LogEchoSettingsCategory>))]
        public class LogEchoSettingsCategory
        {
            public override string ToString() => "Log Echo Settings";

            [Category(Operation), Description("Collect log lines and send them as ONE Discord message every N seconds instead of one message per line. This is what stops the rate-limit bursts. 0 = send immediately (old behaviour).")]
            [DisplayName("Batch Seconds")]
            public int BatchSeconds { get; set; } = 10;

            [Category(Operation), Description("Skip routine raid-loop chatter (lobby/den/seed/offset mechanics) when relaying to Discord. Warnings and errors are ALWAYS relayed regardless. Everything still goes to logs\\SysBotLog.txt.")]
            [DisplayName("Skip Routine Chatter")]
            public bool SkipRoutine { get; set; } = true;

            [Category(Operation), Description("Collapse a line that repeats within this many seconds into a single '(+N repeats suppressed)' note. 0 = relay every repeat.")]
            [DisplayName("Dedupe Seconds")]
            public int DedupeSeconds { get; set; } = 300;

            [Category(Operation), Description("Extra case-insensitive substrings to skip, separated by ';'. Applied before the keep-warnings rule, so this can silence a noisy warning too.")]
            [DisplayName("Extra Skip Patterns")]
            public string ExtraSkipPatterns { get; set; } = string.Empty;
        }

        public AnnouncementSettingsCategory AnnouncementSettings { get; set; } = new();

        [Category(Operation), TypeConverter(typeof(CategoryConverter<AnnouncementSettingsCategory>))]
        public class AnnouncementSettingsCategory
        {
            public override string ToString() => "Announcement Settings";

            [Category("Embed Settings"), Description("Thumbnail option for announcements.")]
            public ThumbnailOption AnnouncementThumbnailOption { get; set; } = ThumbnailOption.Gengar;

            [Category("Embed Settings"), Description("Custom thumbnail URL for announcements.")]
            public string CustomAnnouncementThumbnailUrl { get; set; } = string.Empty;

            public EmbedColorOption AnnouncementEmbedColor { get; set; } = EmbedColorOption.Blue;

            [Category("Embed Settings"), Description("Enable random thumbnail selection for announcements.")]
            public bool RandomAnnouncementThumbnail { get; set; } = false;

            [Category("Embed Settings"), Description("Enable random color selection for announcements.")]
            public bool RandomAnnouncementColor { get; set; } = false;
        }
    }
}