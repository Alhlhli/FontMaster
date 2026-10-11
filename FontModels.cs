using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace FontMaster
{
    public class FontItem
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string FamilyName { get; set; }
        public string SubFamily { get; set; }
        public string FullFontName { get; set; }
        public long FileSize { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsFromZip { get; set; }
        public string ZipSource { get; set; }
        public bool IsSelected { get; set; }
        public string FormatName { get; set; }
        public string StatusMessage { get; set; }

        public FontItem()
        {
            FamilyName = string.Empty;
            SubFamily = "Regular";
            FullFontName = string.Empty;
            FormatName = "TTF";
            StatusMessage = string.Empty;
            IsSelected = false;
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(FamilyName))
                {
                    if (!string.IsNullOrEmpty(SubFamily) && !SubFamily.Equals("Regular", StringComparison.OrdinalIgnoreCase))
                    {
                        return FamilyName + " - " + SubFamily;
                    }
                    return FamilyName;
                }
                return Path.GetFileNameWithoutExtension(FileName);
            }
        }

        public string FormattedSize
        {
            get
            {
                return FormatBytes(FileSize);
            }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes >= 1048576)
                return string.Format("{0:0.##} MB", (double)bytes / 1048576);
            if (bytes >= 1024)
                return string.Format("{0:0.##} KB", (double)bytes / 1024);
            return bytes + " B";
        }
    }

    /// <summary>
    /// Reads TrueType and OpenType font names directly from font file headers.
    /// Fast, non-locking, and handles Arabic / Unicode font names.
    /// </summary>
    public static class FontHeaderReader
    {
        public static bool TryGetFontInfo(string filePath, out string familyName, out string subFamily, out string fullName)
        {
            familyName = null;
            subFamily = null;
            fullName = null;

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    if (fs.Length < 12)
                        return false;

                    uint sfntVersion = ReadUInt32BE(br);
                    // Check for standard TTF (0x00010000 or 'true'), OpenType ('OTTO'), or TTC ('ttcf')
                    if (sfntVersion == 0x74746366) // 'ttcf' (TrueType Collection)
                    {
                        uint majorVersion = ReadUInt16BE(br);
                        uint minorVersion = ReadUInt16BE(br);
                        uint numFonts = ReadUInt32BE(br);
                        if (numFonts > 0)
                        {
                            uint firstOffset = ReadUInt32BE(br);
                            fs.Seek(firstOffset, SeekOrigin.Begin);
                            sfntVersion = ReadUInt32BE(br);
                        }
                    }

                    ushort numTables = ReadUInt16BE(br);
                    ushort searchRange = ReadUInt16BE(br);
                    ushort entrySelector = ReadUInt16BE(br);
                    ushort rangeShift = ReadUInt16BE(br);

                    long nameTableOffset = -1;
                    uint nameTableLength = 0;

                    for (int i = 0; i < numTables; i++)
                    {
                        uint tag = ReadUInt32BE(br);
                        uint checksum = ReadUInt32BE(br);
                        uint offset = ReadUInt32BE(br);
                        uint length = ReadUInt32BE(br);

                        // Tag 'name' = 0x6E616D65
                        if (tag == 0x6E616D65)
                        {
                            nameTableOffset = offset;
                            nameTableLength = length;
                            break;
                        }
                    }

                    if (nameTableOffset <= 0 || nameTableOffset + nameTableLength > fs.Length)
                        return false;

                    fs.Seek(nameTableOffset, SeekOrigin.Begin);

                    ushort format = ReadUInt16BE(br);
                    ushort count = ReadUInt16BE(br);
                    ushort stringOffset = ReadUInt16BE(br);

                    long storageOffset = nameTableOffset + stringOffset;

                    for (int i = 0; i < count; i++)
                    {
                        ushort platformId = ReadUInt16BE(br);
                        ushort encodingId = ReadUInt16BE(br);
                        ushort languageId = ReadUInt16BE(br);
                        ushort nameId = ReadUInt16BE(br);
                        ushort length = ReadUInt16BE(br);
                        ushort offset = ReadUInt16BE(br);

                        long currentRecordPos = fs.Position;

                        if (nameId == 1 || nameId == 2 || nameId == 4)
                        {
                            long stringAbsolutePos = storageOffset + offset;
                            if (stringAbsolutePos + length <= fs.Length)
                            {
                                fs.Seek(stringAbsolutePos, SeekOrigin.Begin);
                                byte[] rawBytes = br.ReadBytes(length);
                                string val = DecodeFontNameString(rawBytes, platformId, encodingId);

                                if (!string.IsNullOrEmpty(val))
                                {
                                    if (nameId == 1 && (familyName == null || (platformId == 3 && languageId == 0x0409) || (platformId == 3 && languageId == 0x0401)))
                                        familyName = val.Trim();
                                    else if (nameId == 2 && (subFamily == null || (platformId == 3 && languageId == 0x0409)))
                                        subFamily = val.Trim();
                                    else if (nameId == 4 && (fullName == null || (platformId == 3 && languageId == 0x0409)))
                                        fullName = val.Trim();
                                }
                            }
                        }

                        fs.Seek(currentRecordPos, SeekOrigin.Begin);
                    }

                    return !string.IsNullOrEmpty(familyName);
                }
            }
            catch
            {
                return false;
            }
        }

        private static string DecodeFontNameString(byte[] bytes, ushort platformId, ushort encodingId)
        {
            try
            {
                // Platform 3 = Windows (UTF-16BE)
                // Platform 0 = Unicode (UTF-16BE)
                if (platformId == 3 || platformId == 0)
                {
                    return Encoding.BigEndianUnicode.GetString(bytes);
                }
                // Platform 1 = Macintosh (Mac Roman or ASCII)
                if (platformId == 1)
                {
                    return Encoding.UTF8.GetString(bytes);
                }
                return Encoding.Default.GetString(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static ushort ReadUInt16BE(BinaryReader br)
        {
            byte[] b = br.ReadBytes(2);
            if (b.Length < 2) return 0;
            return (ushort)((b[0] << 8) | b[1]);
        }

        private static uint ReadUInt32BE(BinaryReader br)
        {
            byte[] b = br.ReadBytes(4);
            if (b.Length < 4) return 0;
            return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | (uint)b[3];
        }
    }
}
