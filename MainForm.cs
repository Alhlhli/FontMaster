using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace FontMaster
{
    public class MainForm : Form
    {
        // Core Managers
        private FontScanner _scanner;
        private List<FontItem> _allFonts;
        private List<FontItem> _filteredFonts;
        private FontItem _selectedFont;
        private PrivateFontCollection _previewPfc;
        private Font _previewFont;

        // UI Controls - Header
        private Panel _pnlHeader;
        private Label _lblAppTitle;
        private Label _lblAppSubtitle;
        private Button _btnThemeToggle;
        private Button _btnLangToggle;
        private Label _lblAdminStatus;
        private Label _lblFreeBadge;

        // Navigation Tabs Bar
        private Panel _pnlTabs;
        private Button _btnTabScanner;
        private Button _btnTabSysFonts;
        private Button _btnTabExport;
        private int _currentTab = 0;

        // Container Panels for Tabs
        private Panel _pnlScannerView;
        private SystemFontsViewerControl _sysFontsView;
        private Panel _pnlExportView;

        // Export & Guidelines View Controls
        private Label _lblExportTitle;
        private Panel _pnlExportButtonsRow;
        private Button _btnExportHtml;
        private Button _btnExportCsv;
        private Label _lblTipsTitle;
        private List<Panel> _tipCards = new List<Panel>();
        private Panel _pnlFooterBrand;

        // Scanner View Controls
        private Panel _pnlScanBar;
        private Label _lblFolderPath;
        private TextBox _txtFolderPath;
        private Button _btnBrowse;
        private CheckBox _chkSubfolders;
        private CheckBox _chkPreviewFonts;
        private Button _btnScan;
        private Label _lblDropBanner;

        // Dashboard Stats Cards
        private Panel _pnlDashboard;
        private StatCardControl _cardTotal;
        private StatCardControl _cardInstalled;
        private StatCardControl _cardToInstall;
        private StatCardControl _cardSelected;

        // Filter Toolbar (Row 1)
        private Panel _pnlFilterBar;
        private Button _btnFilterAll;
        private Button _btnFilterUninstalled;
        private Button _btnFilterInstalled;
        private Button _btnFilterTTF;
        private Button _btnFilterOTF;
        private Button _btnFilterZip;
        private string _activeFilter = "ALL";
        private TextBox _txtSearch;

        // Action Toolbar (Row 2 - Zero Overlap)
        private Panel _pnlActionBar;
        private Button _btnSelectUninstalledOnly;
        private Button _btnSelectAll;
        private Button _btnDeselectAll;
        private Button _btnInstallSelected;
        private Button _btnExportReport;
        private ContextMenuStrip _mnuExport;
        private Dictionary<string, Font> _previewFontCache = new Dictionary<string, Font>(StringComparer.OrdinalIgnoreCase);
        private List<PrivateFontCollection> _previewPfcList = new List<PrivateFontCollection>();

        // Main Content Splitter (Grid + Preview)
        private SplitContainer _splitMain;
        private DataGridView _gridFonts;

        // Mini Preview Panel Controls
        private Panel _pnlPreview;
        private Label _lblPreviewTitle;
        private TextBox _txtPreviewSample;
        private Panel _pnlPreviewBox;
        private Label _lblDetailFamily;
        private Label _lblDetailStyle;
        private Label _lblDetailFormat;
        private Label _lblDetailSize;
        private Label _lblDetailSource;
        private Button _btnInstallSingle;
        private Button _btnOpenFontFile;

        // Bottom Status Bar
        private StatusStrip _statusStrip;
        private ToolStripProgressBar _progressBar;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _statusCountLabel;
        private ToolStripStatusLabel _lblCopyright;

        // Background Worker for responsive scanning and installing
        private BackgroundWorker _bgWorkerScan;
        private BackgroundWorker _bgWorkerInstall;

        public MainForm()
        {
            _scanner = new FontScanner();
            _allFonts = new List<FontItem>();
            _filteredFonts = new List<FontItem>();

            InitializeForm();
            InitializeComponents();

            ThemeManager.ThemeChanged += OnThemeChanged;
            Localization.LanguageChanged += OnLanguageChanged;

            ApplyTheme();
            ApplyLocalization();

            // Set default directory to current directory and auto-scan
            _txtFolderPath.Text = Application.StartupPath;
            StartScan();
        }

        private void InitializeForm()
        {
            Text = "FontMaster Pro - مدير الخطوط الذكي";
            Size = new Size(1280, 800);
            MinimumSize = new Size(1024, 680);
            StartPosition = FormStartPosition.CenterScreen;
            AllowDrop = true;

            // Load app icon
            try
            {
                if (File.Exists("app.ico"))
                {
                    Icon = new Icon("app.ico");
                }
            }
            catch { }

            DragEnter += MainForm_DragEnter;
            DragDrop += MainForm_DragDrop;
        }

        private void InitializeComponents()
        {
            SuspendLayout();

            // ==================== HEADER ====================
            _pnlHeader = new Panel();
            _pnlHeader.Dock = DockStyle.Top;
            _pnlHeader.Height = 70;
            _pnlHeader.Padding = new Padding(16, 10, 16, 10);

            _lblAppTitle = new Label();
            _lblAppTitle.AutoSize = true;
            _lblAppTitle.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            _lblAppTitle.Location = new Point(16, 10);

            _lblAppSubtitle = new Label();
            _lblAppSubtitle.AutoSize = true;
            _lblAppSubtitle.Font = new Font("Segoe UI", 8.5f);
            _lblAppSubtitle.Location = new Point(18, 42);

            _btnLangToggle = CreateHeaderButton("English 🌐", 90);
            _btnLangToggle.Click += delegate { Localization.ToggleLanguage(); };

            _btnThemeToggle = CreateHeaderButton("الوضع الليلي 🌙", 115);
            _btnThemeToggle.Click += delegate { ThemeManager.ToggleTheme(); };

            _lblFreeBadge = new Label();
            _lblFreeBadge.Text = "★ مجاني 100% | 100% Free";
            _lblFreeBadge.AutoSize = true;
            _lblFreeBadge.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblFreeBadge.ForeColor = ThemeManager.Success;

            _lblAdminStatus = new Label();
            _lblAdminStatus.AutoSize = true;
            _lblAdminStatus.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _lblAdminStatus.Location = new Point(300, 15);

            _pnlHeader.Controls.Add(_lblAppTitle);
            _pnlHeader.Controls.Add(_lblAppSubtitle);
            _pnlHeader.Controls.Add(_btnLangToggle);
            _pnlHeader.Controls.Add(_btnThemeToggle);
            _pnlHeader.Controls.Add(_lblFreeBadge);
            _pnlHeader.Controls.Add(_lblAdminStatus);
            _pnlHeader.Resize += delegate { PositionHeaderControls(); };

            // ==================== TABS BAR ====================
            _pnlTabs = new Panel();
            _pnlTabs.Dock = DockStyle.Top;
            _pnlTabs.Height = 44;
            _pnlTabs.Padding = new Padding(16, 2, 16, 2);

            _btnTabScanner = CreateTabButton("استكشاف وتثبيت الخطوط", 0);
            _btnTabSysFonts = CreateTabButton("معاين خطوط النظام الشامل", 1);
            _btnTabExport = CreateTabButton("تصدير وإرشادات", 2);

            _pnlTabs.Controls.Add(_btnTabScanner);
            _pnlTabs.Controls.Add(_btnTabSysFonts);
            _pnlTabs.Controls.Add(_btnTabExport);

            // ==================== TAB 1: SCANNER VIEW ====================
            _pnlScannerView = new Panel();
            _pnlScannerView.Dock = DockStyle.Fill;

            // Scan Bar
            _pnlScanBar = new Panel();
            _pnlScanBar.Dock = DockStyle.Top;
            _pnlScanBar.Height = 48;
            _pnlScanBar.Padding = new Padding(16, 8, 16, 8);

            _lblFolderPath = new Label();
            _lblFolderPath.Text = "مجلد البحث:";
            _lblFolderPath.AutoSize = true;
            _lblFolderPath.Location = new Point(16, 14);
            _lblFolderPath.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            _txtFolderPath = new TextBox();
            _txtFolderPath.Location = new Point(95, 12);
            _txtFolderPath.Width = 420;
            _txtFolderPath.Font = new Font("Segoe UI", 9.5f);

            _btnBrowse = CreateSmallButton("تصفح...", 75);
            _btnBrowse.Location = new Point(522, 11);
            _btnBrowse.Click += BtnBrowse_Click;

            _chkSubfolders = new CheckBox();
            _chkSubfolders.Text = "تضمين المجلدات الفرعية";
            _chkSubfolders.AutoSize = true;
            _chkSubfolders.Checked = true;
            _chkSubfolders.Location = new Point(608, 14);
            _chkSubfolders.Font = new Font("Segoe UI", 9f);

            _chkPreviewFonts = new CheckBox();
            _chkPreviewFonts.Text = "معاينة الخطوط";
            _chkPreviewFonts.AutoSize = true;
            _chkPreviewFonts.Checked = true;
            _chkPreviewFonts.Location = new Point(770, 14);
            _chkPreviewFonts.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _chkPreviewFonts.CheckedChanged += ChkPreviewFonts_CheckedChanged;

            _btnScan = new Button();
            _btnScan.Text = "استكشاف الخطوط 🔍";
            _btnScan.Size = new Size(160, 32);
            _btnScan.Location = new Point(895, 8);
            _btnScan.FlatStyle = FlatStyle.Flat;
            _btnScan.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnScan.Cursor = Cursors.Hand;
            _btnScan.Click += delegate { StartScan(); };

            _pnlScanBar.Controls.Add(_lblFolderPath);
            _pnlScanBar.Controls.Add(_txtFolderPath);
            _pnlScanBar.Controls.Add(_btnBrowse);
            _pnlScanBar.Controls.Add(_chkSubfolders);
            _pnlScanBar.Controls.Add(_chkPreviewFonts);
            _pnlScanBar.Controls.Add(_btnScan);
            _pnlScanBar.Resize += delegate { PositionScanBarControls(); };

            // Dashboard Stats Cards
            _pnlDashboard = new Panel();
            _pnlDashboard.Dock = DockStyle.Top;
            _pnlDashboard.Height = 84;
            _pnlDashboard.Padding = new Padding(14, 4, 14, 4);

            _cardTotal = new StatCardControl("إجمالي المكتشفة", "0", Color.FromArgb(99, 102, 241));
            _cardInstalled = new StatCardControl("المثبتة مسبقاً", "0", Color.FromArgb(16, 185, 129));
            _cardToInstall = new StatCardControl("المطلوب تثبيتها", "0", Color.FromArgb(14, 165, 233));
            _cardSelected = new StatCardControl("المحددة حالياً", "0", Color.FromArgb(245, 158, 11));

            _pnlDashboard.Controls.Add(_cardTotal);
            _pnlDashboard.Controls.Add(_cardInstalled);
            _pnlDashboard.Controls.Add(_cardToInstall);
            _pnlDashboard.Controls.Add(_cardSelected);
            _pnlDashboard.Resize += delegate { PositionStatCards(); };

            // Filter Toolbar (Row 1)
            _pnlFilterBar = new Panel();
            _pnlFilterBar.Dock = DockStyle.Top;
            _pnlFilterBar.Height = 40;
            _pnlFilterBar.Padding = new Padding(16, 6, 16, 6);

            _btnFilterAll = CreateFilterChip("الكل", "ALL", 55);
            _btnFilterUninstalled = CreateFilterChip("غير مثبتة", "UNINSTALLED", 85);
            _btnFilterInstalled = CreateFilterChip("مثبتة", "INSTALLED", 70);
            _btnFilterTTF = CreateFilterChip("TTF", "TTF", 55);
            _btnFilterOTF = CreateFilterChip("OTF", "OTF", 55);
            _btnFilterZip = CreateFilterChip("أرشيف ZIP", "ZIP", 90);

            _txtSearch = new TextBox();
            _txtSearch.Width = 190;
            _txtSearch.Font = new Font("Segoe UI", 9f);
            _txtSearch.TextChanged += delegate { ApplyFilter(); };

            _lblDropBanner = new Label();
            _lblDropBanner.Text = "📥 اسحب وأفلت مجلداً أو ملفات خطوط / ZIP هنا للبحث الفوري";
            _lblDropBanner.AutoSize = true;
            _lblDropBanner.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblDropBanner.ForeColor = ThemeManager.Primary;
            _lblDropBanner.Cursor = Cursors.Hand;

            _pnlFilterBar.Controls.Add(_btnFilterAll);
            _pnlFilterBar.Controls.Add(_btnFilterUninstalled);
            _pnlFilterBar.Controls.Add(_btnFilterInstalled);
            _pnlFilterBar.Controls.Add(_btnFilterTTF);
            _pnlFilterBar.Controls.Add(_btnFilterOTF);
            _pnlFilterBar.Controls.Add(_btnFilterZip);
            _pnlFilterBar.Controls.Add(_txtSearch);
            _pnlFilterBar.Controls.Add(_lblDropBanner);
            _pnlFilterBar.Resize += delegate { PositionFilterBarControls(); };

            // Action Toolbar (Row 2 - Zero Overlap)
            _pnlActionBar = new Panel();
            _pnlActionBar.Dock = DockStyle.Top;
            _pnlActionBar.Height = 44;
            _pnlActionBar.Padding = new Padding(16, 6, 16, 6);

            _btnSelectUninstalledOnly = CreateActionButton("تحديد غير المثبت فقط ✨", 165, ThemeManager.Info);
            _btnSelectUninstalledOnly.Click += delegate { SelectUninstalledOnly(); };

            _btnSelectAll = CreateActionButton("تحديد الكل", 80, ThemeManager.BorderColor);
            _btnSelectAll.Click += delegate { SelectAll(true); };

            _btnDeselectAll = CreateActionButton("إلغاء التحديد", 85, ThemeManager.BorderColor);
            _btnDeselectAll.Click += delegate { SelectAll(false); };

            _btnExportReport = CreateActionButton("تصدير تقرير 📊", 130, ThemeManager.Primary);
            _btnExportReport.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _btnExportReport.Click += BtnExportReport_Click;

            _btnInstallSelected = CreateActionButton("تثبيت الخطوط المحددة 🚀", 185, ThemeManager.Success);
            _btnInstallSelected.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnInstallSelected.Click += delegate { StartInstallSelected(); };

            _mnuExport = new ContextMenuStrip();
            ToolStripMenuItem mnuCsv = new ToolStripMenuItem("تصدير جدول CSV (إكسل) 📄", null, delegate { ExportCsvReport(); });
            ToolStripMenuItem mnuHtml = new ToolStripMenuItem("تصدير تقرير تفاعلي HTML 🌐", null, delegate { ExportHtmlReport(); });
            _mnuExport.Items.Add(mnuCsv);
            _mnuExport.Items.Add(mnuHtml);

            _pnlActionBar.Controls.Add(_btnSelectUninstalledOnly);
            _pnlActionBar.Controls.Add(_btnSelectAll);
            _pnlActionBar.Controls.Add(_btnDeselectAll);
            _pnlActionBar.Controls.Add(_btnExportReport);
            _pnlActionBar.Controls.Add(_btnInstallSelected);
            _pnlActionBar.Resize += delegate { PositionActionBarControls(); };

            // Splitter: Left Grid, Right Preview
            _splitMain = new SplitContainer();
            _splitMain.Dock = DockStyle.Fill;
            _splitMain.SplitterWidth = 6;
            _splitMain.FixedPanel = FixedPanel.Panel2;

            // Grid Configuration
            _gridFonts = new DataGridView();
            _gridFonts.Dock = DockStyle.Fill;
            _gridFonts.AllowUserToAddRows = false;
            _gridFonts.AllowUserToDeleteRows = false;
            _gridFonts.AllowUserToResizeRows = false;
            _gridFonts.RowHeadersVisible = false;
            _gridFonts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridFonts.MultiSelect = false;
            _gridFonts.BackgroundColor = ThemeManager.FormBg;
            _gridFonts.BorderStyle = BorderStyle.None;
            _gridFonts.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _gridFonts.ColumnHeadersHeight = 36;
            _gridFonts.RowTemplate.Height = 52;
            _gridFonts.Font = new Font("Segoe UI", 9.5f);
            _gridFonts.EnableHeadersVisualStyles = false;

            SetupGridColumns();
            _gridFonts.CellValueChanged += GridFonts_CellValueChanged;
            _gridFonts.CurrentCellDirtyStateChanged += delegate
            {
                if (_gridFonts.IsCurrentCellDirty)
                    _gridFonts.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _gridFonts.SelectionChanged += GridFonts_SelectionChanged;
            _gridFonts.CellFormatting += GridFonts_CellFormatting;
            _gridFonts.CellPainting += GridFonts_CellPainting;

            // Preview Panel Configuration
            _pnlPreview = new Panel();
            _pnlPreview.Dock = DockStyle.Fill;
            _pnlPreview.Padding = new Padding(12);

            _lblPreviewTitle = new Label();
            _lblPreviewTitle.Text = "معاينة الخط الحيّة";
            _lblPreviewTitle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            _lblPreviewTitle.Dock = DockStyle.Top;
            _lblPreviewTitle.Height = 28;

            _txtPreviewSample = new TextBox();
            _txtPreviewSample.Dock = DockStyle.Top;
            _txtPreviewSample.Font = new Font("Segoe UI", 10.5f);
            _txtPreviewSample.Text = "أبجد هوز حطي كلمن • Quick Brown Fox • 0123456789";
            _txtPreviewSample.TextChanged += delegate
            {
                _pnlPreviewBox.Invalidate();
                if (_chkPreviewFonts != null && _chkPreviewFonts.Checked)
                {
                    _gridFonts.Invalidate();
                }
            };

            _pnlPreviewBox = new DoubleBufferedPanel();
            _pnlPreviewBox.Dock = DockStyle.Fill;
            _pnlPreviewBox.Paint += PnlPreviewBox_Paint;

            // Font Details in preview panel
            Panel pnlDetails = new Panel();
            pnlDetails.Dock = DockStyle.Bottom;
            pnlDetails.Height = 200;
            pnlDetails.Padding = new Padding(4);

            _lblDetailFamily = CreateDetailLabel(pnlDetails, 4);
            _lblDetailStyle = CreateDetailLabel(pnlDetails, 28);
            _lblDetailFormat = CreateDetailLabel(pnlDetails, 52);
            _lblDetailSize = CreateDetailLabel(pnlDetails, 76);
            _lblDetailSource = CreateDetailLabel(pnlDetails, 100);

            _btnInstallSingle = new Button();
            _btnInstallSingle.Text = "تثبيت هذا الخط فوراً 🚀";
            _btnInstallSingle.Size = new Size(200, 30);
            _btnInstallSingle.Location = new Point(6, 126);
            _btnInstallSingle.FlatStyle = FlatStyle.Flat;
            _btnInstallSingle.BackColor = ThemeManager.Success;
            _btnInstallSingle.ForeColor = Color.White;
            _btnInstallSingle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _btnInstallSingle.Cursor = Cursors.Hand;
            _btnInstallSingle.Click += BtnInstallSingle_Click;

            _btnOpenFontFile = new Button();
            _btnOpenFontFile.Text = "فتح ملف الخط في ويندوز 🔍";
            _btnOpenFontFile.Size = new Size(200, 28);
            _btnOpenFontFile.Location = new Point(6, 162);
            _btnOpenFontFile.FlatStyle = FlatStyle.Flat;
            _btnOpenFontFile.Font = new Font("Segoe UI", 8.5f);
            _btnOpenFontFile.Cursor = Cursors.Hand;
            _btnOpenFontFile.Click += delegate
            {
                if (_selectedFont != null && File.Exists(_selectedFont.FilePath))
                {
                    try { Process.Start(_selectedFont.FilePath); }
                    catch { }
                }
            };

            pnlDetails.Controls.Add(_lblDetailFamily);
            pnlDetails.Controls.Add(_lblDetailStyle);
            pnlDetails.Controls.Add(_lblDetailFormat);
            pnlDetails.Controls.Add(_lblDetailSize);
            pnlDetails.Controls.Add(_lblDetailSource);
            pnlDetails.Controls.Add(_btnInstallSingle);
            pnlDetails.Controls.Add(_btnOpenFontFile);

            _pnlPreview.Controls.Add(_pnlPreviewBox);
            _pnlPreview.Controls.Add(pnlDetails);
            _pnlPreview.Controls.Add(_txtPreviewSample);
            _pnlPreview.Controls.Add(_lblPreviewTitle);

            _splitMain.Panel1.Controls.Add(_gridFonts);
            _splitMain.Panel2.Controls.Add(_pnlPreview);
            _splitMain.SplitterDistance = 820;

            _pnlScannerView.Controls.Add(_splitMain);
            _pnlScannerView.Controls.Add(_pnlActionBar);
            _pnlScannerView.Controls.Add(_pnlFilterBar);
            _pnlScannerView.Controls.Add(_pnlDashboard);
            _pnlScannerView.Controls.Add(_pnlScanBar);

            // ==================== TAB 2: SYSTEM FONTS VIEW ====================
            _sysFontsView = new SystemFontsViewerControl();
            _sysFontsView.Dock = DockStyle.Fill;
            _sysFontsView.Visible = false;

            // ==================== TAB 3: EXPORT & ABOUT ====================
            _pnlExportView = CreateExportView();
            _pnlExportView.Dock = DockStyle.Fill;
            _pnlExportView.Visible = false;

            // ==================== STATUS STRIP ====================
            _statusStrip = new StatusStrip();
            _statusStrip.Height = 28;
            _progressBar = new ToolStripProgressBar();
            _progressBar.Width = 140;
            _progressBar.Visible = false;
            _statusLabel = new ToolStripStatusLabel();
            _statusLabel.Spring = true;
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            _statusLabel.Text = "جاهز";
            _statusCountLabel = new ToolStripStatusLabel();
            _statusCountLabel.Text = "الخطوط: 0";

            _lblCopyright = new ToolStripStatusLabel();
            _lblCopyright.Text = "© العامر للهندسة والإعمار";
            _lblCopyright.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);

            _statusStrip.Items.Add(_statusLabel);
            _statusStrip.Items.Add(_progressBar);
            _statusStrip.Items.Add(_statusCountLabel);
            _statusStrip.Items.Add(_lblCopyright);

            // Add all to Form
            Controls.Add(_pnlScannerView);
            Controls.Add(_sysFontsView);
            Controls.Add(_pnlExportView);
            Controls.Add(_pnlTabs);
            Controls.Add(_pnlHeader);
            Controls.Add(_statusStrip);

            // Background Workers
            _bgWorkerScan = new BackgroundWorker();
            _bgWorkerScan.WorkerReportsProgress = true;
            _bgWorkerScan.DoWork += BgWorkerScan_DoWork;
            _bgWorkerScan.ProgressChanged += BgWorkerScan_ProgressChanged;
            _bgWorkerScan.RunWorkerCompleted += BgWorkerScan_RunWorkerCompleted;

            _bgWorkerInstall = new BackgroundWorker();
            _bgWorkerInstall.WorkerReportsProgress = true;
            _bgWorkerInstall.DoWork += BgWorkerInstall_DoWork;
            _bgWorkerInstall.ProgressChanged += BgWorkerInstall_ProgressChanged;
            _bgWorkerInstall.RunWorkerCompleted += BgWorkerInstall_RunWorkerCompleted;

            ResumeLayout(false);
            PerformLayout();
        }

        private void SetupGridColumns()
        {
            _gridFonts.Columns.Clear();

            // 0: Checkbox
            DataGridViewCheckBoxColumn colCheck = new DataGridViewCheckBoxColumn();
            colCheck.Name = "ColCheck";
            colCheck.HeaderText = "اختيار";
            colCheck.Width = 55;
            _gridFonts.Columns.Add(colCheck);

            // 1: Status
            DataGridViewTextBoxColumn colStatus = new DataGridViewTextBoxColumn();
            colStatus.Name = "ColStatus";
            colStatus.HeaderText = "الحالة";
            colStatus.Width = 120;
            colStatus.ReadOnly = true;
            _gridFonts.Columns.Add(colStatus);

            // 2: Font Name
            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
            colName.Name = "ColName";
            colName.HeaderText = "اسم الخط وعائلته";
            colName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colName.ReadOnly = true;
            _gridFonts.Columns.Add(colName);

            // 3: Type
            DataGridViewTextBoxColumn colType = new DataGridViewTextBoxColumn();
            colType.Name = "ColType";
            colType.HeaderText = "النوع";
            colType.Width = 65;
            colType.ReadOnly = true;
            _gridFonts.Columns.Add(colType);

            // 4: Size
            DataGridViewTextBoxColumn colSize = new DataGridViewTextBoxColumn();
            colSize.Name = "ColSize";
            colSize.HeaderText = "الحجم";
            colSize.Width = 80;
            colSize.ReadOnly = true;
            _gridFonts.Columns.Add(colSize);

            // 5: Source
            DataGridViewTextBoxColumn colSource = new DataGridViewTextBoxColumn();
            colSource.Name = "ColSource";
            colSource.HeaderText = "المصدر";
            colSource.Width = 110;
            colSource.ReadOnly = true;
            _gridFonts.Columns.Add(colSource);

            // 6: File Name
            DataGridViewTextBoxColumn colFile = new DataGridViewTextBoxColumn();
            colFile.Name = "ColFile";
            colFile.HeaderText = "اسم الملف";
            colFile.Width = 180;
            colFile.ReadOnly = true;
            _gridFonts.Columns.Add(colFile);
        }

        private Button CreateHeaderButton(string text, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(width, 32);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private Button CreateTabButton(string text, int tabIndex)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(200, 36);
            btn.Dock = DockStyle.Left;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Click += delegate { SwitchTab(tabIndex); };
            return btn;
        }

        private Button CreateSmallButton(string text, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(width, 28);
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 8.5f);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private Button CreateFilterChip(string text, string filterKey, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Tag = filterKey;
            btn.Size = new Size(width, 28);
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 8.5f);
            btn.Cursor = Cursors.Hand;
            btn.Click += delegate
            {
                _activeFilter = filterKey;
                HighlightActiveFilterChip();
                ApplyFilter();
            };
            return btn;
        }

        private Button CreateActionButton(string text, int width, Color bg)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(width, 30);
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private Label CreateDetailLabel(Control parent, int top)
        {
            Label lbl = new Label();
            lbl.Location = new Point(6, top);
            lbl.AutoSize = true;
            lbl.Font = new Font("Segoe UI", 8.5f);
            return lbl;
        }

        private Panel CreateExportView()
        {
            Panel p = new Panel();
            p.AutoScroll = true;
            p.Padding = new Padding(24);

            _tipCards.Clear();

            // Section 1: Export Actions Title & Buttons
            _lblExportTitle = new Label();
            _lblExportTitle.Text = Localization.IsArabic ? "تصدير التقارير التفاعلية وجداول البيانات:" : "Export Interactive Reports & Spreadsheets:";
            _lblExportTitle.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            _lblExportTitle.ForeColor = ThemeManager.TextColor;
            _lblExportTitle.AutoSize = false;
            p.Controls.Add(_lblExportTitle);

            _pnlExportButtonsRow = new Panel();
            _pnlExportButtonsRow.Height = 46;

            _btnExportHtml = new Button();
            _btnExportHtml.Text = "تصدير تقرير تفاعلي HTML 🌐";
            _btnExportHtml.Size = new Size(270, 42);
            _btnExportHtml.FlatStyle = FlatStyle.Flat;
            _btnExportHtml.BackColor = ThemeManager.Primary;
            _btnExportHtml.ForeColor = Color.White;
            _btnExportHtml.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            _btnExportHtml.Cursor = Cursors.Hand;
            _btnExportHtml.Click += delegate { ExportHtmlReport(); };

            _btnExportCsv = new Button();
            _btnExportCsv.Text = "تصدير جدول CSV (إكسل) 📄";
            _btnExportCsv.Size = new Size(260, 42);
            _btnExportCsv.FlatStyle = FlatStyle.Flat;
            _btnExportCsv.BackColor = ThemeManager.Success;
            _btnExportCsv.ForeColor = Color.White;
            _btnExportCsv.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            _btnExportCsv.Cursor = Cursors.Hand;
            _btnExportCsv.Click += delegate { ExportCsvReport(); };

            _pnlExportButtonsRow.Controls.Add(_btnExportHtml);
            _pnlExportButtonsRow.Controls.Add(_btnExportCsv);
            p.Controls.Add(_pnlExportButtonsRow);

            // Section 2: Features and Guidelines Title
            _lblTipsTitle = new Label();
            _lblTipsTitle.Text = Localization.IsArabic ? "مميزات وإرشادات استخدام البرنامج:" : "Program Features & User Guidelines:";
            _lblTipsTitle.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            _lblTipsTitle.ForeColor = ThemeManager.TextColor;
            _lblTipsTitle.AutoSize = false;
            p.Controls.Add(_lblTipsTitle);

            // 5 Enlarged Rich Cards (Height 140)
            Panel card1 = CreateTipCard("🔍", "استكشاف ومعاينة الخطوط المتقدمة",
                "• تفعيل خيار «معاينة الخطوط» يعرض كل خط مكتشف في القائمة بشكله ورسمه الأصلي فوراً.\r\n" +
                "• يدعم البرنامج استكشاف الخطوط بداخل ملفات الأرشيف المضغوطة (ZIP) وقراءتها وتثبيتها دون الحاجة لفك الضغط يدوياً.\r\n" +
                "• يمكن سحب وإفلات أي مجلد أو ملف خطوط أو ملف ZIP مباشرة على نافذة البرنامج لبدء الفحص فوراً.",
                Color.FromArgb(99, 102, 241));
            _tipCards.Add(card1);
            p.Controls.Add(card1);

            Panel card2 = CreateTipCard("⚡", "التثبيت الذكي وتخطي المثبت مسبقاً",
                "• فحص ومقارنة تلقائية مع سجل خطوط ويندوز لتحديد الخطوط الجديدة فوراً.\r\n" +
                "• زر «تحديد غير المثبت فقط» يختار الخطوط الجديدة بنقرة واحدة ويتجاوز الخطوط المكررة لتوفير الوقت.\r\n" +
                "• إرسال إشعار فوري للنظام (WM_FONTCHANGE) عند تثبيت أي خط ليتعرف عليه ويندوز فوراً دون الحاجة لإعادة تشغيل الجهاز.",
                Color.FromArgb(16, 185, 129));
            _tipCards.Add(card2);
            p.Controls.Add(card2);

            Panel card3 = CreateTipCard("👁️", "معاين خطوط النظام الشامل",
                "• تبويب مستقل يتيح كتابة أي جملة أو نص وتجربته عبر مئات الخطوط المثبتة على جهازك في نفس الوقت.\r\n" +
                "• التحكم التفاعلي بحجم الخط (من 10pt حتى 60pt) وفلترة الخطوط بالاسم مع إمكانية نسخ اسم الخط للحافظة بنقرة واحدة.",
                Color.FromArgb(14, 165, 233));
            _tipCards.Add(card3);
            p.Controls.Add(card3);

            Panel card4 = CreateTipCard("📊", "تقارير احترافية تفاعلية وجداول بيانات",
                "• تصدير ملف CSV ذكي يكتشف تلقائياً إعدادات أوفيس (فاصلة منقوطة للأوفيس العربي، وفاصلة عادية للإنجليزي) مع صفين لرؤوس الأعمدة (إنجليزي وعربي).\r\n" +
                "• تصدير تقرير HTML تفاعلي فخم يدعم الثيم الفاتح والداكن والمعاينة الحية للخطوط وتجربة الجمل ونسخ الأسماء دون أي اتصال بالإنترنت.",
                Color.FromArgb(245, 158, 11));
            _tipCards.Add(card4);
            p.Controls.Add(card4);

            Panel card5 = CreateTipCard("🎁", "برنامج مجاني 100% بالكامل",
                "• هذا البرنامج مجاني بالكامل وبدون أي إعلانات أو قيود، صُمم لمساعدة المهندسين والمصممين ومستخدمي الكمبيوتر في تنظيم وإدارة خطوطهم بأعلى كفاءة وسرعة.",
                Color.FromArgb(139, 92, 246));
            _tipCards.Add(card5);
            p.Controls.Add(card5);

            // Section 3: Footer Branded Card
            _pnlFooterBrand = new Panel();
            _pnlFooterBrand.Height = 125;
            _pnlFooterBrand.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (GraphicsPath path = ThemeManager.CreateRoundedPath(_pnlFooterBrand.ClientRectangle, 10))
                {
                    using (SolidBrush b = new SolidBrush(ThemeManager.CardBg)) e.Graphics.FillPath(b, path);
                    using (Pen pen = new Pen(ThemeManager.Primary, 1.8f)) e.Graphics.DrawPath(pen, path);
                }
            };

            Label lblBrandName = new Label();
            lblBrandName.Name = "lblBrandName";
            lblBrandName.Text = "العامر للهندسة والإعمار";
            lblBrandName.Font = new Font("Segoe UI", 17f, FontStyle.Bold);
            lblBrandName.ForeColor = ThemeManager.Primary;
            lblBrandName.AutoSize = false;

            Label lblBrandEn = new Label();
            lblBrandEn.Name = "lblBrandEn";
            lblBrandEn.Text = "Al-Amer for Engineering & Construction";
            lblBrandEn.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblBrandEn.ForeColor = ThemeManager.TextMutedColor;
            lblBrandEn.AutoSize = false;

            Label lblBrandRights = new Label();
            lblBrandRights.Name = "lblBrandRights";
            lblBrandRights.Text = "برنامج مدير ومثبت الخطوط الذكي (FontMaster Pro v2.0) - جميع الحقوق محفوظة © العامر للهندسة والإعمار";
            lblBrandRights.Font = new Font("Segoe UI", 10f);
            lblBrandRights.ForeColor = ThemeManager.TextColor;
            lblBrandRights.AutoSize = false;

            _pnlFooterBrand.Controls.Add(lblBrandName);
            _pnlFooterBrand.Controls.Add(lblBrandEn);
            _pnlFooterBrand.Controls.Add(lblBrandRights);
            p.Controls.Add(_pnlFooterBrand);

            p.Resize += delegate { LayoutExportView(); };
            return p;
        }

        private Panel CreateTipCard(string icon, string title, string description, Color accentColor)
        {
            Panel card = new Panel();
            card.Height = 140;
            card.Tag = accentColor;
            card.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (GraphicsPath path = ThemeManager.CreateRoundedPath(card.ClientRectangle, 8))
                {
                    using (SolidBrush b = new SolidBrush(ThemeManager.CardBg)) e.Graphics.FillPath(b, path);
                    using (Pen pen = new Pen(ThemeManager.BorderColor, 1.2f)) e.Graphics.DrawPath(pen, path);
                }
                using (SolidBrush bAccent = new SolidBrush(accentColor))
                {
                    if (Localization.IsArabic)
                        e.Graphics.FillRectangle(bAccent, card.Width - 6, 0, 6, card.Height);
                    else
                        e.Graphics.FillRectangle(bAccent, 0, 0, 6, card.Height);
                }
            };

            Label lblIcon = new Label();
            lblIcon.Name = "lblIcon";
            lblIcon.Text = icon;
            lblIcon.Font = new Font("Segoe UI Emoji", 26f);
            lblIcon.Size = new Size(54, 54);
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;

            Label lblTitle = new Label();
            lblTitle.Name = "lblTitle";
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            lblTitle.ForeColor = accentColor;
            lblTitle.Height = 30;

            Label lblBody = new Label();
            lblBody.Name = "lblBody";
            lblBody.Text = description;
            lblBody.Font = new Font("Segoe UI", 10.5f, FontStyle.Regular); // Enlarged font for detailed content!
            lblBody.ForeColor = ThemeManager.TextColor;
            lblBody.Height = 90;

            card.Controls.Add(lblIcon);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblBody);

            return card;
        }

        private void LayoutExportView()
        {
            if (_pnlExportView == null) return;

            int clientW = _pnlExportView.ClientSize.Width;
            int contentW = Math.Min(960, Math.Max(560, clientW - 60));
            int startX = Math.Max(20, (clientW - contentW) / 2);

            int curY = 20;

            if (_lblExportTitle != null)
            {
                _lblExportTitle.Location = new Point(startX, curY);
                _lblExportTitle.Width = contentW;
                _lblExportTitle.Height = 32;
                _lblExportTitle.TextAlign = Localization.IsArabic ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
                _lblExportTitle.RightToLeft = Localization.IsArabic ? RightToLeft.Yes : RightToLeft.No;
                curY += 38;
            }

            if (_pnlExportButtonsRow != null)
            {
                _pnlExportButtonsRow.Location = new Point(startX, curY);
                _pnlExportButtonsRow.Width = contentW;
                if (Localization.IsArabic)
                {
                    _btnExportHtml.Location = new Point(contentW - _btnExportHtml.Width, 0);
                    _btnExportCsv.Location = new Point(contentW - _btnExportHtml.Width - _btnExportCsv.Width - 12, 0);
                }
                else
                {
                    _btnExportHtml.Location = new Point(0, 0);
                    _btnExportCsv.Location = new Point(_btnExportHtml.Width + 12, 0);
                }
                curY += 58;
            }

            if (_lblTipsTitle != null)
            {
                _lblTipsTitle.Location = new Point(startX, curY);
                _lblTipsTitle.Width = contentW;
                _lblTipsTitle.Height = 32;
                _lblTipsTitle.TextAlign = Localization.IsArabic ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
                _lblTipsTitle.RightToLeft = Localization.IsArabic ? RightToLeft.Yes : RightToLeft.No;
                curY += 38;
            }

            foreach (Panel card in _tipCards)
            {
                card.Location = new Point(startX, curY);
                card.Width = contentW;
                LayoutSingleCard(card, contentW);
                curY += card.Height + 14;
            }

            if (_pnlFooterBrand != null)
            {
                curY += 6;
                _pnlFooterBrand.Location = new Point(startX, curY);
                _pnlFooterBrand.Width = contentW;
                LayoutFooterCard(_pnlFooterBrand, contentW);
                curY += _pnlFooterBrand.Height + 30;
            }
        }

        private void LayoutSingleCard(Panel card, int cardWidth)
        {
            Label lblIcon = card.Controls["lblIcon"] as Label;
            Label lblTitle = card.Controls["lblTitle"] as Label;
            Label lblBody = card.Controls["lblBody"] as Label;

            if (Localization.IsArabic)
            {
                if (lblIcon != null)
                {
                    lblIcon.Location = new Point(cardWidth - 68, 16);
                }
                if (lblTitle != null)
                {
                    lblTitle.Location = new Point(20, 14);
                    lblTitle.Width = cardWidth - 96;
                    lblTitle.TextAlign = ContentAlignment.TopRight;
                    lblTitle.RightToLeft = RightToLeft.Yes;
                }
                if (lblBody != null)
                {
                    lblBody.Location = new Point(20, 46);
                    lblBody.Width = cardWidth - 96;
                    lblBody.TextAlign = ContentAlignment.TopRight;
                    lblBody.RightToLeft = RightToLeft.Yes;
                }
            }
            else
            {
                if (lblIcon != null)
                {
                    lblIcon.Location = new Point(14, 16);
                }
                if (lblTitle != null)
                {
                    lblTitle.Location = new Point(74, 14);
                    lblTitle.Width = cardWidth - 96;
                    lblTitle.TextAlign = ContentAlignment.TopLeft;
                    lblTitle.RightToLeft = RightToLeft.No;
                }
                if (lblBody != null)
                {
                    lblBody.Location = new Point(74, 46);
                    lblBody.Width = cardWidth - 96;
                    lblBody.TextAlign = ContentAlignment.TopLeft;
                    lblBody.RightToLeft = RightToLeft.No;
                }
            }
            card.Invalidate();
        }

        private void LayoutFooterCard(Panel footer, int width)
        {
            Label lblBrandName = footer.Controls["lblBrandName"] as Label;
            Label lblBrandEn = footer.Controls["lblBrandEn"] as Label;
            Label lblBrandRights = footer.Controls["lblBrandRights"] as Label;

            ContentAlignment align = Localization.IsArabic ? ContentAlignment.TopRight : ContentAlignment.TopLeft;
            RightToLeft rtl = Localization.IsArabic ? RightToLeft.Yes : RightToLeft.No;

            if (lblBrandName != null)
            {
                lblBrandName.Location = new Point(24, 14);
                lblBrandName.Width = width - 48;
                lblBrandName.TextAlign = align;
                lblBrandName.RightToLeft = rtl;
            }
            if (lblBrandEn != null)
            {
                lblBrandEn.Location = new Point(24, 48);
                lblBrandEn.Width = width - 48;
                lblBrandEn.TextAlign = align;
                lblBrandEn.RightToLeft = rtl;
            }
            if (lblBrandRights != null)
            {
                lblBrandRights.Location = new Point(24, 80);
                lblBrandRights.Width = width - 48;
                lblBrandRights.TextAlign = align;
                lblBrandRights.RightToLeft = rtl;
            }
            footer.Invalidate();
        }

        private void UpdateExportViewTheme()
        {
            if (_pnlExportView == null) return;
            _pnlExportView.BackColor = ThemeManager.FormBg;

            if (_lblExportTitle != null) _lblExportTitle.ForeColor = ThemeManager.TextColor;
            if (_lblTipsTitle != null) _lblTipsTitle.ForeColor = ThemeManager.TextColor;

            foreach (Panel card in _tipCards)
            {
                card.BackColor = ThemeManager.CardBg;
                Label lblBody = card.Controls["lblBody"] as Label;
                if (lblBody != null)
                {
                    lblBody.ForeColor = ThemeManager.TextColor;
                }
                card.Invalidate();
            }

            if (_pnlFooterBrand != null)
            {
                _pnlFooterBrand.BackColor = ThemeManager.CardBg;
                Label lblBrandName = _pnlFooterBrand.Controls["lblBrandName"] as Label;
                Label lblBrandEn = _pnlFooterBrand.Controls["lblBrandEn"] as Label;
                Label lblBrandRights = _pnlFooterBrand.Controls["lblBrandRights"] as Label;

                if (lblBrandName != null) lblBrandName.ForeColor = ThemeManager.Primary;
                if (lblBrandEn != null) lblBrandEn.ForeColor = ThemeManager.TextMutedColor;
                if (lblBrandRights != null) lblBrandRights.ForeColor = ThemeManager.TextColor;
                _pnlFooterBrand.Invalidate();
            }
        }

        private void SwitchTab(int tabIndex)
        {
            _currentTab = tabIndex;
            _pnlScannerView.Visible = (_currentTab == 0);
            _sysFontsView.Visible = (_currentTab == 1);
            _pnlExportView.Visible = (_currentTab == 2);

            _btnTabScanner.BackColor = (_currentTab == 0) ? ThemeManager.CardBg : ThemeManager.FormBg;
            _btnTabSysFonts.BackColor = (_currentTab == 1) ? ThemeManager.CardBg : ThemeManager.FormBg;
            _btnTabExport.BackColor = (_currentTab == 2) ? ThemeManager.CardBg : ThemeManager.FormBg;

            if (_currentTab == 1)
            {
                _sysFontsView.LoadInstalledFontFamilies();
            }
            else if (_currentTab == 2)
            {
                LayoutExportView();
                UpdateExportViewTheme();
            }
        }

        private void PositionHeaderControls()
        {
            int right = _pnlHeader.ClientSize.Width - 16;
            _btnThemeToggle.Location = new Point(right - _btnThemeToggle.Width, 18);
            right -= _btnThemeToggle.Width + 8;
            _btnLangToggle.Location = new Point(right - _btnLangToggle.Width, 18);
            right -= _btnLangToggle.Width + 14;
            _lblAdminStatus.Location = new Point(right - _lblAdminStatus.Width, 24);
            right -= _lblAdminStatus.Width + 16;
            _lblFreeBadge.Location = new Point(right - _lblFreeBadge.Width, 24);
        }

        private void PositionStatCards()
        {
            int totalWidth = _pnlDashboard.ClientSize.Width - 28;
            int cardW = Math.Max(120, totalWidth / 4 - 10);
            int h = _pnlDashboard.ClientSize.Height - 8;

            _cardTotal.SetBounds(14, 4, cardW, h);
            _cardInstalled.SetBounds(14 + (cardW + 10), 4, cardW, h);
            _cardToInstall.SetBounds(14 + (cardW + 10) * 2, 4, cardW, h);
            _cardSelected.SetBounds(14 + (cardW + 10) * 3, 4, cardW, h);
        }

        private void PositionScanBarControls()
        {
            int w = _pnlScanBar.ClientSize.Width;
            int right = w - 16;
            _btnScan.Location = new Point(right - _btnScan.Width, 8); right -= _btnScan.Width + 12;
            _chkPreviewFonts.Location = new Point(right - _chkPreviewFonts.Width, 14); right -= _chkPreviewFonts.Width + 12;
            _chkSubfolders.Location = new Point(right - _chkSubfolders.Width, 14); right -= _chkSubfolders.Width + 12;
            _btnBrowse.Location = new Point(right - _btnBrowse.Width, 11); right -= _btnBrowse.Width + 8;
            int left = _lblFolderPath.Right + 8;
            int txtWidth = Math.Max(120, right - left);
            _txtFolderPath.SetBounds(left, 12, txtWidth, _txtFolderPath.Height);
        }

        private void PositionFilterBarControls()
        {
            int left = 16;
            _btnFilterAll.Location = new Point(left, 6); left += _btnFilterAll.Width + 6;
            _btnFilterUninstalled.Location = new Point(left, 6); left += _btnFilterUninstalled.Width + 6;
            _btnFilterInstalled.Location = new Point(left, 6); left += _btnFilterInstalled.Width + 6;
            _btnFilterTTF.Location = new Point(left, 6); left += _btnFilterTTF.Width + 6;
            _btnFilterOTF.Location = new Point(left, 6); left += _btnFilterOTF.Width + 6;
            _btnFilterZip.Location = new Point(left, 6); left += _btnFilterZip.Width + 14;

            _txtSearch.Location = new Point(left, 8);

            int right = _pnlFilterBar.ClientSize.Width - 16;
            _lblDropBanner.Location = new Point(right - _lblDropBanner.Width, 10);
        }

        private void PositionActionBarControls()
        {
            int left = 16;
            _btnSelectUninstalledOnly.Location = new Point(left, 7); left += _btnSelectUninstalledOnly.Width + 8;
            _btnSelectAll.Location = new Point(left, 7); left += _btnSelectAll.Width + 6;
            _btnDeselectAll.Location = new Point(left, 7);

            int right = _pnlActionBar.ClientSize.Width - 16;
            _btnInstallSelected.Location = new Point(right - _btnInstallSelected.Width, 7); right -= _btnInstallSelected.Width + 10;
            _btnExportReport.Location = new Point(right - _btnExportReport.Width, 7);
        }

        // ==================== SCAN & INSTALL LOGIC ====================
        private void StartScan()
        {
            if (_bgWorkerScan.IsBusy) return;

            string path = _txtFolderPath.Text.Trim();
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show("يرجى تحديد مسار مجلد صحيح للبحث.", "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _btnScan.Enabled = false;
            _progressBar.Visible = true;
            _progressBar.Style = ProgressBarStyle.Marquee;
            _statusLabel.Text = Localization.Get("Scanning");

            bool recursive = _chkSubfolders.Checked;
            _bgWorkerScan.RunWorkerAsync(new object[] { path, recursive });
        }

        private void BgWorkerScan_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;
            string path = (string)args[0];
            bool recursive = (bool)args[1];

            _scanner.RefreshInstalledFontsCache();

            List<FontItem> items = _scanner.Scan(path, recursive, delegate(int current, string fileName)
            {
                _bgWorkerScan.ReportProgress(current, fileName);
            });

            e.Result = items;
        }

        private void BgWorkerScan_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string fileName = e.UserState as string;
            if (!string.IsNullOrEmpty(fileName))
            {
                _statusLabel.Text = string.Format("فحص: {0} ({1})", fileName, e.ProgressPercentage);
            }
        }

        private void BgWorkerScan_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _btnScan.Enabled = true;
            _progressBar.Visible = false;

            if (e.Error != null)
            {
                MessageBox.Show("حدث خطأ أثناء الفحص: " + e.Error.Message, "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _allFonts = e.Result as List<FontItem> ?? new List<FontItem>();
            UpdateDashboardStats();
            ApplyFilter();

            _statusLabel.Text = string.Format(Localization.Get("ScanDone"), _allFonts.Count);
        }

        private void StartInstallSelected()
        {
            if (_bgWorkerInstall.IsBusy) return;

            List<FontItem> selectedToInstall = new List<FontItem>();
            foreach (FontItem item in _allFonts)
            {
                if (item.IsSelected && !item.IsInstalled)
                {
                    selectedToInstall.Add(item);
                }
            }

            if (selectedToInstall.Count == 0)
            {
                MessageBox.Show(Localization.Get("NoFontsSelected"), "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _btnInstallSelected.Enabled = false;
            _progressBar.Visible = true;
            _progressBar.Style = ProgressBarStyle.Blocks;
            _progressBar.Minimum = 0;
            _progressBar.Maximum = selectedToInstall.Count;
            _progressBar.Value = 0;

            _bgWorkerInstall.RunWorkerAsync(selectedToInstall);
        }

        private void BgWorkerInstall_DoWork(object sender, DoWorkEventArgs e)
        {
            List<FontItem> list = (List<FontItem>)e.Argument;
            int installedCount = 0;
            int failedCount = 0;
            int skippedCount = 0;

            for (int i = 0; i < list.Count; i++)
            {
                FontItem item = list[i];
                string error;
                bool ok = FontInstaller.InstallFont(item, out error);
                if (ok)
                {
                    installedCount++;
                }
                else
                {
                    failedCount++;
                }
                _bgWorkerInstall.ReportProgress(i + 1, item.DisplayName);
            }

            _scanner.RefreshInstalledFontsCache();
            e.Result = new int[] { installedCount, failedCount, skippedCount };
        }

        private void BgWorkerInstall_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _progressBar.Value = Math.Min(e.ProgressPercentage, _progressBar.Maximum);
            string name = e.UserState as string;
            _statusLabel.Text = string.Format("جارٍ تثبيت: {0} ({1}/{2})", name, e.ProgressPercentage, _progressBar.Maximum);
        }

        private void BgWorkerInstall_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _btnInstallSelected.Enabled = true;
            _progressBar.Visible = false;

            if (e.Result is int[])
            {
                int[] res = (int[])e.Result;
                string msg = string.Format(Localization.Get("InstallComplete"), res[0], res[1], res[2]);
                MessageBox.Show(msg, "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            UpdateDashboardStats();
            _gridFonts.Refresh();
            _statusLabel.Text = "اكتمل التثبيت بنجاح.";
        }

        // ==================== FILTERING & SELECTION ====================
        private void ApplyFilter()
        {
            string q = _txtSearch.Text.Trim();
            _filteredFonts.Clear();

            foreach (FontItem item in _allFonts)
            {
                // Status / Type Filter
                if (_activeFilter == "UNINSTALLED" && item.IsInstalled) continue;
                if (_activeFilter == "INSTALLED" && !item.IsInstalled) continue;
                if (_activeFilter == "TTF" && !item.Extension.Equals("TTF", StringComparison.OrdinalIgnoreCase)) continue;
                if (_activeFilter == "OTF" && !item.Extension.Equals("OTF", StringComparison.OrdinalIgnoreCase)) continue;
                if (_activeFilter == "ZIP" && !item.IsFromZip) continue;

                // Search query
                if (!string.IsNullOrEmpty(q))
                {
                    bool match = (item.DisplayName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                 (item.FileName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!match) continue;
                }

                _filteredFonts.Add(item);
            }

            PopulateGrid();
            UpdateDashboardStats();
        }

        private void PopulateGrid()
        {
            _gridFonts.Rows.Clear();
            foreach (FontItem f in _filteredFonts)
            {
                string status = f.IsInstalled ? Localization.Get("StatusInstalled") : Localization.Get("StatusReady");
                string source = f.IsFromZip ? ("ZIP: " + Path.GetFileName(f.ZipSource)) : "مجلد";

                int rowIndex = _gridFonts.Rows.Add(
                    f.IsSelected,
                    status,
                    f.DisplayName,
                    f.FormatName,
                    f.FormattedSize,
                    source,
                    f.FileName
                );
                _gridFonts.Rows[rowIndex].Tag = f;
            }

            if (_gridFonts.Rows.Count > 0)
            {
                _gridFonts.Rows[0].Selected = true;
                UpdatePreviewPanel(_gridFonts.Rows[0].Tag as FontItem);
            }
            else
            {
                UpdatePreviewPanel(null);
            }
        }

        private void HighlightActiveFilterChip()
        {
            Button[] chips = new Button[] { _btnFilterAll, _btnFilterUninstalled, _btnFilterInstalled, _btnFilterTTF, _btnFilterOTF, _btnFilterZip };
            foreach (Button b in chips)
            {
                bool active = (string)b.Tag == _activeFilter;
                b.BackColor = active ? ThemeManager.Primary : ThemeManager.CardBg;
                b.ForeColor = active ? Color.White : ThemeManager.TextColor;
            }
        }

        private void SelectAll(bool select)
        {
            foreach (FontItem item in _filteredFonts)
            {
                item.IsSelected = select;
            }
            foreach (DataGridViewRow row in _gridFonts.Rows)
            {
                row.Cells["ColCheck"].Value = select;
            }
            UpdateDashboardStats();
        }

        private void SelectUninstalledOnly()
        {
            foreach (FontItem item in _allFonts)
            {
                item.IsSelected = !item.IsInstalled;
            }
            foreach (DataGridViewRow row in _gridFonts.Rows)
            {
                FontItem item = row.Tag as FontItem;
                if (item != null)
                {
                    row.Cells["ColCheck"].Value = item.IsSelected;
                }
            }
            UpdateDashboardStats();
        }

        private void UpdateDashboardStats()
        {
            int total = _allFonts.Count;
            int installed = 0;
            int selected = 0;

            foreach (FontItem item in _allFonts)
            {
                if (item.IsInstalled) installed++;
                if (item.IsSelected) selected++;
            }

            int toInstall = total - installed;

            _cardTotal.Value = total.ToString();
            _cardInstalled.Value = installed.ToString();
            _cardToInstall.Value = toInstall.ToString();
            _cardSelected.Value = selected.ToString();

            _btnInstallSelected.Text = string.Format("{0} ({1}) 🚀", Localization.Get("InstallSelected"), selected);
            _statusCountLabel.Text = string.Format("الكل: {0} | محدد: {1}", total, selected);
        }

        private void GridFonts_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                DataGridViewRow row = _gridFonts.Rows[e.RowIndex];
                FontItem item = row.Tag as FontItem;
                if (item != null)
                {
                    item.IsSelected = Convert.ToBoolean(row.Cells[0].Value);
                    UpdateDashboardStats();
                }
            }
        }

        private void GridFonts_SelectionChanged(object sender, EventArgs e)
        {
            if (_gridFonts.SelectedRows.Count > 0)
            {
                FontItem item = _gridFonts.SelectedRows[0].Tag as FontItem;
                UpdatePreviewPanel(item);
            }
        }

        private void GridFonts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 1) // Status Column
            {
                DataGridViewRow row = _gridFonts.Rows[e.RowIndex];
                FontItem item = row.Tag as FontItem;
                if (item != null)
                {
                    if (item.IsInstalled)
                    {
                        e.CellStyle.ForeColor = ThemeManager.Success;
                    }
                    else
                    {
                        e.CellStyle.ForeColor = ThemeManager.Info;
                    }
                }
            }
        }

        // ==================== LIVE MINI PREVIEW ====================
        private void UpdatePreviewPanel(FontItem item)
        {
            _selectedFont = item;
            if (_previewPfc != null)
            {
                _previewPfc.Dispose();
                _previewPfc = null;
            }
            if (_previewFont != null)
            {
                _previewFont.Dispose();
                _previewFont = null;
            }

            if (item == null)
            {
                _lblDetailFamily.Text = "العائلة: -";
                _lblDetailStyle.Text = "النمط: -";
                _lblDetailFormat.Text = "الصيغة: -";
                _lblDetailSize.Text = "الحجم: -";
                _lblDetailSource.Text = "المصدر: -";
                _btnInstallSingle.Enabled = false;
                _pnlPreviewBox.Invalidate();
                return;
            }

            _lblDetailFamily.Text = Localization.Get("FamilyLabel") + " " + item.FamilyName;
            _lblDetailStyle.Text = Localization.Get("StyleLabel") + " " + item.SubFamily;
            _lblDetailFormat.Text = Localization.Get("FormatLabel") + " " + item.FormatName;
            _lblDetailSize.Text = Localization.Get("FileSizeLabel") + " " + item.FormattedSize;
            _lblDetailSource.Text = Localization.Get("ColSource") + ": " + (item.IsFromZip ? Path.GetFileName(item.ZipSource) : "ملف مباشر");

            _btnInstallSingle.Enabled = !item.IsInstalled;

            // Load font into PrivateFontCollection for accurate rendering
            try
            {
                if (File.Exists(item.FilePath))
                {
                    _previewPfc = new PrivateFontCollection();
                    _previewPfc.AddFontFile(item.FilePath);
                    if (_previewPfc.Families.Length > 0)
                    {
                        _previewFont = new Font(_previewPfc.Families[0], 28f, FontStyle.Regular);
                    }
                }
            }
            catch
            {
                try
                {
                    _previewFont = new Font(item.FamilyName, 28f, FontStyle.Regular);
                }
                catch { }
            }

            _pnlPreviewBox.Invalidate();
        }

        private void PnlPreviewBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            Rectangle rect = _pnlPreviewBox.ClientRectangle;
            using (SolidBrush bgBrush = new SolidBrush(ThemeManager.CardBg))
            {
                g.FillRectangle(bgBrush, rect);
            }
            using (Pen borderPen = new Pen(ThemeManager.BorderColor, 1f))
            {
                g.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
            }

            if (_selectedFont == null)
            {
                using (SolidBrush textBrush = new SolidBrush(ThemeManager.TextMutedColor))
                using (Font f = new Font("Segoe UI", 11f))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("حدد خطاً من القائمة لمعاينته هنا", f, textBrush, rect, sf);
                }
                return;
            }

            string sampleText = !string.IsNullOrEmpty(_txtPreviewSample.Text) 
                ? _txtPreviewSample.Text 
                : "أبجد هوز حطي كلمن • Quick Brown Fox • 0123456789";

            Font sampleFont = _previewFont ?? new Font("Segoe UI", 24f);

            int yOffset = 14;
            int xMargin = 14;
            int w = rect.Width - (xMargin * 2);

            StringFormat format = new StringFormat
            {
                Alignment = Localization.IsArabic ? StringAlignment.Far : StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.EllipsisWord
            };

            // Section 1: Main Custom Sample Preview (24pt)
            using (Font fSample = new Font(sampleFont.FontFamily, 24f, FontStyle.Regular))
            using (SolidBrush brush = new SolidBrush(ThemeManager.TextColor))
            {
                Rectangle rSample = new Rectangle(xMargin, yOffset, w, 55);
                g.DrawString(sampleText, fSample, brush, rSample, format);
                yOffset += 60;
            }

            // Divider 1
            using (Pen divider = new Pen(ThemeManager.BorderColor, 1f))
            {
                g.DrawLine(divider, xMargin, yOffset, xMargin + w, yOffset);
            }
            yOffset += 8;

            // Section 2: Arabic Sentence Preview (18pt)
            string arabicSentence = "نص عربي: لا إله إلا الله محمد رسول الله • الحمد لله رب العالمين";
            using (Font fAr = new Font(sampleFont.FontFamily, 18f, FontStyle.Regular))
            using (SolidBrush brush = new SolidBrush(ThemeManager.TextColor))
            {
                Rectangle rAr = new Rectangle(xMargin, yOffset, w, 40);
                g.DrawString(arabicSentence, fAr, brush, rAr, format);
                yOffset += 44;
            }

            // Section 3: English Sentence Preview (15pt)
            string englishSentence = "English: The quick brown fox jumps over the lazy dog";
            using (Font fEn = new Font(sampleFont.FontFamily, 15f, FontStyle.Regular))
            using (SolidBrush brush = new SolidBrush(ThemeManager.TextColor))
            {
                Rectangle rEn = new Rectangle(xMargin, yOffset, w, 34);
                g.DrawString(englishSentence, fEn, brush, rEn, format);
                yOffset += 38;
            }

            // Section 4: Numerals and Punctuation (14pt)
            string numeralsText = "الأرقام والرموز: 0 1 2 3 4 5 6 7 8 9 • ! @ # $ % & * ( )";
            using (Font fNum = new Font(sampleFont.FontFamily, 14f, FontStyle.Regular))
            using (SolidBrush brush = new SolidBrush(ThemeManager.TextMutedColor))
            {
                Rectangle rNum = new Rectangle(xMargin, yOffset, w, 32);
                g.DrawString(numeralsText, fNum, brush, rNum, format);
                yOffset += 36;
            }

            // Divider 2
            using (Pen divider = new Pen(ThemeManager.BorderColor, 1f))
            {
                g.DrawLine(divider, xMargin, yOffset, xMargin + w, yOffset);
            }
            yOffset += 8;

            // Section 5: Alphabet glyph sets
            string glyphsAr = "أ ب ت ث ج ح خ د ذ ر ز س ش ص ض ط ظ ع غ ف ق ك ل م ن هـ و ي";
            string glyphsEn = "ABCDEFGHIJKLMNOPQRSTUVWXYZ  abcdefghijklmnopqrstuvwxyz";

            using (Font fGlyph = new Font(sampleFont.FontFamily, 12f))
            using (SolidBrush brush = new SolidBrush(ThemeManager.TextMutedColor))
            {
                g.DrawString(glyphsAr, fGlyph, brush, new Rectangle(xMargin, yOffset, w, 26), format);
                yOffset += 28;
                g.DrawString(glyphsEn, fGlyph, brush, new Rectangle(xMargin, yOffset, w, 26), format);
            }
        }

        private void BtnInstallSingle_Click(object sender, EventArgs e)
        {
            if (_selectedFont == null) return;
            string err;
            bool ok = FontInstaller.InstallFont(_selectedFont, out err);
            if (ok)
            {
                MessageBox.Show("تم تثبيت الخط بنجاح: " + _selectedFont.DisplayName, "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _scanner.RefreshInstalledFontsCache();
                UpdateDashboardStats();
                UpdatePreviewPanel(_selectedFont);
                _gridFonts.Refresh();
            }
            else
            {
                MessageBox.Show("فشل تثبيت الخط: " + err, "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = Localization.Get("SelectDirectory");
                fbd.SelectedPath = _txtFolderPath.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    _txtFolderPath.Text = fbd.SelectedPath;
                    StartScan();
                }
            }
        }

        private void BtnExportReport_Click(object sender, EventArgs e)
        {
            if (_mnuExport != null)
            {
                _mnuExport.Show(_btnExportReport, new Point(0, _btnExportReport.Height));
            }
        }

        private string GetCsvDelimiter()
        {
            try
            {
                string listSep = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                if (!string.IsNullOrEmpty(listSep) && listSep.Trim() == ";")
                {
                    return ";";
                }
                string twoLetter = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant();
                if (twoLetter == "ar" || Localization.IsArabic)
                {
                    return ";";
                }
            }
            catch { }
            return ",";
        }

        private void ExportCsvReport()
        {
            if (_allFonts.Count == 0)
            {
                MessageBox.Show("لا توجد خطوط مكتشفة لتصديرها.", "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                sfd.FileName = "FontInventory_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string sep = GetCsvDelimiter();
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, new System.Text.UTF8Encoding(true)))
                        {
                            // Row 1: English Headers
                            sw.WriteLine(string.Format("Font Name{0}Family Name{0}Style{0}Format{0}Installed Status{0}File Size{0}Source{0}File Path", sep));

                            // Row 2: Arabic Headers
                            sw.WriteLine(string.Format("اسم الخط{0}عائلة الخط{0}النمط{0}الصيغة{0}حالة التثبيت{0}حجم الملف{0}المصدر{0}مسار الملف", sep));

                            // Data rows
                            foreach (FontItem f in _allFonts)
                            {
                                string status = f.IsInstalled ? "مثبت مسبقاً (Installed)" : "غير مثبت (Not Installed)";
                                string source = f.IsFromZip ? ("ZIP: " + Path.GetFileName(f.ZipSource)) : "مجلد (Folder)";

                                sw.WriteLine(string.Format("\"{0}\"{8}\"{1}\"{8}\"{2}\"{8}\"{3}\"{8}\"{4}\"{8}\"{5}\"{8}\"{6}\"{8}\"{7}\"",
                                    f.DisplayName.Replace("\"", "\"\""),
                                    f.FamilyName.Replace("\"", "\"\""),
                                    f.SubFamily.Replace("\"", "\"\""),
                                    f.FormatName,
                                    status,
                                    f.FormattedSize,
                                    source.Replace("\"", "\"\""),
                                    f.FilePath.Replace("\"", "\"\""),
                                    sep
                                ));
                            }
                        }
                        MessageBox.Show(Localization.Get("ExportSuccess") + sfd.FileName, "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطأ أثناء التصدير: " + ex.Message, "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportHtmlReport()
        {
            if (_allFonts.Count == 0)
            {
                MessageBox.Show("لا توجد خطوط مكتشفة لتصديرها.", "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "HTML Files (*.html)|*.html|All Files (*.*)|*.*";
                sfd.FileName = "FontReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        HtmlReportGenerator.GenerateReport(_allFonts, sfd.FileName);
                        MessageBox.Show(Localization.Get("ExportSuccess") + sfd.FileName, "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        try
                        {
                            Process.Start(sfd.FileName);
                        }
                        catch { }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطأ أثناء تصدير تقرير HTML: " + ex.Message, "FontMaster", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ChkPreviewFonts_CheckedChanged(object sender, EventArgs e)
        {
            int newHeight = _chkPreviewFonts.Checked ? 54 : 36;
            _gridFonts.RowTemplate.Height = newHeight;
            foreach (DataGridViewRow row in _gridFonts.Rows)
            {
                row.Height = newHeight;
            }
            _gridFonts.Invalidate();
        }

        private void GridFonts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (_chkPreviewFonts != null && _chkPreviewFonts.Checked && e.RowIndex >= 0 && e.ColumnIndex == 2) // ColName
            {
                DataGridViewRow row = _gridFonts.Rows[e.RowIndex];
                FontItem item = row.Tag as FontItem;
                if (item != null)
                {
                    e.PaintBackground(e.CellBounds, true);

                    bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;
                    Color textCol = isSelected ? ThemeManager.TextColor : ThemeManager.TextColor;
                    Color mutedCol = isSelected ? ThemeManager.TextMutedColor : ThemeManager.TextMutedColor;

                    // 1. Draw Caption: Display Name (small, top)
                    using (SolidBrush mutedBrush = new SolidBrush(mutedCol))
                    using (Font captionFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    {
                        StringFormat sfTop = new StringFormat
                        {
                            Alignment = Localization.IsArabic ? StringAlignment.Far : StringAlignment.Near,
                            LineAlignment = StringAlignment.Near
                        };
                        Rectangle rTop = new Rectangle(e.CellBounds.Left + 8, e.CellBounds.Top + 4, e.CellBounds.Width - 16, 16);
                        e.Graphics.DrawString(item.DisplayName, captionFont, mutedBrush, rTop, sfTop);
                    }

                    // 2. Draw Sample Text in the actual discovered font!
                    Font liveFont = GetCachedPreviewFont(item.FilePath, item.FamilyName, 15f);
                    using (SolidBrush textBrush = new SolidBrush(textCol))
                    {
                        StringFormat sfMain = new StringFormat
                        {
                            Alignment = Localization.IsArabic ? StringAlignment.Far : StringAlignment.Near,
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisWord,
                            FormatFlags = StringFormatFlags.NoWrap
                        };
                        Rectangle rMain = new Rectangle(e.CellBounds.Left + 8, e.CellBounds.Top + 18, e.CellBounds.Width - 16, e.CellBounds.Height - 22);
                        string previewText = !string.IsNullOrEmpty(_txtPreviewSample.Text) ? _txtPreviewSample.Text : item.DisplayName;
                        e.Graphics.DrawString(previewText, liveFont, textBrush, rMain, sfMain);
                    }

                    e.Handled = true;
                }
            }
        }

        private Font GetCachedPreviewFont(string filePath, string fallbackFamily, float size)
        {
            string key = string.Format("{0}_{1}", filePath, size);
            if (_previewFontCache.ContainsKey(key))
            {
                return _previewFontCache[key];
            }

            Font createdFont = null;
            try
            {
                if (File.Exists(filePath))
                {
                    PrivateFontCollection pfc = new PrivateFontCollection();
                    pfc.AddFontFile(filePath);
                    if (pfc.Families.Length > 0)
                    {
                        createdFont = new Font(pfc.Families[0], size, FontStyle.Regular);
                        _previewPfcList.Add(pfc);
                    }
                }
            }
            catch { }

            if (createdFont == null)
            {
                try
                {
                    createdFont = new Font(fallbackFamily, size, FontStyle.Regular);
                }
                catch
                {
                    createdFont = new Font("Segoe UI", size, FontStyle.Regular);
                }
            }

            _previewFontCache[key] = createdFont;
            return createdFont;
        }

        private void ClearFontPreviewCache()
        {
            foreach (KeyValuePair<string, Font> kvp in _previewFontCache)
            {
                try { kvp.Value.Dispose(); } catch { }
            }
            _previewFontCache.Clear();

            foreach (PrivateFontCollection pfc in _previewPfcList)
            {
                try { pfc.Dispose(); } catch { }
            }
            _previewPfcList.Clear();
        }

        // ==================== DRAG & DROP ====================
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                string first = files[0];
                if (Directory.Exists(first))
                {
                    _txtFolderPath.Text = first;
                    StartScan();
                }
                else
                {
                    string parent = Path.GetDirectoryName(first);
                    if (Directory.Exists(parent))
                    {
                        _txtFolderPath.Text = parent;
                        StartScan();
                    }
                }
            }
        }

        // ==================== THEME & LOCALIZATION ====================
        private void ApplyTheme()
        {
            BackColor = ThemeManager.FormBg;
            ForeColor = ThemeManager.TextColor;

            _pnlHeader.BackColor = ThemeManager.CardBg;
            _lblAppTitle.ForeColor = ThemeManager.TextColor;
            _lblAppSubtitle.ForeColor = ThemeManager.TextMutedColor;

            _pnlTabs.BackColor = ThemeManager.FormBg;
            _btnTabScanner.BackColor = (_currentTab == 0) ? ThemeManager.CardBg : ThemeManager.FormBg;
            _btnTabSysFonts.BackColor = (_currentTab == 1) ? ThemeManager.CardBg : ThemeManager.FormBg;
            _btnTabExport.BackColor = (_currentTab == 2) ? ThemeManager.CardBg : ThemeManager.FormBg;
            _btnTabScanner.ForeColor = ThemeManager.TextColor;
            _btnTabSysFonts.ForeColor = ThemeManager.TextColor;
            _btnTabExport.ForeColor = ThemeManager.TextColor;

            _pnlScanBar.BackColor = ThemeManager.CardBg;
            _lblFolderPath.ForeColor = ThemeManager.TextColor;
            _txtFolderPath.BackColor = ThemeManager.CardAltBg;
            _txtFolderPath.ForeColor = ThemeManager.TextColor;
            _chkSubfolders.ForeColor = ThemeManager.TextColor;
            _btnBrowse.BackColor = ThemeManager.CardAltBg;
            _btnBrowse.ForeColor = ThemeManager.TextColor;
            _btnScan.BackColor = ThemeManager.Primary;
            _btnScan.ForeColor = Color.White;

            _pnlDashboard.BackColor = ThemeManager.FormBg;
            _pnlFilterBar.BackColor = ThemeManager.CardBg;
            _txtSearch.BackColor = ThemeManager.CardAltBg;
            _txtSearch.ForeColor = ThemeManager.TextColor;

            HighlightActiveFilterChip();

            // Grid Styling
            _gridFonts.BackgroundColor = ThemeManager.FormBg;
            _gridFonts.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.GridHeaderBg;
            _gridFonts.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.TextColor;
            _gridFonts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _gridFonts.DefaultCellStyle.BackColor = ThemeManager.GridRowBg;
            _gridFonts.DefaultCellStyle.ForeColor = ThemeManager.TextColor;
            _gridFonts.DefaultCellStyle.SelectionBackColor = ThemeManager.GridSelectBg;
            _gridFonts.DefaultCellStyle.SelectionForeColor = ThemeManager.TextColor;
            _gridFonts.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.GridRowAltBg;

            // Preview Panel Styling
            _pnlPreview.BackColor = ThemeManager.CardBg;
            _lblPreviewTitle.ForeColor = ThemeManager.TextColor;
            _txtPreviewSample.BackColor = ThemeManager.CardAltBg;
            _txtPreviewSample.ForeColor = ThemeManager.TextColor;
            _pnlPreviewBox.BackColor = ThemeManager.CardBg;

            _lblDetailFamily.ForeColor = ThemeManager.TextColor;
            _lblDetailStyle.ForeColor = ThemeManager.TextColor;
            _lblDetailFormat.ForeColor = ThemeManager.TextColor;
            _lblDetailSize.ForeColor = ThemeManager.TextColor;
            _lblDetailSource.ForeColor = ThemeManager.TextColor;

            _btnThemeToggle.Text = ThemeManager.IsDarkMode ? Localization.Get("ThemeDark") : Localization.Get("ThemeLight");
            _btnThemeToggle.BackColor = ThemeManager.CardAltBg;
            _btnThemeToggle.ForeColor = ThemeManager.TextColor;

            _btnLangToggle.BackColor = ThemeManager.CardAltBg;
            _btnLangToggle.ForeColor = ThemeManager.TextColor;

            _pnlActionBar.BackColor = ThemeManager.CardBg;

            bool isAdmin = FontInstaller.IsAdministrator();
            _lblAdminStatus.Text = isAdmin ? "★ " + Localization.Get("AdminMode") : "● " + Localization.Get("UserMode");
            _lblAdminStatus.ForeColor = isAdmin ? ThemeManager.Success : ThemeManager.Warning;

            _lblFreeBadge.ForeColor = ThemeManager.Success;
            _lblDropBanner.ForeColor = ThemeManager.Primary;

            _btnOpenFontFile.BackColor = ThemeManager.CardAltBg;
            _btnOpenFontFile.ForeColor = ThemeManager.TextColor;

            _statusStrip.BackColor = ThemeManager.CardBg;
            _statusStrip.ForeColor = ThemeManager.TextColor;
            if (_lblCopyright != null) _lblCopyright.ForeColor = ThemeManager.Primary;
            if (_chkPreviewFonts != null) _chkPreviewFonts.ForeColor = ThemeManager.TextColor;
            if (_btnExportReport != null)
            {
                _btnExportReport.BackColor = ThemeManager.Primary;
                _btnExportReport.ForeColor = Color.White;
            }

            _sysFontsView.ApplyThemeAndLang();
            UpdateExportViewTheme();

            Invalidate(true);
        }

        private void ApplyLocalization()
        {
            RightToLeft = Localization.IsArabic ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = Localization.IsArabic;

            _lblAppTitle.Text = Localization.Get("AppTitle");
            _lblAppSubtitle.Text = Localization.Get("AppSubtitle");
            _btnLangToggle.Text = Localization.Get("LangToggle");
            _btnThemeToggle.Text = ThemeManager.IsDarkMode ? Localization.Get("ThemeDark") : Localization.Get("ThemeLight");

            _btnTabScanner.Text = Localization.Get("TabScanner");
            _btnTabSysFonts.Text = Localization.Get("TabSystemFonts");
            _btnTabExport.Text = Localization.Get("TabAbout");

            _lblFolderPath.Text = Localization.Get("ScanFolderLabel");
            _btnBrowse.Text = Localization.Get("Browse");
            _chkSubfolders.Text = Localization.Get("IncludeSubfolders");
            if (_chkPreviewFonts != null) _chkPreviewFonts.Text = Localization.Get("PreviewFonts");
            _btnScan.Text = Localization.Get("ScanButton");

            _cardTotal.Title = Localization.Get("StatTotal");
            _cardInstalled.Title = Localization.Get("StatInstalled");
            _cardToInstall.Title = Localization.Get("StatToInstall");
            _cardSelected.Title = Localization.Get("StatSelected");

            _lblDropBanner.Text = Localization.Get("DragDropBanner");
            _lblFreeBadge.Text = Localization.Get("FreeSoftware");

            _btnFilterAll.Text = Localization.Get("FilterAll");
            _btnFilterUninstalled.Text = Localization.Get("FilterUninstalled");
            _btnFilterInstalled.Text = Localization.Get("FilterInstalled");
            _btnFilterTTF.Text = Localization.Get("FilterTTF");
            _btnFilterOTF.Text = Localization.Get("FilterOTF");
            _btnFilterZip.Text = Localization.Get("FilterZip");

            _btnSelectUninstalledOnly.Text = Localization.Get("SelectUninstalledOnly");
            _btnSelectAll.Text = Localization.Get("SelectAll");
            _btnDeselectAll.Text = Localization.Get("DeselectAll");
            _btnInstallSelected.Text = Localization.Get("InstallSelected");
            if (_btnExportReport != null) _btnExportReport.Text = Localization.Get("ExportReport");
            if (_lblCopyright != null) _lblCopyright.Text = Localization.Get("CopyrightText");

            _gridFonts.Columns["ColCheck"].HeaderText = Localization.Get("ColCheck");
            _gridFonts.Columns["ColStatus"].HeaderText = Localization.Get("ColStatus");
            _gridFonts.Columns["ColName"].HeaderText = Localization.Get("ColName");
            _gridFonts.Columns["ColType"].HeaderText = Localization.Get("ColType");
            _gridFonts.Columns["ColSize"].HeaderText = Localization.Get("ColSize");
            _gridFonts.Columns["ColSource"].HeaderText = Localization.Get("ColSource");
            _gridFonts.Columns["ColFile"].HeaderText = Localization.Get("ColFile");

            _lblPreviewTitle.Text = Localization.Get("PreviewTitle");
            _btnInstallSingle.Text = Localization.Get("InstallThisFont");
            _btnOpenFontFile.Text = Localization.IsArabic ? "فتح ملف الخط في ويندوز 🔍" : "Open Font File 🔍";

            if (_lblExportTitle != null)
                _lblExportTitle.Text = Localization.IsArabic ? "تصدير التقارير التفاعلية وجداول البيانات:" : "Export Interactive Reports & Spreadsheets:";
            if (_lblTipsTitle != null)
                _lblTipsTitle.Text = Localization.IsArabic ? "مميزات وإرشادات استخدام البرنامج:" : "Program Features & User Guidelines:";
            if (_btnExportHtml != null)
                _btnExportHtml.Text = Localization.IsArabic ? "تصدير تقرير تفاعلي HTML 🌐" : "Export Interactive HTML Report 🌐";
            if (_btnExportCsv != null)
                _btnExportCsv.Text = Localization.IsArabic ? "تصدير جدول CSV (إكسل) 📄" : "Export CSV (Excel) 📄";

            PositionHeaderControls();
            PositionStatCards();
            PositionScanBarControls();
            PositionFilterBarControls();
            PositionActionBarControls();
            LayoutExportView();
            UpdateExportViewTheme();

            _sysFontsView.ApplyThemeAndLang();
        }

        private void OnThemeChanged()
        {
            ApplyTheme();
        }

        private void OnLanguageChanged()
        {
            ApplyLocalization();
        }
    }

    /// <summary>
    /// Custom modern statistic card control with rounded borders and vibrant accent line.
    /// </summary>
    public class StatCardControl : UserControl
    {
        private string _title;
        private string _value;
        private Color _accent;

        public string Title
        {
            get { return _title; }
            set { _title = value; Invalidate(); }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; Invalidate(); }
        }

        public StatCardControl(string title, string value, Color accent)
        {
            _title = title;
            _value = value;
            _accent = accent;

            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            using (GraphicsPath path = ThemeManager.CreateRoundedPath(rect, 8))
            {
                using (SolidBrush bgBrush = new SolidBrush(ThemeManager.CardBg))
                {
                    g.FillPath(bgBrush, path);
                }
                using (Pen borderPen = new Pen(ThemeManager.BorderColor, 1f))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Accent bottom bar
            using (SolidBrush accentBrush = new SolidBrush(_accent))
            {
                Rectangle bar = new Rectangle(rect.X + 8, rect.Bottom - 4, rect.Width - 16, 3);
                using (GraphicsPath barPath = ThemeManager.CreateRoundedPath(bar, 1))
                {
                    g.FillPath(accentBrush, barPath);
                }
            }

            // Value text
            using (Font valFont = new Font("Segoe UI", 18f, FontStyle.Bold))
            using (SolidBrush valBrush = new SolidBrush(ThemeManager.TextColor))
            {
                StringFormat sfVal = new StringFormat
                {
                    Alignment = Localization.IsArabic ? StringAlignment.Far : StringAlignment.Near,
                    LineAlignment = StringAlignment.Near
                };
                Rectangle valRect = new Rectangle(12, 10, rect.Width - 24, 32);
                g.DrawString(_value, valFont, valBrush, valRect, sfVal);
            }

            // Title text
            using (Font titleFont = new Font("Segoe UI", 8.5f))
            using (SolidBrush titleBrush = new SolidBrush(ThemeManager.TextMutedColor))
            {
                StringFormat sfTitle = new StringFormat
                {
                    Alignment = Localization.IsArabic ? StringAlignment.Far : StringAlignment.Near,
                    LineAlignment = StringAlignment.Near
                };
                Rectangle titleRect = new Rectangle(12, 46, rect.Width - 24, 20);
                g.DrawString(_title, titleFont, titleBrush, titleRect, sfTitle);
            }
        }
    }
}
