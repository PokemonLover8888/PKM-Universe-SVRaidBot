# PKM-Universe-SVRaidBot — fork notes

Private fork of [hexbyt3/SVRaidBot](https://github.com/hexbyt3/SVRaidBot). Local-only for now; will push to GitHub once stable.

## Remotes

| Remote      | URL                                          | Purpose                          |
| ----------- | -------------------------------------------- | -------------------------------- |
| `origin`    | https://github.com/hexbyt3/SVRaidBot.git     | Upstream — pull bug fixes from here |
| `jonklee99` | https://github.com/jonklee99/SV-Raid.git     | QoL fork — cherry-pick selectively |

## Brand fork point

All PKM Universe branding sits on top of `2b8f26d` ("Fix Encounter9RNG.GenerateData returning early for shiny raids", Dec 27 2025). The four cherry-picks from `jonklee99` add owner-DM, persistent raid counts, multi-prefix support, and uppercase raid codes.

## Files that carry the PKM Universe branding

When pulling upstream commits, expect conflicts in these — re-apply the patches if `git rebase` clobbers them:

- `SysBot.Pokemon/Helpers/SVRaidBot.cs` — `Version`, `BotName`, `Website`, `Discord` constants
- `SysBot.Pokemon/Settings/Integrations/DiscordSettings.cs` — default `CommandPrefix = "!"`, `BotGameStatus`
- `SysBot.Pokemon/SV/BotRaid/RotatingRaidBotSV.cs` — multi-raid disclaimer `"pkm-universe.com"`, footer icon URL
- `SysBot.Pokemon.Discord/Commands/General/InfoModule.cs` — `!info` embed (gold, pkm-universe links)
- `SysBot.Pokemon.WinForms/Main.cs` — window/tray title "Raid-SV — PKM Universe Raid System"
- `SysBot.Pokemon.WinForms/SysBot.Pokemon.WinForms.csproj` — `AssemblyName=PKM-Universe-Raid`, Company, Product

## Sync workflow

```powershell
# 1. Fetch upstream
git fetch origin
git fetch jonklee99

# 2. See what's new
git log HEAD..origin/main --oneline
git log HEAD..jonklee99/main --oneline

# 3. Rebase our branding on top of new upstream
git rebase origin/main
#   resolve conflicts in the files listed above, then `git rebase --continue`

# 4. Or cherry-pick a specific fix only
git cherry-pick <sha>

# 5. Rebuild self-contained EXE
dotnet publish SysBot.Pokemon.WinForms/SysBot.Pokemon.WinForms.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -o publish/win-x64

# 6. Deploy to bot folder
Copy-Item publish/win-x64/PKM-Universe-Raid.exe `
  C:\Users\ericr\OneDrive\Desktop\Raid-SV-Bot\ -Force
```

## Why single-file self-contained

Per project convention (`feedback_za_bots_publish.md`, `feedback_never_copy_exe.md`): every bot EXE ships its own .NET runtime so the bot folder has exactly **one** EXE and zero loose DLLs. Never copy build-output DLLs into bot folders for raid-bot — the publish output is the source of truth.

## Build target

`net9.0-windows` (matches upstream). Both .NET 9 and .NET 10 desktop runtimes are installed locally; the SDK in use is 10.0.102. Self-contained publish embeds .NET 9 runtime so the host machine doesn't need it.

## Skipped jonklee99 commits

- `1cf20f1` — window-size tweak (we have our own branding for the title bar)
- `db4c1f7` — removed upstream's update checker (keep upstream's check; it's harmless and points at hexbyte's repo)
