using Microsoft.Win32;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Polus.Patcher;

public sealed class MainForm : Form
{
    private readonly TextBox pathBox = new() { ReadOnly = true, Width = 450 };
    private readonly TextBox logBox = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private string? gamePath;
    private string? backupPath;

    public MainForm()
    {
        Text = "Polus";
        Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "polus.ico"));
        Width = 760;
        Height = 500;
        MinimumSize = new Size(680, 430);
        StartPosition = FormStartPosition.CenterScreen;

        var heading = new Label { Text = "POLUS  /  AMONG US MOD SETUP", AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold) };
        var pathRow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = false };
        pathRow.Controls.Add(new Label { Text = "Game folder:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        pathRow.Controls.Add(pathBox);
        var detect = new Button { Text = "Detect Steam", AutoSize = true };
        detect.Click += (_, _) => DetectSteam();
        var browse = new Button { Text = "Browse...", AutoSize = true };
        browse.Click += (_, _) => Browse();
        pathRow.Controls.Add(detect);
        pathRow.Controls.Add(browse);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
        var setup = new Button { Text = "Setup", Width = 120, Height = 36 };
        setup.Click += (_, _) => Setup();
        var patch = new Button { Text = "Patch", Width = 120, Height = 36 };
        patch.Click += (_, _) => Patch();
        var restore = new Button { Text = "Restore", Width = 120, Height = 36 };
        restore.Click += (_, _) => Restore();
        actions.Controls.AddRange(new Control[] { setup, patch, restore });

        var note = new Label {
            Text = "Patch installs a BepInEx package ZIP that you select. Use a compatible BepInEx 6 IL2CPP Windows x64 package for your exact game version.",
            AutoSize = true, MaximumSize = new Size(700, 0), ForeColor = Color.DimGray
        };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 1, RowCount = 5 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(pathRow, 0, 1);
        layout.Controls.Add(actions, 0, 2);
        layout.Controls.Add(note, 0, 3);
        layout.Controls.Add(logBox, 0, 4);
        Controls.Add(layout);
        Log("Ready. Detect your Steam installation or browse to the folder containing Among Us.exe.");
    }

    private void DetectSteam()
    {
        var found = FindSteamInstall();
        if (found is null) { Log("Steam install not found automatically. Use Browse."); return; }
        SetGamePath(found);
        Log("Detected: " + found);
    }

    private static string? FindSteamInstall()
    {
        var roots = new List<string>();
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            if (key?.GetValue("SteamPath") is string path) roots.Add(path.Replace('/', Path.DirectorySeparatorChar));
        }
        catch { }
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
            if (key?.GetValue("InstallPath") is string path) roots.Add(path);
        }
        catch { }

        foreach (var root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var libraries = new List<string> { root };
            var vdf = Path.Combine(root, "steamapps", "libraryfolders.vdf");
            if (File.Exists(vdf))
            {
                foreach (Match match in Regex.Matches(File.ReadAllText(vdf), "\"path\"\\s*\"([^\"]+)\""))
                    libraries.Add(match.Groups[1].Value.Replace("\\\\", "\\"));
            }
            foreach (var library in libraries.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var candidate = Path.Combine(library, "steamapps", "common", "Among Us");
                if (File.Exists(Path.Combine(candidate, "Among Us.exe"))) return candidate;
            }
        }
        return null;
    }

    private void Browse()
    {
        using var dialog = new FolderBrowserDialog { Description = "Select the folder containing Among Us.exe" };
        if (dialog.ShowDialog() == DialogResult.OK) SetGamePath(dialog.SelectedPath);
    }

    private void SetGamePath(string path)
    {
        if (!File.Exists(Path.Combine(path, "Among Us.exe")))
        {
            MessageBox.Show("That folder does not contain Among Us.exe.", "Invalid folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        gamePath = Path.GetFullPath(path);
        pathBox.Text = gamePath;
    }

    private bool CloseGameIfRunning()
    {
        var processes = Process.GetProcessesByName("Among Us");
        if (processes.Length == 0) return true;
        if (MessageBox.Show("Among Us is running. Ask it to close before continuing?", "Polus",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return false;
        foreach (var process in processes)
        {
            try
            {
                process.CloseMainWindow();
                if (!process.WaitForExit(7000))
                {
                    Log("The game did not close normally. Close it manually and retry.");
                    return false;
                }
            }
            catch (Exception ex) { Log("Could not close game: " + ex.Message); return false; }
        }
        return true;
    }

    private void Setup()
    {
        if (gamePath is null) { DetectSteam(); if (gamePath is null) return; }
        if (!CloseGameIfRunning()) return;
        backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Polus", "Backups", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(backupPath);
        File.WriteAllText(Path.Combine(backupPath, "game-root.txt"), gamePath);
        Log("Setup complete. Backup workspace: " + backupPath);
    }

    private void Patch()
    {
        if (gamePath is null) { MessageBox.Show("Detect or browse to the Among Us folder first."); return; }
        if (!CloseGameIfRunning()) return;
        using var dialog = new OpenFileDialog { Filter = "BepInEx package (*.zip)|*.zip", Title = "Select compatible BepInEx IL2CPP x64 ZIP" };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        if (backupPath is null) Setup();
        if (backupPath is null) return;

        var changed = new List<string>();
        try
        {
            using var archive = ZipFile.OpenRead(dialog.FileName);
            var safeRoot = Path.GetFullPath(gamePath) + Path.DirectorySeparatorChar;
            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name)) continue;
                var target = Path.GetFullPath(Path.Combine(gamePath, entry.FullName));
                if (!target.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("The ZIP contains an unsafe path outside the game folder.");
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                if (File.Exists(target))
                {
                    var saved = Path.Combine(backupPath, "original", Path.GetRelativePath(gamePath, target));
                    Directory.CreateDirectory(Path.GetDirectoryName(saved)!);
                    File.Copy(target, saved, true);
                }
                using var input = entry.Open();
                using var output = File.Create(target);
                input.CopyTo(output);
                changed.Add(Path.GetRelativePath(gamePath, target));
            }
            File.WriteAllLines(Path.Combine(backupPath, "changed-files.txt"), changed);
            Log($"Patch package extracted. Files installed: {changed.Count}");
            Log("This installs the selected loader package only. The custom gameplay plugin is not implemented yet.");
        }
        catch (Exception ex)
        {
            Log("Patch failed: " + ex.Message);
            MessageBox.Show(ex.Message, "Patch failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Restore()
    {
        if (gamePath is null) { MessageBox.Show("Select the game folder first."); return; }
        using var dialog = new FolderBrowserDialog { Description = "Select the Polus backup folder" };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        var backup = dialog.SelectedPath;
        var list = Path.Combine(backup, "changed-files.txt");
        var rootFile = Path.Combine(backup, "game-root.txt");
        if (!File.Exists(list) || !File.Exists(rootFile) ||
            !string.Equals(Path.GetFullPath(File.ReadAllText(rootFile).Trim()), Path.GetFullPath(gamePath), StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("This backup is invalid or belongs to a different game folder.");
            return;
        }
        if (!CloseGameIfRunning()) return;

        var safeRoot = Path.GetFullPath(gamePath) + Path.DirectorySeparatorChar;
        foreach (var relative in File.ReadAllLines(list))
        {
            var target = Path.GetFullPath(Path.Combine(gamePath, relative));
            if (!target.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase)) continue;
            var saved = Path.Combine(backup, "original", relative);
            if (File.Exists(saved))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(saved, target, true);
            }
            else if (File.Exists(target)) File.Delete(target);
        }
        Log("Restore completed for files recorded in the selected backup.");
    }

    private void Log(string message) =>
        logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
}
