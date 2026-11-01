#!/usr/bin/env node
/**
 * LOG JANITOR — keeps the bots' own NLog files from eating the disk.
 *
 * Why this exists: LogUtil configures the file target with
 *     ArchiveNumbering = ArchiveNumberingMode.Date
 *     ArchiveAboveSize = 104857600   // 100MB (never)
 * Date numbering gives one archive NAME per day, so a size rollover inside the same day has
 * nowhere to roll to and the 100MB cap is inert - the author's own "(never)" comment is
 * literally true. SysBotLog.txt therefore grows without limit until midnight.
 *
 * That is harmless on a normal day (~3 MB per bot) and brutal during an outage: when the
 * consoles go unreachable the bots retry in a tight loop and log every failure, about
 * 300 KB/s. On 2026-10-28 a three-hour power cut produced
 *     Electivire 3,483 MB   Greninja 1,542 MB   Infernape 1,171 MB
 * in a single day - roughly 1000x normal - and 7.6 GB had accumulated across the fleet
 * before anyone looked.
 *
 * MaxArchiveFiles = 14 does prune by COUNT, but 14 outage-sized files is still ~50 GB, and
 * retired bot folders have nothing running to prune them at all (some held archives back to
 * 2025-11). So this enforces a BYTE budget per bot, deleting oldest-first.
 *
 * Scope: only files named SysBotLog.<yyyy-MM-dd>.txt - rotated archives that nothing reads.
 * The live SysBotLog.txt is never touched; NLog holds it open and the watchdog tails it.
 *
 * Note the limit of this approach: it bounds what ACCUMULATES, not what a single bad day
 * writes. Capping one day's spike needs a code fix in SysBot.Base/Util/LogUtil.cs
 * (ArchiveNumbering = DateAndSequence, so ArchiveAboveSize can actually fire) plus a
 * republish of every bot EXE. Until then this cleans up after the spike instead.
 */

const fs = require('fs');
const path = require('path');

const DESKTOP = 'C:\\Users\\ericr\\OneDrive\\Desktop';

// Per-bot byte budget for rotated archives. A normal bot writes ~3 MB/day, so 400 MB is
// months of ordinary history while still collapsing one outage-sized file on sight.
const MAX_ARCHIVE_MB = 400;

// Hard age cap as well: nothing here is worth keeping past this, whatever the size.
const KEEP_DAYS = 45;

// A live log this big means something is spewing right now - worth saying out loud.
const LIVE_WARN_MB = 500;

const SWEEP_HOURS = 6;

const ARCHIVE_RE = /^SysBotLog\.(\d{4}-\d{2}-\d{2})\.txt$/;

function log(msg) {
  console.log(`[${new Date().toISOString()}] ${msg}`);
}

/** Today in LOCAL time: NLog names archives off the local clock, not UTC. */
function localToday() {
  const d = new Date();
  return [d.getFullYear(), String(d.getMonth() + 1).padStart(2, '0'),
    String(d.getDate()).padStart(2, '0')].join('-');
}

/** Every archive under a logs dir, including nested subfolders NLog sometimes creates. */
function findArchives(dir, today, out = []) {
  let entries;
  try { entries = fs.readdirSync(dir, { withFileTypes: true }); } catch { return out; }

  for (const e of entries) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) { findArchives(p, today, out); continue; }

    const m = e.name.match(ARCHIVE_RE);
    // Never the live file, and never today's archive: a rollover may be mid-write.
    if (!m || m[1] === today) continue;

    try { out.push({ p, date: m[1], size: fs.statSync(p).size }); } catch { /* vanished */ }
  }
  return out;
}

function logDirs() {
  let entries;
  try { entries = fs.readdirSync(DESKTOP, { withFileTypes: true }); } catch { return []; }
  return entries
    .filter((e) => e.isDirectory())
    .map((e) => path.join(DESKTOP, e.name, 'logs'))
    .filter((d) => fs.existsSync(d));
}

function sweep() {
  const today = localToday();
  const cutoff = new Date(Date.now() - KEEP_DAYS * 86400000).toISOString().slice(0, 10);
  const budget = MAX_ARCHIVE_MB * 1048576;

  let freedTotal = 0;
  let deletedTotal = 0;

  for (const dir of logDirs()) {
    const bot = path.basename(path.dirname(dir));

    // Warn about a live log that is running away, even though we will not touch it.
    try {
      const liveMB = fs.statSync(path.join(dir, 'SysBotLog.txt')).size / 1048576;
      if (liveMB > LIVE_WARN_MB) {
        log(`${bot}: LIVE log is ${liveMB.toFixed(0)} MB - something is spewing (check the console connection)`);
      }
    } catch { /* no live log: a retired bot folder */ }

    const archives = findArchives(dir, today);
    if (!archives.length) continue;

    // Oldest first, so both passes below delete the least useful files.
    archives.sort((a, b) => (a.date < b.date ? -1 : a.date > b.date ? 1 : 0));

    const doomed = [];
    let kept = 0;

    // Pass 1: anything past the age cap goes regardless of size.
    const survivors = [];
    for (const f of archives) {
      if (f.date < cutoff) doomed.push({ ...f, why: 'age' });
      else survivors.push(f);
    }

    // Pass 2: walk NEWEST first and keep until the byte budget is spent.
    for (let i = survivors.length - 1; i >= 0; i--) {
      const f = survivors[i];
      if (kept + f.size <= budget) kept += f.size;
      else doomed.push({ ...f, why: 'budget' });
    }

    if (!doomed.length) continue;

    let freed = 0, failed = 0;
    for (const f of doomed) {
      try { fs.unlinkSync(f.p); freed += f.size; } catch { failed++; }
    }

    const byAge = doomed.filter((d) => d.why === 'age').length;
    log(`${bot}: deleted ${doomed.length - failed} archives (${byAge} aged out, ` +
        `${doomed.length - byAge} over ${MAX_ARCHIVE_MB} MB budget), freed ` +
        `${(freed / 1048576).toFixed(0)} MB, kept ${(kept / 1048576).toFixed(0)} MB` +
        `${failed ? `, ${failed} locked` : ''}`);

    freedTotal += freed;
    deletedTotal += doomed.length - failed;
  }

  if (deletedTotal) {
    log(`sweep done: ${deletedTotal} files, ${(freedTotal / 1073741824).toFixed(2)} GB freed`);
  } else {
    log('sweep done: nothing over budget');
  }
}

log(`Log janitor started. Budget ${MAX_ARCHIVE_MB} MB of archives per bot, ` +
    `hard age cap ${KEEP_DAYS} days, sweep every ${SWEEP_HOURS}h.`);
sweep();
setInterval(sweep, SWEEP_HOURS * 3600 * 1000);
