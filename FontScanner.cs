using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;

namespace FontMaster
{
    public class FontScanner
    {
        private HashSet<string> _installedFontNames;
        private HashSet<string> _installedFileNames;
        private string _tempZipDir;

        public FontScanner()
        {
            _tempZipDir = Path.Combine(Path.GetTempPath(), "FontMaster_Cache");
            RefreshInstalledFontsCache();
        }

        public void RefreshInstalledFontsCache()
        {
            _installedFontNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _installedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Query System InstalledFontCollection
            try
            {
                using (InstalledFontCollection ifc = new InstalledFontCollection())
                {
                    foreach (FontFamily ff in ifc.Families)
                    {
                        if (!string.IsNullOrEmpty(ff.Name))
                        {
                            _installedFontNames.Add(ff.Name);
                        }
                    }
                }
            }
            catch { }

            // 2. Query HKLM Registry (System-wide fonts)
            ReadFontsFromRegistry(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts");

            // 3. Query HKCU Registry (User fonts in Windows 10/11)
            ReadFontsFromRegistry(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts");

            // 4. Query Windows\Fonts directory directly
            try
            {
                string winFonts = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                if (Directory.Exists(winFonts))
                {
                    string[] fontFiles = Directory.GetFiles(winFonts);
                    foreach (string f in fontFiles)
                    {
                        _installedFileNames.Add(Path.GetFileName(f));
                    }
                }
            }
            catch { }

            // 5. Query LocalAppData Fonts (User fonts)
            try
            {
                string userFonts = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts");
                if (Directory.Exists(userFonts))
                {
                    string[] fontFiles = Directory.GetFiles(userFonts);
                    foreach (string f in fontFiles)
                    {
                        _installedFileNames.Add(Path.GetFileName(f));
                    }
                }
            }
            catch { }
        }

        private void ReadFontsFromRegistry(RegistryKey root, string subKey)
        {
            try
            {
                using (RegistryKey key = root.OpenSubKey(subKey))
                {
                    if (key != null)
                    {
                        string[] valueNames = key.GetValueNames();
                        foreach (string vn in valueNames)
                        {
                            // Value names usually look like "Arial (TrueType)" or "Segoe UI Bold (TrueType)"
                            string cleanName = CleanFontRegistryName(vn);
                            if (!string.IsNullOrEmpty(cleanName))
                            {
                                _installedFontNames.Add(cleanName);
                            }

                            object val = key.GetValue(vn);
                            if (val != null)
                            {
                                string fileVal = Path.GetFileName(val.ToString());
                                if (!string.IsNullOrEmpty(fileVal))
                                {
                                    _installedFileNames.Add(fileVal);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private string CleanFontRegistryName(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            int parenIdx = raw.IndexOf('(');
            if (parenIdx > 0)
            {
                return raw.Substring(0, parenIdx).Trim();
            }
            return raw.Trim();
        }

        public bool IsFontInstalled(string familyName, string fullName, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && _installedFileNames.Contains(Path.GetFileName(fileName)))
                return true;

            if (!string.IsNullOrEmpty(familyName) && _installedFontNames.Contains(familyName))
                return true;

            if (!string.IsNullOrEmpty(fullName) && _installedFontNames.Contains(fullName))
                return true;

            return false;
        }

        public List<FontItem> Scan(string rootDirectory, bool includeSubdirs, Action<int, string> progressCallback)
        {
            List<FontItem> results = new List<FontItem>();

            if (string.IsNullOrEmpty(rootDirectory) || !Directory.Exists(rootDirectory))
                return results;

            SearchOption opt = includeSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            // 1. Scan direct font files
            string[] extensions = new string[] { "*.ttf", "*.otf", "*.ttc", "*.fon" };
            List<string> candidateFiles = new List<string>();

            foreach (string ext in extensions)
            {
                try
                {
                    string[] files = Directory.GetFiles(rootDirectory, ext, opt);
                    candidateFiles.AddRange(files);
                }
                catch { }
            }

            int total = candidateFiles.Count;
            int current = 0;

            foreach (string filePath in candidateFiles)
            {
                current++;
                if (progressCallback != null)
                {
                    progressCallback(current, Path.GetFileName(filePath));
                }

                FontItem item = CreateFontItemFromFile(filePath, false, null);
                if (item != null)
                {
                    results.Add(item);
                }
            }

            // 2. Scan ZIP archives
            try
            {
                string[] zipFiles = Directory.GetFiles(rootDirectory, "*.zip", opt);
                foreach (string zipPath in zipFiles)
                {
                    try
                    {
                        if (progressCallback != null)
                        {
                            progressCallback(current, "ZIP: " + Path.GetFileName(zipPath));
                        }

                        List<FontItem> zipItems = ExtractFontsFromZip(zipPath);
                        results.AddRange(zipItems);
                    }
                    catch { }
                }
            }
            catch { }

            return results;
        }

        private List<FontItem> ExtractFontsFromZip(string zipPath)
        {
            List<FontItem> list = new List<FontItem>();

            if (!Directory.Exists(_tempZipDir))
            {
                Directory.CreateDirectory(_tempZipDir);
            }

            string zipName = Path.GetFileNameWithoutExtension(zipPath);
            string targetDir = Path.Combine(_tempZipDir, zipName + "_" + Guid.NewGuid().ToString("N").Substring(0, 8));

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string ext = Path.GetExtension(entry.FullName).ToLowerInvariant();
                        if (ext == ".ttf" || ext == ".otf" || ext == ".ttc" || ext == ".fon")
                        {
                            if (!Directory.Exists(targetDir))
                                Directory.CreateDirectory(targetDir);

                            string destFile = Path.Combine(targetDir, entry.Name);
                            // Avoid collision
                            if (File.Exists(destFile))
                            {
                                destFile = Path.Combine(targetDir, Guid.NewGuid().ToString("N").Substring(0, 4) + "_" + entry.Name);
                            }

                            entry.ExtractToFile(destFile, true);

                            FontItem item = CreateFontItemFromFile(destFile, true, zipPath);
                            if (item != null)
                            {
                                list.Add(item);
                            }
                        }
                    }
                }
            }
            catch { }

            return list;
        }

        public FontItem CreateFontItemFromFile(string filePath, bool isFromZip, string zipSource)
        {
            try
            {
                FileInfo fi = new FileInfo(filePath);
                if (!fi.Exists) return null;

                string ext = fi.Extension.ToUpperInvariant().TrimStart('.');
                string familyName = null;
                string subFamily = null;
                string fullName = null;

                bool parsed = FontHeaderReader.TryGetFontInfo(filePath, out familyName, out subFamily, out fullName);

                if (!parsed || string.IsNullOrEmpty(familyName))
                {
                    // Fallback using PrivateFontCollection
                    try
                    {
                        using (PrivateFontCollection pfc = new PrivateFontCollection())
                        {
                            pfc.AddFontFile(filePath);
                            if (pfc.Families.Length > 0)
                            {
                                familyName = pfc.Families[0].Name;
                            }
                        }
                    }
                    catch { }
                }

                if (string.IsNullOrEmpty(familyName))
                {
                    familyName = Path.GetFileNameWithoutExtension(fi.Name);
                }

                bool isInstalled = IsFontInstalled(familyName, fullName, fi.Name);

                FontItem item = new FontItem();
                item.FilePath = filePath;
                item.FileName = fi.Name;
                item.Extension = ext;
                item.FamilyName = familyName;
                item.SubFamily = string.IsNullOrEmpty(subFamily) ? "Regular" : subFamily;
                item.FullFontName = string.IsNullOrEmpty(fullName) ? familyName : fullName;
                item.FileSize = fi.Length;
                item.IsInstalled = isInstalled;
                item.IsFromZip = isFromZip;
                item.ZipSource = zipSource;
                item.FormatName = ext;
                item.StatusMessage = isInstalled ? "Installed" : "Ready";
                item.IsSelected = !isInstalled; // Auto-select uninstalled fonts by default!

                return item;
            }
            catch
            {
                return null;
            }
        }

        public void CleanUpTempCache()
        {
            try
            {
                if (Directory.Exists(_tempZipDir))
                {
                    Directory.Delete(_tempZipDir, true);
                }
            }
            catch { }
        }
    }
}
