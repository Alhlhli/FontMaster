using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FontMaster
{
    public static class ThemeManager
    {
        public static bool IsDarkMode = true;
        public static event Action ThemeChanged;

        // Dark Palette
        public static readonly Color DarkBg = Color.FromArgb(15, 23, 42);          // Deep Slate #0F172A
        public static readonly Color DarkCard = Color.FromArgb(30, 41, 59);        // Slate #1E293B
        public static readonly Color DarkCardAlt = Color.FromArgb(24, 33, 47);
        public static readonly Color DarkBorder = Color.FromArgb(51, 65, 85);       // Border #334155
        public static readonly Color DarkText = Color.FromArgb(248, 250, 252);     // Light text #F8FAFC
        public static readonly Color DarkTextMuted = Color.FromArgb(148, 163, 184); // Muted #94A3B8
        public static readonly Color DarkGridHeader = Color.FromArgb(30, 41, 59);
        public static readonly Color DarkGridRow = Color.FromArgb(15, 23, 42);
        public static readonly Color DarkGridRowAlt = Color.FromArgb(22, 31, 46);
        public static readonly Color DarkGridSelect = Color.FromArgb(49, 46, 129);

        // Light Palette
        public static readonly Color LightBg = Color.FromArgb(243, 244, 246);      // Clean soft gray #F3F4F6
        public static readonly Color LightCard = Color.FromArgb(255, 255, 255);    // Pure White #FFFFFF
        public static readonly Color LightCardAlt = Color.FromArgb(249, 250, 251); // #F9FAFB
        public static readonly Color LightBorder = Color.FromArgb(209, 213, 219);  // Clean visible border #D1D5DB
        public static readonly Color LightText = Color.FromArgb(17, 24, 39);       // Deep charcoal #111827
        public static readonly Color LightTextMuted = Color.FromArgb(75, 85, 99);  // High-contrast muted text #4B5563
        public static readonly Color LightGridHeader = Color.FromArgb(243, 244, 246);
        public static readonly Color LightGridRow = Color.FromArgb(255, 255, 255);
        public static readonly Color LightGridRowAlt = Color.FromArgb(249, 250, 251);
        public static readonly Color LightGridSelect = Color.FromArgb(224, 231, 255);

        // Brand Accents
        public static readonly Color Primary = Color.FromArgb(99, 102, 241);       // Indigo #6366F1
        public static readonly Color PrimaryHover = Color.FromArgb(79, 70, 229);   // #4F46E5
        public static readonly Color Success = Color.FromArgb(16, 185, 129);       // Emerald #10B981
        public static readonly Color SuccessBg = Color.FromArgb(6, 78, 59);
        public static readonly Color SuccessBgLight = Color.FromArgb(209, 250, 229);
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);      // Amber #F59E0B
        public static readonly Color Danger = Color.FromArgb(239, 68, 68);         // Red #EF4444
        public static readonly Color Info = Color.FromArgb(14, 165, 233);          // Sky #0EA5E9

        // Dynamic getters
        public static Color FormBg { get { return IsDarkMode ? DarkBg : LightBg; } }
        public static Color CardBg { get { return IsDarkMode ? DarkCard : LightCard; } }
        public static Color CardAltBg { get { return IsDarkMode ? DarkCardAlt : LightCardAlt; } }
        public static Color BorderColor { get { return IsDarkMode ? DarkBorder : LightBorder; } }
        public static Color TextColor { get { return IsDarkMode ? DarkText : LightText; } }
        public static Color TextMutedColor { get { return IsDarkMode ? DarkTextMuted : LightTextMuted; } }
        public static Color GridHeaderBg { get { return IsDarkMode ? DarkGridHeader : LightGridHeader; } }
        public static Color GridRowBg { get { return IsDarkMode ? DarkGridRow : LightGridRow; } }
        public static Color GridRowAltBg { get { return IsDarkMode ? DarkGridRowAlt : LightGridRowAlt; } }
        public static Color GridSelectBg { get { return IsDarkMode ? DarkGridSelect : LightGridSelect; } }

        public static void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
            if (ThemeChanged != null)
            {
                ThemeChanged();
            }
        }

        public static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top Left
            path.AddArc(arc, 180, 90);

            // Top Right
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom Right
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom Left
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void DrawBadge(Graphics g, Rectangle bounds, string text, Color bgColor, Color textColor, Font font)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = CreateRoundedPath(bounds, bounds.Height / 2))
            {
                using (SolidBrush brush = new SolidBrush(bgColor))
                {
                    g.FillPath(brush, path);
                }
            }

            using (SolidBrush brush = new SolidBrush(textColor))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(text, font, brush, bounds, sf);
            }
        }
    }
}
