using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using CertificateApp.Models;
using CertificateApp.Services;

namespace CertificateApp.Views
{
    public class MainForm : Form
    {
        // خانات إعدادات الموقع الإداري (تم جعلها Nullable لحل الـ Warnings)
        private TextBox? txtSettingsWilaya, txtSettingsDaira, txtSettingsBaladia;
        private Button? btnSaveSettings;

        // خانات المدخلات للشهادة
        private TextBox? txtW1Name, txtW1Birth, txtW1Address, txtW1Card, txtW1CardDate;
        private TextBox? txtW2Name, txtW2Birth, txtW2Address, txtW2Card, txtW2CardDate;
        private TextBox? txtTName, txtTBirth, txtTCard, txtTCardDate, txtLatinName, txtSearch;
        private DataGridView? gridView;
        private Button? btnSave, btnPrintPreview;
        private List<CertificateRecord> allRecords = new List<CertificateRecord>();

        public MainForm()
        {
            this.Text = "نظام إدارة وتسيير شهادات عدم العمل بأجر - نسخة مفتوحة المصدر للبلديات";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            InitializeComponent();
            
            try {
                DatabaseService.InitializeDatabase();
                LoadData();
                LoadAdminSettings(); 
            } catch { }
        }

        private void InitializeComponent()
        {
            Panel inputPanel = new Panel { Dock = DockStyle.Right, Width = 480, BackColor = Color.GhostWhite, AutoScroll = true };
            this.Controls.Add(inputPanel);

            int y = 15;
            void AddField(string labelText, out TextBox box)
            {
                Label lbl = new Label { Text = labelText, Location = new Point(340, y), Width = 120, Font = new Font("Sakal Majalla", 12, FontStyle.Bold) };
                box = new TextBox { Location = new Point(20, y), Width = 310, Font = new Font("Segoe UI", 10) };
                inputPanel.Controls.Add(lbl);
                inputPanel.Controls.Add(box);
                y += 35;
            }

            // قسم إعدادات المراجع الإدارية
            Label lblSettings = new Label { Text = "⚙️ إعدادات مراجع جهة الإصدار (تعدل في أي وقت):", Location = new Point(20, y), Width = 430, ForeColor = Color.Brown, Font = new Font("Sakal Majalla", 13, FontStyle.Bold) };
            inputPanel.Controls.Add(lblSettings); y += 30;
            AddField("الولاية:", out txtSettingsWilaya);
            AddField("الدائرة:", out txtSettingsDaira);
            AddField("البلدية الحالية:", out txtSettingsBaladia);
            
            btnSaveSettings = new Button { Text = "حفظ المراجع الإدارية", Location = new Point(20, y), Width = 180, Height = 30, BackColor = Color.PeachPuff, Font = new Font("Sakal Majalla", 11, FontStyle.Bold) };
            btnSaveSettings.Click += BtnSaveSettings_Click;
            inputPanel.Controls.Add(btnSaveSettings);
            y += 45;

            // حقول الشاهد الأول
            Label lblW1 = new Label { Text = "■ بيانات الشاهد الأول:", Location = new Point(20, y), Width = 400, ForeColor = Color.Blue, Font = new Font("Sakal Majalla", 13, FontStyle.Bold) };
            inputPanel.Controls.Add(lblW1); y += 30;
            AddField("الإسم واللقب:", out txtW1Name);
            AddField("المولود في:", out txtW1Birth);
            AddField("الساكن بـ:", out txtW1Address);
            AddField("رقم ب.ت.و:", out txtW1Card);
            AddField("الصادرة بتاريخ:", out txtW1CardDate);

            // حقول الشاهد الثاني
            y += 10;
            Label lblW2 = new Label { Text = "■ بيانات الشاهد الثاني:", Location = new Point(20, y), Width = 400, ForeColor = Color.Blue, Font = new Font("Sakal Majalla", 13, FontStyle.Bold) };
            inputPanel.Controls.Add(lblW2); y += 30;
            AddField("الإسم واللقب:", out txtW2Name);
            AddField("المولود في:", out txtW2Birth);
            AddField("الساكن بـ:", out txtW2Address);
            AddField("رقم ب.ت.و:", out txtW2Card);
            AddField("الصادرة بتاريخ:", out txtW2CardDate);

            // حقول الشخص المعني بالشهادة
            y += 10;
            Label lblT = new Label { Text = "■ بيانات المعني (المسمى):", Location = new Point(20, y), Width = 400, ForeColor = Color.DarkGreen, Font = new Font("Sakal Majalla", 13, FontStyle.Bold) };
            inputPanel.Controls.Add(lblT); y += 30;
            AddField("الإسم واللقب:", out txtTName);
            AddField("المولود (ة) في:", out txtTBirth);
            AddField("رقم ب.ت.و:", out txtTCard);
            AddField("الصادرة بتاريخ:", out txtTCardDate);
            AddField("الاسم باللاتينية:", out txtLatinName);

            // أزرار التحكم وحفظ السجلات
            y += 15;
            btnSave = new Button { Text = "حفظ المستفيد في القاعدة", Location = new Point(240, y), Width = 200, Height = 40, BackColor = Color.LightGreen, Font = new Font("Sakal Majalla", 12, FontStyle.Bold) };
            btnPrintPreview = new Button { Text = "معاينة وطباعة الشهادة كاملة", Location = new Point(20, y), Width = 200, Height = 40, BackColor = Color.LightSkyBlue, Font = new Font("Sakal Majalla", 12, FontStyle.Bold) };
            
            btnSave.Click += BtnSave_Click;
            btnPrintPreview.Click += BtnPrintPreview_Click;
            
            inputPanel.Controls.Add(btnSave);
            inputPanel.Controls.Add(btnPrintPreview);

            // الجزء الأيسر: إدارة السجلات والبحث
            Panel listPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            this.Controls.Add(listPanel);

            Label lblSearch = new Label { Text = "🔎 ابحث بسرعة بالاسم أو برقم بطاقة التعريف الوطنية:", Dock = DockStyle.Top, Height = 25, Font = new Font("Sakal Majalla", 12, FontStyle.Bold) };
            txtSearch = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 11) };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            gridView = new DataGridView { Dock = DockStyle.Fill, Margin = new Padding(0, 10, 0, 0), AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true };
            gridView.CellClick += GridView_CellClick;

