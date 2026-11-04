# Polus

Polus is a Windows patcher and source scaffold for a host-configured Among Us custom-role mod targeting the Steam installation.

## Target game build

Polus targets the latest observed Steam public-branch build of Among Us: **Build ID 25134893** (SteamDB reports it was updated on 29 September 2026). This is a Steam build identifier, not an assumed in-game semantic version. The mobile v19.0.1 announcement is not used as the Steam compatibility target. Re-check the public branch build before each release. [SteamDB build history](https://steamdb.info/app/945360/depots/) · [Official Among Us updates](https://www.innersloth.com/games/among-us/).

## Current scope

- Windows x64 WinForms patcher.
- Steam library discovery and manual game-folder selection.
- Setup step with confirmation-based game closing and backup workspace.
- Patch step for a user-selected BepInEx IL2CPP x64 ZIP, with ZIP path-safety checks.
- Restore support for files touched by the patcher.
- Catalog of 35 original role concepts (12 Crewmate, 11 Impostor, 12 Neutral), host-settings validation, and a game-independent host-authoritative match-state foundation.
- GitHub Actions workflow to compile and package the Windows patcher.

Important: this is not yet a completed in-game role mod. The catalog entries remain design specifications; the match runtime handles generic assignment/cooldown/objective state only. Actual Among Us gameplay hooks, per-role ability handlers, role UI, custom RPC integration, and mobile-client compatibility are not implemented. Unmodified mobile clients cannot execute custom client-side roles merely because the host is on PC.

The patcher does not bypass anti-cheat, disable game protections, or modify matchmaking security. Use mods only in lobbies where all participants agree.

## Build

Open the Actions tab, select Build Polus, and run it. The workflow also runs on pushes to main. Download the Polus-Windows-x64 artifact from the completed run.

Local build on Windows with .NET 8 SDK:

    dotnet publish .\\src\\Patcher\\Patcher.csproj -c Release -r win-x64 --self-contained true

Output: src/Patcher/bin/Release/net8.0-windows/win-x64/publish/.

## Install

1. Run Polus.exe.
2. Select Detect Steam or browse to the folder containing Among Us.exe.
3. Press Setup to create a backup workspace.
4. Press Patch and select a BepInEx 6 IL2CPP Windows x64 ZIP that supports your installed game version.
5. To undo the patch, use Restore and select the backup folder.

Game updates can break mod compatibility. Verify compatibility for the exact installed game build before patching.

## Layout

- .github/workflows/build.yml — Windows build pipeline.
- src/Patcher/ — Windows patcher.
- src/RoleCore/ — game-independent role/configuration model and match runtime.
- roles/roles.json — 35 role specifications.

Project source is MIT licensed. Third-party game and mod-loader components are not included; their licenses remain with their respective authors.
