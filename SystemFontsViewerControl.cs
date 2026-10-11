using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace FontMaster
{
    public class SystemFontsViewerControl : UserControl
    {
        private TextBox _txtSample;
        private TextBox _txtSearch;
        private TrackBar _trackFontSize;
        private Label _lblFontSizeValue;
        private Label _lblTotalFonts;
        private Panel _topBar;
        private Panel _cardsContainer;
        private List<string> _allFontFamilies;
        private List<string> _filteredFamilies;
        private int _sampleFontSize = 26;
        private VScrollBar _vScrollBar;
        private int _scrollOffset = 0;
        private int _itemHeight = 110;

        public SystemFontsViewerControl()
        {
            _allFontFamilies = new List<string>();
            _filteredFamilies = new List<string>();

            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            InitializeComponents();
            LoadInstalledFontFamilies();

            ThemeManager.ThemeChanged += OnThemeOrLangChanged;
            Localization.LanguageChanged += OnThemeOrLangChanged;
        }

        private void InitializeComponents()
        {
            SuspendLayout();

            _topBar = new Panel();
            _topBar.Dock = DockStyle.Top;
            _topBar.Height = 115;
            _topBar.Padding = new Padding(12, 10, 12, 10);

            // Row 1: Search & Stats
            Label lblSearch = new Label();
            lblSearch.Name = "lblSearch";
            lblSearch.Text = Localization.Get("SysSearchFont");
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(12, 12);
            lblSearch.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            _txtSearch = new TextBox();
            _txtSearch.Location = new Point(180, 9);
            _txtSearch.Width = 240;
            _txtSearch.Font = new Font("Segoe UI", 9.5f);
            _txtSearch.TextChanged += delegate { ApplyFilter(); };

            _lblTotalFonts = new Label();
            _lblTotalFonts.AutoSize = true;
            _lblTotalFonts.Location = new Point(440, 12);
            _lblTotalFonts.Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            // Row 2: Sample text input & Font size trackbar
            Label lblSample = new Label();
            lblSample.Name = "lblSample";
            lblSample.Text = Localization.Get("SampleTextLabel");
            lblSample.AutoSize = true;
            lblSample.Location = new Point(12, 48);
            lblSample.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            _txtSample = new TextBox();
            _txtSample.Location = new Point(180, 45);
            _txtSample.Width = 480;
            _txtSample.Font = new Font("Segoe UI", 11f);
            _txtSample.Text = Localization.IsArabic
                ? "بِسْمِ اللَّـهِ الرَّحْمَـٰنِ الرَّحِيمِ - خط عربي رائع وجذاب 1234567890"
                : "The quick brown fox jumps over the lazy dog 1234567890";
            _txtSample.TextChanged += delegate { InvalidateCards(); };

            Label lblSize = new Label();
            lblSize.Name = "lblSize";
            lblSize.Text = Localization.Get("FontSizeLabel");
            lblSize.AutoSize = true;
            lblSize.Location = new Point(680, 48);
            lblSize.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            _trackFontSize = new TrackBar();
            _trackFontSize.Location = new Point(780, 42);
            _trackFontSize.Width = 140;
            _trackFontSize.Minimum = 14;
            _trackFontSize.Maximum = 60;
            _trackFontSize.Value = _sampleFontSize;
            _trackFontSize.TickStyle = TickStyle.None;
            _trackFontSize.ValueChanged += delegate
            {
                _sampleFontSize = _trackFontSize.Value;
                _lblFontSizeValue.Text = _sampleFontSize + " pt";
                _itemHeight = Math.Max(100, _sampleFontSize * 2 + 50);
                UpdateScrollBar();
                InvalidateCards();
            };

            _lblFontSizeValue = new Label();
            _lblFontSizeValue.Text = _sampleFontSize + " pt";
            _lblFontSizeValue.AutoSize = true;
            _lblFontSizeValue.Location = new Point(925, 48);
            _lblFontSizeValue.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            // Quick Preset Buttons
            Button btnArPreset1 = CreatePresetButton("عربي تجريبي", "أبجد هوز حطي كلمن سعفص قرشت ثخذ ضظغ");
            btnArPreset1.Location = new Point(180, 80);
            Button btnArPreset2 = CreatePresetButton("شعر وأمثال", "الخيل والليل والبيداء تعرفني والسيف والرمح والقرطاس والقلم");
            btnArPreset2.Location = new Point(275, 80);
            Button btnEnPreset = CreatePresetButton("English Pangram", "Sphinx of black quartz, judge my vow! 12345");
            btnEnPreset.Location = new Point(370, 80);
            Button btnNumbers = CreatePresetButton("أرقام ورموز", "0 1 2 3 4 5 6 7 8 9 - ٪ & @ # $ € £ ¥");
            btnNumbers.Location = new Point(495, 80);

            _topBar.Controls.Add(lblSearch);
            _topBar.Controls.Add(_txtSearch);
            _topBar.Controls.Add(_lblTotalFonts);
            _topBar.Controls.Add(lblSample);
            _topBar.Controls.Add(_txtSample);
            _topBar.Controls.Add(lblSize);
            _topBar.Controls.Add(_trackFontSize);
            _topBar.Controls.Add(_lblFontSizeValue);
            _topBar.Controls.Add(btnArPreset1);
            _topBar.Controls.Add(btnArPreset2);
            _topBar.Controls.Add(btnEnPreset);
            _topBar.Controls.Add(btnNumbers);

            // Container Panel for Cards with virtual scrolling
            _cardsContainer = new DoubleBufferedPanel();
            _cardsContainer.Dock = DockStyle.Fill;
            _cardsContainer.Paint += CardsContainer_Paint;
            _cardsContainer.MouseClick += CardsContainer_MouseClick;
            _cardsContainer.MouseWheel += CardsContainer_MouseWheel;
            _cardsContainer.Resize += delegate { UpdateScrollBar(); InvalidateCards(); };

            _vScrollBar = new VScrollBar();
            _vScrollBar.Dock = DockStyle.Right;
            _vScrollBar.Width = 16;
            _vScrollBar.ValueChanged += delegate
            {
                _scrollOffset = _vScrollBar.Value;
                InvalidateCards();
            };

            Controls.Add(_cardsContainer);
            Controls.Add(_vScrollBar);
            Controls.Add(_topBar);

            ApplyThemeAndLang();
            ResumeLayout(false);
        }

        private Button CreatePresetButton(string label, string sampleText)
        {
            Button btn = new Button();
            btn.Text = label;
            btn.Size = new Size(90, 26);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 8f);
            btn.Cursor = Cursors.Hand;
            btn.Click += delegate
            {
                _txtSample.Text = sampleText;
                InvalidateCards();
            };
            return btn;
        }

        public void LoadInstalledFontFamilies()
        {
            _allFontFamilies.Clear();
            try
            {
                using (InstalledFontCollection ifc = new InstalledFontCollection())
                {
                    foreach (FontFamily ff in ifc.Families)
                    {
                        if (!string.IsNullOrEmpty(ff.Name))
                        {
                            _allFontFamilies.Add(ff.Name);
                        }
                    }
                }
            }
            catch { }

            _allFontFamilies.Sort(StringComparer.OrdinalIgnoreCase);
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string q = _txtSearch.Text.Trim();
            _filteredFamilies.Clear();

            if (string.IsNullOrEmpty(q))
            {
                _filteredFamilies.AddRange(_allFontFamilies);
            }
            else
            {
                foreach (string name in _allFontFamilies)
                {
                    if (name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _filteredFamilies.Add(name);
                    }
                }
            }

            _lblTotalFonts.Text = string.Format("{0} {1} / {2}",
                Localization.Get("TotalSystemFonts"),
                _filteredFamilies.Count,
                _allFontFamilies.Count);

            UpdateScrollBar();
            InvalidateCards();
        }

        private void UpdateScrollBar()
        {
            int totalHeight = _filteredFamilies.Count * _itemHeight;
            int viewHeight = _cardsContainer.Height;

            if (totalHeight > viewHeight && viewHeight > 0)
            {
                _vScrollBar.Visible = true;
                _vScrollBar.Maximum = Math.Max(0, totalHeight - viewHeight + _itemHeight);
                _vScrollBar.LargeChange = viewHeight;
                _vScrollBar.SmallChange = _itemHeight;
            }
            else
            {
                _vScrollBar.Visible = false;
                _vScrollBar.Value = 0;
                _scrollOffset = 0;
            }
        }

        private void CardsContainer_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_vScrollBar.Visible)
            {
                int newScroll = _scrollOffset - (e.Delta / 120) * (_itemHeight);
                newScroll = Math.Max(0, Math.Min(newScroll, _vScrollBar.Maximum));
                _vScrollBar.Value = newScroll;
                _scrollOffset = newScroll;
                InvalidateCards();
            }
        }

        private void InvalidateCards()
        {
            _cardsContainer.Invalidate();
        }

        private void CardsContainer_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            Rectangle client = _cardsContainer.ClientRectangle;
            using (SolidBrush bgBrush = new SolidBrush(ThemeManager.FormBg))
            {
                g.FillRectangle(bgBrush, client);
            }

            if (_filteredFamilies.Count == 0)
            {
                using (SolidBrush textBrush = new SolidBrush(ThemeManager.TextMutedColor))
                using (Font noFont = new Font("Segoe UI", 12f))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("لا توجد خطوط مطابقة للبحث", noFont, textBrush, client, sf);
                }
                return;
            }

            int firstVisibleIndex = Math.Max(0, _scrollOffset / _itemHeight);
            int lastVisibleIndex = Math.Min(_filteredFamilies.Count - 1, (_scrollOffset + client.Height) / _itemHeight + 1);

            int cardMargin = 8;
            int cardWidth = client.Width - (cardMargin * 2) - 4;

            for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
            {
                string familyName = _filteredFamilies[i];
                int yPos = (i * _itemHeight) - _scrollOffset + cardMargin;
                Rectangle cardRect = new Rectangle(cardMargin, yPos, cardWidth, _itemHeight - cardMargin);

                if (cardRect.Bottom < 0 || cardRect.Top > client.Height)
                    continue;

                // Card background
                using (GraphicsPath cardPath = ThemeManager.CreateRoundedPath(cardRect, 8))
                {
                    using (SolidBrush cardBrush = new SolidBrush(ThemeManager.CardBg))
                    {
                        g.FillPath(cardBrush, cardPath);
                    }
                    using (Pen borderPen = new Pen(ThemeManager.BorderColor, 1f))
                    {
                        g.DrawPath(borderPen, cardPath);
                    }
                }

                // Header inside card: Family Name & Index badge
                int headerY = cardRect.Top + 6;
                string indexStr = "#" + (i + 1);

                // Badge background for font name
                Rectangle badgeRect = new Rectangle(cardRect.Left + 12, headerY, 60, 20);
                ThemeManager.DrawBadge(g, badgeRect, indexStr, ThemeManager.IsDarkMode ? Color.FromArgb(40, 50, 75) : Color.FromArgb(235, 240, 255), ThemeManager.Primary, Font);

                // Family name
                using (Font nameFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                using (SolidBrush nameBrush = new SolidBrush(ThemeManager.TextColor))
                {
                    g.DrawString(familyName, nameFont, nameBrush, cardRect.Left + 80, headerY + 1);
                }

                // Copy Name & Reveal buttons inside card
                Rectangle btnCopyRect = new Rectangle(cardRect.Right - 180, headerY, 80, 22);
                Rectangle btnRevealRect = new Rectangle(cardRect.Right - 90, headerY, 80, 22);

                DrawMiniButton(g, btnCopyRect, Localization.Get("CopyFontName"));
                DrawMiniButton(g, btnRevealRect, "استعراض");

                // Preview rendering of custom sample text
                int sampleY = headerY + 28;
                Rectangle sampleRect = new Rectangle(cardRect.Left + 14, sampleY, cardRect.Width - 28, cardRect.Height - 34);

                Font sampleFont = null;
                try
                {
                    sampleFont = new Font(familyName, _sampleFontSize, FontStyle.Regular);
                }
                catch
                {
                    try { sampleFont = new Font(familyName, _sampleFontSize, FontStyle.Bold); }
                    catch { }
                }

                if (sampleFont != null)
                {
                    using (sampleFont)
                    using (SolidBrush sampleBrush = new SolidBrush(ThemeManager.TextColor))
                    {
                        StringFormat sf = new StringFormat
                        {
                            Alignment = Localization.IsArabic ? StringAlignment.Far : StringAlignment.Near,
                            LineAlignment = StringAlignment.Center,
                            FormatFlags = StringFormatFlags.NoWrap,
                            Trimming = StringTrimming.EllipsisWord
                        };
                        g.DrawString(_txtSample.Text, sampleFont, sampleBrush, sampleRect, sf);
                    }
                }
                else
                {
                    using (Font defFont = new Font("Segoe UI", 10f, FontStyle.Italic))
                    using (SolidBrush mutedBrush = new SolidBrush(ThemeManager.TextMutedColor))
                    {
                        g.DrawString("(تعذر تحميل نمط هذا الخط للمعاينة)", defFont, mutedBrush, sampleRect);
                    }
                }
            }
        }

        private void DrawMiniButton(Graphics g, Rectangle r, string text)
        {
            using (GraphicsPath p = ThemeManager.CreateRoundedPath(r, 4))
            {
                using (SolidBrush b = new SolidBrush(ThemeManager.IsDarkMode ? Color.FromArgb(45, 55, 72) : Color.FromArgb(240, 243, 248)))
                {
                    g.FillPath(b, p);
                }
                using (Pen pen = new Pen(ThemeManager.BorderColor, 1f))
                {
                    g.DrawPath(pen, p);
                }
            }
            using (SolidBrush textB = new SolidBrush(ThemeManager.TextColor))
            using (Font bf = new Font("Segoe UI", 8f))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(text, bf, textB, r, sf);
            }
        }

        private void CardsContainer_MouseClick(object sender, MouseEventArgs e)
        {
            int cardMargin = 8;
            int clientWidth = _cardsContainer.ClientRectangle.Width;
            int cardWidth = clientWidth - (cardMargin * 2) - 4;

            int clickedIndex = (_scrollOffset + e.Y) / _itemHeight;
            if (clickedIndex >= 0 && clickedIndex < _filteredFamilies.Count)
            {
                string familyName = _filteredFamilies[clickedIndex];
                int yPos = (clickedIndex * _itemHeight) - _scrollOffset + cardMargin;
                Rectangle cardRect = new Rectangle(cardMargin, yPos, cardWidth, _itemHeight - cardMargin);
                int headerY = cardRect.Top + 6;

                Rectangle btnCopyRect = new Rectangle(cardRect.Right - 180, headerY, 80, 22);
                Rectangle btnRevealRect = new Rectangle(cardRect.Right - 90, headerY, 80, 22);

                if (btnCopyRect.Contains(e.Location))
                {
                    try
                    {
                        Clipboard.SetText(familyName);
                        MessageBox.Show(Localization.Get("Copied") + "\n" + familyName, "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch { }
                }
                else if (btnRevealRect.Contains(e.Location))
                {
                    FontInstaller.RevealInWindowsFonts(familyName);
                }
            }
        }

        public void ApplyThemeAndLang()
        {
            RightToLeft = Localization.IsArabic ? RightToLeft.Yes : RightToLeft.No;

            BackColor = ThemeManager.FormBg;
            _topBar.BackColor = ThemeManager.CardBg;
            _cardsContainer.BackColor = ThemeManager.FormBg;

            foreach (Control c in _topBar.Controls)
            {
                if (c is Label)
                {
                    c.ForeColor = ThemeManager.TextColor;
                    if (c.Name == "lblSearch") c.Text = Localization.Get("SysSearchFont");
                    if (c.Name == "lblSample") c.Text = Localization.Get("SampleTextLabel");
                    if (c.Name == "lblSize") c.Text = Localization.Get("FontSizeLabel");
                }
                else if (c is TextBox)
                {
                    c.BackColor = ThemeManager.CardAltBg;
                    c.ForeColor = ThemeManager.TextColor;
                }
                else if (c is Button)
                {
                    c.BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(45, 55, 75) : Color.FromArgb(235, 240, 250);
                    c.ForeColor = ThemeManager.TextColor;
                }
            }

            _lblFontSizeValue.ForeColor = ThemeManager.Primary;
            _lblTotalFonts.Text = string.Format("{0} {1} / {2}",
                Localization.Get("TotalSystemFonts"),
                _filteredFamilies.Count,
                _allFontFamilies.Count);

            InvalidateCards();
        }

        private void OnThemeOrLangChanged()
        {
            ApplyThemeAndLang();
        }
    }

    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            UpdateStyles();
        }
    }
}
