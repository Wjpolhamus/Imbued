using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

public static class LauncherCreator
{
    public static void CreateLauncherIfNeeded()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string exePath = Process.GetCurrentProcess().MainModule!.FileName!;
        string appName = "HomeServer";

        //
        // LINUX LAUNCHER
        //
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string desktopFile = Path.Combine(
                home,
                ".local/share/applications",
                $"{appName}.desktop"
            );

            if (!File.Exists(desktopFile))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(desktopFile)!);

                // IMPORTANT:
                // Exec now opens the webpage instead of running the server.
                // Terminal=false so it doesn't open a terminal window.
                // Icon=web-browser gives a normal icon.
                string contents = $@"
[Desktop Entry]
Type=Application
Name={appName}
Exec=xdg-open http://localhost:5000
Icon=web-browser
Terminal=false
Categories=Network;
";

                File.WriteAllText(desktopFile, contents);

                // Make the launcher executable
                Process.Start("chmod", $"+x \"{desktopFile}\"");
            }
        }

        //
        // WINDOWS SHORTCUT
        //
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string desktop = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"{appName}.lnk"
            );

            if (!File.Exists(desktop))
            {
                string psScript = $@"
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut('{desktop}')
$Shortcut.TargetPath = '{exePath}'
$Shortcut.Save()
";

                string tempScript = Path.Combine(Path.GetTempPath(), "create_shortcut.ps1");
                File.WriteAllText(tempScript, psScript);

                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{tempScript}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
        }
    }
}

