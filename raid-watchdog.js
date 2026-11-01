#!/usr/bin/env node
/**
 * RAID WATCHDOG — detects the failure mode that nothing else can see.
 *
 * When Scarlet/Violet dies on a console, sys-botbase (a sysmodule) keeps running, so:
 *   - the TCP connection still succeeds          -> ping and port checks LIE
 *   - the bot process never exits                -> pm2 reports "online", 0 restarts
 *   - Discord stays connected                    -> the bot looks green everywhere
 *   - /api/bot/instances still says RotatingRaidBot (status is set at start, not derived)
 * ...while the bot sits blocked forever on a Switch read that has no timeout. Infernape did
 * exactly this on 2026-09-08: hung at 20:13, still "healthy" at 01:00, 5 hours of nothing.
 *
 * The only honest signals are:
 *   1. getTitleID  — a real reply proves the GAME is alive, not just the console.
 *   2. log freshness — a working raid bot writes a line every few seconds.
 *
 * Actions:
 *   HUNG  (log stale, game running)  -> pm2 restart the bot. This is the proven fix; it is
 *                                       exactly what a human does, and the bot's own
 *                                       reopen-game recovery takes it from there.
 *   NOGAME(log stale, no game)       -> ALERT ONLY. A restart cannot help until the game is
 *                                       relaunched, and this watchdog never presses buttons
 *                                       on a console it cannot see.
 *   DOWN  (console unreachable)      -> alert only.
 *
 * MEMORY GUARD (added 2026-10-24): pm2's own max_memory_restart is DEAD on this machine -
 * `pm2 jlist` reports monit.memory = 0 for every single app, so the caps configured on
 * raid-A..D and host-* have never once fired. raid-D (the SWSH den bot) leaks ~21 MB/s and
 * had reached 8.9 GB working set before this was noticed. The guard below reads the real
 * working set from tasklist and restarts a bot that runs away. Limits are deliberately far
 * above each bot's normal ceiling: this is a runaway net, not a tuning knob. Setting it near
 * the normal range would restart raid-D roughly once a minute and it would never host a raid.
 *
 * Config (optional): raid-watchdog.config.json beside this file
 *   { "webhookUrl": "https://discord.com/api/webhooks/...", "staleMinutes": 15 }
 * With no config it still auto-recovers; it just logs instead of pinging you.
 */

const fs = require('fs');
const net = require('net');
const path = require('path');
const https = require('https');
const { execFile } = require('child_process');

const DESKTOP = 'C:\\Users\\ericr\\OneDrive\\Desktop';

const BOTS = [
  { app: 'raid-A', folder: 'Electivire-SV-Raid-Bot', webPort: 9090 },
  { app: 'raid-B', folder: 'Greninja-SV-Raid-Bot', webPort: 9110 },
  { app: 'raid-C', folder: 'Infernape-SV-Raid-Bot', webPort: 9100 },
  // SWSH den bot: no web API, so its Switch IP is fixed here.
  { app: 'raid-D', folder: 'SWSH-DenRaidBot-Test', ip: '10.0.0.242' },
];

// Runaway-memory net. limitMB must stay well clear of the bot's ordinary peak.
//   raid-A/B/C idle around 200 MB -> 2000 is ~10x headroom, only a true runaway trips it.
//   raid-D sawtooths to ~3 GB in normal operation and GC pulls it back; only the 6 GB+
//   excursions are pathological, so that is where the net sits.
const MEM_WATCH = [
  { app: 'raid-A', limitMB: 2000 },
  { app: 'raid-B', limitMB: 2000 },
  { app: 'raid-C', limitMB: 2000 },
  { app: 'raid-D', limitMB: 6000 },
];

const CFG_PATH = path.join(__dirname, 'raid-watchdog.config.json');
const STATE_PATH = path.join(__dirname, 'raid-watchdog.state.json');

const DEFAULTS = {
  webhookUrl: '',
  checkIntervalSec: 180,     // how often to sweep
  staleMinutes: 15,          // a healthy raid bot writes every few seconds; 15 min is dead
  restartCooldownMin: 20,    // never restart the same bot more often than this
  maxRestartsPerDay: 8,      // beyond this, stop restarting and just shout - something is wrong
  alertRepeatMin: 60,        // don't re-alert the same condition more often than this
  memCheckSec: 30,           // memory moves far faster than a hang; sweep it on its own clock
  memCooldownMin: 15,        // never memory-restart the same bot more often than this
  maxMemRestartsPerDay: 12,  // beyond this, stop restarting and shout instead
};

