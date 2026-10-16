using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace SuperMarketMDI;

/// <summary>
/// Form con 1: Quản lý kho hàng (kế thừa từ Bài 1 & 2).
/// Menu System/Tools + ToolStrip + nhập liệu + ảnh sản phẩm + DataGridView + StatusStrip.
/// </summary>
public class frmProductCatalog : Form
{
    private MenuStrip menuStrip;
    private ToolStrip toolStrip;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblSummary, lblClock;
    private Panel pnlInput;
    private TextBox txtMa, txtTen;
    private ComboBox cboLoai;
    private NumericUpDown nudGia, nudSl;
    private PictureBox picAnh;
    private Button btnPick, btnAdd, btnEdit, btnDelete, btnClear;
    private DataGridView dgv;
    private DataTable _table;
    private string _imagePath = "";
    private System.Windows.Forms.Timer _clock;

    public frmProductCatalog()
    {
        Text = "Quản lý kho hàng";
        Width = 1000;
        Height = 700;
        BuildUi();
        InitData();

        _clock = new System.Windows.Forms.Timer { Interval = 1000 };
        _clock.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        _clock.Start();
        lblClock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        FormClosed += (s, e) => { _clock.Stop(); _clock.Dispose(); picAnh.Image?.Dispose(); };
    }

