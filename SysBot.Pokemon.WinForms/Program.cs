using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms
{
    internal static class Program
    {
        // IMPORTANT: with a self-extracting single-file publish, Application.StartupPath and
        // AppContext.BaseDirectory point at a RANDOM temp extraction folder that changes every
        // launch — so config/history/logs written there never persist. Environment.ProcessPath
        // returns the real EXE path (the bot folder), which is what we want for all persisted data.
        public static readonly string WorkingDirectory =
            Path.GetDirectoryName(Environment.ProcessPath) ?? Application.StartupPath;
        public static string ConfigPath { get; private set; } = Path.Combine(WorkingDirectory, "config.json");

        // Win32 helpers for finding/restoring the existing window when a duplicate launch is blocked.
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);
        private const int SW_RESTORE = 9;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
#if NETCOREAPP
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
            // The raid loop does heavy synchronous Switch I/O that can saturate the thread pool.
            // .NET only injects new pool threads ~1/sec, which delays Discord button-interaction
            // handlers past the 3-second ACK window ("This interaction failed"). Raising the floor
            // keeps ready threads available so handler continuations run immediately.
            ThreadPool.SetMinThreads(256, 256);

            var cmd = Environment.GetCommandLineArgs();
            var cfg = Array.Find(cmd, z => z.EndsWith(".json"));
            if (cfg != null)
                ConfigPath = cmd[0];

            // Single-instance guard: scope the mutex to THIS EXE's folder so each bot folder
            // (Raid-SV-Bot, Mew-SV-Bot, etc.) can still run independently, but two copies of
            // the SAME folder cannot stack up.
            var lockKey  = "Local\\PKMUniverseRaid_" + WorkingDirectory.Replace('\\', '_').Replace(':', '_').Replace('/', '_');
            var eventKey = lockKey + "_show";
            using var mutex = new Mutex(true, lockKey, out bool createdNew);
            if (!createdNew)
            {
                // Already running. Two cases:
                //  a) Window is visible (taskbar/normal) — find its HWND and bring to front.
                //  b) Window is stealth-hidden in the tray — HWND lookup fails, so signal a
                //     named event that the running instance listens for, prompting it to
                //     restore from tray.
                var me = Process.GetCurrentProcess();
                var existing = Process.GetProcessesByName(me.ProcessName)
                    .FirstOrDefault(p => p.Id != me.Id && SafeMainModuleFileName(p) == me.MainModule?.FileName);
                if (existing != null && existing.MainWindowHandle != IntPtr.Zero)
                {
                    if (IsIconic(existing.MainWindowHandle))
                        ShowWindow(existing.MainWindowHandle, SW_RESTORE);
                    SetForegroundWindow(existing.MainWindowHandle);
                }
                // Fire the event regardless — covers the tray-stealth case where HWND is hidden.
                try
                {
                    using var ev = EventWaitHandle.OpenExisting(eventKey);
                    ev.Set();
                }
                catch (WaitHandleCannotBeOpenedException) { /* running instance didn't register the event yet */ }
                return;
            }

            // Init the per-trainer raid history store before the bot starts hosting.
            SysBot.Base.RaidHistoryStore.Init(WorkingDirectory);

            // We're the singular running instance. Register the named event and listen on a
            // background thread so subsequent launches can signal us to restore from tray.
            var showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, eventKey);
            Main? mainForm = null;
            var listener = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        showEvent.WaitOne();
                        mainForm?.RestoreFromTray();
                    }
                    catch (ObjectDisposedException) { return; }
                    catch { /* keep listening */ }
                }
            }) { IsBackground = true, Name = "RaidSV-ShowFromTrayListener" };
            listener.Start();

            Application.EnableVisualStyles();

            // FontAwesome.Sharp's IconButton constructor intermittently throws
            //   ArgumentException: Font 'Segoe UI Black' does not support style 'Regular'
            // inside new Main() -> InitializeComponent(). That runs before Application.Run
            // pumps messages, so no ThreadException handler can catch it and the process dies.
            // Roughly 3 of 10 bots failed to launch this way per fleet restart (2026-07-19).
            const int maxFormAttempts = 4;
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    mainForm = new Main();
                    break;
                }
                catch (ArgumentException ex) when (attempt < maxFormAttempts
                                                   && ex.Message.Contains("does not support style"))
                {
                    Console.WriteLine($"[Font Warning] Startup font race (attempt {attempt}/{maxFormAttempts}): {ex.Message}");
                    System.Threading.Thread.Sleep(750 * attempt);
                }
            }

            Application.Run(mainForm);
            showEvent.Dispose();
        }

        private static string? SafeMainModuleFileName(Process p)
        {
            try { return p.MainModule?.FileName; }
            catch { return null; }
        }
    }
}
