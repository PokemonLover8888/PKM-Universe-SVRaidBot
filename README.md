<div align="center">

<img src="https://pkm-universe.com/pkm-universe-icon.png" width="120" alt="PKM Universe" />

# PKM Universe Raid System

### Automated Scarlet/Violet Tera Raid hosting for **PKM Universe Reborn**

[![Website](https://img.shields.io/badge/web-pkm--universe.com-d4af37)](https://pkm-universe.com)
[![Discord](https://img.shields.io/badge/discord-PKM%20Universe%20Reborn-5865f2)](https://discord.gg/pkm-universe-reborn)
![Built on](https://img.shields.io/badge/fork%20of-hexbyt3%2FSVRaidBot-555)

</div>

---

> A private, rebranded fork of [hexbyt3/SVRaidBot](https://github.com/hexbyt3/SVRaidBot) tailored for the PKM Universe Reborn community. Branded **Raid-SV**, gold-themed, with a custom dashboard, Discord button UX, per-trainer history, and an auto-posted weekly leaderboard.

<!-- SCREENSHOTS: drag-drop your own Dashboard / embed / Configuration screenshots here in the GitHub README editor. -->
<!-- Suggested order: 1) Dashboard hero card, 2) a raid embed with buttons, 3) the Configuration inner-sidebar. -->

---

## What's different from upstream

This fork keeps everything hexbyte's SVRaidBot does and adds a PKM Universe layer on top:

### Branding & UI
- **Gold theme** (`#d4af37`) throughout — window, embeds, buttons
- **Raid-SV** window/tray/EXE title with the PKM Universe icon
- **`!` command prefix** by default (so it doesn't clash with the `$` trade bots)
- **Dashboard view** — animated empty state, live hero card (sprite, raid code, circular countdown ring, joined-trainer dots), Stats / Queue / Connection cards with a sparkline, and a live activity feed rail
- **Configuration page** rebuilt with an inner sidebar (General / Discord / Raids / Embed / Lobby / Advanced) and a searchable, reflection-driven settings form — no more raw PropertyGrid

### Discord experience
- **Buttons** beneath every raid embed: `🔑 Get Code`, `📜 Rules`, `🔗 pkm-universe.com` — one click instead of reacting
- **Verified-gate** — only members with the configured raid role can claim codes (admins/sudo bypass)
- **Auto-threads** — each raid embed spawns a "Strategy chat" thread
- **Edit-in-place recap** — when a raid ends, the original embed turns green/red, shows `RAID WON/FAILED` + the trainer list, and updates the running win-rate
- **Tera-typed embed color** + visual `★★★` star ratings

### Community features
- **`!myraids`** — your last 20 claimed raids with win/loss and win-rate
- **`!leaderboard [week|month|all]`** — top 10 raid joiners with medals
- **Weekly auto-posted leaderboard** — pinned every Sunday (set the channel in Config → Discord → `Weekly Leaderboard Channel ID`)

### Robustness
- Single-instance guard (no more stacked processes), restore-from-tray on relaunch, self-cleaning `.port` files, and logs anchored next to the EXE

---

# 📱 Access from Any Device on Your Network

## Quick Setup

### 1. Enable Network Access (choose one):
- **Option A:** Right-click `PKM-Universe-Raid.exe` → Run as Administrator
- **Option B:** Run in admin cmd: `netsh http add urlacl url=http://+:9090/ user=Everyone`

### 2. Allow Through Firewall:
```cmd
netsh advfirewall firewall add rule name="Raid-SV Web" dir=in action=allow protocol=TCP localport=9090
```

### 3. Connect From Your Phone:
- Get your PC's IP: `ipconfig` (look for IPv4 Address)
- On your phone: `http://YOUR-PC-IP:9090`
- Example: `http://192.168.1.100:9090`

## Requirements
- Same WiFi network · Windows Firewall rule (step 2) · Admin rights (first time only)

---

# Features

## Auto Teleport
Teleports to the nearest raid den if the den is lost.

## Raid Requests
- **Add:** `!ra <seed> <difficulty> <storyprogress>`
- **Remove:** `!rqc` (removes you from the queue)
- **Preview:** `!rv <seed> <difficulty> <storyprogress>` shows the full raid embed
- **Disable / Limit:** the bot owner can toggle requests or cap them per user/time window

## Full Event Support
Request an active event; the bot teleports to the nearest Event den and overwrites the seed. Auto-detects Might and Distribution group IDs.

## Mystery Raids
Turn on Mystery Raids for a random shiny raid each rotation.

## Disable Overworld Spawns
Stops wild overworld encounters between raids.

## Embed Settings
Extensive embed customization — language, type icons, screenshots, code privacy, and more.

## Auto Story Progress
Automatically edits game flags per raid — no need for separate "baby" and "adult" bots.

---

# Raid Bot Guide

- **ActiveRaids** — the rotation list. Edit `PartyPK` on an entry to use a specific Showdown set for that raid only. On first launch the bot seeds two random shiny raids so you can start immediately.
- **RaidSettings**
  - `GenerateRaidsFromFile` — `True` reads seeds from `raidfilessv/raidsv.txt` in the format `<seed>-<species>-<stars>-<storyprogress>` (e.g. `3739A70B-Goomy-3-4`)
  - `SaveSeedsToFile` — back up ActiveRaids to file
  - `RandomRotation` — shuffle the list (requests still prioritized)
  - `MysteryRaids` — inject random shiny raids (mutually exclusive with RandomRotation)
  - `KeepDaySeed` / `EnableTimeRollback` — keep the day seed stable
- **Embed Toggles** — RaidEmbedDescription, tera icon style, IncludeMoves/Rewards/Seed/Countdown/TypeAdvantage, screenshots, `HideRaidCode`
- **LobbyOptions** — `SkipRaid` / `Continue` / `OpenLobby` methods, `MashA` vs `AFK` action, extra disband/party timing
- **RaiderBanList** — `!ban <name|NID>` to ban; keep `AllowIfEmpty` false
- **DiscordSettings**
  - `Token` — from the [Discord Developer Portal](https://discord.com/developers/applications/)
  - `CommandPrefix` — default `!`
  - `RoleSudo` — `!sudo @you` to claim owner control
  - `RoleRaidRequest` — members with this role can claim raid codes (the verified-gate)
  - `ChannelWhitelist` — `!addchannel`
  - `LoggingChannels` — `!loghere`
  - `EchoChannels` — `!aec` (where raid embeds post)
  - `LeaderboardChannelId` — where the weekly leaderboard auto-posts (0 = disabled)

## Announcement Settings
`!announce TEXT` sends a styled embed to all servers using the bot. Configure thumbnail, color, and randomization options.

---

# In-Game Setup
- Stand in front of your raid crystal
- Game options: `Give Nicknames` Off · `Send to Boxes` Automatic · `Auto Save` Off · `Text Speed` Fast
- Start the bot

## Program Setup
- Enter a raid description (or leave blank)
- `!aec` to post embeds in a specific channel
- Paste your seed into the Seed parameter; set **Code the raid** if you want a private code
- Set `DateTimeFormat`, `TimeToScrollDownForRollover` (~800 Lite / ~950 OLED / ~920 V1), and run `ConfigureRolloverCorrection` once (game closed) to dial in timing

---

<div align="center">

**PKM Universe Reborn** · [pkm-universe.com](https://pkm-universe.com)

Built on [hexbyt3/SVRaidBot](https://github.com/hexbyt3/SVRaidBot) · Licensed under AGPL-3.0

</div>