function loadCfg() {
  try {
    return { ...DEFAULTS, ...JSON.parse(fs.readFileSync(CFG_PATH, 'utf8')) };
  } catch {
    return { ...DEFAULTS };
  }
}

function loadState() {
  try {
    return JSON.parse(fs.readFileSync(STATE_PATH, 'utf8'));
  } catch {
    return {};
  }
}

function saveState(s) {
  try {
    fs.writeFileSync(STATE_PATH, JSON.stringify(s, null, 2));
  } catch (e) {
    log(`WARN could not write state: ${e.message}`);
  }
}

function log(msg) {
  console.log(`[${new Date().toISOString()}] ${msg}`);
}

/** Ask sys-botbase what title is running. Empty/zeros = console is up but NO GAME. */
function getTitleID(ip, timeoutMs = 5000) {
  return new Promise((resolve) => {
    const sock = new net.Socket();
    let done = false;
    const finish = (v) => {
      if (done) return;
      done = true;
      sock.destroy();
      resolve(v);
    };
    sock.setTimeout(timeoutMs);
    sock.on('timeout', () => finish(null));
    sock.on('error', () => finish(null));
    sock.on('data', (d) => finish(d.toString('ascii').trim()));
    sock.connect(6000, ip, () => sock.write('getTitleID\r\n'));
  });
}

/** SV bots log "2026-10-25 23:19:15.7184|..."; the SWSH den bot logs "10-25-2026 7:47:52.5772 PM|...". */
function parseLogTime(l) {
  let m = l.match(/^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})/);
  if (m) {
    const t = new Date(m[1].replace(' ', 'T'));
    return isNaN(t) ? null : t;
  }
  m = l.match(/^(\d{2})-(\d{2})-(\d{4}) (\d{1,2}):(\d{2}):(\d{2})\.\d+ (AM|PM)/);
  if (!m) return null;
  let h = parseInt(m[4], 10) % 12;
  if (m[7] === 'PM') h += 12;
  const t = new Date(+m[3], +m[1] - 1, +m[2], h, +m[5], +m[6]);
  return isNaN(t) ? null : t;
}

/**
 * Age in minutes of the newest RAID line in the bot's log.
 * Discord/gateway lines are excluded on purpose: a hung bot keeps emitting those forever,
 * which is precisely why it looks alive. NLog writes this file as UTF-16 with a BOM.
 */
function logAgeMinutes(folder) {
  const file = path.join(DESKTOP, folder, 'logs', 'SysBotLog.txt');
  let text;
  try {
    const size = fs.statSync(file).size;
    let want = Math.min(size, 256 * 1024);
    let start = size - want;
    // UTF-16 is two bytes per code unit. An odd start offset shifts every character by
    // one byte and decodes the whole tail into garbage, which reads as "no raid line".
    if (start % 2 !== 0) { start++; want--; }
    const fd = fs.openSync(file, 'r');
    const buf = Buffer.alloc(want);
    fs.readSync(fd, buf, 0, want, start);
    fs.closeSync(fd);
    text = buf.toString('utf16le');
  } catch (e) {
    return { age: null, readOk: false, err: e.message };
  }

  const lines = text.split(/\r?\n/);
  for (let i = lines.length - 1; i >= 0; i--) {
    const l = lines[i];
    if (l.includes('SysCord') || l.includes('Gateway') ||
        l.includes('Discord connection lost') || l.includes('Client reconnected')) continue;
    const t = parseLogTime(l);
    if (!t) continue;
    return { age: (Date.now() - t.getTime()) / 60000, last: l.slice(0, 120), readOk: true };
  }

  // Not a read failure: the file opened and decoded, it simply holds no raid line at all.
  // Report where the tail begins so the caller can put a floor under how long this bot
  // has been producing nothing but Discord noise.
  let tailStart = null;
  for (const l of lines) {
    const t = parseLogTime(l);
    if (t) { tailStart = t.getTime(); break; }
  }
  return { age: null, readOk: true, tailStart, err: 'no raid line in tail' };
}

