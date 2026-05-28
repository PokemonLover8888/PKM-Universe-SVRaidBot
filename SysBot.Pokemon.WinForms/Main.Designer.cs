using SysBot.Pokemon.WinForms.Properties;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SysBot.Pokemon.WinForms
{
    partial class Main
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing && trayIcon != null)
            {
                trayIcon.Dispose();
            }
            if (disposing && _logoBrush != null)
            {
                _logoBrush.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));

            // Initialize timer for animations
            animationTimer = new System.Windows.Forms.Timer(this.components);
            animationTimer.Interval = 16; // Back to 60fps for smooth animations
            animationTimer.Tick += AnimationTimer_Tick;

            // Initialize tray icon
            trayIcon = new NotifyIcon(this.components);
            trayContextMenu = new ContextMenuStrip(this.components);

            // Main Panel Structure
            mainLayoutPanel = new TableLayoutPanel();
            sidebarPanel = new Panel();
            contentPanel = new Panel();
            headerPanel = new Panel();
            particlePanel = new Panel(); // Background particles

            // Sidebar Components
            logoPanel = new Panel();
            navButtonsPanel = new FlowLayoutPanel();
            btnNavDashboard = new Button();
            btnNavBots = new Button();
            btnNavHub = new Button();
            btnNavLogs = new Button();
            btnNavExit = new Button();
            sidebarBottomPanel = new Panel();
            btnUpdate = new Button();
            statusIndicator = new Panel();

            // Header Components
            titleLabel = new Label();
            controlButtonsPanel = new FlowLayoutPanel();
            btnStart = new Button();
            btnStop = new Button();
            btnReboot = new Button();
            btnRefreshMap = new Button();

            // Content Panels
            botsPanel = new Panel();
            hubPanel = new Panel();
            logsPanel = new Panel();

            // Bots Panel Components
            botHeaderPanel = new Panel();
            addBotPanel = new Panel();
            TB_IP = new TextBox();
            NUD_Port = new NumericUpDown();
            CB_Protocol = new ComboBox();
            CB_Routine = new ComboBox();
            B_New = new Button();
            FLP_Bots = new FlowLayoutPanel();

            // Hub Panel Components
            PG_Hub = new PropertyGrid();

            // Logs Panel Components
            RTB_Logs = new RichTextBox();
            logsHeaderPanel = new Panel();
            logSearchBox = new TextBox();
            searchStatusLabel = new Label();
            btnClearLogs = new Button();

            mainLayoutPanel.SuspendLayout();
            sidebarPanel.SuspendLayout();
            navButtonsPanel.SuspendLayout();
            sidebarBottomPanel.SuspendLayout();
            headerPanel.SuspendLayout();
            controlButtonsPanel.SuspendLayout();
            contentPanel.SuspendLayout();
            botsPanel.SuspendLayout();
            botHeaderPanel.SuspendLayout();
            addBotPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NUD_Port).BeginInit();
            hubPanel.SuspendLayout();
            logsPanel.SuspendLayout();
            logsHeaderPanel.SuspendLayout();
            SuspendLayout();

            // Form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1280, 720);
            MinimumSize = new Size(1100, 600);
            BackColor = Color.FromArgb(23, 26, 33);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            Icon = Resources.icon;
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Raid-SV — PKM Universe Raid System";
            FormClosing += Main_FormClosing;
            Shown += Main_Shown;
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.DoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // Particle Background Panel - DISABLED to prevent flickering
            // particlePanel.Dock = DockStyle.Fill;
            // particlePanel.BackColor = Color.Transparent;
            // particlePanel.SendToBack();
            // particlePanel.Paint += ParticlePanel_Paint;
            // Controls.Add(particlePanel);

            // Main Layout Panel
            mainLayoutPanel.ColumnCount = 2;
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayoutPanel.Controls.Add(sidebarPanel, 0, 0);
            mainLayoutPanel.Controls.Add(contentPanel, 1, 0);
            mainLayoutPanel.Dock = DockStyle.Fill;
            mainLayoutPanel.Location = new Point(0, 0);
            mainLayoutPanel.Margin = new Padding(0);
            mainLayoutPanel.Name = "mainLayoutPanel";
            mainLayoutPanel.RowCount = 1;
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayoutPanel.TabIndex = 0;
            mainLayoutPanel.BackColor = Color.Transparent;

            // Sidebar Panel
            sidebarPanel.BackColor = Color.FromArgb(18, 18, 18);
            // ORDER MATTERS for Dock=Bottom siblings: the FIRST-added gets the outer (bottom-most) edge.
            // So sidebarBottomPanel is added FIRST → sits at the very bottom (CHECK FOR UPDATES).
            // statusPillsPanel is added next → sits above the bottom panel.
            sidebarPanel.Controls.Add(navButtonsPanel);
            sidebarPanel.Controls.Add(sidebarBottomPanel);
            sidebarPanel.Controls.Add(BuildStatusPillsPanel());
            sidebarPanel.Controls.Add(logoPanel);
            sidebarPanel.Dock = DockStyle.Fill;
            sidebarPanel.Location = new Point(0, 0);
            sidebarPanel.Margin = new Padding(0);
            sidebarPanel.Name = "sidebarPanel";
            sidebarPanel.Size = new Size(260, 720);
            sidebarPanel.TabIndex = 0;

            // Logo Panel
            logoPanel.BackColor = Color.FromArgb(15, 15, 15);
            logoPanel.Dock = DockStyle.Top;
            logoPanel.Height = 100;
            logoPanel.Location = new Point(0, 0);
            logoPanel.Name = "logoPanel";
            logoPanel.Size = new Size(260, 100);
            logoPanel.TabIndex = 2;
            logoPanel.Paint += LogoPanel_Paint;
            EnableDoubleBuffering(logoPanel);

            // Navigation Buttons Panel
            navButtonsPanel.AutoSize = false;
            navButtonsPanel.Controls.Add(btnNavDashboard);
            navButtonsPanel.Controls.Add(btnNavBots);
            navButtonsPanel.Controls.Add(btnNavHub);
            navButtonsPanel.Controls.Add(btnNavLogs);
            navButtonsPanel.Controls.Add(btnNavExit);
            navButtonsPanel.Dock = DockStyle.Fill;
            navButtonsPanel.FlowDirection = FlowDirection.TopDown;
            navButtonsPanel.Location = new Point(0, 100);
            navButtonsPanel.Margin = new Padding(0);
            navButtonsPanel.Name = "navButtonsPanel";
            navButtonsPanel.Padding = new Padding(0, 40, 0, 0);
            navButtonsPanel.Size = new Size(260, 540);
            navButtonsPanel.TabIndex = 1;
            navButtonsPanel.BackColor = Color.Transparent;

            // Navigation Button Style (Dashboard added as index 3, custom icon)
            ConfigureNavButton(btnNavDashboard, "DASHBOARD", 3, "Live raid view");
            ConfigureNavButton(btnNavBots, "BOTS", 0, "Manage bot connections");
            ConfigureNavButton(btnNavHub, "CONFIGURATION", 1, "System settings");
            ConfigureNavButton(btnNavLogs, "SYSTEM LOGS", 2, "View activity logs");
            ConfigureExitButton();

            // Sidebar Bottom Panel
            sidebarBottomPanel.SuspendLayout();
            sidebarBottomPanel.Controls.Add(btnUpdate);
            sidebarBottomPanel.Dock = DockStyle.Bottom;
            sidebarBottomPanel.Height = 80;
            sidebarBottomPanel.Location = new Point(0, 640);
            sidebarBottomPanel.Name = "sidebarBottomPanel";
            sidebarBottomPanel.Padding = new Padding(20, 10, 20, 20);
            sidebarBottomPanel.TabIndex = 0;
            sidebarBottomPanel.BackColor = Color.FromArgb(15, 15, 15);
            sidebarBottomPanel.ResumeLayout(false);

            statusIndicator.BackColor = Color.FromArgb(100, 100, 100); // Default gray
            statusIndicator.Size = new Size(12, 12);
            statusIndicator.Location = new Point(190, 10); // Fixed position from left
            statusIndicator.Name = "statusIndicator";
            statusIndicator.Enabled = false; // Prevent mouse interaction
            statusIndicator.Anchor = AnchorStyles.Top | AnchorStyles.Right; // Anchor to top-right
            CreateCircularRegion(statusIndicator);
            btnUpdate.Controls.Add(statusIndicator);
            statusIndicator.BringToFront();
            statusIndicator.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = statusIndicator.ClientRectangle;
                rect.Inflate(-1, -1);

                using var brush = new SolidBrush(statusIndicator.BackColor);
                e.Graphics.FillEllipse(brush, rect);

                // Add inner highlight
                if (hasUpdate)
                {
                    var highlightRect = new Rectangle(2, 2, 4, 4);
                    using var highlightBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
                    e.Graphics.FillEllipse(highlightBrush, highlightRect);
                }
            };

            // Update Button
            btnUpdate.BackColor = Color.FromArgb(30, 30, 30);
            btnUpdate.Dock = DockStyle.Fill;
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 40, 40);
            btnUpdate.FlatStyle = FlatStyle.Flat;
            btnUpdate.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnUpdate.ForeColor = Color.FromArgb(176, 176, 176);
            btnUpdate.Location = new Point(20, 10);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(220, 50);
            btnUpdate.TabIndex = 0;
            btnUpdate.Text = "CHECK FOR UPDATES";
            btnUpdate.UseVisualStyleBackColor = false;
            btnUpdate.Click += Updater_Click;
            btnUpdate.Cursor = Cursors.Hand;
            btnUpdate.Tag = new ButtonAnimationState();

            // Add tooltip
            var updateTooltip = new ToolTip();
            updateTooltip.SetToolTip(btnUpdate, "Check for updates");
            btnUpdate.MouseEnter += (s, e) => {
                if (hasUpdate)
                {
                    updateTooltip.SetToolTip(btnUpdate, "Update available! Click to download.");
                }
                else
                {
                    updateTooltip.SetToolTip(btnUpdate, "No updates available");
                }
            };

            // Reposition status indicator on resize
            btnUpdate.Resize += (s, e) => {
                if (statusIndicator != null && btnUpdate.Controls.Contains(statusIndicator))
                {
                    statusIndicator.Location = new Point(btnUpdate.ClientSize.Width - 25, 20);
                }
            };
            // Reposition status indicator on resize
            btnUpdate.Resize += (s, e) => {
                if (statusIndicator != null && btnUpdate.Controls.Contains(statusIndicator))
                {
                    statusIndicator.Location = new Point(btnUpdate.ClientSize.Width - 25, 20);
                }
            };
            btnUpdate.Layout += (s, e) => {
                if (statusIndicator != null && btnUpdate.Controls.Contains(statusIndicator))
                {
                    statusIndicator.Location = new Point(btnUpdate.ClientSize.Width - 25, 20);
                }
            };
            btnUpdate.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                var animState = btnUpdate.Tag as ButtonAnimationState;

                // Draw hover glow background - only if hovering
                if (animState != null && animState.HoverProgress > 0 && animState.IsHovering)
                {
                    using var glowBrush = new SolidBrush(Color.FromArgb((int)(20 * animState.HoverProgress), 88, 101, 242));
                    e.Graphics.FillRectangle(glowBrush, btnUpdate.ClientRectangle);
                }

                // Determine icon color with hover effect
                var iconColor = btnUpdate.ForeColor;
                if (animState != null && animState.HoverProgress > 0)
                {
                    iconColor = Color.FromArgb(
                        (int)(176 + (224 - 176) * animState.HoverProgress),
                        (int)(176 + (224 - 176) * animState.HoverProgress),
                        (int)(176 + (224 - 176) * animState.HoverProgress)
                    );
                }

                // Draw icon
                using var iconFont = new Font("Segoe MDL2 Assets", 14F);
                var iconText = "\uE895"; // Download/Update icon

                using var iconBrush = new SolidBrush(iconColor);
                var iconSize = e.Graphics.MeasureString(iconText, iconFont);

                // Position icon on the left
                var iconX = 20;
                var iconY = (btnUpdate.Height - iconSize.Height) / 2;
                e.Graphics.DrawString(iconText, iconFont, iconBrush, iconX, iconY);

                // Draw text after icon
                using var textFont = new Font("Segoe UI", 9F, FontStyle.Regular);
                var text = "CHECK FOR UPDATES";
                var textSize = e.Graphics.MeasureString(text, textFont);
                var textX = iconX + iconSize.Width + 10;
                var textY = (btnUpdate.Height - textSize.Height) / 2;
                e.Graphics.DrawString(text, textFont, iconBrush, textX, textY);

                // Draw glow around status indicator if update available
                if (hasUpdate && statusIndicator != null)
                {
                    var indicatorBounds = new Rectangle(
                        statusIndicator.Left - 3,
                        statusIndicator.Top - 3,
                        statusIndicator.Width + 6,
                        statusIndicator.Height + 6
                    );

                    // Multi-layer glow
                    for (int i = 3; i > 0; i--)
                    {
                        var glowAlpha = (int)(20 / i * (0.5 + 0.5 * Math.Sin(pulsePhase)));
                        using var glowBrush = new SolidBrush(Color.FromArgb(glowAlpha, 87, 242, 135));
                        var glowRect = new Rectangle(
                            indicatorBounds.X - i * 2,
                            indicatorBounds.Y - i * 2,
                            indicatorBounds.Width + i * 4,
                            indicatorBounds.Height + i * 4
                        );
                        e.Graphics.FillEllipse(glowBrush, glowRect);
                    }
                }
            };
            btnUpdate.Text = ""; // Clear text since we're drawing it manually
            ConfigureHoverAnimation(btnUpdate);

            // Content Panel
            contentPanel.BackColor = Color.FromArgb(28, 28, 28);
            BuildDashboardPanel();
            contentPanel.Controls.Add(dashboardPanel!);
            contentPanel.Controls.Add(botsPanel);
            contentPanel.Controls.Add(hubPanel);
            contentPanel.Controls.Add(logsPanel);
            contentPanel.Controls.Add(headerPanel);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(260, 0);
            contentPanel.Margin = new Padding(0);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new Size(1020, 720);
            contentPanel.TabIndex = 1;

            // Header Panel
            headerPanel.BackColor = Color.FromArgb(28, 28, 28);
            headerPanel.Controls.Add(controlButtonsPanel);
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 100;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(1020, 100);
            headerPanel.TabIndex = 3;
            headerPanel.Paint += HeaderPanel_Paint;
            headerPanel.Resize += HeaderPanel_Resize;

            // Title Label
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(224, 224, 224);
            titleLabel.Location = new Point(40, 25);
            titleLabel.MaximumSize = new Size(400, 45);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(250, 45);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Dashboard";

            // Control Buttons Panel
            controlButtonsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            controlButtonsPanel.AutoSize = true;
            controlButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            controlButtonsPanel.Controls.Add(btnStart);
            controlButtonsPanel.Controls.Add(btnStop);
            controlButtonsPanel.Controls.Add(btnReboot);
            controlButtonsPanel.Controls.Add(btnRefreshMap);
            controlButtonsPanel.FlowDirection = FlowDirection.LeftToRight;
            controlButtonsPanel.Location = new Point(contentPanel.Width - 520, 30);
            controlButtonsPanel.Name = "controlButtonsPanel";
            controlButtonsPanel.Size = new Size(480, 40);
            controlButtonsPanel.TabIndex = 1;
            controlButtonsPanel.BackColor = Color.Transparent;
            controlButtonsPanel.WrapContents = false;

            // Control Buttons — PKM Universe gold theme
            ConfigureControlButton(btnStart, "START ALL", Color.FromArgb(0xD4, 0xAF, 0x37));     // primary gold (most used)
            ConfigureControlButton(btnStop, "STOP ALL", Color.FromArgb(0x8B, 0x2C, 0x2C));        // muted red (semantic danger, on-brand)
            ConfigureControlButton(btnReboot, "REBOOT", Color.FromArgb(0x4A, 0x3D, 0x1E));        // dark gold neutral
            ConfigureControlButton(btnRefreshMap, "REFRESH MAP", Color.FromArgb(0x4A, 0x3D, 0x1E)); // dark gold neutral

            btnStart.Click += B_Start_Click;
            btnStop.Click += B_Stop_Click;
            btnReboot.Click += B_RebootAndStop_Click;
            btnRefreshMap.Click += RefreshMap_Click;

            // Bots Panel
            botsPanel.BackColor = Color.Transparent;
            botsPanel.Controls.Add(FLP_Bots);
            botsPanel.Controls.Add(botHeaderPanel);
            botsPanel.Dock = DockStyle.Fill;
            botsPanel.Location = new Point(0, 100);
            botsPanel.Name = "botsPanel";
            botsPanel.Padding = new Padding(40);
            botsPanel.Size = new Size(1020, 620);
            botsPanel.TabIndex = 0;
            botsPanel.Visible = false; // Dashboard is the default landing page now

            // Bot Header Panel
            botHeaderPanel.BackColor = Color.FromArgb(35, 35, 35);
            botHeaderPanel.Controls.Add(addBotPanel);
            botHeaderPanel.Dock = DockStyle.Top;
            botHeaderPanel.Height = 100;
            botHeaderPanel.Location = new Point(40, 40);
            botHeaderPanel.Name = "botHeaderPanel";
            botHeaderPanel.Size = new Size(940, 100);
            botHeaderPanel.TabIndex = 1;
            CreateRoundedPanel(botHeaderPanel);

            // Add Bot Panel
            addBotPanel.SuspendLayout();
            addBotPanel.Controls.Add(B_New);
            addBotPanel.Controls.Add(CB_Routine);
            addBotPanel.Controls.Add(CB_Protocol);
            addBotPanel.Controls.Add(NUD_Port);
            addBotPanel.Controls.Add(TB_IP);
            addBotPanel.Dock = DockStyle.Fill;
            addBotPanel.Location = new Point(0, 0);
            addBotPanel.Name = "addBotPanel";
            addBotPanel.Size = new Size(940, 100);
            addBotPanel.TabIndex = 0;
            addBotPanel.BackColor = Color.Transparent;
            EnableDoubleBuffering(addBotPanel);
            addBotPanel.ResumeLayout(false);

            // Bot Configuration Controls
            TB_IP.BackColor = Color.FromArgb(28, 28, 28);
            TB_IP.BorderStyle = BorderStyle.FixedSingle;
            TB_IP.Font = new Font("Segoe UI", 11F);
            TB_IP.ForeColor = Color.FromArgb(224, 224, 224);
            TB_IP.Location = new Point(30, 35);
            TB_IP.Name = "TB_IP";
            TB_IP.PlaceholderText = "IP Address";
            TB_IP.Size = new Size(150, 25);
            TB_IP.TabIndex = 0;
            TB_IP.Text = "192.168.0.1";

            ConfigureNumericUpDown(NUD_Port, 190, 35, 80);
            NUD_Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NUD_Port.Value = new decimal(new int[] { 6000, 0, 0, 0 });

            CB_Protocol.SuspendLayout();
            ConfigureComboBox(CB_Protocol, 280, 35, 120);
            CB_Protocol.SelectedIndexChanged += CB_Protocol_SelectedIndexChanged;
            CB_Protocol.ResumeLayout();

            CB_Routine.SuspendLayout();
            ConfigureComboBox(CB_Routine, 410, 35, 200);
            CB_Routine.ResumeLayout();

            // Add Bot Button with glow effect
            B_New.BackColor = Color.FromArgb(87, 242, 135);
            B_New.FlatAppearance.BorderSize = 0;
            B_New.FlatStyle = FlatStyle.Flat;
            B_New.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            B_New.ForeColor = Color.FromArgb(28, 28, 28);
            B_New.Location = new Point(620, 30);
            B_New.Name = "B_New";
            B_New.Size = new Size(120, 40);
            B_New.TabIndex = 4;
            B_New.Text = "ADD BOT";
            B_New.UseVisualStyleBackColor = false;
            B_New.Click += B_New_Click;
            B_New.Cursor = Cursors.Hand;
            ConfigureGlowButton(B_New);

            // Bot List Panel
            FLP_Bots.AutoScroll = true;
            FLP_Bots.BackColor = Color.Transparent;
            FLP_Bots.Dock = DockStyle.Fill;
            FLP_Bots.FlowDirection = FlowDirection.TopDown;
            FLP_Bots.Location = new Point(40, 140);
            FLP_Bots.Margin = new Padding(0, 20, 0, 0);
            FLP_Bots.Name = "FLP_Bots";
            FLP_Bots.Padding = new Padding(0);
            FLP_Bots.Size = new Size(940, 440);
            FLP_Bots.TabIndex = 0;
            FLP_Bots.WrapContents = false;
            FLP_Bots.Resize += FLP_Bots_Resize;

            // Hub Panel (Configuration page) — Discord-style inner sidebar
            hubPanel.BackColor = Color.Transparent;
            hubPanel.Dock = DockStyle.Fill;
            hubPanel.Location = new Point(0, 100);
            hubPanel.Name = "hubPanel";
            hubPanel.Padding = new Padding(28);
            hubPanel.Size = new Size(1020, 620);
            hubPanel.TabIndex = 1;
            hubPanel.Visible = false;

            // Inner sidebar (left) — section nav within the Configuration page
            configInnerSidebar = new Panel
            {
                BackColor = Color.FromArgb(15, 15, 15),
                Dock = DockStyle.Left,
                Width = 200,
                Padding = new Padding(0, 16, 0, 16),
                Name = "configInnerSidebar",
            };
            CreateRoundedPanel(configInnerSidebar);

            var sidebarHeader = new Label
            {
                Text = "SETTINGS",
                ForeColor = Color.FromArgb(0xD4, 0xAF, 0x37),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(20, 12, 0, 0),
                BackColor = Color.Transparent,
            };

            var navFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 4, 0, 0),
            };
            configInnerSidebar.Controls.Add(navFlow);
            configInnerSidebar.Controls.Add(sidebarHeader);

            configSectionContent = new Panel
            {
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 16, 16, 16),
                Name = "configSectionContent",
                AutoScroll = false, // Each inner section handles its own scrolling — no nested scroll containers
            };

            // Build each section, all with Visible=false to start; one will be shown by first nav click
            sectionPanels = new Dictionary<string, Panel>
            {
                { "General",  BuildConfigSection_General()  },
                { "Discord",  BuildConfigSection_Discord()  },
                { "Raids",    BuildConfigSection_Raids()    },
                { "Embed",    BuildConfigSection_Embed()    },
                { "Lobby",    BuildConfigSection_Lobby()    },
                { "Advanced", BuildConfigSection_Advanced() },
            };

            foreach (var kv in sectionPanels)
            {
                kv.Value.Dock = DockStyle.Fill;
                kv.Value.Visible = false;
                configSectionContent.Controls.Add(kv.Value);
            }

            // Build nav buttons (added in reverse so flow order matches map order)
            foreach (var key in new[] { "General", "Discord", "Raids", "Embed", "Lobby", "Advanced" })
            {
                var btn = MakeConfigNavButton(key);
                navFlow.Controls.Add(btn);
            }

            hubPanel.Controls.Add(configSectionContent);
            hubPanel.Controls.Add(configInnerSidebar);

            // Show General by default
            SwitchConfigSection("General");

            // PG_Hub kept as a field for compatibility with code that calls PG_Hub.Refresh(),
            // but it's NEVER added to the form — the Advanced section now uses ConfigFormBuilder.
            // We give it a parent-less existence so .Refresh() calls remain no-ops without throwing.
            PG_Hub.Name = "PG_Hub_unused";

            // Logs Panel
            logsPanel.BackColor = Color.Transparent;
            logsPanel.Dock = DockStyle.Fill;
            logsPanel.Location = new Point(0, 100);
            logsPanel.Name = "logsPanel";
            logsPanel.Padding = new Padding(40);
            logsPanel.Size = new Size(1020, 620);
            logsPanel.TabIndex = 2;
            logsPanel.Visible = false;

            // Logs Container
            var logsContainer = new Panel();
            logsContainer.BackColor = Color.FromArgb(35, 35, 35);
            logsContainer.Dock = DockStyle.Fill;
            logsContainer.Location = new Point(40, 110);
            logsContainer.Margin = new Padding(0, 20, 0, 0);
            logsContainer.Name = "logsContainer";
            logsContainer.Padding = new Padding(2);
            logsContainer.Size = new Size(940, 470);
            CreateRoundedPanel(logsContainer);
            logsPanel.Controls.Add(logsContainer);
            logsPanel.Controls.Add(logsHeaderPanel);

            // Logs Header Panel
            logsHeaderPanel.BackColor = Color.FromArgb(35, 35, 35);
            logsHeaderPanel.Controls.Add(btnClearLogs);
            logsHeaderPanel.Controls.Add(searchStatusLabel);
            logsHeaderPanel.Controls.Add(logSearchBox);
            logsHeaderPanel.Dock = DockStyle.Top;
            logsHeaderPanel.Height = 60;
            logsHeaderPanel.Location = new Point(40, 40);
            logsHeaderPanel.Name = "logsHeaderPanel";
            logsHeaderPanel.Padding = new Padding(20, 15, 20, 15);
            logsHeaderPanel.Size = new Size(940, 60);
            logsHeaderPanel.TabIndex = 1;
            CreateRoundedPanel(logsHeaderPanel);

            // Log Search Box
            logSearchBox.BackColor = Color.FromArgb(28, 28, 28);
            logSearchBox.BorderStyle = BorderStyle.FixedSingle;
            logSearchBox.Dock = DockStyle.Fill;
            logSearchBox.Font = new Font("Segoe UI", 10F);
            logSearchBox.ForeColor = Color.FromArgb(224, 224, 224);
            logSearchBox.Location = new Point(20, 15);
            logSearchBox.Name = "logSearchBox";
            logSearchBox.PlaceholderText = "Search logs (Enter = next, Shift+Enter = previous)...";
            logSearchBox.Size = new Size(700, 30);
            logSearchBox.TabIndex = 0;
            logSearchBox.TextChanged += LogSearchBox_TextChanged;
            logSearchBox.KeyDown += LogSearchBox_KeyDown;

            // Search Status Label
            searchStatusLabel.AutoSize = false;
            searchStatusLabel.BackColor = Color.Transparent;
            searchStatusLabel.Dock = DockStyle.Right;
            searchStatusLabel.Font = new Font("Segoe UI", 9F);
            searchStatusLabel.ForeColor = Color.FromArgb(176, 176, 176);
            searchStatusLabel.Location = new Point(670, 15);
            searchStatusLabel.Name = "searchStatusLabel";
            searchStatusLabel.Size = new Size(130, 30);
            searchStatusLabel.TabIndex = 2;
            searchStatusLabel.Text = "";
            searchStatusLabel.TextAlign = ContentAlignment.MiddleRight;

            // Clear Logs Button
            btnClearLogs.BackColor = Color.FromArgb(237, 66, 69);
            btnClearLogs.Dock = DockStyle.Right;
            btnClearLogs.FlatAppearance.BorderSize = 0;
            btnClearLogs.FlatStyle = FlatStyle.Flat;
            btnClearLogs.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnClearLogs.ForeColor = Color.White;
            btnClearLogs.Location = new Point(800, 15);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new Size(120, 30);
            btnClearLogs.TabIndex = 1;
            btnClearLogs.Text = "CLEAR LOGS";
            btnClearLogs.UseVisualStyleBackColor = false;
            btnClearLogs.Cursor = Cursors.Hand;
            btnClearLogs.Click += (s, e) => {
                RTB_Logs.Clear();
                logSearchBox.Clear();
                searchStatusLabel.Text = "";
                currentSearchIndex = -1;
                lastSearchText = "";
            };
            ConfigureGlowButton(btnClearLogs);

            // Rich Text Box
            RTB_Logs.BackColor = Color.FromArgb(28, 28, 28);
            RTB_Logs.BorderStyle = BorderStyle.None;
            RTB_Logs.Dock = DockStyle.Fill;
            RTB_Logs.Font = new Font("Consolas", 10F);
            RTB_Logs.ForeColor = Color.FromArgb(224, 224, 224);
            RTB_Logs.Location = new Point(2, 2);
            RTB_Logs.Name = "RTB_Logs";
            RTB_Logs.ReadOnly = true;
            RTB_Logs.Size = new Size(936, 466);
            RTB_Logs.TabIndex = 0;
            RTB_Logs.Text = "";
            RTB_Logs.TextChanged += RTB_Logs_TextChanged;
            RTB_Logs.KeyDown += RTB_Logs_KeyDown;
            logsContainer.Controls.Add(RTB_Logs);

            Controls.Add(mainLayoutPanel);
            // Particle panel removed to prevent flickering
            // Controls.Add(particlePanel);

            // Configure Tray Icon
            ConfigureTrayIcon();

            mainLayoutPanel.ResumeLayout(false);
            sidebarPanel.ResumeLayout(false);
            navButtonsPanel.ResumeLayout(false);
            sidebarBottomPanel.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            controlButtonsPanel.ResumeLayout(false);
            contentPanel.ResumeLayout(false);
            botsPanel.ResumeLayout(false);
            botHeaderPanel.ResumeLayout(false);
            addBotPanel.ResumeLayout(false);
            addBotPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NUD_Port).EndInit();
            hubPanel.ResumeLayout(false);
            logsPanel.ResumeLayout(false);
            logsHeaderPanel.ResumeLayout(false);
            logsHeaderPanel.PerformLayout();
            ResumeLayout(false);

            // Start animation timer
            animationTimer.Start();

            // Enable double buffering on all panels
            EnableDoubleBuffering(mainLayoutPanel);
            EnableDoubleBuffering(sidebarPanel);
            EnableDoubleBuffering(contentPanel);
            EnableDoubleBuffering(headerPanel);
            EnableDoubleBuffering(particlePanel);
            EnableDoubleBuffering(botsPanel);
            EnableDoubleBuffering(hubPanel);
            EnableDoubleBuffering(logsPanel);
            EnableDoubleBuffering(FLP_Bots);
        }

        private void EnableDoubleBuffering(Control control)
        {
            if (control == null) return;

            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        private void ConfigureExitButton()
        {
            btnNavExit.BackColor = Color.FromArgb(18, 18, 18);
            btnNavExit.Cursor = Cursors.Hand;
            btnNavExit.FlatAppearance.BorderSize = 0;
            btnNavExit.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 30, 30);
            btnNavExit.FlatStyle = FlatStyle.Flat;
            btnNavExit.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            btnNavExit.ForeColor = Color.FromArgb(237, 66, 69); // Red color for exit
            btnNavExit.Location = new Point(0, 220);
            btnNavExit.Margin = new Padding(0, 40, 0, 0); // Extra margin to separate from other buttons
            btnNavExit.Name = "btnNavExit";
            btnNavExit.Padding = new Padding(60, 0, 0, 0);
            btnNavExit.Size = new Size(260, 50);
            btnNavExit.TabIndex = 3;
            btnNavExit.Text = "EXIT";
            btnNavExit.TextAlign = ContentAlignment.MiddleLeft;
            btnNavExit.UseVisualStyleBackColor = false;
            btnNavExit.Tag = new ButtonAnimationState();

            btnNavExit.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                var animState = btnNavExit.Tag as ButtonAnimationState;

                // Draw hover background
                if (animState != null && animState.HoverProgress > 0)
                {
                    using var brush = new SolidBrush(Color.FromArgb((int)(30 * animState.HoverProgress), 237, 66, 69));
                    e.Graphics.FillRectangle(brush, btnNavExit.ClientRectangle);
                }

                // Draw icon
                var iconRect = new Rectangle(20, (btnNavExit.Height - 24) / 2, 24, 24);

                // Draw exit icon
                using var iconFont = new Font("Segoe MDL2 Assets", 16F);
                var iconText = "\uE8BB"; // Power/Exit icon

                using var iconBrush = new SolidBrush(btnNavExit.ForeColor);
                var textSize = e.Graphics.MeasureString(iconText, iconFont);
                var textX = iconRect.X + (iconRect.Width - textSize.Width) / 2;
                var textY = iconRect.Y + (iconRect.Height - textSize.Height) / 2;

                e.Graphics.DrawString(iconText, iconFont, iconBrush, textX, textY);
            };

            btnNavExit.Click += (s, e) => ExitApplication();
            ConfigureHoverAnimation(btnNavExit);
        }

        private void ConfigureTrayIcon()
        {
            // Configure tray icon
            trayIcon.Icon = Icon;
            trayIcon.Text = (Config?.Hub?.BotName != null && !string.IsNullOrEmpty(Config.Hub.BotName)) ? Config.Hub.BotName : "Raid-SV";
            trayIcon.Visible = false;
            trayIcon.DoubleClick += (s, e) => ShowFromTray();

            // Configure tray context menu
            trayContextMenu.BackColor = Color.FromArgb(35, 35, 35);
            trayContextMenu.Font = new Font("Segoe UI", 10F);
            trayContextMenu.Renderer = new MainMenuRenderer();

            // Add menu items
            var showMenuItem = new ToolStripMenuItem("Show Window")
            {
                ForeColor = Color.FromArgb(224, 224, 224)
            };
            showMenuItem.Click += (s, e) => ShowFromTray();

            var startMenuItem = new ToolStripMenuItem("Start All Bots")
            {
                ForeColor = Color.FromArgb(87, 242, 135)
            };
            startMenuItem.Click += (s, e) => B_Start_Click(s, e);

            var stopMenuItem = new ToolStripMenuItem("Stop All Bots")
            {
                ForeColor = Color.FromArgb(237, 66, 69)
            };
            stopMenuItem.Click += (s, e) => B_Stop_Click(s, e);

            var exitMenuItem = new ToolStripMenuItem("Exit")
            {
                ForeColor = Color.FromArgb(237, 66, 69)
            };
            exitMenuItem.Click += (s, e) => ExitApplication();

            trayContextMenu.Items.AddRange(new ToolStripItem[] {
                showMenuItem,
                new ToolStripSeparator(),
                startMenuItem,
                stopMenuItem,
                new ToolStripSeparator(),
                exitMenuItem
            });

            trayIcon.ContextMenuStrip = trayContextMenu;
        }

        // Custom menu renderer for dark theme
        private class MainMenuRenderer : ToolStripProfessionalRenderer
        {
            public MainMenuRenderer() : base(new MainColorTable()) { }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var rc = new Rectangle(Point.Empty, e.Item.Size);
                var c = e.Item.Selected ? Color.FromArgb(50, 50, 50) : Color.FromArgb(35, 35, 35);
                using (var brush = new SolidBrush(c))
                    e.Graphics.FillRectangle(brush, rc);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                if (!e.Item.Enabled)
                    e.TextColor = Color.FromArgb(100, 100, 100);
                base.OnRenderItemText(e);
            }
        }

        private class MainColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(50, 50, 50);
            public override Color MenuItemBorder => Color.FromArgb(88, 101, 242);
            public override Color MenuBorder => Color.FromArgb(50, 50, 50);
            public override Color ToolStripDropDownBackground => Color.FromArgb(35, 35, 35);
            public override Color ImageMarginGradientBegin => Color.FromArgb(35, 35, 35);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(35, 35, 35);
            public override Color ImageMarginGradientEnd => Color.FromArgb(35, 35, 35);
            public override Color SeparatorDark => Color.FromArgb(50, 50, 50);
            public override Color SeparatorLight => Color.FromArgb(60, 60, 60);
        }

        private void ConfigureNavButton(Button btn, string text, int index, string tooltip)
        {
            btn.BackColor = Color.FromArgb(18, 18, 18);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 30, 30);
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            btn.ForeColor = Color.FromArgb(176, 176, 176); // #B0B0B0
            btn.Location = new Point(0, 40 + (index * 60));
            btn.Margin = new Padding(0, 0, 0, 10);
            btn.Name = $"btnNav{text.Replace(" ", "")}";
            btn.Padding = new Padding(60, 0, 0, 0);
            btn.Size = new Size(260, 50);
            btn.TabIndex = index;
            btn.Text = text;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.UseVisualStyleBackColor = false;
            btn.Tag = new ButtonAnimationState();

            // Add icon space
            btn.Paint += (s, e) => {
                var animState = btn.Tag as ButtonAnimationState;

                // Draw selection indicator
                if (btn.BackColor == Color.FromArgb(30, 30, 30))
                {
                    using var brush = new LinearGradientBrush(
                        new RectangleF(0, 0, 4, btn.Height),
                        Color.FromArgb(87, 242, 135),
                        Color.FromArgb(88, 101, 242),
                        LinearGradientMode.Vertical);
                    e.Graphics.FillRectangle(brush, 0, 0, 4, btn.Height);
                }

                // Draw icon without glow
                var iconRect = new Rectangle(20, (btn.Height - 24) / 2, 24, 24);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                // Draw font icon
                using var iconFont = new Font("Segoe MDL2 Assets", 16F);
                string iconText = index switch
                {
                    0 => "\uE77B", // Robot icon for Bots
                    1 => "\uE713", // Settings icon for Configuration
                    2 => "\uE7C3", // Document icon for Logs
                    3 => "\uE9D9", // Gauge / dashboard icon
                    _ => "\uE700"  // Default icon
                };

                // Adjust color based on selection state
                using var iconBrush = new SolidBrush(
                    btn.BackColor == Color.FromArgb(30, 30, 30)
                        ? Color.FromArgb(224, 224, 224)  // Selected - bright
                        : Color.FromArgb(150, 150, 150)  // Unselected - dimmed
                );

                // Center the icon in the rectangle
                var textSize = e.Graphics.MeasureString(iconText, iconFont);
                var textX = iconRect.X + (iconRect.Width - textSize.Width) / 2;
                var textY = iconRect.Y + (iconRect.Height - textSize.Height) / 2;

                e.Graphics.DrawString(iconText, iconFont, iconBrush, textX, textY);
            };

            btn.Click += (s, e) => {
                // Reset all nav buttons
                foreach (Button navBtn in navButtonsPanel.Controls)
                {
                    navBtn.BackColor = Color.FromArgb(18, 18, 18);
                    navBtn.ForeColor = Color.FromArgb(176, 176, 176);
                }

                // Highlight selected with animation
                btn.BackColor = Color.FromArgb(30, 30, 30);
                btn.ForeColor = Color.FromArgb(224, 224, 224);

                // Show appropriate panel with fade transition
                TransitionPanels(index);

                // Update title with fade effect
                titleLabel.Text = index switch
                {
                    0 => "Bot Management",
                    1 => "Configuration",
                    2 => "System Logs",
                    3 => "Dashboard",
                    _ => "Raid-SV"
                };
            };

            // Configure hover animation
            ConfigureHoverAnimation(btn);

            // Set initial selection — DASHBOARD is the default landing view
            if (index == 3)
            {
                btn.BackColor = Color.FromArgb(30, 30, 30);
                btn.ForeColor = Color.FromArgb(224, 224, 224);
            }
        }

        private void ConfigureControlButton(Button btn, string text, Color baseColor)
        {
            btn.BackColor = baseColor;
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            // Pick text color by perceived luminance: light text on dark bg, dark text on bright bg.
            double luminance = (0.299 * baseColor.R + 0.587 * baseColor.G + 0.114 * baseColor.B) / 255.0;
            btn.ForeColor = luminance > 0.55 ? Color.FromArgb(28, 28, 28) : Color.FromArgb(0xE8, 0xC2, 0x52); // dark text or gold-light
            btn.Margin = new Padding(5, 0, 5, 0);
            btn.Name = $"btn{text.Replace(" ", "")}";
            btn.Padding = new Padding(5, 0, 5, 0);
            btn.Size = new Size(115, 40);
            btn.TabIndex = 0;
            btn.Text = text;
            btn.UseVisualStyleBackColor = false;
            btn.Tag = new ButtonAnimationState { BaseColor = baseColor };

            // Create rounded corners
            CreateRoundedButton(btn);

            // Configure glow effect
            ConfigureGlowButton(btn);
        }

        // -------- Configuration page section nav helpers --------

        private Button MakeConfigNavButton(string section)
        {
            var btn = new Button
            {
                Text = section.ToUpperInvariant(),
                Tag = section,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0xB0, 0xB0, 0xB0),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0),
                Width = 196,
                Height = 42,
                Margin = new Padding(0),
                Cursor = Cursors.Hand,
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 28, 28);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(35, 35, 35);
            btn.Click += (s, e) => SwitchConfigSection(section);
            configNavButtons[section] = btn;
            return btn;
        }

        private bool advancedFormBuilt = false;
        private void SwitchConfigSection(string section)
        {
            currentConfigSection = section;
            foreach (var kv in sectionPanels)
                kv.Value.Visible = (kv.Key == section);
            foreach (var kv in configNavButtons)
            {
                bool active = (kv.Key == section);
                kv.Value.ForeColor = active ? Color.FromArgb(0xD4, 0xAF, 0x37) : Color.FromArgb(0xB0, 0xB0, 0xB0);
                kv.Value.BackColor = active ? Color.FromArgb(28, 28, 28) : Color.Transparent;
                kv.Value.Font = new Font("Segoe UI", 10F, active ? FontStyle.Bold : FontStyle.Regular);
            }

            // LAZY: build the Advanced reflection form only on first visit. Subsequent visits reuse it.
            if (section == "Advanced" && !advancedFormBuilt && advancedFormHost != null && RunningEnvironment?.Config != null)
            {
                advancedFormBuilt = true;
                SysBot.Pokemon.WinForms.Controls.ConfigFormBuilder.BuildFormFor(RunningEnvironment.Config, advancedFormHost);
            }
        }

        // -------- Section content builders --------
        // Each returns a Panel that lives inside configSectionContent (Dock=Fill, Visible toggled by nav).
        // We use the section-header + field-row pattern; styling is centralized in MakeFieldRow.

        private Panel BuildConfigSection_General()
        {
            var p = NewSectionPanel("General", "Basic identity and behavior.");
            qsBotName = new TextBox();
            qsGameStatus = new TextBox();
            p.Controls.Add(MakeFieldRow(
                "Bot Display Name",
                "Shown in the window title bar and in the status-patrol JSON. Helps you tell instances apart.",
                qsBotName));
            p.Controls.Add(MakeFieldRow(
                "Bot Game Status",
                "The 'Playing …' line shown under your bot's Discord name. Pure flavor.",
                qsGameStatus));
            return p;
        }

        private Panel BuildConfigSection_Discord()
        {
            var p = NewSectionPanel("Discord", "Connecting the bot to your Discord server.");
            qsDiscordToken = new TextBox { UseSystemPasswordChar = true };
            qsCommandPrefix = new TextBox();
            p.Controls.Add(MakeFieldRow(
                "Bot Token",
                "From the Discord Developer Portal. Don't share. Hidden by default.",
                qsDiscordToken));
            p.Controls.Add(MakeFieldRow(
                "Command Prefix",
                "What you type before commands. Default: ! (so $ trade-bot commands don't clash).",
                qsCommandPrefix));
            return p;
        }

        private Panel BuildConfigSection_Raids()
        {
            var p = NewSectionPanel("Raids", "How and when raids get hosted.");
            qsTotalRaids = new NumericUpDown { Minimum = 0, Maximum = 100000, Value = 0 };
            qsRotateRandom = new CheckBox { Text = "Shuffle the raid order each cycle", AutoSize = true };
            qsKeepDaySeed = new CheckBox { Text = "Keep the in-game day seed locked (prevents raid reroll)", AutoSize = true };
            p.Controls.Add(MakeFieldRow(
                "Total Raids Before Stopping",
                "0 = run forever (recommended for 24/7 hosting). Any positive number caps the session.",
                qsTotalRaids));
            p.Controls.Add(MakeFieldRow(
                "Rotate Raid List in Random Order",
                "On = pick a random raid from the list each time. Off = go in order top to bottom.",
                qsRotateRandom));
            p.Controls.Add(MakeFieldRow(
                "Keep Current Day Seed",
                "On = freeze the game's day so raid seeds stay deterministic. Recommended ON.",
                qsKeepDaySeed));
            return p;
        }

        private Panel BuildConfigSection_Embed()
        {
            var p = NewSectionPanel("Embed", "How your raid posts look in Discord.");
            qsHideRaidCode = new CheckBox { Text = "Hide code from public embed (DM via reaction instead)", AutoSize = true };
            qsUseScreenshots = new CheckBox { Text = "Attach an in-game screenshot to each raid post", AutoSize = true };
            p.Controls.Add(MakeFieldRow(
                "Raid Code Privacy",
                "ON: users react ✅ to the embed and get the code in DM. OFF: code appears in the post for anyone.",
                qsHideRaidCode));
            p.Controls.Add(MakeFieldRow(
                "Use Screenshots",
                "Adds visual confirmation of the in-game raid to each Discord post. Slight performance cost.",
                qsUseScreenshots));
            return p;
        }

        private Panel BuildConfigSection_Lobby()
        {
            var p = NewSectionPanel("Lobby", "How the bot acts inside the in-game raid lobby.");
            var note = new Label
            {
                Text = "Lobby behavior knobs (button-mash speed, retry limits, A-button mode) live in the Advanced section for now.",
                ForeColor = Color.FromArgb(0x8A, 0x8A, 0x8A),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 12, 0, 0),
                BackColor = Color.Transparent,
            };
            p.Controls.Add(note);
            return p;
        }

        private Panel advancedFormHost = null!;
        private Panel BuildConfigSection_Advanced()
        {
            // For the Advanced section we DON'T want the outer section to scroll —
            // ConfigFormBuilder's own contentArea handles scrolling internally.
            // Nested AutoScroll panels caused the top category to clip.
            var p = NewSectionPanel("Advanced", "Every knob the bot exposes. Most people never come in here.", autoScroll: false);
            advancedFormHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                AutoScroll = false,
                Padding = new Padding(0, 6, 0, 6),
                Name = "advancedFormHost",
            };
            p.Controls.Add(advancedFormHost);
            return p;
        }

        private Panel NewSectionPanel(string title, string subtitle, bool autoScroll = true)
        {
            var p = new Panel { BackColor = Color.Transparent, AutoScroll = autoScroll };
            var header = new Label
            {
                Text = title.ToUpperInvariant(),
                ForeColor = Color.FromArgb(0xD4, 0xAF, 0x37),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.Transparent,
            };
            var sub = new Label
            {
                Text = subtitle,
                ForeColor = Color.FromArgb(0x8A, 0x8A, 0x8A),
                Font = new Font("Segoe UI", 10F),
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.Transparent,
            };
            var divider = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(40, 40, 40),
                Margin = new Padding(0, 4, 0, 14),
            };
            // Reverse insert so docked Top order matches code order
            p.Controls.Add(divider);
            p.Controls.Add(sub);
            p.Controls.Add(header);
            return p;
        }

        private Panel MakeFieldRow(string label, string helpText, Control input)
        {
            var row = new Panel
            {
                BackColor = Color.Transparent,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 14, 0, 14),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
            };

            var labelLbl = new Label
            {
                Text = label,
                ForeColor = Color.FromArgb(0xE0, 0xE0, 0xE0),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 24,
                BackColor = Color.Transparent,
            };
            var helpLbl = new Label
            {
                Text = helpText,
                ForeColor = Color.FromArgb(0x8A, 0x8A, 0x8A),
                Font = new Font("Segoe UI", 9F),
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.Transparent,
            };

            // Style the input control
            Control inputToAdd = input;
            if (input is TextBox tb)
            {
                tb.BackColor = Color.FromArgb(18, 18, 18);
                tb.ForeColor = Color.FromArgb(0xE8, 0xE8, 0xE8);
                tb.BorderStyle = BorderStyle.FixedSingle;
                tb.Font = new Font("Segoe UI", 11F);
                tb.Height = 32;

                // Masked field (token/key): wrap with a Show/Hide reveal toggle.
                if (tb.UseSystemPasswordChar)
                {
                    tb.Dock = DockStyle.Fill;
                    var holder = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.Transparent };
                    var reveal = new Button
                    {
                        Text = "Show",
                        Dock = DockStyle.Right,
                        Width = 70,
                        FlatStyle = FlatStyle.Flat,
                        ForeColor = Color.FromArgb(0xD4, 0xAF, 0x37),
                        BackColor = Color.Transparent,
                        Cursor = Cursors.Hand,
                        Font = new Font("Segoe UI", 9F),
                    };
                    reveal.FlatAppearance.BorderColor = Color.FromArgb(0x6A, 0x55, 0x22);
                    reveal.FlatAppearance.BorderSize = 1;
                    reveal.FlatAppearance.MouseOverBackColor = Color.FromArgb(0x1F, 0x1F, 0x1F);
                    reveal.Click += (s, e) =>
                    {
                        tb.UseSystemPasswordChar = !tb.UseSystemPasswordChar;
                        reveal.Text = tb.UseSystemPasswordChar ? "Show" : "Hide";
                    };
                    holder.Controls.Add(tb);     // Fill — takes remaining width
                    holder.Controls.Add(reveal); // Right — 70px button
                    inputToAdd = holder;
                }
                else
                {
                    tb.Dock = DockStyle.Top;
                }
            }
            else if (input is NumericUpDown nud)
            {
                nud.BackColor = Color.FromArgb(18, 18, 18);
                nud.ForeColor = Color.FromArgb(0xE8, 0xE8, 0xE8);
                nud.BorderStyle = BorderStyle.FixedSingle;
                nud.Font = new Font("Segoe UI", 11F);
                nud.Dock = DockStyle.Top;
                nud.Height = 32;
                nud.Width = 200;
            }
            else if (input is CheckBox cb)
            {
                cb.ForeColor = Color.FromArgb(0xE0, 0xE0, 0xE0);
                cb.Font = new Font("Segoe UI", 10F);
                cb.BackColor = Color.Transparent;
                cb.FlatStyle = FlatStyle.Flat;
                cb.Dock = DockStyle.Top;
                cb.Height = 26;
            }

            // Reverse-insert so docked Top order is label / help / input
            row.Controls.Add(inputToAdd);
            row.Controls.Add(helpLbl);
            row.Controls.Add(labelLbl);
            return row;
        }

        // ===========================================================
        //   PHASE 2: DASHBOARD VIEW
        // ===========================================================

        private void BuildDashboardPanel()
        {
            dashboardPanel = new Panel
            {
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Location = new Point(0, 100),
                Size = new Size(1020, 620),
                Padding = new Padding(28),
                Name = "dashboardPanel",
                Visible = true, // Default landing view
            };

            // Right rail: Live Feed (320 wide, full height)
            var rail = new Panel
            {
                BackColor = Color.FromArgb(20, 20, 20),
                Dock = DockStyle.Right,
                Width = 320,
                Padding = new Padding(18, 14, 18, 14),
            };
            var railHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.Transparent,
            };
            railHeader.Controls.Add(new Label
            {
                Text = "LIVE FEED",
                ForeColor = Color.FromArgb(0xD4, 0xAF, 0x37),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.Transparent,
            });
            railHeader.Controls.Add(new Label
            {
                Text = "● LIVE",
                ForeColor = Color.FromArgb(0x4A, 0xDE, 0x80),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Right,
                Width = 60,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent,
            });
            dashLiveFeed = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0),
            };
            rail.Controls.Add(dashLiveFeed);
            rail.Controls.Add(railHeader);

            // Left main: hero card on top + 3 cards below
            var mainArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 18, 0),
                AutoScroll = true,
            };

            // 3 secondary cards row (bottom of main area)
            var bottomCards = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 200,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
            };
            bottomCards.ColumnStyles.Clear();
            bottomCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            bottomCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            bottomCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            bottomCards.Controls.Add(BuildDashboardCard("QUEUE", out dashQueueList,
                "No raids queued yet.\nAdd one via BOTS → ADD BOT,\nor use !ra <seed> in Discord."), 0, 0);
            bottomCards.Controls.Add(BuildStatsCard(), 1, 0);
            bottomCards.Controls.Add(BuildConnectionCard(), 2, 0);

            // Hero card (top of main area)
            var heroCard = BuildDashboardHeroCard();

            mainArea.Controls.Add(bottomCards);
            mainArea.Controls.Add(heroCard);

            dashboardPanel.Controls.Add(mainArea);
            dashboardPanel.Controls.Add(rail);

            // Demo button — bottom-right of dashboard. Fakes a 60s raid so you can see the UI come alive
            // without needing an actual Switch + raid running.
            var demoBtn = new Button
            {
                Text = "▶  Run 60s Demo",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(0xD4, 0xAF, 0x37),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Width = 160,
                Height = 30,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(dashboardPanel.Width - 340 - 180, dashboardPanel.Height - 50),
            };
            demoBtn.FlatAppearance.BorderColor = Color.FromArgb(0x6A, 0x55, 0x22);
            demoBtn.FlatAppearance.BorderSize = 1;
            demoBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0x1F, 0x1F, 0x1F);
            demoBtn.Click += (s, e) => _ = RunDashboardDemoAsync();
            dashboardPanel.Controls.Add(demoBtn);
            demoBtn.BringToFront();

            // Seed initial live feed entries
            PopulateInitialLiveFeed();

            // Subscribe to live raid events — bot emits via DashboardState.Emit(), we pipe to the rail.
            SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.LiveEventOccurred += (icon, body, when) =>
            {
                AddLiveFeedEntry(icon, body, when);
            };

            // Poll DashboardState every 1 second so the hero card / stats / timer stay current.
            var pollTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            pollTimer.Tick += (s, e) =>
            {
                if (dashboardPanel != null && dashboardPanel.Visible)
                    RefreshDashboard();
            };
            pollTimer.Start();
        }

        private TableLayoutPanel? dashHeroBody;
        private Panel? dashEmptyState;
        private float dashPulsePhase = 0f;
        private System.Windows.Forms.Timer? dashAnimTimer;
        private Panel? dashTimerRing;
        private Panel? dashJoinedDotsPanel;
        private Panel? dashStatsSparkline;
        private float dashTimerProgress = 0f;   // 0..1 — how much of the timer ring is filled (1 = full time left)
        private int dashTotalJoined = 0;
        private int dashTotalCapacity = 4;
        private float[] dashSparklineData = new float[] { 0.2f, 0.3f, 0.25f, 0.45f, 0.4f, 0.55f, 0.5f, 0.7f, 0.65f, 0.8f, 0.75f, 0.9f };

        private Panel BuildDashboardHeroCard()
        {
            var hero = new Panel
            {
                Dock = DockStyle.Top,
                Height = 320,
                BackColor = Color.FromArgb(0x10, 0x10, 0x10),
                Padding = new Padding(28, 22, 28, 22),
                Margin = new Padding(0, 0, 0, 18),
            };
            // Subtle radial gradient + top gold accent line
            hero.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(-200, -260, hero.Width + 400, 700);
                    using var brush = new PathGradientBrush(path)
                    {
                        CenterColor = Color.FromArgb(40, 0xD4, 0xAF, 0x37),
                        SurroundColors = new[] { Color.FromArgb(0, 0xD4, 0xAF, 0x37) },
                    };
                    e.Graphics.FillRectangle(brush, 0, 0, hero.Width, hero.Height);
                }
                using var pen = new Pen(Color.FromArgb(0xD4, 0xAF, 0x37), 2);
                e.Graphics.DrawLine(pen, 0, 0, hero.Width, 0);
            };

            var heroLabel = new Label
            {
                Text = "CURRENTLY HOSTING",
                ForeColor = Color.FromArgb(0xD4, 0xAF, 0x37),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 22,
                BackColor = Color.Transparent,
            };

            // Empty state — centered animated Pokeball + pulsing "WAITING FOR RAID"
            dashEmptyState = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = true,
            };
            dashEmptyState.Paint += DrawEmptyState_Paint;

            // 3-column body: sprite | pokemon info | code & timer (only visible when raid live)
            dashHeroBody = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Visible = false,
            };
            dashHeroBody.ColumnStyles.Clear();
            dashHeroBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            dashHeroBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            dashHeroBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            var heroBody = dashHeroBody;

            dashHeroSprite = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
            };
            heroBody.Controls.Add(dashHeroSprite, 0, 0);

            var infoCol = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(12, 0, 12, 0) };
            dashHeroPokemonName = new Label
            {
                Text = "Waiting for raid…",
                ForeColor = Color.FromArgb(0xE8, 0xE8, 0xE8),
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.Transparent,
            };
            dashHeroMeta = new Label
            {
                Text = "No bot connected. Add one in BOTS to begin.",
                ForeColor = Color.FromArgb(0x8A, 0x8A, 0x8A),
                Font = new Font("Segoe UI", 10F),
                Dock = DockStyle.Top,
                Height = 28,
                BackColor = Color.Transparent,
            };
            dashHeroMoves = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(0xE0, 0xE0, 0xE0),
                Font = new Font("Segoe UI", 10F),
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.Transparent,
            };
            infoCol.Controls.Add(dashHeroMoves);
            infoCol.Controls.Add(dashHeroMeta);
            infoCol.Controls.Add(dashHeroPokemonName);
            heroBody.Controls.Add(infoCol, 1, 0);

            // Code + Timer column
            var actionCol = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(8, 0, 0, 0) };
            var codeBlock = BuildHeroBlock("RAID CODE", "—", Color.FromArgb(0xD4, 0xAF, 0x37), out dashCodeValue);
            codeBlock.Dock = DockStyle.Top;
            codeBlock.Height = 90;
            codeBlock.Margin = new Padding(0, 0, 0, 12);

            // Timer block — circular gold progress ring around the text
            var timerBlock = BuildTimerRingBlock(out dashTimerValue);
            timerBlock.Dock = DockStyle.Top;
            timerBlock.Height = 110;
            dashTimerRing = timerBlock;

            // Joined dots — actual drawn dots, filled/hollow based on join count
            dashJoinedDotsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.Transparent,
            };
            dashJoinedDotsPanel.Paint += DrawJoinedDots_Paint;

            // Keep a hidden Label so existing wiring code that references dashJoinedDots stays compiling
            dashJoinedDots = new Label { Visible = false };

            actionCol.Controls.Add(dashJoinedDotsPanel);
            actionCol.Controls.Add(timerBlock);
            actionCol.Controls.Add(codeBlock);
            heroBody.Controls.Add(actionCol, 2, 0);

            // Both states live inside the hero card; we toggle Visible on each
            hero.Controls.Add(heroBody);
            hero.Controls.Add(dashEmptyState!);
            hero.Controls.Add(heroLabel);

            // Drive pulse animation (12fps is plenty for this)
            dashAnimTimer = new System.Windows.Forms.Timer { Interval = 80 };
            dashAnimTimer.Tick += (s, e) =>
            {
                dashPulsePhase = (dashPulsePhase + 0.08f) % (float)(Math.PI * 2);
                if (dashEmptyState != null && dashEmptyState.Visible)
                    dashEmptyState.Invalidate();
            };
            dashAnimTimer.Start();

            return hero;
        }

        /// <summary>Timer block with a circular gold progress ring drawn around the value text.</summary>
        private Panel BuildTimerRingBlock(out Label valueLabel)
        {
            var block = new Panel
            {
                BackColor = Color.FromArgb(0x0A, 0x0A, 0x0A),
                Padding = new Padding(0),
            };
            valueLabel = new Label
            {
                Text = "—:—",
                ForeColor = Color.FromArgb(0x4A, 0xDE, 0x80),
                Font = new Font("Cascadia Code", 18F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            var hostedValueLabel = valueLabel;
            block.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Background outline (subtle dark border)
                using var bgPen = new Pen(Color.FromArgb(28, 28, 28), 1);
                e.Graphics.DrawRectangle(bgPen, 0, 0, block.Width - 1, block.Height - 1);

                // Section label
                using var labelFont = new Font("Segoe UI", 8F, FontStyle.Bold);
                using var labelBrush = new SolidBrush(Color.FromArgb(0x8A, 0x8A, 0x8A));
                e.Graphics.DrawString("TIME REMAINING", labelFont, labelBrush, 12, 6);

                // Circular progress ring — centered on the right portion of the block
                int ringSize = Math.Min(block.Height - 16, 90);
                int ringX = block.Width - ringSize - 10;
                int ringY = (block.Height - ringSize) / 2;

                // Track (faint gold)
                using (var trackPen = new Pen(Color.FromArgb(40, 0xD4, 0xAF, 0x37), 4))
                    e.Graphics.DrawArc(trackPen, ringX, ringY, ringSize, ringSize, 0, 360);

                // Progress fill
                float progress = Math.Clamp(dashTimerProgress, 0f, 1f);
                if (progress > 0.001f)
                {
                    Color progressColor = progress > 0.5f
                        ? Color.FromArgb(0x4A, 0xDE, 0x80)
                        : progress > 0.2f
                            ? Color.FromArgb(0xFB, 0xBF, 0x24)
                            : Color.FromArgb(0xF8, 0x71, 0x71);
                    using var progPen = new Pen(progressColor, 4) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                    e.Graphics.DrawArc(progPen, ringX, ringY, ringSize, ringSize, -90, 360 * progress);
                }
            };
            block.Controls.Add(valueLabel);
            return block;
        }

        /// <summary>Paint the joined-dots row: filled gold for joined trainers, hollow for empty slots.</summary>
        private void DrawJoinedDots_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int dotSize = 14;
            int gap = 8;
            int totalW = (dotSize * dashTotalCapacity) + (gap * (dashTotalCapacity - 1));
            int startX = (p.Width - totalW) / 2;
            int y = (p.Height - dotSize) / 2;

            for (int i = 0; i < dashTotalCapacity; i++)
            {
                int x = startX + i * (dotSize + gap);
                if (i < dashTotalJoined)
                {
                    // Filled gold with subtle glow
                    using var glow = new SolidBrush(Color.FromArgb(60, 0xD4, 0xAF, 0x37));
                    e.Graphics.FillEllipse(glow, x - 2, y - 2, dotSize + 4, dotSize + 4);
                    using var fill = new SolidBrush(Color.FromArgb(0xE8, 0xC2, 0x52));
                    e.Graphics.FillEllipse(fill, x, y, dotSize, dotSize);
                }
                else
                {
                    // Hollow gold-faint ring
                    using var ring = new Pen(Color.FromArgb(80, 0xD4, 0xAF, 0x37), 2);
                    e.Graphics.DrawEllipse(ring, x, y, dotSize, dotSize);
                }
            }

            // Label beneath
            using var lblFont = new Font("Segoe UI", 8.5F);
            using var lblBrush = new SolidBrush(Color.FromArgb(0x8A, 0x8A, 0x8A));
            string label = $"{dashTotalJoined} of {dashTotalCapacity} joined";
            var sz = e.Graphics.MeasureString(label, lblFont);
            e.Graphics.DrawString(label, lblFont, lblBrush, (p.Width - sz.Width) / 2, y + dotSize + 2);
        }

        /// <summary>Paint a tiny sparkline trend chart for the stats card.</summary>
        private void DrawStatsSparkline_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            if (dashSparklineData == null || dashSparklineData.Length < 2) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            float maxV = 1f;
            foreach (var v in dashSparklineData) if (v > maxV) maxV = v;
            float w = p.Width;
            float h = p.Height - 2;
            float dx = w / (dashSparklineData.Length - 1);

            var pts = new PointF[dashSparklineData.Length];
            for (int i = 0; i < dashSparklineData.Length; i++)
            {
                float nx = i * dx;
                float ny = h - (dashSparklineData[i] / maxV) * (h - 4) + 2;
                pts[i] = new PointF(nx, ny);
            }

            // Fill under the line (subtle gold)
            using (var path = new GraphicsPath())
            {
                path.AddLines(pts);
                path.AddLine(pts[^1].X, pts[^1].Y, pts[^1].X, h);
                path.AddLine(pts[^1].X, h, pts[0].X, h);
                path.CloseFigure();
                using var areaBrush = new SolidBrush(Color.FromArgb(40, 0xD4, 0xAF, 0x37));
                e.Graphics.FillPath(areaBrush, path);
            }
            // Line itself
            using var linePen = new Pen(Color.FromArgb(0xE8, 0xC2, 0x52), 1.5f);
            e.Graphics.DrawLines(linePen, pts);
            // End-dot
            using var dotBrush = new SolidBrush(Color.FromArgb(0xE8, 0xC2, 0x52));
            e.Graphics.FillEllipse(dotBrush, pts[^1].X - 2.5f, pts[^1].Y - 2.5f, 5, 5);
        }

        /// <summary>Paint a stylized centered Pokeball + pulsing "WAITING FOR RAID" text for the empty state.</summary>
        private void DrawEmptyState_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            // --- Pokeball ---
            int size = 120;
            int cx = p.Width / 2;
            int cy = (p.Height / 2) - 22;
            var rect = new Rectangle(cx - size / 2, cy - size / 2, size, size);

            // Outer ring (gold faint)
            float ringPulse = 0.5f + 0.5f * (float)Math.Sin(dashPulsePhase);
            int ringAlpha = (int)(36 + 28 * ringPulse);
            using (var ringPen = new Pen(Color.FromArgb(ringAlpha, 0xD4, 0xAF, 0x37), 2))
                e.Graphics.DrawEllipse(ringPen, cx - size / 2 - 18, cy - size / 2 - 18, size + 36, size + 36);

            // Top half (gold gradient)
            using (var topPath = new GraphicsPath())
            {
                topPath.AddPie(rect, 180, 180);
                using var topBrush = new LinearGradientBrush(
                    new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 2),
                    Color.FromArgb(0xE8, 0xC2, 0x52),
                    Color.FromArgb(0xB8, 0x94, 0x1F),
                    LinearGradientMode.Vertical);
                e.Graphics.FillPath(topBrush, topPath);
            }
            // Bottom half (dark)
            using (var botPath = new GraphicsPath())
            {
                botPath.AddPie(rect, 0, 180);
                using var botBrush = new LinearGradientBrush(
                    new Rectangle(rect.X, rect.Y + rect.Height / 2, rect.Width, rect.Height / 2),
                    Color.FromArgb(0x28, 0x28, 0x28),
                    Color.FromArgb(0x10, 0x10, 0x10),
                    LinearGradientMode.Vertical);
                e.Graphics.FillPath(botBrush, botPath);
            }
            // Center band
            using (var bandBrush = new SolidBrush(Color.FromArgb(0x14, 0x14, 0x14)))
                e.Graphics.FillRectangle(bandBrush, rect.X, rect.Y + rect.Height / 2 - 6, rect.Width, 12);
            // Center button (pulsing gold)
            int btnSize = 32;
            int btnAlpha = (int)(140 + 100 * ringPulse);
            using (var btnPath = new GraphicsPath())
            {
                btnPath.AddEllipse(cx - btnSize / 2, cy - btnSize / 2, btnSize, btnSize);
                using var btnBrush = new SolidBrush(Color.FromArgb(0x10, 0x10, 0x10));
                e.Graphics.FillPath(btnBrush, btnPath);
                using var btnRing = new Pen(Color.FromArgb(btnAlpha, 0xE8, 0xC2, 0x52), 3);
                e.Graphics.DrawPath(btnRing, btnPath);
            }
            // Outer ball outline
            using (var outline = new Pen(Color.FromArgb(0xD4, 0xAF, 0x37), 2))
                e.Graphics.DrawEllipse(outline, rect);

            // --- Pulsing text ---
            int textAlpha = (int)(140 + 80 * ringPulse);
            using var titleFont = new Font("Segoe UI", 13F, FontStyle.Bold);
            using var titleBrush = new SolidBrush(Color.FromArgb(textAlpha, 0xD4, 0xAF, 0x37));
            string title = "WAITING FOR RAID";
            var titleSize = e.Graphics.MeasureString(title, titleFont);
            e.Graphics.DrawString(title, titleFont, titleBrush, cx - titleSize.Width / 2, cy + size / 2 + 18);

            using var subFont = new Font("Segoe UI", 9F);
            using var subBrush = new SolidBrush(Color.FromArgb(0x8A, 0x8A, 0x8A));
            string sub = "Connect a bot in BOTS to begin hosting.";
            var subSize = e.Graphics.MeasureString(sub, subFont);
            e.Graphics.DrawString(sub, subFont, subBrush, cx - subSize.Width / 2, cy + size / 2 + 44);
        }

        private Panel BuildHeroBlock(string label, string value, Color valueColor, out Label valueLabel)
        {
            var block = new Panel
            {
                BackColor = Color.FromArgb(0x0A, 0x0A, 0x0A),
                Padding = new Padding(12, 8, 12, 8),
            };
            block.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(60, 0xD4, 0xAF, 0x37), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, block.Width - 1, block.Height - 1);
            };
            var lbl = new Label
            {
                Text = label,
                ForeColor = Color.FromArgb(0x8A, 0x8A, 0x8A),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 16,
                BackColor = Color.Transparent,
            };
            valueLabel = new Label
            {
                Text = value,
                ForeColor = valueColor,
                Font = new Font("Cascadia Code", 22F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
            };
            block.Controls.Add(valueLabel);
            block.Controls.Add(lbl);
            return block;
        }

        private Panel BuildStatsCard()
        {
            var card = new Panel
            {
                BackColor = Color.FromArgb(0x14, 0x14, 0x14),
                Padding = new Padding(18, 14, 18, 14),
                Margin = new Padding(4, 0, 4, 0),
                Dock = DockStyle.Fill,
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(80, 0xD4, 0xAF, 0x37), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            var hdr = new Label
            {
                Text = "STATS",
                ForeColor = Color.FromArgb(0xD4, 0xAF, 0x37),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 22,
                BackColor = Color.Transparent,
            };
            // Big primary number ("Completed")
            var bigNum = new Label
            {
                Text = "0",
                ForeColor = Color.FromArgb(0xE8, 0xC2, 0x52),
                Font = new Font("Segoe UI", 36F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 0, 0, 0),
            };
            var bigSub = new Label
            {
                Text = "raids completed",
                ForeColor = Color.FromArgb(0x8A, 0x8A, 0x8A),
                Font = new Font("Segoe UI", 9F),
                Dock = DockStyle.Top,
                Height = 18,
                BackColor = Color.Transparent,
            };
            // Sparkline panel (last-N raids trend)
            dashStatsSparkline = new Panel
            {
                Dock = DockStyle.Top,
                Height = 28,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 6, 0, 0),
            };
            dashStatsSparkline.Paint += DrawStatsSparkline_Paint;

            // Secondary breakdown
            dashStatsValues = new Label
            {
                Text = "Wins      0\nLosses    0\nWin rate  —\nUptime    —",
                ForeColor = Color.FromArgb(0xE0, 0xE0, 0xE0),
                Font = new Font("Cascadia Mono", 9F),
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0, 10, 0, 0),
            };
            // Reverse insert: header / big / sub / sparkline / breakdown
            card.Controls.Add(dashStatsValues);
            card.Controls.Add(dashStatsSparkline);
            card.Controls.Add(bigSub);
            card.Controls.Add(bigNum);
            card.Controls.Add(hdr);
            return card;
        }

        private Panel BuildConnectionCard()
        {
            var card = new Panel
            {
                BackColor = Color.FromArgb(0x14, 0x14, 0x14),
                Padding = new Padding(18, 14, 18, 14),
                Margin = new Padding(4, 0, 4, 0),
                Dock = DockStyle.Fill,
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(80, 0xD4, 0xAF, 0x37), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            var hdr = new Label
            {
                Text = "CONNECTION",
                ForeColor = Color.FromArgb(0xD4, 0xAF, 0x37),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 22,
                BackColor = Color.Transparent,
            };
            dashConnectionStatus = new Label
            {
                Text = "○  Switch       not connected\n○  Discord      not configured\n○  sys-botbase  unknown\nMode             —",
                ForeColor = Color.FromArgb(0xE0, 0xE0, 0xE0),
                Font = new Font("Cascadia Mono", 9F),
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0, 6, 0, 0),
            };
            card.Controls.Add(dashConnectionStatus);
            card.Controls.Add(hdr);
            return card;
        }

        private Panel BuildDashboardCard(string title, out Label valueLabel, string initialBody)
        {
            var card = new Panel
            {
                BackColor = Color.FromArgb(0x14, 0x14, 0x14),
                Padding = new Padding(18, 14, 18, 14),
                Margin = new Padding(4, 0, 4, 0),
                Dock = DockStyle.Fill,
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(80, 0xD4, 0xAF, 0x37), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            var hdr = new Label
            {
                Text = title,
                ForeColor = Color.FromArgb(0xD4, 0xAF, 0x37),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 22,
                BackColor = Color.Transparent,
            };
            valueLabel = new Label
            {
                Text = initialBody,
                ForeColor = Color.FromArgb(0xE0, 0xE0, 0xE0),
                Font = new Font("Cascadia Mono", 9F),
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft,
            };
            card.Controls.Add(valueLabel);
            card.Controls.Add(hdr);
            return card;
        }

        private void PopulateInitialLiveFeed()
        {
            if (dashLiveFeed == null) return;
            AddLiveFeedEntry("·", "Waiting for activity — connect a bot to begin.", "—");
        }

        public void AddLiveFeedEntry(string icon, string text, string when)
        {
            if (dashLiveFeed == null) return;
            void doIt()
            {
                var row = new Panel
                {
                    Width = dashLiveFeed.ClientSize.Width - 4,
                    Height = 36,
                    Margin = new Padding(0, 0, 0, 2),
                    BackColor = Color.Transparent,
                };
                var icoLbl = new Label
                {
                    Text = icon,
                    ForeColor = Color.FromArgb(0xD4, 0xAF, 0x37),
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    Dock = DockStyle.Left,
                    Width = 24,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                };
                var bodyLbl = new Label
                {
                    Text = text,
                    ForeColor = Color.FromArgb(0xE0, 0xE0, 0xE0),
                    Font = new Font("Segoe UI", 9F),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                };
                var whenLbl = new Label
                {
                    Text = when,
                    ForeColor = Color.FromArgb(0x8A, 0x8A, 0x8A),
                    Font = new Font("Cascadia Mono", 8F),
                    Dock = DockStyle.Right,
                    Width = 50,
                    TextAlign = ContentAlignment.MiddleRight,
                    BackColor = Color.Transparent,
                };
                row.Controls.Add(bodyLbl);
                row.Controls.Add(whenLbl);
                row.Controls.Add(icoLbl);
                dashLiveFeed.Controls.Add(row);
                dashLiveFeed.Controls.SetChildIndex(row, 0); // newest on top
                while (dashLiveFeed.Controls.Count > 30)
                    dashLiveFeed.Controls.RemoveAt(dashLiveFeed.Controls.Count - 1);
            }
            if (dashLiveFeed.IsHandleCreated && dashLiveFeed.InvokeRequired)
                dashLiveFeed.BeginInvoke((MethodInvoker)doIt);
            else
                doIt();
        }

        /// <summary>
        /// Refresh dashboard live values. Phase 2a: best-effort polling from anywhere we can get state.
        /// Phase 2b will hook to RotatingRaidBotSV's real fields.
        /// </summary>
        private void RefreshDashboard()
        {
            // Show empty state if no bot configured OR no active raid currently hosting.
            var state = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.HasActiveRaid;
            bool hasBot = Bots != null && Bots.Count > 0;
            bool showHero = hasBot && state;

            if (dashEmptyState != null) dashEmptyState.Visible = !showHero;
            if (dashHeroBody  != null) dashHeroBody.Visible  =  showHero;

            // Pull live values regardless — the cards (Stats/Connection/Queue) are always visible
            // even without an active raid, so totals etc. should reflect current state.
            var s = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.CompletedTotal;
            var w = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.WinCount;
            var l = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.LossCount;
            var up = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.UptimeMinutes;
            double winRate = s > 0 ? Math.Round((double)w / s * 100, 1) : 0;
            string upStr = up >= 1440 ? $"{up / 1440}d {(up % 1440) / 60}h" : up >= 60 ? $"{up / 60}h {up % 60}m" : $"{up}m";
            if (dashStatsValues != null)
                dashStatsValues.Text = $"Wins      {w}\nLosses    {l}\nWin rate  {winRate}%\nUptime    {upStr}";

            if (dashConnectionStatus != null)
            {
                string sw = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SwitchConnected ? "●" : "○";
                string ds = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.DiscordConnected ? "●" : "○";
                string sb = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SysBotBaseReady ? "●" : "○";
                string ip = string.IsNullOrEmpty(SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SwitchIp) ? "—" : SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SwitchIp;
                dashConnectionStatus.Text = $"{sw}  Switch       {ip}\n{ds}  Discord      {(SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.DiscordConnected ? "connected" : "not configured")}\n{sb}  sys-botbase  {(SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SysBotBaseReady ? "ready" : "unknown")}";
            }

            if (dashQueueList != null)
            {
                var q = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.QueueSnapshot;
                if (q.Count == 0)
                    dashQueueList.Text = "No raids queued yet.\nAdd one via BOTS → ADD BOT,\nor use !ra <seed> in Discord.";
                else
                    dashQueueList.Text = string.Join("\n", q.Take(6));
            }

            if (showHero)
            {
                if (dashHeroPokemonName != null)
                    dashHeroPokemonName.Text = $"{(SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.IsShiny ? "★ " : "")}{SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.CurrentSpecies}";
                if (dashHeroMeta != null)
                    dashHeroMeta.Text = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.MetaLine;
                if (dashHeroMoves != null)
                    dashHeroMoves.Text = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.MoveList;
                if (dashCodeValue != null)
                    dashCodeValue.Text = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.CurrentCode;

                int sec = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.TimerSecondsRemaining;
                if (dashTimerValue != null && sec >= 0)
                    dashTimerValue.Text = $"{sec / 60:D2}:{sec % 60:D2}";
                int total = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.TimerSecondsTotal;
                dashTimerProgress = (total > 0 && sec >= 0) ? Math.Clamp((float)sec / total, 0f, 1f) : 0f;
                dashTimerRing?.Invalidate();

                dashTotalJoined = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.JoinedCount;
                dashTotalCapacity = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.JoinedCapacity;
                dashJoinedDotsPanel?.Invalidate();

                // Async sprite load — only when species changes
                string url = SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SpriteUrl;
                if (dashHeroSprite != null && !string.IsNullOrEmpty(url) && dashHeroSprite.Tag as string != url)
                {
                    dashHeroSprite.Tag = url;
                    _ = LoadSpriteAsync(url);
                }
            }
        }

        private bool _demoRunning = false;
        private async System.Threading.Tasks.Task RunDashboardDemoAsync()
        {
            if (_demoRunning) return;
            _demoRunning = true;
            var D = typeof(SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState);
            try
            {
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.HasActiveRaid = true;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.CurrentSpecies = "Charizard";
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.IsShiny = true;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.Stars = 7;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.TeraTypeEnglish = "Fire";
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SpriteUrl = "https://raw.githubusercontent.com/hexbyt3/HomeImages/master/256x256/poke_capture_0006_000_mf_s_00000000_f_n.png";
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.CurrentCode = "GOLDFISH";
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.MetaLine = "7-Star Black Crystal · Mightiest Mark · Lv 100";
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.MoveList = "Flare Blitz · Air Slash · Dragon Pulse · Roost";
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.TimerSecondsTotal = 60;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.TimerSecondsRemaining = 60;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.JoinedCount = 0;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.JoinedCapacity = 4;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.CompletedTotal = 2758;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.WinCount = 2479;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.LossCount = 279;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.UptimeMinutes = 31 * 24 * 60 + 5 * 60;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SwitchConnected = true;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SwitchIp = "192.168.0.1";
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.DiscordConnected = true;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SysBotBaseReady = true;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.SetQueueSnapshot(new[]
                {
                    "Charizard · ★★★★★★★",
                    "Tinkaton · ★★★★★★",
                    "Eevee event · ★★★",
                    "Iron Bundle · ★★★★★★★",
                    "Annihilape · ★★★★★",
                });
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.Emit("+", "Quilava156 joined Charizard raid");

                // Tick timer 60s → 0s, fire join events at intervals
                for (int t = 60; t >= 0; t--)
                {
                    SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.TimerSecondsRemaining = t;
                    if (t == 45) { SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.JoinedCount = 1; SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.Emit("+", "Quilava156 claimed the code"); }
                    if (t == 30) { SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.JoinedCount = 2; SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.Emit("+", "Tye95 claimed the code"); }
                    if (t == 18) { SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.JoinedCount = 3; SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.Emit("+", "ShinyHunter claimed the code"); }
                    if (t == 8)  { SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.JoinedCount = 4; SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.Emit("★", "Lobby full — starting raid"); }
                    await System.Threading.Tasks.Task.Delay(1000);
                }
                // Raid won
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.Emit("✓", "Charizard raid WON · 4/4 trainers");
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.WinCount = 2480;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.CompletedTotal = 2759;
                await System.Threading.Tasks.Task.Delay(2500);
            }
            finally
            {
                // Reset to empty so the empty-state Pokeball animation comes back.
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.HasActiveRaid = false;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.TimerSecondsRemaining = -1;
                SysBot.Pokemon.SV.BotRaid.Helpers.DashboardState.JoinedCount = 0;
                _demoRunning = false;
            }
        }

        private async System.Threading.Tasks.Task LoadSpriteAsync(string url)
        {
            try
            {
                using var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(8) };
                var bytes = await http.GetByteArrayAsync(url).ConfigureAwait(false);
                using var ms = new System.IO.MemoryStream(bytes);
                var img = Image.FromStream(ms);
                if (dashHeroSprite != null)
                {
                    if (dashHeroSprite.InvokeRequired)
                        dashHeroSprite.BeginInvoke(new Action(() => dashHeroSprite.Image = img));
                    else
                        dashHeroSprite.Image = img;
                }
            }
            catch { /* sprite load is best-effort */ }
        }

        private Panel BuildStatusPillsPanel()
        {
            statusPillsPanel = new Panel
            {
                BackColor = Color.FromArgb(15, 15, 15),
                Dock = DockStyle.Bottom,
                Height = 140,
                Padding = new Padding(22, 18, 22, 14),
                Name = "statusPillsPanel",
            };

            statusHeaderLabel = new Label
            {
                Text = "STATUS",
                ForeColor = Color.FromArgb(0xD4, 0xAF, 0x37),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(220, 14),
                Location = new Point(22, 14),
                BackColor = Color.Transparent,
            };
            // Letter-spaced look via text padding hack; Segoe UI Bold @ 8pt with tracking reads as a label.

            statusDiscordLabel = MakeStatusLine("Discord",     "connected",   PillState.Good, yOffset: 38);
            statusSwitchLabel  = MakeStatusLine("Switch",      "192.168.0.1", PillState.Good, yOffset: 64);
            statusSysbotLabel  = MakeStatusLine("sys-botbase", "ready",       PillState.Good, yOffset: 90);

            statusPillsPanel.Controls.Add(statusHeaderLabel);
            statusPillsPanel.Controls.Add(statusDiscordLabel);
            statusPillsPanel.Controls.Add(statusSwitchLabel);
            statusPillsPanel.Controls.Add(statusSysbotLabel);

            // Top divider line
            statusPillsPanel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(28, 28, 28), 1);
                e.Graphics.DrawLine(pen, 16, 4, statusPillsPanel.Width - 16, 4);
            };

            return statusPillsPanel;
        }

        private enum PillState { Good, Warn, Bad }

        private Label MakeStatusLine(string service, string value, PillState state, int yOffset)
        {
            var dotColor = state switch
            {
                PillState.Good => Color.FromArgb(0x4A, 0xDE, 0x80),
                PillState.Warn => Color.FromArgb(0xFB, 0xBF, 0x24),
                PillState.Bad  => Color.FromArgb(0xF8, 0x71, 0x71),
                _              => Color.Gray,
            };

            // One Label per row, custom-painted with: colored dot · service name · value.
            var row = new Label
            {
                Location = new Point(22, yOffset),
                Size = new Size(216, 20),
                BackColor = Color.Transparent,
                Text = string.Empty,
                Name = $"status_{service}",
                Tag = new StatusRowData { Service = service, Value = value, DotColor = dotColor },
            };
            row.Paint += StatusRow_Paint;
            return row;
        }

        private class StatusRowData
        {
            public string Service { get; set; } = string.Empty;
            public string Value   { get; set; } = string.Empty;
            public Color  DotColor{ get; set; } = Color.Gray;
        }

        private void StatusRow_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Label label || label.Tag is not StatusRowData data)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Colored dot
            using (var dotBrush = new SolidBrush(data.DotColor))
                e.Graphics.FillEllipse(dotBrush, 0, 6, 8, 8);
            // Faint glow halo behind dot
            using (var haloBrush = new SolidBrush(Color.FromArgb(40, data.DotColor)))
                e.Graphics.FillEllipse(haloBrush, -2, 4, 12, 12);

            // Service name (left), value (right)
            using var nameFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            using var valueFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            using var nameBrush = new SolidBrush(Color.FromArgb(0xE0, 0xE0, 0xE0));
            using var valueBrush = new SolidBrush(Color.FromArgb(0x8A, 0x8A, 0x8A));
            e.Graphics.DrawString(data.Service, nameFont, nameBrush, 16, 3);

            var valueSize = e.Graphics.MeasureString(data.Value, valueFont);
            e.Graphics.DrawString(data.Value, valueFont, valueBrush, label.Width - valueSize.Width - 2, 3);
        }

        /// <summary>Update a sidebar status row at runtime. Safe to call from any thread.</summary>
        public void SetStatusPill(string which, string value, bool good)
        {
            Label? target = which switch
            {
                "discord"     => statusDiscordLabel,
                "switch"      => statusSwitchLabel,
                "sys-botbase" => statusSysbotLabel,
                _             => null,
            };
            if (target?.Tag is StatusRowData data)
            {
                data.Value = value;
                data.DotColor = good
                    ? Color.FromArgb(0x4A, 0xDE, 0x80)
                    : Color.FromArgb(0xF8, 0x71, 0x71);
                if (target.IsHandleCreated)
                {
                    if (target.InvokeRequired) target.BeginInvoke(new Action(target.Invalidate));
                    else target.Invalidate();
                }
            }
        }

        private void ConfigureTextBox(TextBox tb, string placeholder, int x, int y, int width)
        {
            tb.BackColor = Color.FromArgb(28, 28, 28);
            tb.BorderStyle = BorderStyle.None;
            tb.Font = new Font("Segoe UI", 11F);
            tb.ForeColor = Color.FromArgb(224, 224, 224);
            tb.Location = new Point(x, y);
            tb.Name = tb.Name;
            tb.PlaceholderText = placeholder;
            tb.Size = new Size(width, 25);
            tb.TabIndex = 0;

            // Focus effects with border color change
            tb.Enter += (s, e) => {
                if (tb.Parent != null)
                    tb.Parent.BackColor = Color.FromArgb(88, 101, 242);
            };
            tb.Leave += (s, e) => {
                if (tb.Parent != null)
                    tb.Parent.BackColor = Color.FromArgb(50, 50, 50);
            };
        }

        private void ConfigureNumericUpDown(NumericUpDown nud, int x, int y, int width)
        {
            nud.BackColor = Color.FromArgb(28, 28, 28);
            nud.BorderStyle = BorderStyle.None;
            nud.Font = new Font("Segoe UI", 11F);
            nud.ForeColor = Color.FromArgb(224, 224, 224);
            nud.Location = new Point(x, y);
            nud.Name = nud.Name;
            nud.Size = new Size(width, 25);
            nud.TabIndex = 1;
        }

        private void ConfigureComboBox(ComboBox cb, int x, int y, int width)
        {
            cb.BackColor = Color.FromArgb(28, 28, 28);
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.FlatStyle = FlatStyle.Flat;
            cb.Font = new Font("Segoe UI", 11F);
            cb.ForeColor = Color.FromArgb(224, 224, 224);
            cb.Location = new Point(x, y);
            cb.Name = cb.Name;
            cb.Size = new Size(width, 28);
            cb.TabIndex = 2;
        }

        private void ConfigureHoverAnimation(Control control)
        {
            var animState = control.Tag as ButtonAnimationState ?? new ButtonAnimationState();
            control.Tag = animState;

            control.MouseEnter += (s, e) => {
                animState.IsHovering = true;
                animState.AnimationStart = DateTime.Now;
            };

            control.MouseLeave += (s, e) => {
                animState.IsHovering = false;
                animState.AnimationStart = DateTime.Now;
            };
        }

        private void ConfigureGlowButton(Button btn)
        {
            ConfigureHoverAnimation(btn);

            btn.Paint += (s, e) => {
                var animState = btn.Tag as ButtonAnimationState;
                if (animState != null && animState.HoverProgress > 0)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    // Multi-layer glow effect
                    var glowAlpha = (int)(60 * animState.HoverProgress);
                    using (var glowBrush = new SolidBrush(Color.FromArgb(glowAlpha, btn.BackColor)))
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            var rect = new Rectangle(-i * 2, -i * 2, btn.Width + i * 4, btn.Height + i * 4);
                            e.Graphics.FillRectangle(glowBrush, rect);
                        }
                    }
                }
            };
        }

        private void CreateRoundedPanel(Panel panel)
        {
            panel.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                var rect = panel.ClientRectangle;
                rect.Inflate(-1, -1);
                GraphicsExtensions.AddRoundedRectangle(path, rect, 8);
                panel.Region = new Region(path);
            };
        }

        private void CreateRoundedButton(Button btn)
        {
            using var path = new GraphicsPath();
            GraphicsExtensions.AddRoundedRectangle(path, btn.ClientRectangle, 6);
            btn.Region = new Region(path);
        }

        private void CreateCircularRegion(Control control)
        {
            using var path = new GraphicsPath();
            path.AddEllipse(0, 0, control.Width, control.Height);
            control.Region = new Region(path);
        }

        private void LogoPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            // Cache the gradient brush to prevent recreation
            if (_logoBrush == null)
            {
                // PKM Universe gold gradient (#b8941f → #e8c252)
                _logoBrush = new LinearGradientBrush(
                    logoPanel.ClientRectangle,
                    Color.FromArgb(0xB8, 0x94, 0x1F),
                    Color.FromArgb(0xE8, 0xC2, 0x52),
                    LinearGradientMode.ForwardDiagonal);
            }

            e.Graphics.FillRectangle(_logoBrush, logoPanel.ClientRectangle);

            // Draw text with glow
            using var font = new Font("Segoe UI", 22F, FontStyle.Bold);
            var text = "RAID-SV";
            var textSize = e.Graphics.MeasureString(text, font);
            var x = (logoPanel.Width - textSize.Width) / 2;
            var y = (logoPanel.Height - textSize.Height) / 2;

            // Simple glow effect - reduced layers
            using var glowBrush = new SolidBrush(Color.FromArgb(30, 255, 255, 255));
            e.Graphics.DrawString(text, font, glowBrush, x - 2, y - 2);

            // Main text
            using var textBrush = new SolidBrush(Color.FromArgb(18, 18, 18));
            e.Graphics.DrawString(text, font, textBrush, x, y);
        }

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            // Bottom border with gradient
            using var brush = new LinearGradientBrush(
                new RectangleF(0, headerPanel.Height - 2, headerPanel.Width, 2),
                Color.FromArgb(50, 88, 101, 242),
                Color.FromArgb(50, 87, 242, 135),
                LinearGradientMode.Horizontal);

            e.Graphics.FillRectangle(brush, 0, headerPanel.Height - 2, headerPanel.Width, 2);
        }

        private void HeaderPanel_Resize(object sender, EventArgs e)
        {
            // Reposition control buttons panel to ensure it stays properly aligned
            if (controlButtonsPanel != null && headerPanel != null)
            {
                int rightMargin = 40;
                int minLeftPosition = titleLabel.Right + 50; // Ensure space between title and buttons
                int desiredX = headerPanel.Width - controlButtonsPanel.Width - rightMargin;

                // Ensure buttons don't overlap with title
                controlButtonsPanel.Location = new Point(Math.Max(minLeftPosition, desiredX), 30);
            }
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            // Ensure proper layout when form is first shown
            HeaderPanel_Resize(headerPanel, EventArgs.Empty);

            // Ensure status indicator is properly positioned
            if (statusIndicator != null && btnUpdate != null)
            {
                statusIndicator.Location = new Point(btnUpdate.ClientSize.Width - 25, 20);
            }

            Refresh();
        }

        private int currentSearchIndex = -1;
        private string lastSearchText = "";

        private void LogSearchBox_TextChanged(object sender, EventArgs e)
        {
            // Reset search when text changes
            currentSearchIndex = -1;
            lastSearchText = logSearchBox.Text;
            searchStatusLabel.Text = "";

            // Clear any existing highlights
            if (RTB_Logs.Text.Length > 0)
            {
                RTB_Logs.SelectAll();
                RTB_Logs.SelectionBackColor = RTB_Logs.BackColor;
                RTB_Logs.SelectionColor = RTB_Logs.ForeColor;
                RTB_Logs.Select(0, 0);
            }

            // Perform initial search if text is not empty
            if (!string.IsNullOrWhiteSpace(logSearchBox.Text))
            {
                HighlightAllMatches();

                // Update match count
                string searchText = logSearchBox.Text.ToLower();
                string content = RTB_Logs.Text.ToLower();
                int matchCount = CountMatches(searchText, content);

                if (matchCount > 0)
                {
                    searchStatusLabel.ForeColor = Color.FromArgb(87, 242, 135);
                    searchStatusLabel.Text = $"{matchCount} matches";
                }
                else
                {
                    searchStatusLabel.ForeColor = Color.FromArgb(237, 66, 69);
                    searchStatusLabel.Text = "No matches";
                }
            }
        }

        private void LogSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevent the ding sound
                if (e.Shift)
                    FindPrevious();
                else
                    FindNext();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Clear search
                logSearchBox.Clear();
                searchStatusLabel.Text = "";
                RTB_Logs.Focus();
            }
            else if (e.KeyCode == Keys.F3 || (e.Control && e.KeyCode == Keys.F))
            {
                // Allow F3 or Ctrl+F to focus search box
                e.SuppressKeyPress = true;
                logSearchBox.Focus();
                logSearchBox.SelectAll();
            }
        }

        private void HighlightAllMatches()
        {
            if (string.IsNullOrWhiteSpace(logSearchBox.Text) || RTB_Logs.Text.Length == 0)
                return;

            string searchText = logSearchBox.Text.ToLower();
            string content = RTB_Logs.Text.ToLower();

            RTB_Logs.SuspendLayout();

            int index = 0;
            while (index < content.Length)
            {
                index = content.IndexOf(searchText, index);
                if (index == -1)
                    break;

                RTB_Logs.Select(index, searchText.Length);
                RTB_Logs.SelectionBackColor = Color.FromArgb(88, 101, 242); // Blue highlight
                RTB_Logs.SelectionColor = Color.White;

                index += searchText.Length;
            }

            RTB_Logs.Select(0, 0);
            RTB_Logs.ResumeLayout();
        }

        private void FindNext()
        {
            if (string.IsNullOrWhiteSpace(logSearchBox.Text) || RTB_Logs.Text.Length == 0)
                return;

            string searchText = logSearchBox.Text.ToLower();
            string content = RTB_Logs.Text.ToLower();

            // Start searching from current position
            int startIndex = currentSearchIndex + 1;
            if (startIndex >= content.Length)
                startIndex = 0;

            int index = content.IndexOf(searchText, startIndex);

            // If not found from current position, try from beginning
            if (index == -1 && startIndex > 0)
            {
                index = content.IndexOf(searchText, 0);
            }

            if (index != -1)
            {
                // First restore previous highlight to blue
                if (currentSearchIndex >= 0 && currentSearchIndex < content.Length - searchText.Length + 1)
                {
                    RTB_Logs.Select(currentSearchIndex, searchText.Length);
                    RTB_Logs.SelectionBackColor = Color.FromArgb(88, 101, 242);
                    RTB_Logs.SelectionColor = Color.White;
                }

                // Highlight current match with different color
                RTB_Logs.Select(index, searchText.Length);
                RTB_Logs.SelectionBackColor = Color.FromArgb(87, 242, 135); // Green for current
                RTB_Logs.SelectionColor = Color.Black;
                RTB_Logs.ScrollToCaret();

                currentSearchIndex = index;

                // Update search status
                int matchCount = CountMatches(searchText, content);
                int currentMatch = CountMatchesBefore(searchText, content, index) + 1;
                searchStatusLabel.ForeColor = Color.FromArgb(87, 242, 135);
                searchStatusLabel.Text = $"{currentMatch} of {matchCount}";
            }
            else
            {
                // No matches found
                searchStatusLabel.ForeColor = Color.FromArgb(237, 66, 69);
                searchStatusLabel.Text = "No matches";
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void FindPrevious()
        {
            if (string.IsNullOrWhiteSpace(logSearchBox.Text) || RTB_Logs.Text.Length == 0)
                return;

            string searchText = logSearchBox.Text.ToLower();
            string content = RTB_Logs.Text.ToLower();

            // Start searching from current position backwards
            int startIndex = currentSearchIndex - 1;
            if (startIndex < 0 || currentSearchIndex == -1)
                startIndex = content.Length - searchText.Length;

            // Ensure we don't go beyond the content length
            if (startIndex >= content.Length)
                startIndex = content.Length - 1;

            int index = -1;
            if (startIndex >= 0 && startIndex < content.Length)
            {
                // Search backwards from current position
                string substring = content.Substring(0, Math.Min(startIndex + searchText.Length, content.Length));
                index = substring.LastIndexOf(searchText);
            }

            // If not found from current position, wrap around to end
            if (index == -1)
            {
                index = content.LastIndexOf(searchText);
            }

            if (index != -1)
            {
                // First restore previous highlight to blue
                if (currentSearchIndex >= 0 && currentSearchIndex < content.Length - searchText.Length + 1)
                {
                    RTB_Logs.Select(currentSearchIndex, searchText.Length);
                    RTB_Logs.SelectionBackColor = Color.FromArgb(88, 101, 242);
                    RTB_Logs.SelectionColor = Color.White;
                }

                // Highlight current match with different color
                RTB_Logs.Select(index, searchText.Length);
                RTB_Logs.SelectionBackColor = Color.FromArgb(87, 242, 135); // Green for current
                RTB_Logs.SelectionColor = Color.Black;
                RTB_Logs.ScrollToCaret();

                currentSearchIndex = index;

                // Update search status
                int matchCount = CountMatches(searchText, content);
                int currentMatch = CountMatchesBefore(searchText, content, index) + 1;
                searchStatusLabel.ForeColor = Color.FromArgb(87, 242, 135);
                searchStatusLabel.Text = $"{currentMatch} of {matchCount}";
            }
            else
            {
                // No matches found
                searchStatusLabel.ForeColor = Color.FromArgb(237, 66, 69);
                searchStatusLabel.Text = "No matches";
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void RTB_Logs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                e.SuppressKeyPress = true;
                logSearchBox.Focus();
                logSearchBox.SelectAll();
            }
            else if (e.KeyCode == Keys.F3)
            {
                e.SuppressKeyPress = true;
                if (!string.IsNullOrWhiteSpace(logSearchBox.Text))
                {
                    if (e.Shift)
                        FindPrevious();
                    else
                        FindNext();
                }
                else
                {
                    logSearchBox.Focus();
                    logSearchBox.SelectAll();
                }
            }
        }

        private int CountMatches(string searchText, string content)
        {
            int count = 0;
            int index = 0;
            while ((index = content.IndexOf(searchText, index)) != -1)
            {
                count++;
                index += searchText.Length;
            }
            return count;
        }

        private int CountMatchesBefore(string searchText, string content, int beforeIndex)
        {
            int count = 0;
            int index = 0;
            while ((index = content.IndexOf(searchText, index)) != -1 && index < beforeIndex)
            {
                count++;
                index += searchText.Length;
            }
            return count;
        }

        private void ParticlePanel_Paint(object sender, PaintEventArgs e)
        {
            // Disabled to prevent flickering
        }

        private void FLP_Bots_Paint(object sender, PaintEventArgs e)
        {
            if (FLP_Bots.Controls.Count == 0)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                using var font = new Font("Segoe UI", 14F, FontStyle.Regular);
                using var brush = new SolidBrush(Color.FromArgb(100, 176, 176, 176));
                var text = "No bots configured. Add a bot using the form above.";
                var size = e.Graphics.MeasureString(text, font);
                e.Graphics.DrawString(text, font, brush,
                    (FLP_Bots.Width - size.Width) / 2,
                    100);
            }
        }

        private void UpdateStatusIndicatorPulse()
        {
            // Throttle updates to reduce flickering
            var now = DateTime.Now;
            if ((now - lastIndicatorUpdate).TotalMilliseconds < PULSE_UPDATE_INTERVAL_MS)
                return;

            lastIndicatorUpdate = now;

            // Increment phase for smooth animation
            pulsePhase += 0.1; // Adjusted for new update interval
            if (pulsePhase > Math.PI * 2)
                pulsePhase -= Math.PI * 2;

            Color newColor;

            if (hasUpdate)
            {
                // Calculate pulse using smooth sine wave
                double pulse = (Math.Sin(pulsePhase) + 1) / 2; // Normalized to 0-1

                // Green pulsing when update available
                int minAlpha = 150;
                int maxAlpha = 255;
                int alpha = (int)(minAlpha + (maxAlpha - minAlpha) * pulse);

                newColor = Color.FromArgb(alpha, 87, 242, 135);
            }
            else
            {
                // Dim gray when no update - no pulsing
                newColor = Color.FromArgb(100, 100, 100);
            }

            // Only update and invalidate if color actually changed
            if (newColor != lastIndicatorColor)
            {
                lastIndicatorColor = newColor;
                statusIndicator.BackColor = newColor;
                statusIndicator.Invalidate();

                // Only invalidate button glow area when update is available and color changed
                if (hasUpdate && btnUpdate != null)
                {
                    btnUpdate.Invalidate(new Rectangle(
                        statusIndicator.Left - 10,
                        statusIndicator.Top - 10,
                        statusIndicator.Width + 20,
                        statusIndicator.Height + 20
                    ));
                }
            }
        }

        // Also update the AnimationTimer_Tick method to be more efficient:
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            bool needsUpdate = false;
            var controlsToUpdate = new List<Control>();

            // Update hover animations only for controls that need it
            foreach (Control control in GetAllControls(this))
            {
                if (control.Tag is ButtonAnimationState animState)
                {
                    var elapsed = (DateTime.Now - animState.AnimationStart).TotalMilliseconds;
                    var duration = 150.0; // 150ms animation

                    var oldProgress = animState.HoverProgress;

                    if (animState.IsHovering)
                    {
                        animState.HoverProgress = Math.Min(1.0, elapsed / duration);
                    }
                    else
                    {
                        animState.HoverProgress = Math.Max(0.0, 1.0 - (elapsed / duration));
                    }

                    // Only add to update list if progress actually changed significantly
                    // and animation is in progress (not at 0 or 1)
                    if (Math.Abs(animState.HoverProgress - oldProgress) > 0.01 &&
                        animState.HoverProgress > 0.001 && animState.HoverProgress < 0.999)
                    {
                        controlsToUpdate.Add(control);
                        needsUpdate = true;
                    }
                }
            }

            // Batch invalidate controls
            foreach (var control in controlsToUpdate)
            {
                control.Invalidate();
            }

            // Update status indicator pulse with throttling
            UpdateStatusIndicatorPulse();
        }

        private void TransitionPanels(int index)
        {
            // Hide all panels
            botsPanel.Visible = false;
            hubPanel.Visible = false;
            logsPanel.Visible = false;
            if (dashboardPanel != null) dashboardPanel.Visible = false;

            // Force refresh of content panel
            contentPanel.Refresh();

            // Show the selected panel
            switch (index)
            {
                case 0:
                    botsPanel.Visible = true;
                    break;
                case 1:
                    hubPanel.Visible = true;
                    break;
                case 2:
                    logsPanel.Visible = true;
                    logsPanel.Refresh(); // Extra refresh for logs panel
                    break;
                case 3:
                    if (dashboardPanel != null)
                    {
                        dashboardPanel.Visible = true;
                        RefreshDashboard();
                    }
                    break;
            }
        }

        private IEnumerable<Control> GetAllControls(Control container)
        {
            var controls = container.Controls.Cast<Control>();
            return controls.SelectMany(ctrl => GetAllControls(ctrl)).Concat(controls);
        }

        // Animation state class
        private class ButtonAnimationState
        {
            public bool IsHovering { get; set; }
            public DateTime AnimationStart { get; set; }
            public double HoverProgress { get; set; }
            public Color BaseColor { get; set; }
        }

        #endregion

        // Controls
        private TableLayoutPanel mainLayoutPanel;
        private Panel sidebarPanel;
        private Panel contentPanel;
        private Panel headerPanel;
        private Panel logoPanel;
        private FlowLayoutPanel navButtonsPanel;
        private Button btnNavBots;
        private Button btnNavHub;
        private Button btnNavLogs;
        private Button btnNavDashboard;
        private Button btnNavExit;
        // Dashboard view (Phase 2)
        private Panel? dashboardPanel;
        private Label? dashHeroPokemonName;
        private Label? dashHeroMeta;
        private Label? dashHeroMoves;
        private Label? dashCodeValue;
        private Label? dashTimerValue;
        private Label? dashJoinedDots;
        private Label? dashQueueList;
        private Label? dashStatsValues;
        private Label? dashConnectionStatus;
        private PictureBox? dashHeroSprite;
        private FlowLayoutPanel? dashLiveFeed;
        private Panel sidebarBottomPanel;
        private Panel statusPillsPanel;
        private Label statusHeaderLabel;
        private Label statusDiscordLabel;
        private Label statusSwitchLabel;
        private Label statusSysbotLabel;
        private Button btnUpdate;
        // Configuration page (inner-sidebar design)
        private Panel configInnerSidebar;
        private Panel configSectionContent;
        private Dictionary<string, Panel> sectionPanels;
        private Dictionary<string, Button> configNavButtons = new();
        private string currentConfigSection = "General";
        // Quick-edit controls (now hosted inside the section panels)
        private TextBox qsBotName;
        private TextBox qsDiscordToken;
        private TextBox qsCommandPrefix;
        private TextBox qsGameStatus;
        private NumericUpDown qsTotalRaids;
        private CheckBox qsHideRaidCode;
        private CheckBox qsUseScreenshots;
        private CheckBox qsRotateRandom;
        private CheckBox qsKeepDaySeed;
        private Label titleLabel;
        private FlowLayoutPanel controlButtonsPanel;
        private Button btnStart;
        private Button btnStop;
        private Button btnReboot;
        private Button btnRefreshMap;
        private Panel botsPanel;
        private Panel hubPanel;
        private Panel logsPanel;
        private Panel botHeaderPanel;
        private Panel addBotPanel;
        private TextBox TB_IP;
        private NumericUpDown NUD_Port;
        private ComboBox CB_Protocol;
        private ComboBox CB_Routine;
        private Button B_New;
        private FlowLayoutPanel FLP_Bots;
        private PropertyGrid PG_Hub;
        private RichTextBox RTB_Logs;
        private Panel logsHeaderPanel;
        private TextBox logSearchBox;
        private Label searchStatusLabel;
        private Button btnClearLogs;
        private Panel particlePanel;
        private Panel statusIndicator;
        private System.Windows.Forms.Timer animationTimer;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayContextMenu;
        private bool isExiting = false;
        private LinearGradientBrush _logoBrush;

        // Compatibility aliases
        private Button updater => btnUpdate;
        private Button B_Start => btnStart;
        private Button B_Stop => btnStop;
        private Button B_RebootAndStop => btnReboot;
        private Button B_RefreshMap => btnRefreshMap;
        private TabControl TC_Main;
        private TabPage Tab_Bots;
        private TabPage Tab_Hub;
        private TabPage Tab_Logs;
        private ComboBox comboBox1;
    }
}