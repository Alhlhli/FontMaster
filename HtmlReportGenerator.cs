using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FontMaster
{
    public static class HtmlReportGenerator
    {
        public static void GenerateReport(List<FontItem> fonts, string outputPath)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"ar\" dir=\"rtl\" data-theme=\"dark\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine("<title>تقرير الخطوط المكتشفة | FontMaster Pro - العامر للهندسة والإعمار</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(":root {");
            sb.AppendLine("  --bg: #0f172a;");
            sb.AppendLine("  --card: #1e293b;");
            sb.AppendLine("  --card-alt: #172033;");
            sb.AppendLine("  --border: #334155;");
            sb.AppendLine("  --text: #f8fafc;");
            sb.AppendLine("  --text-muted: #94a3b8;");
            sb.AppendLine("  --primary: #6366f1;");
            sb.AppendLine("  --primary-hover: #4f46e5;");
            sb.AppendLine("  --success: #10b981;");
            sb.AppendLine("  --success-bg: rgba(16, 185, 129, 0.15);");
            sb.AppendLine("  --info: #0ea5e9;");
            sb.AppendLine("  --info-bg: rgba(14, 165, 233, 0.15);");
            sb.AppendLine("  --radius: 12px;");
            sb.AppendLine("}");
            sb.AppendLine("[data-theme=\"light\"] {");
            sb.AppendLine("  --bg: #f1f5f9;");
            sb.AppendLine("  --card: #ffffff;");
            sb.AppendLine("  --card-alt: #f8fafc;");
            sb.AppendLine("  --border: #cbd5e1;");
            sb.AppendLine("  --text: #0f172a;");
            sb.AppendLine("  --text-muted: #475569;");
            sb.AppendLine("  --primary: #4f46e5;");
            sb.AppendLine("  --primary-hover: #4338ca;");
            sb.AppendLine("  --success-bg: rgba(16, 185, 129, 0.12);");
            sb.AppendLine("  --info-bg: rgba(14, 165, 233, 0.12);");
            sb.AppendLine("}");
            sb.AppendLine("* { box-sizing: border-box; margin: 0; padding: 0; }");
            sb.AppendLine("body { background: var(--bg); color: var(--text); padding: 24px; transition: background 0.3s ease, color 0.3s ease; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Noto Sans Arabic', sans-serif; }");
            sb.AppendLine(".container { max-width: 1300px; margin: 0 auto; }");
            sb.AppendLine(".header { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 16px; padding-bottom: 20px; border-bottom: 1px solid var(--border); margin-bottom: 24px; }");
            sb.AppendLine(".brand h1 { font-size: 24px; font-weight: 800; color: var(--text); }");
            sb.AppendLine(".brand p { font-size: 13px; color: var(--text-muted); margin-top: 4px; }");
            sb.AppendLine(".company-badge { display: inline-block; background: var(--primary); color: #fff; padding: 3px 10px; border-radius: 20px; font-size: 11px; font-weight: bold; margin-bottom: 6px; }");
            sb.AppendLine(".controls-top { display: flex; gap: 10px; align-items: center; }");
            sb.AppendLine(".btn-tool { background: var(--card); border: 1px solid var(--border); color: var(--text); padding: 8px 16px; border-radius: 8px; cursor: pointer; font-size: 13px; font-weight: 600; display: flex; align-items: center; gap: 6px; transition: 0.2s; }");
            sb.AppendLine(".btn-tool:hover { border-color: var(--primary); color: var(--primary); }");
            sb.AppendLine(".dashboard { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 16px; margin-bottom: 24px; }");
            sb.AppendLine(".stat-card { background: var(--card); border: 1px solid var(--border); border-radius: var(--radius); padding: 16px; }");
            sb.AppendLine(".stat-card .val { font-size: 28px; font-weight: 800; color: var(--primary); }");
            sb.AppendLine(".stat-card .lbl { font-size: 12px; color: var(--text-muted); margin-top: 4px; }");
            sb.AppendLine(".interactive-bar { background: var(--card); border: 1px solid var(--border); border-radius: var(--radius); padding: 18px; margin-bottom: 24px; display: flex; flex-direction: column; gap: 14px; }");
            sb.AppendLine(".input-row { display: flex; gap: 12px; align-items: center; flex-wrap: wrap; }");
            sb.AppendLine(".input-text { flex: 1; min-width: 280px; background: var(--card-alt); border: 1px solid var(--border); border-radius: 8px; padding: 10px 14px; color: var(--text); font-size: 15px; outline: none; }");
            sb.AppendLine(".input-text:focus { border-color: var(--primary); }");
            sb.AppendLine(".slider-box { display: flex; align-items: center; gap: 8px; font-size: 13px; color: var(--text-muted); }");
            sb.AppendLine(".cards-grid { display: grid; grid-template-columns: 1fr; gap: 14px; }");
            sb.AppendLine(".font-card { background: var(--card); border: 1px solid var(--border); border-radius: var(--radius); padding: 18px; transition: transform 0.15s ease, border-color 0.2s; }");
            sb.AppendLine(".font-card:hover { border-color: var(--primary); }");
            sb.AppendLine(".card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; flex-wrap: wrap; gap: 10px; }");
            sb.AppendLine(".font-meta { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }");
            sb.AppendLine(".font-title { font-size: 16px; font-weight: 700; color: var(--text); }");
            sb.AppendLine(".badge { padding: 3px 10px; border-radius: 20px; font-size: 11px; font-weight: 600; }");
            sb.AppendLine(".badge-ttf { background: var(--info-bg); color: var(--info); }");
            sb.AppendLine(".badge-installed { background: var(--success-bg); color: var(--success); }");
            sb.AppendLine(".badge-zip { background: rgba(245, 158, 11, 0.15); color: #f59e0b; }");
            sb.AppendLine(".btn-copy { background: var(--card-alt); border: 1px solid var(--border); color: var(--text); padding: 5px 12px; border-radius: 6px; font-size: 12px; cursor: pointer; transition: 0.2s; }");
            sb.AppendLine(".btn-copy:hover { background: var(--primary); color: #fff; border-color: var(--primary); }");
            sb.AppendLine(".preview-render { padding: 16px; background: var(--card-alt); border-radius: 8px; min-height: 75px; display: flex; align-items: center; font-size: 24px; color: var(--text); overflow-x: auto; word-break: break-word; }");
            sb.AppendLine(".preview-text { display: inline-block; width: 100%; line-height: 1.4; font-family: inherit; }");
            sb.AppendLine(".footer { margin-top: 40px; padding-top: 24px; border-top: 1px solid var(--border); text-align: center; color: var(--text-muted); font-size: 13px; }");
            sb.AppendLine(".toast { position: fixed; bottom: 24px; left: 50%; transform: translateX(-50%); background: #10b981; color: #fff; padding: 10px 20px; border-radius: 20px; font-size: 13px; font-weight: 600; opacity: 0; pointer-events: none; transition: opacity 0.3s; z-index: 999; }");
            sb.AppendLine(".toast.show { opacity: 1; }");

            // Embed fonts with @font-face (base64) - NO charset=utf-8, proper MIME and format()
            int embeddedCount = 0;
            int total = fonts.Count;
            int installedCount = 0;

            for (int i = 0; i < fonts.Count; i++)
            {
                FontItem f = fonts[i];
                if (f.IsInstalled) installedCount++;

                if (File.Exists(f.FilePath) && f.FileSize < 10 * 1024 * 1024 && embeddedCount < 300)
                {
                    try
                    {
                        byte[] fontBytes = File.ReadAllBytes(f.FilePath);
                        string base64 = Convert.ToBase64String(fontBytes);
                        bool isOtf = f.Extension != null && f.Extension.Equals("OTF", StringComparison.OrdinalIgnoreCase);
                        string mime = isOtf ? "font/otf" : "font/ttf";
                        string format = isOtf ? "opentype" : "truetype";
                        string escapedFam = (f.FamilyName ?? string.Empty).Replace("'", "\\'");
                        string escapedFull = (f.FullFontName ?? string.Empty).Replace("'", "\\'");

                        sb.AppendLine(string.Format(
                            "@font-face {{ font-family: 'Font_{0}'; src: local('{1}'), local('{2}'), url('data:{3};base64,{4}') format('{5}'); font-display: swap; }}",
                            i, escapedFam, escapedFull, mime, base64, format));
                        embeddedCount++;
                    }
                    catch { }
                }
            }

            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"container\">");

            // Header
            sb.AppendLine("<div class=\"header\">");
            sb.AppendLine("  <div class=\"brand\">");
            sb.AppendLine("    <div class=\"company-badge\">العامر للهندسة والإعمار • برنامج مجاني 100%</div>");
            sb.AppendLine("    <h1 id=\"title\">تقرير الخطوط المكتشفة | FontMaster Pro</h1>");
            sb.AppendLine("    <p id=\"subtitle\">تم الإنشاء بواسطة مدير ومثبت الخطوط الذكي - العامر للهندسة والإعمار</p>");
            sb.AppendLine("  </div>");
            sb.AppendLine("  <div class=\"controls-top\">");
            sb.AppendLine("    <button class=\"btn-tool\" onclick=\"toggleTheme()\" id=\"btnTheme\">الوضع الفاتح ☀️</button>");
            sb.AppendLine("    <button class=\"btn-tool\" onclick=\"toggleLang()\" id=\"btnLang\">English 🌐</button>");
            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");

            // Dashboard Stats
            int uninstalledCount = total - installedCount;
            sb.AppendLine("<div class=\"dashboard\">");
            sb.AppendLine(string.Format("  <div class=\"stat-card\"><div class=\"val\">{0}</div><div class=\"lbl\" id=\"lblStatTotal\">إجمالي الخطوط المكتشفة</div></div>", total));
            sb.AppendLine(string.Format("  <div class=\"stat-card\"><div class=\"val\" style=\"color: var(--success);\">{0}</div><div class=\"lbl\" id=\"lblStatInstalled\">المثبتة مسبقاً بالنظام</div></div>", installedCount));
            sb.AppendLine(string.Format("  <div class=\"stat-card\"><div class=\"val\" style=\"color: var(--info);\">{0}</div><div class=\"lbl\" id=\"lblStatPending\">المطلوب تثبيتها (جديدة)</div></div>", uninstalledCount));
            sb.AppendLine(string.Format("  <div class=\"stat-card\"><div class=\"val\" style=\"color: #f59e0b;\">{0}</div><div class=\"lbl\" id=\"lblStatDate\">تاريخ التقرير: {1}</div></div>", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm")));
            sb.AppendLine("</div>");

            // Interactive Sample Sentence Input & Size Slider
            sb.AppendLine("<div class=\"interactive-bar\">");
            sb.AppendLine("  <div class=\"input-row\">");
            sb.AppendLine("    <input type=\"text\" id=\"sampleInput\" class=\"input-text\" value=\"أبجد هوز حطي كلمن • Quick Brown Fox • 0123456789\" oninput=\"updateSample(this.value)\" placeholder=\"اكتب أي جملة أو نص لمعاينته في كافة الخطوط فوراً...\">");
            sb.AppendLine("    <input type=\"text\" id=\"searchInput\" class=\"input-text\" style=\"flex: 0.6;\" oninput=\"filterFonts(this.value)\" placeholder=\"بحث باسم الخط...\">");
            sb.AppendLine("    <div class=\"slider-box\">");
            sb.AppendLine("      <span id=\"lblSizeText\">الحجم:</span>");
            sb.AppendLine("      <input type=\"range\" id=\"sizeSlider\" min=\"14\" max=\"60\" value=\"26\" oninput=\"updateFontSize(this.value)\">");
            sb.AppendLine("      <span id=\"sizeVal\">26px</span>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");

            // Cards List
            sb.AppendLine("<div class=\"cards-grid\" id=\"cardsGrid\">");
            for (int i = 0; i < fonts.Count; i++)
            {
                FontItem f = fonts[i];
                string escapedFam = (f.FamilyName ?? string.Empty).Replace("'", "\\'");
                string fontFamStyle = string.Format("'Font_{0}', '{1}', sans-serif", i, escapedFam);
                string statusText = f.IsInstalled ? "مثبت مسبقاً" : "جاهز للتثبيت";
                string statusBadgeClass = f.IsInstalled ? "badge-installed" : "badge-ttf";

                sb.AppendLine(string.Format("<div class=\"font-card\" data-name=\"{0}\">", f.DisplayName.ToLowerInvariant()));
                sb.AppendLine("  <div class=\"card-header\">");
                sb.AppendLine("    <div class=\"font-meta\">");
                sb.AppendLine(string.Format("      <span class=\"font-title\">{0}</span>", f.DisplayName));
                sb.AppendLine(string.Format("      <span class=\"badge {0}\">{1}</span>", statusBadgeClass, statusText));
                sb.AppendLine(string.Format("      <span class=\"badge badge-ttf\">{0}</span>", f.FormatName));
                if (f.IsFromZip)
                {
                    sb.AppendLine(string.Format("      <span class=\"badge badge-zip\">ZIP: {0}</span>", Path.GetFileName(f.ZipSource)));
                }
                sb.AppendLine(string.Format("      <span style=\"font-size: 11px; color: var(--text-muted);\">{0}</span>", f.FormattedSize));
                sb.AppendLine("    </div>");
                sb.AppendLine(string.Format("    <button class=\"btn-copy\" onclick=\"copyText('{0}')\">نسخ الاسم 📋</button>", f.DisplayName.Replace("'", "\\'")));
                sb.AppendLine("  </div>");
                sb.AppendLine(string.Format("  <div class=\"preview-render\" style=\"font-family: {0};\">", fontFamStyle));
                sb.AppendLine(string.Format("    <span class=\"preview-text\" style=\"font-family: {0};\">أبجد هوز حطي كلمن • Quick Brown Fox • 0123456789</span>", fontFamStyle));
                sb.AppendLine("  </div>");
                sb.AppendLine("</div>");
            }
            sb.AppendLine("</div>");

            // Footer
            sb.AppendLine("<div class=\"footer\">");
            sb.AppendLine("  <p><strong>برنامج مدير ومثبت الخطوط الذكي (FontMaster Pro) — برنامج مجاني 100%</strong></p>");
            sb.AppendLine("  <p style=\"margin-top: 6px;\">جميع الحقوق محفوظة © العامر للهندسة والإعمار - Al-Amer for Engineering & Construction</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div id=\"toast\" class=\"toast\">تم نسخ اسم الخط إلى الحافظة!</div>");

            // Interactive JS
            sb.AppendLine("<script>");
            sb.AppendLine("let currentTheme = 'dark';");
            sb.AppendLine("let isArabic = true;");
            sb.AppendLine("function toggleTheme() {");
            sb.AppendLine("  currentTheme = currentTheme === 'dark' ? 'light' : 'dark';");
            sb.AppendLine("  document.documentElement.setAttribute('data-theme', currentTheme);");
            sb.AppendLine("  document.getElementById('btnTheme').textContent = currentTheme === 'dark' ? (isArabic ? 'الوضع الفاتح ☀️' : 'Light Mode ☀️') : (isArabic ? 'الوضع الليلي 🌙' : 'Dark Mode 🌙');");
            sb.AppendLine("}");
            sb.AppendLine("function toggleLang() {");
            sb.AppendLine("  isArabic = !isArabic;");
            sb.AppendLine("  document.documentElement.setAttribute('dir', isArabic ? 'rtl' : 'ltr');");
            sb.AppendLine("  document.documentElement.setAttribute('lang', isArabic ? 'ar' : 'en');");
            sb.AppendLine("  document.getElementById('btnLang').textContent = isArabic ? 'English 🌐' : 'عربي 🌐';");
            sb.AppendLine("  document.getElementById('title').textContent = isArabic ? 'تقرير الخطوط المكتشفة | FontMaster Pro' : 'Discovered Fonts Inventory Report | FontMaster Pro';");
            sb.AppendLine("  document.getElementById('subtitle').textContent = isArabic ? 'تم الإنشاء بواسطة مدير ومثبت الخطوط الذكي - العامر للهندسة والإعمار' : 'Generated by Smart Font Manager - Al-Amer for Engineering & Construction';");
            sb.AppendLine("  document.getElementById('lblStatTotal').textContent = isArabic ? 'إجمالي الخطوط المكتشفة' : 'Total Fonts Discovered';");
            sb.AppendLine("  document.getElementById('lblStatInstalled').textContent = isArabic ? 'المثبتة مسبقاً بالنظام' : 'Already Installed in Windows';");
            sb.AppendLine("  document.getElementById('lblStatPending').textContent = isArabic ? 'المطلوب تثبيتها (جديدة)' : 'Ready to Install (New)';");
            sb.AppendLine("  document.getElementById('searchInput').placeholder = isArabic ? 'بحث باسم الخط...' : 'Filter by font name...';");
            sb.AppendLine("  document.getElementById('sampleInput').placeholder = isArabic ? 'اكتب أي جملة أو نص لمعاينته في كافة الخطوط فوراً...' : 'Type any phrase to preview across all fonts live...';");
            sb.AppendLine("}");
            sb.AppendLine("function updateSample(val) {");
            sb.AppendLine("  const text = val || 'أبجد هوز حطي كلمن • Quick Brown Fox • 0123456789';");
            sb.AppendLine("  const previews = document.querySelectorAll('.preview-text');");
            sb.AppendLine("  previews.forEach(p => p.textContent = text);");
            sb.AppendLine("}");
            sb.AppendLine("function updateFontSize(val) {");
            sb.AppendLine("  document.getElementById('sizeVal').textContent = val + 'px';");
            sb.AppendLine("  const boxes = document.querySelectorAll('.preview-render');");
            sb.AppendLine("  boxes.forEach(b => b.style.fontSize = val + 'px');");
            sb.AppendLine("}");
            sb.AppendLine("function filterFonts(q) {");
            sb.AppendLine("  const term = q.trim().toLowerCase();");
            sb.AppendLine("  const cards = document.querySelectorAll('.font-card');");
            sb.AppendLine("  cards.forEach(card => {");
            sb.AppendLine("    const name = card.getAttribute('data-name');");
            sb.AppendLine("    card.style.display = name.includes(term) ? '' : 'none';");
            sb.AppendLine("  });");
            sb.AppendLine("}");
            sb.AppendLine("function copyText(txt) {");
            sb.AppendLine("  if (navigator.clipboard) {");
            sb.AppendLine("    navigator.clipboard.writeText(txt).then(() => showToast(isArabic ? 'تم نسخ: ' + txt : 'Copied: ' + txt));");
            sb.AppendLine("  } else {");
            sb.AppendLine("    const el = document.createElement('textarea'); el.value = txt; document.body.appendChild(el); el.select(); document.execCommand('copy'); document.body.removeChild(el);");
            sb.AppendLine("    showToast(isArabic ? 'تم نسخ: ' + txt : 'Copied: ' + txt);");
            sb.AppendLine("  }");
            sb.AppendLine("}");
            sb.AppendLine("function showToast(msg) {");
            sb.AppendLine("  const t = document.getElementById('toast');");
            sb.AppendLine("  t.textContent = msg;");
            sb.AppendLine("  t.classList.add('show');");
            sb.AppendLine("  setTimeout(() => t.classList.remove('show'), 2000);");
            sb.AppendLine("}");
            sb.AppendLine("</script>");

            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(true));
        }
    }
}
