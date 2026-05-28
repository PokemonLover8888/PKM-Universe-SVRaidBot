using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.SV.BotRaid.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace SysBot.Pokemon.WinForms
{
    public sealed partial class Main : Form
    {
        private readonly List<PokeBotState> Bots = new();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal ProgramConfig Config { get; set; }
        private IPokeBotRunner RunningEnvironment { get; set; }
        public readonly ISwitchConnectionAsync? SwitchConnection;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static bool IsUpdating { get; set; } = false;
        private System.Windows.Forms.Timer _autoSaveTimer;
        private System.Windows.Forms.Timer _updateCheckTimer;
        private TcpListener? _tcpListener;
        private CancellationTokenSource? _cts;
        private bool hasUpdate = false;
        private double pulsePhase = 0;
        private Color lastIndicatorColor = Color.Empty;
        private DateTime lastIndicatorUpdate = DateTime.MinValue;
        private const int PULSE_UPDATE_INTERVAL_MS = 50; 

        public Main()
        {
            // Sweep stale .port files (PIDs no longer alive) before we register our own.
            SweepStalePortFiles();

            InitializeComponent();
            Load += async (sender, e) => await InitializeAsync();

            // Initialize dummy controls for compatibility
            TC_Main = new TabControl { Visible = false };
            Tab_Bots = new TabPage();
            Tab_Hub = new TabPage();
            Tab_Logs = new TabPage();
            TC_Main.TabPages.AddRange(new[] { Tab_Bots, Tab_Hub, Tab_Logs });
            comboBox1 = new ComboBox { Visible = false };
            TC_Main.SendToBack();
            comboBox1.SendToBack();
        }

        private async Task InitializeAsync()
        {
            if (IsUpdating)
                return;
            string discordName = string.Empty;

            // Update checker - check silently on startup
            try
            {
                var (updateAvailable, _, _) = await UpdateChecker.CheckForUpdatesAsync(forceShow: false);
                hasUpdate = updateAvailable;
            }
            catch { /* Ignore update check errors on startup */ }

            if (File.Exists(Program.ConfigPath))
            {
                try
                {
                    var lines = File.ReadAllText(Program.ConfigPath);
                    Config = JsonSerializer.Deserialize(lines, ProgramConfigContext.Default.ProgramConfig) ?? new ProgramConfig();
                }
                catch (Exception ex)
                {
                    LogUtil.LogError($"Config corrupted, trying backup: {ex.Message}", "Config");

                    // Try to load from most recent backup
                    var backupFiles = Directory.GetFiles(Path.GetDirectoryName(Program.ConfigPath), "*.backup_*")
                        .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                        .FirstOrDefault();

                    if (backupFiles != null && File.Exists(backupFiles))
                    {
                        var backupLines = File.ReadAllText(backupFiles);
                        Config = JsonSerializer.Deserialize(backupLines, ProgramConfigContext.Default.ProgramConfig) ?? new ProgramConfig();
                        File.WriteAllText(Program.ConfigPath, backupLines); // Restore backup
                        LogUtil.LogInfo($"Restored config from backup", "Config");
                    }
                    else
                    {
                        Config = new ProgramConfig();
                        LogUtil.LogError("No valid backup found, using new config", "Config");
                    }
                }

                LogConfig.MaxArchiveFiles = Config.Hub.MaxArchiveFiles;
                LogConfig.LoggingEnabled = Config.Hub.LoggingEnabled;

                RunningEnvironment = GetRunner(Config);
                foreach (var bot in Config.Bots)
                {
                    bot.Initialize();
                    AddBot(bot);
                }
            }
            else
            {
                Config = new ProgramConfig();
                RunningEnvironment = GetRunner(Config);
            }

            LoadControls();
            Text = $"{(string.IsNullOrEmpty(Config.Hub.BotName) ? "Raid-SV — PKM Universe Raid System" : Config.Hub.BotName)} {SVRaidBot.Version} ({Config.Mode})";
            trayIcon.Text = (Config?.Hub?.BotName != null && !string.IsNullOrEmpty(Config.Hub.BotName)) ? Config.Hub.BotName : "Raid-SV";
            _ = Task.Run(BotMonitor);
            InitUtil.InitializeStubs(Config.Mode);
            // Start periodic update checks
            StartUpdateCheckTimer();

            LogUtil.LogInfo($"Bot initialization complete", "System");
            _ = Task.Run(() =>
            {
                try
                {
                    this.InitWebServer();
                }
                catch (Exception ex)
                {
                    LogUtil.LogError($"Failed to initialize web server: {ex.Message}", "System");
                }
            });
        }

        private void RTB_Logs_TextChanged(object sender, EventArgs e)
        {
            RTB_Logs.Invalidate();
            RTB_Logs.Update();
        }

        private static IPokeBotRunner GetRunner(ProgramConfig cfg) => cfg.Mode switch
        {
            ProgramMode.SV => new PokeBotRunnerImpl<PK9>(cfg.Hub, new BotFactory9SV()),
            _ => throw new IndexOutOfRangeException("Unsupported mode."),
        };

        private async Task BotMonitor()
        {
            while (!Disposing)
            {
                try
                {
                    foreach (var c in FLP_Bots.Controls.OfType<BotController>())
                        c.ReadState();
                }
                catch
                {
                    // Updating the collection by adding/removing bots will change the iterator
                    // Can try a for-loop or ToArray, but those still don't prevent concurrent mutations of the array.
                    // Just try, and if failed, ignore. Next loop will be fine. Locks on the collection are kinda overkill, since this task is not critical.
                }

                // Push real connection state to the Dashboard + sidebar status pills.
                try { UpdateConnectionStatus(); } catch { /* best-effort */ }

                await Task.Delay(2_000).ConfigureAwait(false);
            }
        }

        private void UpdateConnectionStatus()
        {
            var D = typeof(SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState);

            // Switch IP from the first configured bot; connection = bot runner running.
            string ip = (Bots != null && Bots.Count > 0) ? (Bots[0].Connection?.IP ?? string.Empty) : string.Empty;
            bool running = RunningEnvironment != null && RunningEnvironment.IsRunning;
            bool hasBot = Bots != null && Bots.Count > 0;

            SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SwitchIp = ip;
            SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SwitchConnected = hasBot && running;
            SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SysBotBaseReady = hasBot && running;
            // DiscordConnected is set by SysCord; just read it for the pill.
            bool discord = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.DiscordConnected;

            // Update the sidebar status pills (these are always visible, regardless of view).
            SetStatusPill("switch",      string.IsNullOrEmpty(ip) ? "not set" : ip, hasBot && running);
            SetStatusPill("discord",     discord ? "connected" : "offline",         discord);
            SetStatusPill("sys-botbase", (hasBot && running) ? "ready" : "idle",     hasBot && running);
        }

        private void LoadControls()
        {
            MinimumSize = Size;
            // Advanced form is built LAZILY on first click of the ADVANCED nav — see SwitchConfigSection.
            WireQuickSettings();
            _autoSaveTimer = new System.Windows.Forms.Timer
            {
                Interval = 10_000,
                Enabled = true
            };
            _autoSaveTimer.Tick += (s, e) => SaveCurrentConfig();
            var routines = ((PokeRoutineType[])Enum.GetValues(typeof(PokeRoutineType))).Where(z => RunningEnvironment.SupportsRoutine(z));
            var list = routines.Select(z => new ComboItem(z.ToString(), (int)z)).ToArray();
            CB_Routine.DisplayMember = nameof(ComboItem.Text);
            CB_Routine.ValueMember = nameof(ComboItem.Value);
            CB_Routine.DataSource = list;
            CB_Routine.SelectedValue = (int)PokeRoutineType.RotatingRaidBot; // default option

            var protocols = (SwitchProtocol[])Enum.GetValues(typeof(SwitchProtocol));
            var listP = protocols.Select(z => new ComboItem(z.ToString(), (int)z)).ToArray();
            CB_Protocol.DisplayMember = nameof(ComboItem.Text);
            CB_Protocol.ValueMember = nameof(ComboItem.Value);
            CB_Protocol.DataSource = listP;
            CB_Protocol.SelectedIndex = (int)SwitchProtocol.WiFi; // default option

            LogUtil.Forwarders.Add(AppendLog);
        }

        private void AppendLog(string message, string identity)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] - {identity}: {message}{Environment.NewLine}";
            if (InvokeRequired)
                Invoke((MethodInvoker)(() => UpdateLog(line)));
            else
                UpdateLog(line);
        }

        private static readonly Color LogColorError = Color.FromArgb(0xF8, 0x71, 0x71);
        private static readonly Color LogColorWarn  = Color.FromArgb(0xFB, 0xBF, 0x24);
        private static readonly Color LogColorInfo  = Color.FromArgb(0xE0, 0xE0, 0xE0);
        private static readonly Color LogColorDebug = Color.FromArgb(0x8A, 0x8A, 0x8A);
        private static readonly Color LogColorTime  = Color.FromArgb(0xD4, 0xAF, 0x37);

        private void UpdateLog(string line)
        {
            // ghetto truncate
            if (RTB_Logs.Lines.Length > 99_999)
                RTB_Logs.Lines = RTB_Logs.Lines.Skip(25_0000).ToArray();

            // Pick a color for this line based on level keywords (case-insensitive).
            Color lineColor = LogColorInfo;
            var lower = line.AsSpan();
            // Check after the timestamp prefix (e.g. "[12:34:56] - ") for level markers.
            if (Contains(lower, "error") || Contains(lower, "fatal") || Contains(lower, "exception") || Contains(lower, "failed"))
                lineColor = LogColorError;
            else if (Contains(lower, "warn"))
                lineColor = LogColorWarn;
            else if (Contains(lower, "debug") || Contains(lower, "trace"))
                lineColor = LogColorDebug;

            // Append timestamp segment in gold, rest in the level color, for at-a-glance scanning.
            int closeBracket = line.IndexOf(']');
            if (closeBracket > 0 && line.StartsWith("["))
            {
                RTB_Logs.SelectionStart = RTB_Logs.TextLength;
                RTB_Logs.SelectionLength = 0;
                RTB_Logs.SelectionColor = LogColorTime;
                RTB_Logs.AppendText(line.Substring(0, closeBracket + 1));

                RTB_Logs.SelectionStart = RTB_Logs.TextLength;
                RTB_Logs.SelectionLength = 0;
                RTB_Logs.SelectionColor = lineColor;
                RTB_Logs.AppendText(line.Substring(closeBracket + 1));
            }
            else
            {
                RTB_Logs.SelectionStart = RTB_Logs.TextLength;
                RTB_Logs.SelectionLength = 0;
                RTB_Logs.SelectionColor = lineColor;
                RTB_Logs.AppendText(line);
            }
            RTB_Logs.ScrollToCaret();
        }

        private static bool Contains(ReadOnlySpan<char> haystack, string needle)
        {
            return haystack.IndexOf(needle.AsSpan(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Delete this process's own SVRaidBot_{pid}.port file. Safe to call repeatedly.</summary>
        private static void CleanupOwnPortFile()
        {
            try
            {
                var dir = Path.GetDirectoryName(Application.ExecutablePath);
                if (string.IsNullOrEmpty(dir)) return;
                var path = Path.Combine(dir, $"SVRaidBot_{Environment.ProcessId}.port");
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch { /* cleanup is best-effort */ }
        }

        /// <summary>
        /// At startup, sweep the EXE folder for SVRaidBot_*.port files whose PIDs no longer
        /// correspond to a live process — those are leftovers from prior crashes or force-kills.
        /// </summary>
        private static void SweepStalePortFiles()
        {
            try
            {
                var dir = Path.GetDirectoryName(Application.ExecutablePath);
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
                foreach (var path in Directory.EnumerateFiles(dir, "SVRaidBot_*.port"))
                {
                    var name = Path.GetFileNameWithoutExtension(path); // "SVRaidBot_12345"
                    var underscore = name.IndexOf('_');
                    if (underscore < 0) continue;
                    if (!int.TryParse(name.AsSpan(underscore + 1), out var pid)) continue;
                    bool alive;
                    try { alive = System.Diagnostics.Process.GetProcessById(pid) != null; }
                    catch (ArgumentException) { alive = false; }
                    catch { alive = true; } // be conservative — don't delete if uncertain
                    if (!alive)
                    {
                        try { File.Delete(path); } catch { }
                    }
                }
            }
            catch { /* sweep is best-effort */ }
        }

        /// <summary>
        /// Hooks the 6 Quick Settings cards to the live Config object: load current values into the controls,
        /// write back to Config on edit, and refresh the PropertyGrid so the Advanced view stays in sync.
        /// </summary>
        private void WireQuickSettings()
        {
            if (Config?.Hub == null)
                return;
            var hub = Config.Hub;

            // 1. Populate current values
            qsBotName.Text       = hub.BotName ?? string.Empty;
            qsDiscordToken.Text  = hub.Discord?.Token ?? string.Empty;
            qsCommandPrefix.Text = (hub.Discord?.CommandPrefix?.Count > 0) ? hub.Discord.CommandPrefix[0] : "!";
            qsGameStatus.Text    = hub.Discord?.BotGameStatus ?? string.Empty;
            qsTotalRaids.Value   = Math.Clamp(hub.RotatingRaidSV?.RaidSettings?.TotalRaidsToHost ?? 0, 0, 100000);
            qsHideRaidCode.Checked = hub.RotatingRaidSV?.EmbedToggles?.HideRaidCode ?? true;
            qsUseScreenshots.Checked = hub.RotatingRaidSV?.EmbedToggles?.TakeScreenshot ?? true;
            qsRotateRandom.Checked   = hub.RotatingRaidSV?.RaidSettings?.RandomRotation ?? false;
            qsKeepDaySeed.Checked    = hub.RotatingRaidSV?.RaidSettings?.KeepDaySeed   ?? true;

            // 2. Wire change handlers — write back to Config + refresh PropertyGrid
            qsBotName.TextChanged += (s, e) =>
            {
                hub.BotName = qsBotName.Text;
                PG_Hub.Refresh();
            };
            qsDiscordToken.TextChanged += (s, e) =>
            {
                if (hub.Discord != null)
                {
                    hub.Discord.Token = qsDiscordToken.Text;
                    PG_Hub.Refresh();
                }
            };
            qsCommandPrefix.TextChanged += (s, e) =>
            {
                if (hub.Discord != null)
                {
                    var prefix = string.IsNullOrWhiteSpace(qsCommandPrefix.Text) ? "!" : qsCommandPrefix.Text;
                    if (hub.Discord.CommandPrefix.Count == 0) hub.Discord.CommandPrefix.Add(prefix);
                    else hub.Discord.CommandPrefix[0] = prefix;
                    PG_Hub.Refresh();
                }
            };
            qsGameStatus.TextChanged += (s, e) =>
            {
                if (hub.Discord != null)
                {
                    hub.Discord.BotGameStatus = qsGameStatus.Text;
                    PG_Hub.Refresh();
                }
            };
            qsTotalRaids.ValueChanged += (s, e) =>
            {
                if (hub.RotatingRaidSV?.RaidSettings != null)
                {
                    hub.RotatingRaidSV.RaidSettings.TotalRaidsToHost = (int)qsTotalRaids.Value;
                    PG_Hub.Refresh();
                }
            };
            qsHideRaidCode.CheckedChanged += (s, e) =>
            {
                if (hub.RotatingRaidSV?.EmbedToggles != null)
                {
                    hub.RotatingRaidSV.EmbedToggles.HideRaidCode = qsHideRaidCode.Checked;
                    PG_Hub.Refresh();
                }
            };
            qsUseScreenshots.CheckedChanged += (s, e) =>
            {
                if (hub.RotatingRaidSV?.EmbedToggles != null)
                {
                    hub.RotatingRaidSV.EmbedToggles.TakeScreenshot = qsUseScreenshots.Checked;
                    PG_Hub.Refresh();
                }
            };
            qsRotateRandom.CheckedChanged += (s, e) =>
            {
                if (hub.RotatingRaidSV?.RaidSettings != null)
                {
                    hub.RotatingRaidSV.RaidSettings.RandomRotation = qsRotateRandom.Checked;
                    PG_Hub.Refresh();
                }
            };
            qsKeepDaySeed.CheckedChanged += (s, e) =>
            {
                if (hub.RotatingRaidSV?.RaidSettings != null)
                {
                    hub.RotatingRaidSV.RaidSettings.KeepDaySeed = qsKeepDaySeed.Checked;
                    PG_Hub.Refresh();
                }
            };
        }

        private ProgramConfig GetCurrentConfiguration()
        {
            if (Config == null)
            {
                throw new InvalidOperationException("Config has not been initialized because a valid license was not entered.");
            }
            Config.Bots = Bots.ToArray();
            return Config;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsUpdating)
            {
                return;
            }

            // If not exiting, minimize to tray instead
            if (!isExiting)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                trayIcon.Visible = true;
                trayIcon.ShowBalloonTip(2000, "Raid-SV", "Application minimized to system tray. Right-click → Exit to quit.", ToolTipIcon.Info);
                return;
            }
            this.StopWebServer();

            // Delete the port info file
            CleanupOwnPortFile();

            if (_autoSaveTimer != null)
            {
                _autoSaveTimer.Stop();
                _autoSaveTimer.Dispose();
            }

            if (_updateCheckTimer != null)
            {
                _updateCheckTimer.Stop();
                _updateCheckTimer.Dispose();
            }

            if (animationTimer != null)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
            }

            // Belt-and-suspenders: try the cleanup again here in case the .port file was
            // recreated by an async path after the first attempt.
            CleanupOwnPortFile();
            // Create a backup copy of config
            try
            {
                if (File.Exists(Program.ConfigPath))
                {
                    File.Copy(Program.ConfigPath, Program.ConfigPath + ".backup", true);
                }
            }
            catch { }
            SaveCurrentConfig();
            var bots = RunningEnvironment;
            if (!bots.IsRunning)
                return;

            async Task WaitUntilNotRunning()
            {
                while (bots.IsRunning)
                    await Task.Delay(10).ConfigureAwait(false);
            }

            // Try to let all bots hard-stop before ending execution of the entire program.
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            bots.StopAll();
            Task.WhenAny(WaitUntilNotRunning(), Task.Delay(5_000)).ConfigureAwait(true).GetAwaiter().GetResult();
        }

        private void SaveCurrentConfig()
        {
            try
            {
                var cfg = GetCurrentConfiguration();
                var lines = JsonSerializer.Serialize(cfg, ProgramConfigContext.Default.ProgramConfig);

                string tempPath = Program.ConfigPath + ".tmp";
                File.WriteAllText(tempPath, lines);
                File.Move(tempPath, Program.ConfigPath, true);
            }
            catch (Exception ex)
            {
                LogUtil.LogError($"Failed to save config: {ex.Message}", "Config");
            }
        }

        [JsonSerializable(typeof(ProgramConfig))]
        [JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
        public sealed partial class ProgramConfigContext : JsonSerializerContext
        { }

        private void B_Start_Click(object sender, EventArgs e)
        {
            // Reset error state when starting
            SV.BotRaid.RotatingRaidBotSV.HasErrored = false;

            SaveCurrentConfig();

            LogUtil.LogInfo("Starting all bots...", "Form");
            RunningEnvironment.InitializeStart();
            SendAll(BotControlCommand.Start);
            btnNavLogs.PerformClick(); // Switch to logs view

            if (Bots.Count == 0)
                WinFormsUtil.Alert("No bots configured, but all supporting services have been started.");
        }

        private void B_RebootAndStop_Click(object sender, EventArgs e)
        {
            B_Stop_Click(sender, e);
            Task.Run(async () =>
            {
                await Task.Delay(3_500).ConfigureAwait(false);
                SaveCurrentConfig();
                LogUtil.LogInfo("Restarting all the consoles...", "Form");
                RunningEnvironment.InitializeStart();
                SendAll(BotControlCommand.RebootAndStop);
                Invoke((MethodInvoker)(() => btnNavLogs.PerformClick())); // Switch to logs view
                if (Bots.Count == 0)
                    WinFormsUtil.Alert("No bots configured, but all supporting services have been issued the reboot command.");
            });
        }

        private async void Updater_Click(object sender, EventArgs e)
        {
            var (updateAvailable, updateRequired, newVersion) = await UpdateChecker.CheckForUpdatesAsync(forceShow: true);
            hasUpdate = updateAvailable; // Update the indicator
        }

        private void StartUpdateCheckTimer()
        {
            _updateCheckTimer = new System.Windows.Forms.Timer
            {
                Interval = 3600000, // Check every hour
                Enabled = true
            };
            _updateCheckTimer.Tick += async (s, e) =>
            {
                try
                {
                    var (updateAvailable, _, _) = await UpdateChecker.CheckForUpdatesAsync(forceShow: false);
                    hasUpdate = updateAvailable;
                }
                catch { /* Ignore update check errors */ }
            };
        }

        private void RefreshMap_Click(object sender, EventArgs e)
        {
            SaveCurrentConfig();
            LogUtil.LogInfo("Sending RefreshMap command to all bots.", "Refresh Map");
            SendAll(BotControlCommand.RefreshMap);
            btnNavLogs.PerformClick(); // Switch to logs view
            if (Bots.Count == 0)
                WinFormsUtil.Alert("No bots configured, but all supporting services have been issued the refresh map command.");
        }

        private void SendAll(BotControlCommand cmd)
        {
            foreach (var c in FLP_Bots.Controls.OfType<BotController>())
                c.SendCommand(cmd, false);

            LogUtil.LogText($"All bots have been issued a command to {cmd}.");
        }

        private void B_Stop_Click(object sender, EventArgs e)
        {
            var env = RunningEnvironment;
            if (!env.IsRunning && (ModifierKeys & Keys.Alt) == 0)
            {
                WinFormsUtil.Alert("Nothing is currently running.");
                return;
            }

            var cmd = BotControlCommand.Stop;

            if ((ModifierKeys & Keys.Control) != 0 || (ModifierKeys & Keys.Shift) != 0) // either, because remembering which can be hard
            {
                if (env.IsRunning)
                {
                    WinFormsUtil.Alert("Commanding all bots to Idle.", "Press Stop (without a modifier key) to hard-stop and unlock control, or press Stop with the modifier key again to resume.");
                    cmd = BotControlCommand.Idle;
                }
                else
                {
                    WinFormsUtil.Alert("Commanding all bots to resume their original task.", "Press Stop (without a modifier key) to hard-stop and unlock control.");
                    cmd = BotControlCommand.Resume;
                }
            }
            SendAll(cmd);
        }

        private void B_New_Click(object sender, EventArgs e)
        {
            var cfg = CreateNewBotConfig();
            if (!AddBot(cfg))
            {
                WinFormsUtil.Alert("Unable to add bot; ensure details are valid and not duplicate with an already existing bot.");
                return;
            }
            System.Media.SystemSounds.Asterisk.Play();
        }

        private bool AddBot(PokeBotState cfg)
        {
            if (!cfg.IsValid())
                return false;

            // Disallow duplicate routines.
            if (Bots.Any(z => z.Connection.Equals(cfg.Connection) && cfg.NextRoutineType == z.NextRoutineType))
                return false;

            PokeRoutineExecutorBase newBot;
            try
            {
                Console.WriteLine($"Current Mode ({Config.Mode}) does not support this type of bot ({cfg.CurrentRoutineType}).");
                newBot = RunningEnvironment.CreateBotFromConfig(cfg);
            }
            catch
            {
                return false;
            }

            try
            {
                RunningEnvironment.Add(newBot);
            }
            catch (ArgumentException ex)
            {
                WinFormsUtil.Error(ex.Message);
                return false;
            }

            AddBotControl(cfg);
            Bots.Add(cfg);
            return true;
        }

        private void AddBotControl(PokeBotState cfg)
        {
            var row = new BotController { Width = FLP_Bots.Width - 60 };
            row.Initialize(RunningEnvironment, cfg);
            FLP_Bots.Controls.Add(row);
            FLP_Bots.SetFlowBreak(row, true);

            // Modern dark theme styling is now handled in BotController itself

            row.Click += (s, e) =>
            {
                var details = cfg.Connection;
                TB_IP.Text = details.IP;
                NUD_Port.Value = details.Port;
                CB_Protocol.SelectedIndex = (int)details.Protocol;
                CB_Routine.SelectedValue = (int)cfg.InitialRoutine;
            };

            row.Remove += (s, e) =>
            {
                Bots.Remove(row.State);
                RunningEnvironment.Remove(row.State, !RunningEnvironment.Config.SkipConsoleBotCreation);
                FLP_Bots.Controls.Remove(row);
            };
        }

        private PokeBotState CreateNewBotConfig()
        {
            var ip = TB_IP.Text;
            var port = (int)NUD_Port.Value;
            var cfg = BotConfigUtil.GetConfig<SwitchConnectionConfig>(ip, port);
            cfg.Protocol = (SwitchProtocol)WinFormsUtil.GetIndex(CB_Protocol);

            var pk = new PokeBotState { Connection = cfg };
            var type = (PokeRoutineType)WinFormsUtil.GetIndex(CB_Routine);
            pk.Initialize(type);
            return pk;
        }

        private void FLP_Bots_Resize(object sender, EventArgs e)
        {
            foreach (var c in FLP_Bots.Controls.OfType<BotController>())
                c.Width = FLP_Bots.Width - 60; // Account for scrollbar and padding
        }

        private void CB_Protocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            TB_IP.Visible = CB_Protocol.SelectedIndex == 0;
        }

        /// <summary>
        /// Public, thread-safe entrypoint for restoring the window from the system tray.
        /// Called from Program.cs when a second launch attempt detects the running instance.
        /// </summary>
        public void RestoreFromTray()
        {
            if (InvokeRequired) { BeginInvoke((MethodInvoker)ShowFromTray); }
            else { ShowFromTray(); }
        }

        private void ShowFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            trayIcon.Visible = false;
            BringToFront();
            Activate();

            int headerHeight = headerPanel.Height + 10;

            // Adjust padding only if panels haven't been initialized yet (checking Padding.Top as an indicator)
            if (hubPanel.Padding.Top <= 40)
                hubPanel.Padding = new Padding(40, headerHeight, 40, 40);

            if (logsPanel.Padding.Top <= 40)
                logsPanel.Padding = new Padding(40, headerHeight, 40, 40);

            // Force layout recalculation explicitly
            hubPanel.PerformLayout();
            PG_Hub.Refresh();

            logsPanel.PerformLayout();
            RTB_Logs.Refresh();

            // Ensure control buttons are properly positioned
            HeaderPanel_Resize(headerPanel, EventArgs.Empty);
        }

        private void ExitApplication()
        {
            var bots = RunningEnvironment;
            if (bots != null && bots.IsRunning)
            {
                var result = MessageBox.Show(
                    "Bots are currently running. Do you want to stop all bots and exit?",
                    "Exit Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;

                bots.StopAll();
            }

            isExiting = true;
            trayIcon.Dispose();
            Application.Exit();
        }
    }

    // Extension methods for graphics
    public static class GraphicsExtensions
    {
        public static void AddRoundedRectangle(this GraphicsPath path, Rectangle rect, int radius)
        {
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
        }
    }
}