    // ===================== UI =====================
    private void BuildUi()
    {
        // --- MenuStrip (AllowMerge=false: không trộn vào menu của Form cha MDI) ---
        menuStrip = new MenuStrip { AllowMerge = false };
        var mSystem = new ToolStripMenuItem("System");
        mSystem.DropDownItems.Add("Open", null, (s, e) => OpenCsv());
        mSystem.DropDownItems.Add("Save Report", null, (s, e) => SaveReport());
        mSystem.DropDownItems.Add(new ToolStripSeparator());
        mSystem.DropDownItems.Add("Exit", null, (s, e) => Close());
        var mTools = new ToolStripMenuItem("Tools");
        mTools.DropDownItems.Add("Import CSV", null, (s, e) => ImportCsv());
        mTools.DropDownItems.Add("Export CSV", null, (s, e) => ExportCsv());
        menuStrip.Items.AddRange(new ToolStripItem[] { mSystem, mTools });

        // --- ToolStrip ---
        toolStrip = new ToolStrip { AllowMerge = false, ImageScalingSize = new Size(16, 16) };
        toolStrip.Items.Add(Tb("Open", SystemIcons.Information, (s, e) => OpenCsv()));
        toolStrip.Items.Add(Tb("Save Report", SystemIcons.Shield, (s, e) => SaveReport()));
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(Tb("Import CSV", SystemIcons.Application, (s, e) => ImportCsv()));
        toolStrip.Items.Add(Tb("Export CSV", SystemIcons.Question, (s, e) => ExportCsv()));
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(Tb("Exit", SystemIcons.Error, (s, e) => Close()));

        // --- Panel nhập liệu ---
        pnlInput = new Panel { Dock = DockStyle.Top, Height = 250 };

        txtMa = new TextBox { Left = 115, Top = 18, Width = 230 };
        txtTen = new TextBox { Left = 115, Top = 63, Width = 230 };
        cboLoai = new ComboBox { Left = 115, Top = 108, Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
        cboLoai.Items.AddRange(new object[] { "Điện tử", "Thực phẩm", "Gia dụng", "Thời trang", "Khác" });
        cboLoai.SelectedIndex = 0;

        nudGia = new NumericUpDown { Left = 470, Top = 18, Width = 170, Maximum = 1_000_000_000_000m, ThousandsSeparator = true };
        nudSl = new NumericUpDown { Left = 470, Top = 63, Width = 170, Maximum = 1_000_000_000, ThousandsSeparator = true };

        btnAdd = Btn("Thêm", 115, BtnAdd_Click);
        btnEdit = Btn("Sửa", 218, BtnEdit_Click);
        btnDelete = Btn("Xóa", 321, BtnDelete_Click);
        btnClear = Btn("Làm mới", 424, (s, e) => ClearInput());

        picAnh = new PictureBox { Left = 770, Top = 15, Width = 195, Height = 175, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
        btnPick = new Button { Text = "[Chọn ảnh]", Left = 770, Top = 196, Width = 195, Height = 36 };
        btnPick.Click += BtnPick_Click;

        pnlInput.Controls.AddRange(new Control[]
        {
            Lbl("Mã SP:", 15, 22), Lbl("Tên SP:", 15, 67), Lbl("Loại:", 15, 112),
            Lbl("Đơn giá:", 385, 22), Lbl("Số lượng:", 385, 67),
            txtMa, txtTen, cboLoai, nudGia, nudSl,
            btnAdd, btnEdit, btnDelete, btnClear, picAnh, btnPick
        });

        // --- DataGridView ---
        dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        dgv.SelectionChanged += Dgv_SelectionChanged;

        // --- StatusStrip ---
        statusStrip = new StatusStrip { AllowMerge = false };
        lblSummary = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
        lblClock = new ToolStripStatusLabel();
        statusStrip.Items.AddRange(new ToolStripItem[] { lblSummary, lblClock });

        // Thứ tự Add quan trọng với Dock: Fill trước, các thanh dock sau
        Controls.Add(dgv);
        Controls.Add(pnlInput);
        Controls.Add(statusStrip);
        Controls.Add(toolStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
    }

    private static Label Lbl(string text, int x, int y) => new Label { Text = text, Left = x, Top = y, AutoSize = true };

    private static Button Btn(string text, int x, EventHandler h)
    {
        var b = new Button { Text = text, Left = x, Top = 155, Width = 90, Height = 38 };
        b.Click += h;
        return b;
    }

    private static ToolStripButton Tb(string text, Icon icon, EventHandler h) =>
        new ToolStripButton(text, icon.ToBitmap(), h)
        {
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
            ImageScaling = ToolStripItemImageScaling.SizeToFit
        };

    // ===================== Dữ liệu =====================
    private void InitData()
    {
        _table = new DataTable();
        _table.Columns.Add("MaSP", typeof(string));
        _table.Columns.Add("TenSP", typeof(string));
        _table.Columns.Add("Loai", typeof(string));
        _table.Columns.Add("DonGia", typeof(decimal));
        _table.Columns.Add("SoLuong", typeof(int));
        _table.Columns.Add("DuongDanAnh", typeof(string));
        _table.Columns.Add("ThanhTien", typeof(decimal), "DonGia * SoLuong"); // cột tính tự động
        _table.PrimaryKey = new[] { _table.Columns["MaSP"] };

        _table.RowChanged += (s, e) => UpdateSummary();
        _table.RowDeleted += (s, e) => UpdateSummary();

        dgv.DataSource = _table;
        dgv.Columns["MaSP"].HeaderText = "Mã SP";
        dgv.Columns["TenSP"].HeaderText = "Tên SP";
        dgv.Columns["Loai"].HeaderText = "Loại";
        dgv.Columns["DonGia"].HeaderText = "Đơn giá";
        dgv.Columns["SoLuong"].HeaderText = "Số lượng";
        dgv.Columns["DuongDanAnh"].HeaderText = "Đường dẫn ảnh";
        dgv.Columns["ThanhTien"].HeaderText = "Thành tiền";
        dgv.Columns["DonGia"].DefaultCellStyle.Format = "N0";
        dgv.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";

        UpdateSummary();
    }

    private void UpdateSummary()
    {
        long qty = 0;
        decimal total = 0;
        foreach (DataRow r in _table.Rows)
        {
            if (r.RowState == DataRowState.Deleted) continue;
            int sl = Convert.ToInt32(r["SoLuong"]);
            qty += sl;
            total += Convert.ToDecimal(r["DonGia"]) * sl;
        }
        lblSummary.Text = $"Số lượng: {qty:N0} ({_table.Rows.Count} mặt hàng)   Tổng giá trị kho: {total:N0}";
    }

    // Chọn dòng trên lưới -> đổ dữ liệu lên các ô nhập
    private void Dgv_SelectionChanged(object sender, EventArgs e)
    {
        // Sau ClearSelection() CurrentRow vẫn còn nhưng không được chọn -> không đổ lại dữ liệu
        if (dgv.CurrentRow == null || !dgv.CurrentRow.Selected) return;
        if (dgv.CurrentRow.DataBoundItem is not DataRowView v) return;

        txtMa.Text = v["MaSP"].ToString();
        txtTen.Text = v["TenSP"].ToString();
        int idx = cboLoai.FindStringExact(v["Loai"].ToString());
        cboLoai.SelectedIndex = idx >= 0 ? idx : 0;
        nudGia.Value = Math.Min(nudGia.Maximum, Convert.ToDecimal(v["DonGia"]));
        nudSl.Value = Math.Min(nudSl.Maximum, Convert.ToInt32(v["SoLuong"]));
        SetImage(v["DuongDanAnh"].ToString());
        txtMa.ReadOnly = true; // không cho đổi khóa chính khi đang sửa
    }

    // ===================== Ảnh =====================
    private void SetImage(string path)
    {
        _imagePath = path ?? "";
        var old = picAnh.Image;
        picAnh.Image = null;
        old?.Dispose();

        if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
        {
            try
            {
                using var fs = new FileStream(_imagePath, FileMode.Open, FileAccess.Read);
                using var img = Image.FromStream(fs);
                picAnh.Image = new Bitmap(img); // bản sao -> không khóa file ảnh
            }
            catch { /* file không phải ảnh hợp lệ -> bỏ qua */ }
        }
    }

    private void BtnPick_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog { Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Tất cả|*.*" };
        if (dlg.ShowDialog() == DialogResult.OK) SetImage(dlg.FileName);
    }

    // ===================== Thêm / Sửa / Xóa / Làm mới =====================
    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(txtMa.Text) || string.IsNullOrWhiteSpace(txtTen.Text))
        {
            MessageBox.Show("Vui lòng nhập Mã SP và Tên SP.", "Thiếu dữ liệu");
            return false;
        }
        return true;
    }

    private void BtnAdd_Click(object sender, EventArgs e)
    {
        if (!ValidateInput()) return;
        string ma = txtMa.Text.Trim();
        if (_table.Rows.Contains(ma))
        {
            MessageBox.Show("Mã sản phẩm đã tồn tại.", "Trùng mã");
            return;
        }
        _table.Rows.Add(ma, txtTen.Text.Trim(), cboLoai.Text, nudGia.Value, (int)nudSl.Value, _imagePath);
        ClearInput();
    }

    private void BtnEdit_Click(object sender, EventArgs e)
    {
        var row = _table.Rows.Find(txtMa.Text.Trim());
        if (row == null)
        {
            MessageBox.Show("Hãy chọn sản phẩm cần sửa trên lưới.", "Chưa chọn");
            return;
        }
        if (!ValidateInput()) return;

        row["TenSP"] = txtTen.Text.Trim();
        row["Loai"] = cboLoai.Text;
        row["DonGia"] = nudGia.Value;
        row["SoLuong"] = (int)nudSl.Value;
        row["DuongDanAnh"] = _imagePath;
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        var row = _table.Rows.Find(txtMa.Text.Trim());
        if (row == null)
        {
            MessageBox.Show("Hãy chọn sản phẩm cần xóa trên lưới.", "Chưa chọn");
            return;
        }
        if (MessageBox.Show($"Xóa sản phẩm {txtMa.Text}?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        row.Delete();
        _table.AcceptChanges();
        UpdateSummary();
        ClearInput();
    }

    private void ClearInput()
    {
        dgv.ClearSelection();
        txtMa.ReadOnly = false;
        txtMa.Clear();
        txtTen.Clear();
        cboLoai.SelectedIndex = 0;
        nudGia.Value = 0;
        nudSl.Value = 0;
        SetImage("");
        txtMa.Focus();
    }

    // ===================== CSV / Báo cáo =====================
    private void OpenCsv()
    {
        using var dlg = new OpenFileDialog { Filter = "CSV|*.csv|Tất cả|*.*" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        _table.Clear();                 // Open = thay toàn bộ dữ liệu
        LoadCsv(dlg.FileName);
        ClearInput();
    }

    private void ImportCsv()
    {
        using var dlg = new OpenFileDialog { Filter = "CSV|*.csv|Tất cả|*.*" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        LoadCsv(dlg.FileName);          // Import = nạp thêm, bỏ qua mã trùng
    }

    private void LoadCsv(string path)
    {
        int ok = 0, skipped = 0;
        try
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var f = ParseCsvLine(lines[i]);
                if (i == 0 && f[0].Trim().Equals("MaSP", StringComparison.OrdinalIgnoreCase)) continue; // dòng tiêu đề

                if (f.Count < 5 ||
                    !decimal.TryParse(f[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var gia) ||
                    !int.TryParse(f[4], out var sl) || gia < 0 || sl < 0 ||
                    string.IsNullOrWhiteSpace(f[0]) || _table.Rows.Contains(f[0].Trim()))
                {
                    skipped++;
                    continue;
                }
                _table.Rows.Add(f[0].Trim(), f[1].Trim(), f[2].Trim(), gia, sl, f.Count > 5 ? f[5] : "");
                ok++;
            }
            MessageBox.Show($"Đã nhập {ok} dòng, bỏ qua {skipped} dòng (trùng mã hoặc sai định dạng).", "Import CSV");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không đọc được file: " + ex.Message, "Lỗi");
        }
    }

    private void ExportCsv()
    {
        using var dlg = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "kho_hang.csv" };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        var sb = new StringBuilder("MaSP,TenSP,Loai,DonGia,SoLuong,DuongDanAnh\r\n");
        foreach (DataRow r in _table.Rows)
        {
            sb.AppendLine(string.Join(",",
                Q(r["MaSP"]), Q(r["TenSP"]), Q(r["Loai"]),
                Convert.ToDecimal(r["DonGia"]).ToString(CultureInfo.InvariantCulture),
                r["SoLuong"], Q(r["DuongDanAnh"])));
        }
        File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
        MessageBox.Show("Đã xuất file CSV.", "Export CSV");
    }

    private void SaveReport()
    {
        using var dlg = new SaveFileDialog { Filter = "Text|*.txt", FileName = "bao_cao_kho.txt" };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine("BÁO CÁO TỒN KHO");
        sb.AppendLine($"Ngày lập: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine(new string('-', 78));
        sb.AppendLine($"{"Mã SP",-10}{"Tên SP",-26}{"Loại",-12}{"Đơn giá",12}{"SL",8}{"Thành tiền",14}");
        sb.AppendLine(new string('-', 78));
        decimal total = 0;
        foreach (DataRow r in _table.Rows)
        {
            decimal tt = Convert.ToDecimal(r["ThanhTien"]);
            total += tt;
            sb.AppendLine($"{r["MaSP"],-10}{r["TenSP"],-26}{r["Loai"],-12}{Convert.ToDecimal(r["DonGia"]),12:N0}{r["SoLuong"],8}{tt,14:N0}");
        }
        sb.AppendLine(new string('-', 78));
        sb.AppendLine(lblSummary.Text);
        File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
        MessageBox.Show("Đã lưu báo cáo.", "Save Report");
    }

    private static string Q(object o) => "\"" + (o?.ToString() ?? "").Replace("\"", "\"\"") + "\"";

    private static List<string> ParseCsvLine(string line)
    {
        var res = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                else if (c == '"') inQuotes = false;
                else sb.Append(c);
            }
            else if (c == '"') inQuotes = true;
            else if (c == ',') { res.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(c);
        }
        res.Add(sb.ToString());
        return res;
    }
}
