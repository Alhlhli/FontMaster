using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace FontMaster
{
    public static class FontInstaller
    {
        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int AddFontResource(string lpszFilename);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            UIntPtr wParam,
            IntPtr lParam,
            uint fuFlags,
            uint uTimeout,
            out UIntPtr lpdwResult);

        private const int HWND_BROADCAST = 0xffff;
        private const uint WM_FONTCHANGE = 0x001D;
        private const uint SMTO_ABORTIFHUNG = 0x0002;

        public static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public static bool InstallFont(FontItem item, out string error)
        {
            error = null;
            if (item == null || string.IsNullOrEmpty(item.FilePath) || !File.Exists(item.FilePath))
            {
                error = "File does not exist.";
                return false;
            }

            try
            {
                bool isAdmin = IsAdministrator();
                string targetDir;
                RegistryKey regKey;

                if (isAdmin)
                {
                    targetDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                    regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", true);
                }
                else
                {
                    // Windows 10/11 per-user font installation
                    targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts");
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }
                    regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", true);
                }

                if (regKey == null)
                {
                    error = "Failed to access Windows Fonts Registry.";
                    return false;
                }

                string destPath = Path.Combine(targetDir, item.FileName);

                // Copy font file to Fonts directory
                if (!File.Exists(destPath) || !destPath.Equals(item.FilePath, StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(item.FilePath, destPath, true);
                }

                // Prepare registry value name, e.g. "Amiri (TrueType)" or "MyFont (OpenType)"
                string formatTag = item.Extension.Equals("OTF", StringComparison.OrdinalIgnoreCase) ? "(OpenType)" : "(TrueType)";
                string valueName = string.Format("{0} {1}", item.FullFontName, formatTag);

                using (regKey)
                {
                    // For system-wide fonts, just the file name is stored. For user fonts, the full path is stored.
                    if (isAdmin)
                    {
                        regKey.SetValue(valueName, item.FileName);
                    }
                    else
                    {
                        regKey.SetValue(valueName, destPath);
                    }
                }

                // Register with GDI
                AddFontResource(destPath);

                // Notify Windows
                NotifyFontChange();

                item.IsInstalled = true;
                item.StatusMessage = "Installed";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static void NotifyFontChange()
        {
            try
            {
                UIntPtr result;
                SendMessageTimeout(
                    (IntPtr)HWND_BROADCAST,
                    WM_FONTCHANGE,
                    UIntPtr.Zero,
                    IntPtr.Zero,
                    SMTO_ABORTIFHUNG,
                    1500,
                    out result);
            }
            catch { }
        }

        public static void RevealInWindowsFonts(string fontFileName)
        {
            OpenSystemFontsFolder();
        }

        public static void OpenSystemFontsFolder()
        {
            try
            {
                Process.Start("explorer.exe", "shell:fonts");
            }
            catch
            {
                try
                {
                    string winFontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                    Process.Start("explorer.exe", winFontsDir);
                }
                catch { }
            }
        }

        public static void RestartAsAdmin()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Process.GetCurrentProcess().MainModule.FileName;
                psi.Verb = "runas";
                psi.UseShellExecute = true;
                Process.Start(psi);
                System.Windows.Forms.Application.Exit();
            }
            catch { }
        }
    }
}
