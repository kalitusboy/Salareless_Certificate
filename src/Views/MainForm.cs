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
        // إعدادات
        private TextBox? txtSettingsWilaya, txtSettingsDaira, txtSettingsBaladia;
        private Button? btnSaveSettings;

        // حقول الإدخال
        private TextBox? txtW1Name, txtW1Birth, txtW1Address, txtW1Card, txtW1CardDate;
        private TextBox? txtW2Name, txtW2Birth, txtW2Address, txtW2Card, txtW2CardDate;
        private TextBox? txtTName, txtTBirth, txtTCard, txtTCardDate, txtLatinName, txtSearch;
        private DataGridView? gridView;
        private Button? btnSave, btnUpdate, btnPrintPreview;
        private List<CertificateRecord> allRecords = new List<CertificateRecord>();
        private int currentSelectedId = -1;

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
            } catch (Exception ex) { MessageBox.Show("خطأ في التهيئة: " + ex.Message); }
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

            // إعدادات المراجع
            Label lblSettings = new Label { Text = "⚙️ إعدادات مراجع جهة الإصدار (تعدل في أي وقت):", Location = new Point(20, y), Width = 430, ForeColor = Color.Brown, Font = new Font("Sakal Majalla", 13, FontStyle.Bold) };
            inputPanel.Controls.Add(lblSettings); y += 30;
            AddField("الولاية:", out txtSettingsWilaya);
            AddField("الدائرة:", out txtSettingsDaira);
            AddField("البلدية الحالية:", out txtSettingsBaladia);
            
            btnSaveSettings = new Button { Text = "حفظ المراجع الإدارية", Location = new Point(20, y), Width = 180, Height = 30, BackColor = Color.PeachPuff, Font = new Font("Sakal Majalla", 11, FontStyle.Bold) };
            btnSaveSettings.Click += BtnSaveSettings_Click;
            inputPanel.Controls.Add(btnSaveSettings);
            y += 45;

            // الشاهد الأول
            Label lblW1 = new Label { Text = "■ بيانات الشاهد الأول:", Location = new Point(20, y), Width = 400, ForeColor = Color.Blue, Font = new Font("Sakal Majalla", 13, FontStyle.Bold) };
            inputPanel.Controls.Add(lblW1); y += 30;
            AddField("الإسم واللقب:", out txtW1Name);
            AddField("المولود في:", out txtW1Birth);
            AddField("الساكن بـ:", out txtW1Address);
            AddField("رقم ب.ت.و:", out txtW1Card);
            AddField("الصادرة بتاريخ:", out txtW1CardDate);

            // الشاهد الثاني
            y += 10;
            Label lblW2 = new Label { Text = "■ بيانات الشاهد الثاني:", Location = new Point(20, y), Width = 400, ForeColor = Color.Blue, Font = new Font("Sakal Majalla", 13, FontStyle.Bold) };
            inputPanel.Controls.Add(lblW2); y += 30;
            AddField("الإسم واللقب:", out txtW2Name);
            AddField("المولود في:", out txtW2Birth);
            AddField("الساكن بـ:", out txtW2Address);
            AddField("رقم ب.ت.و:", out txtW2Card);
            AddField("الصادرة بتاريخ:", out txtW2CardDate);

            // المعني
            y += 10;
            Label lblT = new Label { Text = "■ بيانات المعني (المسمى):", Location = new Point(20, y), Width = 400, ForeColor = Color.DarkGreen, Font = new Font("Sakal Majalla", 13, FontStyle.Bold) };
            inputPanel.Controls.Add(lblT); y += 30;
            AddField("الإسم واللقب:", out txtTName);
            AddField("المولود (ة) في:", out txtTBirth);
            AddField("رقم ب.ت.و:", out txtTCard);
            AddField("الصادرة بتاريخ:", out txtTCardDate);
            AddField("الاسم باللاتينية:", out txtLatinName);

            // أزرار التحكم
            y += 15;
            btnSave = new Button { Text = "حفظ جديد", Location = new Point(240, y), Width = 100, Height = 40, BackColor = Color.LightGreen, Font = new Font("Sakal Majalla", 12, FontStyle.Bold) };
            btnUpdate = new Button { Text = "تحديث السجل", Location = new Point(130, y), Width = 100, Height = 40, BackColor = Color.Gold, Font = new Font("Sakal Majalla", 12, FontStyle.Bold) };
            btnPrintPreview = new Button { Text = "طباعة الشهادة", Location = new Point(20, y), Width = 100, Height = 40, BackColor = Color.LightSkyBlue, Font = new Font("Sakal Majalla", 12, FontStyle.Bold) };
            
            btnSave.Click += BtnSave_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnPrintPreview.Click += BtnPrintPreview_Click;
            
            inputPanel.Controls.Add(btnSave);
            inputPanel.Controls.Add(btnUpdate);
            inputPanel.Controls.Add(btnPrintPreview);
            
            y += 50;
            Label lblInfo = new Label { Text = "ملاحظة: يمكن ترك أي حقل فارغاً، وسيتم إعلامك عند الطباعة.", Location = new Point(20, y), Width = 440, ForeColor = Color.Gray, Font = new Font("Sakal Majalla", 10, FontStyle.Italic) };
            inputPanel.Controls.Add(lblInfo);

            // الجزء الأيسر (البحث والجدول)
            Panel listPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            this.Controls.Add(listPanel);

            Label lblSearch = new Label { Text = "🔎 ابحث بالاسم أو رقم بطاقة التعريف (يشمل الشهود والمعني):", Dock = DockStyle.Top, Height = 25, Font = new Font("Sakal Majalla", 12, FontStyle.Bold) };
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
            currentSelectedId = -1;
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
            MessageBox.Show("تم حفظ المراجع الجغرافية بنجاح.");
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
                currentSelectedId = record.Id;
                
                txtW1Name!.Text = record.Witness1Name;
                txtW1Birth!.Text = record.Witness1Birth;
                txtW1Address!.Text = record.Witness1Address;
                txtW1Card!.Text = record.Witness1Card;
                txtW1CardDate!.Text = record.Witness1CardDate;

                txtW2Name!.Text = record.Witness2Name;
                txtW2Birth!.Text = record.Witness2Birth;
                txtW2Address!.Text = record.Witness2Address;
                txtW2Card!.Text = record.Witness2Card;
                txtW2CardDate!.Text = record.Witness2CardDate;

                txtTName!.Text = record.TargetName;
                txtTBirth!.Text = record.TargetBirth;
                txtTCard!.Text = record.TargetCard;
                txtTCardDate!.Text = record.TargetCardDate;
                txtLatinName!.Text = record.LatinName;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTName?.Text))
            {
                MessageBox.Show("الرجاء إدخال اسم المعني لحفظ السجل.");
                return;
            }

            var record = new CertificateRecord {
                Witness1Name = txtW1Name?.Text ?? "", Witness1Birth = txtW1Birth?.Text ?? "", Witness1Address = txtW1Address?.Text ?? "", Witness1Card = txtW1Card?.Text ?? "", Witness1CardDate = txtW1CardDate?.Text ?? "",
                Witness2Name = txtW2Name?.Text ?? "", Witness2Birth = txtW2Birth?.Text ?? "", Witness2Address = txtW2Address?.Text ?? "", Witness2Card = txtW2Card?.Text ?? "", Witness2CardDate = txtW2CardDate?.Text ?? "",
                TargetName = txtTName?.Text ?? "", TargetBirth = txtTBirth?.Text ?? "", TargetCard = txtTCard?.Text ?? "", TargetCardDate = txtTCardDate?.Text ?? "", LatinName = txtLatinName?.Text ?? "",
                IssueDate = DateTime.Now.ToString("yyyy-MM-dd")
            };

            DatabaseService.SaveCertificate(record);
            MessageBox.Show("تم حفظ السجل بنجاح.");
            LoadData();
            ClearFields();
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (currentSelectedId == -1)
            {
                MessageBox.Show("الرجاء اختيار سجل من الجدول أولاً لتحديثه.");
                return;
            }
            
            var existing = DatabaseService.GetCertificateById(currentSelectedId);
            if (existing == null)
            {
                MessageBox.Show("السجل غير موجود.");
                return;
            }
            
            // تحديث البيانات
            existing.Witness1Name = txtW1Name?.Text ?? "";
            existing.Witness1Birth = txtW1Birth?.Text ?? "";
            existing.Witness1Address = txtW1Address?.Text ?? "";
            existing.Witness1Card = txtW1Card?.Text ?? "";
            existing.Witness1CardDate = txtW1CardDate?.Text ?? "";
            
            existing.Witness2Name = txtW2Name?.Text ?? "";
            existing.Witness2Birth = txtW2Birth?.Text ?? "";
            existing.Witness2Address = txtW2Address?.Text ?? "";
            existing.Witness2Card = txtW2Card?.Text ?? "";
            existing.Witness2CardDate = txtW2CardDate?.Text ?? "";
            
            existing.TargetName = txtTName?.Text ?? "";
            existing.TargetBirth = txtTBirth?.Text ?? "";
            existing.TargetCard = txtTCard?.Text ?? "";
            existing.TargetCardDate = txtTCardDate?.Text ?? "";
            existing.LatinName = txtLatinName?.Text ?? "";
            
            DatabaseService.UpdateCertificate(existing);
            MessageBox.Show("تم تحديث السجل بنجاح.");
            LoadData();
            ClearFields();
            currentSelectedId = -1;
        }
        
        private void ClearFields()
        {
            txtW1Name!.Text = txtW1Birth!.Text = txtW1Address!.Text = txtW1Card!.Text = txtW1CardDate!.Text = "";
            txtW2Name!.Text = txtW2Birth!.Text = txtW2Address!.Text = txtW2Card!.Text = txtW2CardDate!.Text = "";
            txtTName!.Text = txtTBirth!.Text = txtTCard!.Text = txtTCardDate!.Text = txtLatinName!.Text = "";
        }

        private void BtnPrintPreview_Click(object? sender, EventArgs e)
        {
            List<string> emptyFields = new List<string>();
            if (string.IsNullOrWhiteSpace(txtTName?.Text)) emptyFields.Add("اسم المعني");
            if (string.IsNullOrWhiteSpace(txtW1Name?.Text)) emptyFields.Add("اسم الشاهد الأول");
            if (string.IsNullOrWhiteSpace(txtW2Name?.Text)) emptyFields.Add("اسم الشاهد الثاني");
            if (string.IsNullOrWhiteSpace(txtLatinName?.Text)) emptyFields.Add("الاسم باللاتينية");
            
            if (emptyFields.Count > 0)
            {
                string msg = "⚠️ تحذير: الحقول التالية فارغة:\n" + string.Join("\n", emptyFields) + "\n\nهل تريد متابعة الطباعة؟";
                var result = MessageBox.Show(msg, "حقول فارغة", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;
            }
            
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += RenderCertificateDynamicStyle;
            PrintPreviewDialog previewDlg = new PrintPreviewDialog {
                Document = printDoc,
                Width = 700,
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

            // الترويسة
            g.DrawString("الجمهورية الجزائرية الديمقراطية الشعبية", headerFont, brush, e.PageBounds.Width / 2, 30, new StringFormat { Alignment = StringAlignment.Center });
            g.DrawString($"ولاية: {currentWilaya}", regularFont, brush, rightMargin, 70, formatRTL);
            g.DrawString($"دائرة: {currentDaira}", regularFont, brush, rightMargin, 95, formatRTL);
            g.DrawString($"بلدية: {currentBaladia}", headerFont, brush, rightMargin, 120, formatRTL);

            g.DrawString("- شهادة عدم العمل بأجر -", titleFont, brush, e.PageBounds.Width / 2, 170, new StringFormat { Alignment = StringAlignment.Center });
            g.DrawString("- عدم تقاضي منح عائلية -", titleFont, brush, e.PageBounds.Width / 2, 205, new StringFormat { Alignment = StringAlignment.Center });

            g.DrawString($"أن رئيس المجلس الشعبي البلدي لبلدية: {currentBaladia}", headerFont, brush, rightMargin, 260, formatRTL);
            g.DrawString("بناء على شهادة :", regularFont, brush, rightMargin, 290, formatRTL);

            // الشاهد الأول
            g.DrawString($"- السيد: {txtW1Name?.Text}", headerFont, brush, rightMargin - 20, 325, formatRTL);
            g.DrawString($"المولود في: {txtW1Birth?.Text}", regularFont, brush, rightMargin - 220, 325, formatRTL);
            g.DrawString($"الساكن بـ: {txtW1Address?.Text}", regularFont, brush, rightMargin - 20, 355, formatRTL);
            g.DrawString($"ب.ت.و رقم: {txtW1Card?.Text}   الصادرة بتاريخ: {txtW1CardDate?.Text}", regularFont, brush, rightMargin - 20, 385, formatRTL);

            // الشاهد الثاني
            g.DrawString($"- السيد: {txtW2Name?.Text}", headerFont, brush, rightMargin - 20, 425, formatRTL);
            g.DrawString($"المولود في: {txtW2Birth?.Text}", regularFont, brush, rightMargin - 220, 425, formatRTL);
            g.DrawString($"الساكن بـ: {txtW2Address?.Text}", regularFont, brush, rightMargin - 20, 455, formatRTL);
            g.DrawString($"ب.ت.و رقم: {txtW2Card?.Text}   الصادرة بتاريخ: {txtW2CardDate?.Text}", regularFont, brush, rightMargin - 20, 485, formatRTL);

            // المعني
            g.DrawString($"يصادق على أن المسمى (ة): {txtTName?.Text}", headerFont, brush, rightMargin, 535, formatRTL);
            g.DrawString($"المولود (ة) في: {txtTBirth?.Text}", regularFont, brush, rightMargin, 565, formatRTL);
            g.DrawString($"ب.ت.و رقم: {txtTCard?.Text}   الصادرة بتاريخ: {txtTCardDate?.Text}", regularFont, brush, rightMargin, 595, formatRTL);

            string lawText = "عن مصالح:\n- لا يمارس أية وظيفة أو عمل يعطيه الحق في المنح العائلية، إثباتا لذلك سلمت هذه الشهادة للإدلاء بها في حدود ما يسمح به القانون.";
            g.DrawString(lawText, regularFont, brush, new RectangleF(60, 635, e.PageBounds.Width - 120, 70), formatRTL);

            // ========== التوقيعات (التعديل النهائي مع الطالب بجانب الشاهدين) ==========
            float leftX = 60;                           // رئيس المجلس في اليسار
            float rightX = e.PageBounds.Width - 180;    // الشاهدين في اليمين
            float studentX = rightX - 100;              // الطالب بجانب الشاهدين (مسافة 100 بكسل ≈ 2.6 سم)

            // الجهة اليسرى: رئيس المجلس والتاريخ
            g.DrawString($"{currentBaladia} في: {computerDateStr}", regularFont, brush, leftX, 720, formatRTL);
            g.DrawString("رئيس المجلس الشعبي البلدي", headerFont, brush, leftX, 750, formatRTL);
            g.DrawString("(التوقيع والختم)", regularFont, brush, leftX, 780, formatRTL);

            // الجهة اليمنى: الشاهدان
            g.DrawString("إمضاء الشاهدان:", headerFont, brush, rightX, 720, formatRTL);
            g.DrawString("01- ..........................", regularFont, brush, rightX, 750, formatRTL);
            g.DrawString("02- ..........................", regularFont, brush, rightX, 780, formatRTL);

            // الطالب: بجانب الشاهدين (نفس الصفوف الأفقية)
            g.DrawString("الطالب:", headerFont, brush, studentX, 720, formatRTL);
            g.DrawString("..........................", regularFont, brush, studentX, 750, formatRTL);

            // الاسم اللاتيني
            g.DrawString("الكتابة السابقة للاسم و اللقب بالأحرف اللاتينية:", headerFont, brush, rightMargin, 830, formatRTL);
            g.DrawString(txtLatinName?.Text.ToUpper() ?? "", new Font("Arial", 12, FontStyle.Bold), brush, rightMargin, 860, formatRTL);
        }
    }
}
