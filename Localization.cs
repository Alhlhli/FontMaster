using System;
using System.Collections.Generic;

namespace FontMaster
{
    public static class Localization
    {
        public static bool IsArabic = true;
        public static event Action LanguageChanged;

        private static readonly Dictionary<string, string> Ar = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> En = new Dictionary<string, string>();

        static Localization()
        {
            // App Branding
            Add("AppTitle", "مدير ومثبت الخطوط الذكي", "Smart Font Manager Pro");
            Add("AppSubtitle", "استكشاف، فحص، معاينة وتثبيت الخطوط بضغطة زر واحدة", "Explore, Inspect, Preview & Install Fonts in 1-Click");

            // Header & Tools
            Add("ThemeDark", "الوضع الليلي 🌙", "Dark Theme 🌙");
            Add("ThemeLight", "الوضع الفاتح ☀️", "Light Theme ☀️");
            Add("LangToggle", "English 🌐", "عربي 🌐");
            Add("AdminMode", "صلاحيات مسؤول", "Administrator Mode");
            Add("UserMode", "وضع المستخدم", "User Mode");
            Add("OpenWindowsFonts", "فتح مجلد خطوط ويندوز", "Open Windows Fonts Folder");

            // Tabs / Navigation
            Add("TabScanner", "استكشاف وتثبيت الخطوط", "Explore & Install Fonts");
            Add("TabSystemFonts", "معاين خطوط النظام الشامل", "System Fonts Inspector");
            Add("TabAbout", "معلومات وإرشادات", "About & Shortcuts");

            // Scan Bar
            Add("ScanFolderLabel", "مجلد البحث:", "Scan Folder:");
            Add("Browse", "تصفح...", "Browse...");
            Add("IncludeSubfolders", "تضمين المجلدات الفرعية", "Include Subfolders");
            Add("ScanButton", "استكشاف الخطوط 🔍", "Scan Fonts 🔍");
            Add("DropHint", "يمكنك سحب وإفلات المجلدات أو ملفات الخطوط أو ملفات ZIP هنا مباشرة", "Drag & Drop folders, font files, or ZIP archives here directly");

            // Dashboard Stats
            Add("StatTotal", "إجمالي الخطوط المكتشفة", "Total Discovered");
            Add("StatInstalled", "المثبتة مسبقاً بالنظام", "Already Installed");
            Add("StatToInstall", "المطلوب تثبيتها (جديدة)", "Ready to Install");
            Add("StatSelected", "المحددة للتثبيت", "Currently Selected");

            // Action Bar
            Add("SelectAll", "تحديد الكل", "Select All");
            Add("DeselectAll", "إلغاء التحديد", "Deselect All");
            Add("SelectUninstalledOnly", "تحديد غير المثبت فقط ✨", "Select Uninstalled Only ✨");
            Add("InstallSelected", "تثبيت الخطوط المحددة 🚀", "Install Selected Fonts 🚀");
            Add("SearchPlaceholder", "بحث باسم الخط أو الملف...", "Search by font or file name...");

            // Filters
            Add("FilterAll", "الكل", "All");
            Add("FilterUninstalled", "غير مثبتة", "Not Installed");
            Add("FilterInstalled", "مثبتة", "Installed");
            Add("FilterTTF", "TrueType (TTF)", "TrueType (TTF)");
            Add("FilterOTF", "OpenType (OTF)", "OpenType (OTF)");
            Add("FilterZip", "من أرشيف ZIP", "From ZIP");

            // Grid Columns
            Add("ColCheck", "اختيار", "Select");
            Add("ColStatus", "الحالة", "Status");
            Add("ColName", "اسم الخط", "Font Name");
            Add("ColType", "النوع", "Type");
            Add("ColSize", "الحجم", "Size");
            Add("ColSource", "المصدر", "Source");
            Add("ColFile", "الملف", "File");

            // Status Values
            Add("StatusInstalled", "مثبت مسبقاً", "Installed");
            Add("StatusReady", "غير مثبت (جاهز)", "Not Installed");
            Add("StatusInstalling", "جارٍ التثبيت...", "Installing...");
            Add("StatusSuccess", "تم التثبيت بنجاح", "Installed Successfully");
            Add("StatusFailed", "فشل التثبيت", "Failed");

            // Preview Panel
            Add("PreviewTitle", "معاينة الخط الحيّة", "Live Font Preview");
            Add("SampleTextLabel", "نص المعاينة المخصص:", "Sample Preview Text:");
            Add("DefaultArabicSample", "أبجد هوز حطي كلمن • Quick Brown Fox • 0123456789", "أبجد هوز حطي كلمن • Quick Brown Fox • 0123456789");
            Add("FontSizeLabel", "حجم المعاينة:", "Font Size:");
            Add("RevealInExplorer", "استعراض في مجلد الخطوط 📁", "Reveal in Windows Fonts 📁");
            Add("InstallThisFont", "تثبيت هذا الخط فوراً", "Install This Font Now");
            Add("FontDetails", "تفاصيل الخط المكتشف", "Font Metadata Details");
            Add("FamilyLabel", "العائلة:", "Family:");
            Add("StyleLabel", "النمط:", "Style:");
            Add("FormatLabel", "الصيغة:", "Format:");
            Add("FileSizeLabel", "حجم الملف:", "File Size:");
            Add("FilePathLabel", "المسار الكامل:", "Full Path:");
            Add("ZipSourceLabel", "مستخرج من ZIP:", "Extracted From ZIP:");

            // System Fonts Viewer Tab
            Add("SysFontTitle", "معاين جميع الخطوط المثبتة على جهازك", "System-Wide Installed Fonts Live Inspector");
            Add("SysFontDesc", "اكتب أي كلمة أو جملة لمعاينتها فورياً عبر مئات الخطوط المثبتة في ويندوز ومقارنتها بدقة", "Type any phrase to preview it across all Windows installed fonts simultaneously");
            Add("SysSearchFont", "تصفية الخطوط بالاسم:", "Filter by Font Family Name:");
            Add("CopyFontName", "نسخ الاسم", "Copy Name");
            Add("Copied", "تم نسخ اسم الخط إلى الحافظة!", "Font name copied to clipboard!");
            Add("TotalSystemFonts", "إجمالي خطوط النظام المثبتة:", "Total System Fonts:");

            // Messages & Dialogs
            Add("NoFontsSelected", "يرجى تحديد خط واحد على الأقل لتثبيته.", "Please select at least one font to install.");
            Add("InstallComplete", "اكتملت عملية التثبيت!\nتم تثبيت: {0} خط\nفشل: {1} خط\nتخطي: {2} خط", "Installation Complete!\nSuccessfully installed: {0}\nFailed: {1}\nSkipped: {2}");
            Add("Scanning", "جارٍ استكشاف الخطوط في المجلد...", "Scanning folder for fonts...");
            Add("ScanDone", "اكتمل البحث! تم العثور على {0} خط.", "Scan completed! Found {0} fonts.");
            Add("ElevationPrompt", "لتثبيت الخطوط لكافة مستخدمي النظام ينصح بالتشغيل كمسؤول.\nهل ترغب بإعادة تشغيل البرنامج كمسؤول؟", "To install fonts system-wide, Administrator privileges are recommended.\nRestart as Administrator now?");
            Add("SelectDirectory", "اختر مجلداً لاستكشاف الخطوط بداخله:", "Select a folder to scan for fonts:");
            Add("ExportList", "تصدير قائمة الخطوط (CSV)", "Export Font List (CSV)");
            Add("ExportSuccess", "تم تصدير القائمة بنجاح إلى: ", "Font list successfully exported to: ");

            // New Features
            Add("PreviewFonts", "معاينة الخطوط", "Preview Fonts");
            Add("ExportReport", "تصدير تقرير 📊", "Export Report 📊");
            Add("ExportCsv", "تصدير ملف CSV (إكسل) 📄", "Export CSV (Excel) 📄");
            Add("ExportHtml", "تصدير تقرير تفاعلي HTML 🌐", "Export Interactive HTML 🌐");
            Add("CopyrightTitle", "العامر للهندسة والإعمار", "Al-Amer for Engineering & Construction");
            Add("CopyrightText", "جميع الحقوق محفوظة © العامر للهندسة والإعمار", "All Rights Reserved © Al-Amer for Engineering & Construction");
            Add("FreeSoftware", "★ مجاني 100% | 100% Free", "★ 100% Free Software");
            Add("DragDropBanner", "📥 اسحب وأفلت المجلدات أو ملفات الخطوط (TTF/OTF) أو ملفات ZIP هنا مباشرة", "📥 Drag & Drop folders, font files (TTF/OTF), or ZIP archives directly here");
        }

        private static void Add(string key, string arabic, string english)
        {
            Ar[key] = arabic;
            En[key] = english;
        }

        public static string Get(string key)
        {
            Dictionary<string, string> dict = IsArabic ? Ar : En;
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            if (En.ContainsKey(key))
            {
                return En[key];
            }
            return key;
        }

        public static void ToggleLanguage()
        {
            IsArabic = !IsArabic;
            if (LanguageChanged != null)
            {
                LanguageChanged();
            }
        }
    }
}