/** The bot's own web API knows which Switch IP it is actually using (survives DHCP drift). */
function getBotIp(webPort) {
  return new Promise((resolve) => {
    const req = require('http').get(
      { host: '127.0.0.1', port: webPort, path: '/api/bot/instances', timeout: 5000 },
      (res) => {
        let d = '';
        res.on('data', (c) => (d += c));
        res.on('end', () => {
          try {
            const inst = JSON.parse(d).Instances[0];
            resolve({ ip: inst.BotStatuses[0].Name, status: inst.BotStatuses[0].Status });
          } catch {
            resolve({ ip: null, status: null });
          }
        });
      }
    );
    req.on('error', () => resolve({ ip: null, status: null }));
    req.on('timeout', () => { req.destroy(); resolve({ ip: null, status: null }); });
  });
}

/**
 * Real working set in MB, straight from tasklist. We do NOT use pm2's monit.memory: on this
 * machine it returns 0 for every process, which is exactly why max_memory_restart never fired.
 */
function processMemoryMB(pid) {
  return new Promise((resolve) => {
    execFile('tasklist', ['/FI', `PID eq ${pid}`, '/FO', 'CSV', '/NH'],
      { windowsHide: true, timeout: 15000 }, (err, stdout) => {
        if (err || !stdout) return resolve(null);
        // Last CSV field is Mem Usage, e.g. "1,827,436 K" (thousands separators are locale-dependent).
        const m = stdout.match(/"([\d.,  ]+) K"\s*$/m);
        if (!m) return resolve(null);
        const kb = parseInt(m[1].replace(/\D/g, ''), 10);
        return resolve(Number.isFinite(kb) ? kb / 1024 : null);
      });
  });
}

/** pm2's pid for an app. Cached, because resolving it means spawning pm2. */
const pidCache = {};
function pm2Pid(app) {
  return new Promise((resolve) => {
    execFile('pm2', ['jlist'], { shell: true, timeout: 30000, windowsHide: true, maxBuffer: 32 * 1024 * 1024 },
      (err, stdout) => {
        if (err || !stdout) return resolve(null);
        try {
          // pm2 can print banner text around the JSON; take the outermost array.
          const a = stdout.indexOf('[');
          const b = stdout.lastIndexOf(']');
          if (a < 0 || b <= a) return resolve(null);
          const entry = JSON.parse(stdout.slice(a, b + 1)).find((x) => x.name === app);
          return resolve(entry && entry.pid ? entry.pid : null);
        } catch {
          return resolve(null);
        }
      });
  });
}

function run(cmd, args) {
  return new Promise((resolve) => {
    // windowsHide: without it, spawning pm2 flashes a console window on the desktop.
    execFile(cmd, args, { shell: true, timeout: 60000, windowsHide: true }, (err) => resolve(!err));
  });
}

// pm2's SIGINT does NOT end a WinForms bot on Windows: pm2 marks it stopped, starts a new
// copy, and the old one keeps driving the same Switch. 2026-10-24 four raid-D copies piled
// up this way (one per memory restart), fought over the console and crashed its connection.
// So: stop, force-kill the old process tree, then start.
async function pm2Restart(app) {
  const oldPid = await pm2Pid(app);
  await run('pm2', ['stop', app]);
  if (oldPid) await run('taskkill', ['/PID', String(oldPid), '/T', '/F']);
  return run('pm2', ['restart', app]);
}

