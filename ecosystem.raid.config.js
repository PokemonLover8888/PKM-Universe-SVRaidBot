// ============================================================================
//  PM2 supervisor for the 3 SV raid bots (Paldea / Kitakami / Blueberry).
//  Same pattern as the trade fleet's ecosystem.hosts.config.js, but these are the
//  WinForms build (PKM-Universe-Raid.exe) — pm2 just supervises them, so each bot
//  still shows a window. They need no clicks: autostart.txt in each folder makes
//  the bot press its own START a few seconds after launch.
//
//  Why one pm2 app per bot (never grouped): all three are SV, and SysCord<T>.Runner
//  is static per game type — two SV bots in one process collide. Same rule as host-D/E.
//
//  Manage:  pm2 restart raid-A raid-B raid-C     (instead of 3 double-clicks)
//  Persist: pm2 save   -> restored by pm2 resurrect on reboot.
//  ⚠ Orphan risk: these are self-contained single-file EXEs and don't always die on a
//    graceful stop. If a bot flaps or double-posts after a restart, hard-kill any stray
//    PKM-Universe-Raid.exe (`taskkill /F /IM PKM-Universe-Raid.exe`) and start again.
// ============================================================================
const D = 'C:\\Users\\ericr\\OneDrive\\Desktop\\';
const EXE = '\\PKM-Universe-Raid.exe';

// rest-proxy.txt beside the exe takes priority over this env var (see SysCord.ResolveRestProxy),
// and all three bots log "REST routed through rate-limit governor: http://127.0.0.1:3460/api/v10/"
// at startup — the LOCAL governor. The old START-*.bat set this var to a remote 167.233.212.104
// instead, which was inert only because the file wins; delete rest-proxy.txt and the fleet would
// have silently routed off-box. Pointed at the local governor so both paths agree. 2026-08-31.
const common = {
  interpreter: 'none',                 // run the .exe directly, not via node
  autorestart: true,
  exp_backoff_restart_delay: 15000,    // 15s, grows on repeat crashes = Discord ban safety
  min_uptime: 30000,                   // must stay up 30s to count as stable
  max_memory_restart: '3000M',         // raid embeds/sprites run heavier than a trade host
  kill_timeout: 8000,
  env: { DISCORD_REST_PROXY: 'http://127.0.0.1:3460/api/v10/' },
};

// Per-bot web API port (Seed Finder / Raid Hub poll these). Set explicitly to what each
// bot already resolves today so the value can't drift: RAIDBOT_WEB_PORT wins over webport.txt.
const raid = (name, folder, webPort) => ({
  name,
  script: D + folder + EXE,
  cwd: D + folder,
  ...common,
  env: { ...common.env, RAIDBOT_WEB_PORT: String(webPort) },
});

// The watchdog is plain Node, not a bot EXE, so it does NOT take `common` (no interpreter:'none').
// It catches the one failure nothing else can see: the game dying while sys-botbase, the process,
// Discord and the web API all keep reporting healthy. See raid-watchdog.js for the full story.
const watchdog = {
  name: 'raid-watchdog',
  script: 'C:\\Users\\ericr\\source\\repos\\PKM-Universe-SVRaidBot\\raid-watchdog.js',
  cwd: 'C:\\Users\\ericr\\source\\repos\\PKM-Universe-SVRaidBot',
  autorestart: true,
  exp_backoff_restart_delay: 15000,
  min_uptime: 30000,
  max_memory_restart: '300M',
};

// SWSH Keldeo den bot — a DIFFERENT program from the SV raid bots (built from PKM-Universe-SWSHDenRaidBot)
// whose exe shares its name with the trade bots ("PKM-Universe Bot.exe"): never `taskkill /IM` it, kill by
// path. Web API port comes from its config.json (ControlPanelPort 8200), not RAIDBOT_WEB_PORT. Its
// rest-proxy.txt (local governor, same as every other bot since 2026-10-10) wins over the env var; both agree.
// Replaces START-SWSH-DenRaidBot-Test.bat (renamed .disabled-pm2-owns-it so it can't start a 2nd copy).
const swshDen = {
  name: 'raid-D',
  script: D + 'SWSH-DenRaidBot-Test\\PKM-Universe Bot.exe',
  cwd: D + 'SWSH-DenRaidBot-Test',
  ...common,
  env: { DISCORD_REST_PROXY: 'http://127.0.0.1:3460/api/v10/' },
};

module.exports = {
  apps: [
    watchdog,
    // The map each bot works is decided by where its player stands in-game, NOT by config —
    // the maps below are what each bot actually reported on 2026-08-30, not a config setting.
    raid('raid-A', 'Electivire-SV-Raid-Bot', 9090), // Kitakami  — Switch 10.0.0.201, no webport.txt = default 9090
    raid('raid-B', 'Greninja-SV-Raid-Bot',   9110), // Blueberry — Switch 10.0.0.233, webport.txt 9110
    raid('raid-C', 'Infernape-SV-Raid-Bot',  9100), // Paldea    — Switch 10.0.0.25,  webport.txt 9100
    swshDen,                                        // SWSH dens — Switch 10.0.0.242 (Sword), web 8200
  ],
};
