# Polus

Polus is a Windows patcher and source scaffold for a host-configured Among Us custom-role mod targeting the Steam installation.

## Current scope

- Windows x64 WinForms patcher.
- Steam library discovery and manual game-folder selection.
- Setup step with confirmation-based game closing and backup workspace.
- Patch step for a user-selected BepInEx IL2CPP x64 ZIP, with ZIP path-safety checks.
- Restore support for files touched by the patcher.
- Catalog of 34 proposed roles and a host-settings validation library.
- GitHub Actions workflow to compile and package the Windows patcher.

Important: this is a patcher foundation, not a completed 34-role gameplay mod. The catalog entries are role specifications only. Actual Among Us gameplay hooks, role UI, custom RPC integration, and mobile-client compatibility are not implemented. Unmodified mobile clients cannot execute custom client-side roles merely because the host is on PC.

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
- src/RoleCore/ — game-independent role/configuration model.
- roles/roles.json — 34 role specifications.

Project source is MIT licensed. Third-party game and mod-loader components are not included; their licenses remain with their respective authors.