            listPanel.Controls.Add(gridView);
            listPanel.Controls.Add(txtSearch);
            listPanel.Controls.Add(lblSearch);
        }

        private void LoadData()
        {
            allRecords = DatabaseService.SearchCertificates("");
            if (gridView != null) gridView.DataSource = allRecords;
        }

        private void LoadAdminSettings()
        {
            var settings = DatabaseService.LoadSettings();
            if (txtSettingsWilaya != null) txtSettingsWilaya.Text = settings.Item1;
            if (txtSettingsDaira != null) txtSettingsDaira.Text = settings.Item2;
            if (txtSettingsBaladia != null) txtSettingsBaladia.Text = settings.Item3;
        }

        private void BtnSaveSettings_Click(object? sender, EventArgs e)
        {
            DatabaseService.SaveSettings(txtSettingsWilaya?.Text ?? "", txtSettingsDaira?.Text ?? "", txtSettingsBaladia?.Text ?? "");
            MessageBox.Show("تم حفظ المراجع الجغرافية بنجاح، سيستخدمها البرنامج تلقائياً في الشهادات المطبوعة.");
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            var results = DatabaseService.SearchCertificates(txtSearch?.Text.Trim() ?? "");
            if (gridView != null) gridView.DataSource = results;
        }

        private void GridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && gridView != null && gridView.SelectedRows.Count > 0)
            {
                var record = (CertificateRecord)gridView.SelectedRows[0].DataBoundItem;
                if (txtW1Name != null) txtW1Name.Text = record.Witness1Name;
                if (txtW1Birth != null) txtW1Birth.Text = record.Witness1Birth;
                if (txtW1Address != null) txtW1Address.Text = record.Witness1Address;
                if (txtW1Card != null) txtW1Card.Text = record.Witness1Card;
                if (txtW1CardDate != null) txtW1CardDate.Text = record.Witness1CardDate;

                if (txtW2Name != null) txtW2Name.Text = record.Witness2Name;
                if (txtW2Birth != null) txtW2Birth.Text = record.Witness2Birth;
                if (txtW2Address != null) txtW2Address.Text = record.Witness2Address;
                if (txtW2Card != null) txtW2Card.Text = record.Witness2Card;
                if (txtW2CardDate != null) txtW2CardDate.Text = record.Witness2CardDate;

                if (txtTName != null) txtTName.Text = record.TargetName;
                if (txtTBirth != null) txtTBirth.Text = record.TargetBirth;
                if (txtTCard != null) txtTCard.Text = record.TargetCard;
                if (txtTCardDate != null) txtTCardDate.Text = record.TargetCardDate;
                if (txtLatinName != null) txtLatinName.Text = record.LatinName;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTName?.Text)) {
                MessageBox.Show("الرجاء إدخال اسم المعني لحفظ السجل.");
                return;
            }

            string currentComputerDate = DateTime.Now.ToString("yyyy-MM-dd");

            var record = new CertificateRecord {
                Witness1Name = txtW1Name?.Text ?? "", Witness1Birth = txtW1Birth?.Text ?? "", Witness1Address = txtW1Address?.Text ?? "", Witness1Card = txtW1Card?.Text ?? "", Witness1CardDate = txtW1CardDate?.Text ?? "",
                Witness2Name = txtW2Name?.Text ?? "", Witness2Birth = txtW2Birth?.Text ?? "", Witness2Address = txtW2Address?.Text ?? "", Witness2Card = txtW2Card?.Text ?? "", Witness2CardDate = txtW2CardDate?.Text ?? "",
                TargetName = txtTName?.Text ?? "", TargetBirth = txtTBirth?.Text ?? "", TargetCard = txtTCard?.Text ?? "", TargetCardDate = txtTCardDate?.Text ?? "", LatinName = txtLatinName?.Text ?? "",
                IssueDate = currentComputerDate
            };

            DatabaseService.SaveCertificate(record);
            MessageBox.Show("تم حفظ بيانات المستفيد والشهود بنجاح في قاعدة البيانات المحلية.");
            LoadData();
        }

        private void BtnPrintPreview_Click(object? sender, EventArgs e)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += RenderCertificateDynamicStyle;

            PrintPreviewDialog previewDlg = new PrintPreviewDialog {
                Document = printDoc,
                Width = 650,
                Height = 850,
                StartPosition = FormStartPosition.CenterScreen
            };
            previewDlg.ShowDialog();
        }

        private void RenderCertificateDynamicStyle(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics ?? throw new Exception("Graphics context is null");
            Font titleFont = new Font("Sakal Majalla", 20, FontStyle.Bold);
            Font headerFont = new Font("Sakal Majalla", 14, FontStyle.Bold);
            Font regularFont = new Font("Sakal Majalla", 14, FontStyle.Regular);
            Brush brush = Brushes.Black;
            
            StringFormat formatRTL = new StringFormat { FormatFlags = StringFormatFlags.DirectionRightToLeft };
            float rightMargin = e.PageBounds.Width - 60;

            string computerDateStr = DateTime.Now.ToString("dd/MM/yyyy");

            string currentWilaya = string.IsNullOrWhiteSpace(txtSettingsWilaya?.Text) ? "................." : txtSettingsWilaya.Text;
            string currentDaira = string.IsNullOrWhiteSpace(txtSettingsDaira?.Text) ? "................." : txtSettingsDaira.Text;
            string currentBaladia = string.IsNullOrWhiteSpace(txtSettingsBaladia?.Text) ? "................." : txtSettingsBaladia.Text;

            // 1. الترويسة
            g.DrawString("الجمهورية الجزائرية الديمقراطية الشعبية", headerFont, brush, e.PageBounds.Width / 2, 30, new StringFormat { Alignment = StringAlignment.Center });
            g.DrawString($"ولاية: {currentWilaya}", regularFont, brush, rightMargin, 70, formatRTL);
            g.DrawString($"دائرة: {currentDaira}", regularFont, brush, rightMargin, 95, formatRTL);
            g.DrawString($"بلدية: {currentBaladia}", headerFont, brush, rightMargin, 120, formatRTL);

            // 2. العنوان
            g.DrawString("- شهادة عدم العمل بأجر -", titleFont, brush, e.PageBounds.Width / 2, 170, new StringFormat { Alignment = StringAlignment.Center });
            g.DrawString("- عدم تقاضي منح عائلية -", titleFont, brush, e.PageBounds.Width / 2, 205, new StringFormat { Alignment = StringAlignment.Center });

            g.DrawString($"أن رئيس المجلس الشعبي البلدي لبلدية: {currentBaladia}", headerFont, brush, rightMargin, 260, formatRTL);
            g.DrawString("بناء على شهادة :", regularFont, brush, rightMargin, 290, formatRTL);

            // 3. الشاهد الأول
            g.DrawString($"- السيد: {txtW1Name?.Text}", headerFont, brush, rightMargin - 20, 325, formatRTL);
            g.DrawString($"المولود في: {txtW1Birth?.Text}", regularFont, brush, rightMargin - 220, 325, formatRTL);
            g.DrawString($"الساكن بـ: {txtW1Address?.Text}", regularFont, brush, rightMargin - 20, 355, formatRTL);
            g.DrawString($"ب.ت.و رقم: {txtW1Card?.Text}   الصادرة بتاريخ: {txtW1CardDate?.Text}", regularFont, brush, rightMargin - 20, 385, formatRTL);

            // 4. الشاهد الثاني
            g.DrawString($"- السيد: {txtW2Name?.Text}", headerFont, brush, rightMargin - 20, 425, formatRTL);
            g.DrawString($"المولود في: {txtW2Birth?.Text}", regularFont, brush, rightMargin - 220, 425, formatRTL);
            g.DrawString($"الساكن بـ: {txtW2Address?.Text}", regularFont, brush, rightMargin - 20, 455, formatRTL);
            g.DrawString($"ب.ت.و رقم: {txtW2Card?.Text}   الصادرة بتاريخ: {txtW2CardDate?.Text}", regularFont, brush, rightMargin - 20, 485, formatRTL);

            // 5. المعني
            g.DrawString($"يصادق على أن المسمى (ة): {txtTName?.Text}", headerFont, brush, rightMargin, 535, formatRTL);
            g.DrawString($"المولود (ة) في: {txtTBirth?.Text}", regularFont, brush, rightMargin, 565, formatRTL);
            g.DrawString($"ب.ت.و رقم: {txtTCard?.Text}   الصادرة بتاريخ: {txtTCardDate?.Text}", regularFont, brush, rightMargin, 595, formatRTL);

            // 6. العبارة القانونية
            string lawText = "عن مصالح:\n- لا يمارس أية وظيفة أو عمل يعطيه الحق في المنح العائلية، إثباتا لذلك سلمت هذه الشهادة للإدلاء بها في حدود ما يسمح به القانون.";
            g.DrawString(lawText, regularFont, brush, new RectangleF(60, 635, e.PageBounds.Width - 120, 70), formatRTL);

            // 7. التوقيعات والتواريخ اللحظية للكمبيوتر
            g.DrawString($"{currentBaladia} في: {computerDateStr}", regularFont, brush, 180, 720, formatRTL);
            g.DrawString("رئيس المجلس الشعبي البلدي", headerFont, brush, 200, 750, formatRTL);
            
            g.DrawString("إمضاء الشاهدان:", headerFont, brush, rightMargin - 50, 720, formatRTL);
            g.DrawString("01- ..........................", regularFont, brush, rightMargin - 50, 750, formatRTL);
            g.DrawString("02- ..........................", regularFont, brush, rightMargin - 50, 780, formatRTL);

            // 8. مربع الاسم اللاتيني
            g.DrawString("الكتابة السابقة للاسم و اللقب بالأحرف اللاتينية:", headerFont, brush, rightMargin, 830, formatRTL);
            g.DrawString(txtLatinName?.Text.ToUpper() ?? "", new Font("Arial", 12, FontStyle.Bold), brush, rightMargin, 860, formatRTL);
        }
    }
}