function alert(cfg, title, body) {
  log(`ALERT ${title} :: ${body}`);
  if (!cfg.webhookUrl) return;
  try {
    const payload = JSON.stringify({
      username: 'Raid Watchdog',
      embeds: [{ title, description: body, color: 0xE74C3C, timestamp: new Date().toISOString() }],
    });
    const u = new URL(cfg.webhookUrl);
    const req = https.request(
      { hostname: u.hostname, path: u.pathname + u.search, method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Content-Length': Buffer.byteLength(payload) } },
      (res) => res.resume()
    );
    req.on('error', (e) => log(`WARN webhook failed: ${e.message}`));
    req.write(payload);
    req.end();
  } catch (e) {
    log(`WARN webhook error: ${e.message}`);
  }
}

function today() {
  return new Date().toISOString().slice(0, 10);
}

async function checkBot(bot, cfg, state) {
  const st = (state[bot.app] = state[bot.app] || { restarts: {}, lastRestart: 0, lastAlert: {} });
  const { ip, status } = bot.webPort ? await getBotIp(bot.webPort) : { ip: bot.ip, status: null };
  let { age, last, err, readOk, tailStart } = logAgeMinutes(bot.folder);

  if (age === null && !readOk) {
    log(`${bot.app} ${bot.folder}: cannot read log (${err})`);
    return;
  }

  // A bot dead long enough for its last raid line to scroll out of the tail has no age at
  // all. That used to return here, so it escaped every check below and the watchdog went
  // quiet exactly when the bot was sickest - 2026-10-28, raid-C sat idle 4.5h in silence
  // because its console had no game loaded. Treat "no raid line" as maximally stale, not
  // as unknown, and let the diagnosis below work out why.
  const noRaidLine = age === null;
  if (noRaidLine) {
    st.noRaidSince = st.noRaidSince || tailStart || Date.now();
    age = (Date.now() - st.noRaidSince) / 60000;
    last = '(no raid line in log tail - only Discord noise)';
  } else {
    delete st.noRaidSince;
  }

  const fresh = age < cfg.staleMinutes;
  if (fresh) {
    const what = noRaidLine ? `no raid line yet, ${age.toFixed(1)}m` : `log ${age.toFixed(1)}m old`;
    log(`${bot.app} OK   ${what}, ip=${ip || '?'} status=${status || '?'}`);
    return;
  }

  // Stale. Now find out WHY - this is the part every other health signal gets wrong.
  const title = ip ? await getTitleID(ip) : null;
  const noGame = title !== null && /^0+$/.test(title);
  const unreachable = title === null;

  const canAlert = (key) => {
    const lastT = st.lastAlert[key] || 0;
    if (Date.now() - lastT < cfg.alertRepeatMin * 60000) return false;
    st.lastAlert[key] = Date.now();
    return true;
  };

  if (unreachable) {
    if (canAlert('down')) {
      alert(cfg, `${bot.folder}: console unreachable`,
        `No reply from ${ip || 'unknown IP'}:6000. Log has been silent ${age.toFixed(0)} min.\n` +
        `Console may be off, asleep, or moved IP (DHCP).`);
    }
    return;
  }

  if (noGame) {
    if (canAlert('nogame')) {
      alert(cfg, `${bot.folder}: GAME NOT RUNNING`,
        `sys-botbase is alive at ${ip} but getTitleID returned ${title} - no game loaded.\n` +
        `Raid log silent ${age.toFixed(0)} min. **Relaunch the game on the console**, ` +
        `then this watchdog will restart the bot automatically.\n` +
        `A bot restart alone will NOT fix this.`);
    }
    return;
  }

  // A bot someone stopped on purpose has a silent log too - leave it stopped.
  if (!(await pm2Pid(bot.app))) {
    log(`${bot.app} log ${age.toFixed(0)}m old but not running under pm2 - leaving it alone`);
    return;
  }

  // Game IS running but the bot stopped working: it is hung on a dead read. Restart fixes it.
  const day = today();
  const count = st.restarts[day] || 0;
  const sinceLast = (Date.now() - st.lastRestart) / 60000;

  if (sinceLast < cfg.restartCooldownMin) {
    log(`${bot.app} HUNG (log ${age.toFixed(0)}m) but restarted ${sinceLast.toFixed(0)}m ago - waiting out cooldown`);
    return;
  }
  if (count >= cfg.maxRestartsPerDay) {
    if (canAlert('giveup')) {
      alert(cfg, `${bot.folder}: repeated hangs - NOT restarting again`,
        `Already restarted ${count}x today and it hung again (log ${age.toFixed(0)}m old, game IS running).\n` +
        `Something deeper is wrong - check the console and the bot config.`);
    }
    return;
  }

  log(`${bot.app} HUNG: log ${age.toFixed(0)}m old, game IS running (${title}). Restarting. Last line: ${last}`);
  const ok = await pm2Restart(bot.app);
  st.lastRestart = Date.now();
  st.restarts[day] = count + 1;
  alert(cfg, `${bot.folder}: auto-restarted (was hung)`,
    `Raid log had been silent ${age.toFixed(0)} min while the game was still running - ` +
    `the classic hung-read. \`pm2 restart ${bot.app}\` ${ok ? 'succeeded' : 'FAILED'}. ` +
    `Restart ${count + 1}/${cfg.maxRestartsPerDay} today.`);
}

async function checkMemory(entry, cfg, state) {
  const { app, limitMB } = entry;

  let pid = pidCache[app];
  let mb = pid ? await processMemoryMB(pid) : null;
  if (mb === null) {
    // No row for the cached pid: the bot restarted and has a new one.
    pid = await pm2Pid(app);
    if (!pid) return;
    pidCache[app] = pid;
    mb = await processMemoryMB(pid);
    if (mb === null) return;
  }

  if (mb < limitMB) {
    if (mb > limitMB * 0.75) log(`${app} memory ${Math.round(mb)} MB (limit ${limitMB})`);
    return;
  }

  const st = (state[app] = state[app] || { restarts: {}, lastRestart: 0, lastAlert: {} });
  const day = today();
  st.memRestarts = st.memRestarts || {};
  const count = st.memRestarts[day] || 0;
  const sinceLast = (Date.now() - (st.lastMemRestart || 0)) / 60000;

  if (sinceLast < cfg.memCooldownMin) {
    log(`${app} over memory limit (${Math.round(mb)} MB) but memory-restarted ${sinceLast.toFixed(0)}m ago - waiting out cooldown`);
    return;
  }
  if (count >= cfg.maxMemRestartsPerDay) {
    const lastT = st.lastAlert['memgiveup'] || 0;
    if (Date.now() - lastT >= cfg.alertRepeatMin * 60000) {
      st.lastAlert['memgiveup'] = Date.now();
      alert(cfg, `${app}: runaway memory - NOT restarting again`,
        `Already memory-restarted ${count}x today and it is back at ${Math.round(mb)} MB ` +
        `(limit ${limitMB} MB). The leak needs a code fix, not another restart.`);
    }
    return;
  }

  log(`${app} RUNAWAY MEMORY: ${Math.round(mb)} MB >= ${limitMB} MB. Restarting.`);
  const ok = await pm2Restart(app);
  st.lastMemRestart = Date.now();
  st.memRestarts[day] = count + 1;
  delete pidCache[app];
  alert(cfg, `${app}: auto-restarted (runaway memory)`,
    `Working set reached ${Math.round(mb)} MB against a ${limitMB} MB limit. ` +
    `\`pm2 restart ${app}\` ${ok ? 'succeeded' : 'FAILED'}. ` +
    `Memory restart ${count + 1}/${cfg.maxMemRestartsPerDay} today.`);
}

async function memSweep() {
  const cfg = loadCfg();
  const state = loadState();
  let touched = false;
  for (const entry of MEM_WATCH) {
    try {
      const before = JSON.stringify(state[entry.app] || null);
      await checkMemory(entry, cfg, state);
      if (JSON.stringify(state[entry.app] || null) !== before) touched = true;
    } catch (e) {
      log(`${entry.app} memory check failed: ${e.message}`);
    }
  }
  // Only write when something actually changed: the hang sweep owns this file too.
  if (touched) saveState(state);
}

async function sweep() {
  const cfg = loadCfg();
  const state = loadState();
  for (const bot of BOTS) {
    try {
      await checkBot(bot, cfg, state);
    } catch (e) {
      log(`${bot.app} check failed: ${e.message}`);
    }
  }
  saveState(state);
}

(async function main() {
  const cfg = loadCfg();
  log(`Raid watchdog started. ${BOTS.length} bots, sweep every ${cfg.checkIntervalSec}s, ` +
      `stale threshold ${cfg.staleMinutes}m, webhook ${cfg.webhookUrl ? 'configured' : 'NOT configured (logs only)'}.`);
  log(`Memory guard: ${MEM_WATCH.map((m) => `${m.app}<${m.limitMB}MB`).join(' ')}, checked every ${cfg.memCheckSec}s.`);
  await sweep();
  setInterval(sweep, cfg.checkIntervalSec * 1000);
  await memSweep();
  setInterval(memSweep, cfg.memCheckSec * 1000);
})();
