using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace InventoryManager
{
    public partial class frmInventoryManager : Form
    {
        // ===== Dữ liệu =====
        private readonly BindingList<Product> _products = new BindingList<Product>();
        private string _currentImagePath = "";
        private const char CsvDelimiter = ';';

        public frmInventoryManager()
        {
            InitializeComponent();
            LoadToolbarIcons();

            cboCategory.SelectedIndex = 0;
            lblClock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            dgvProducts.DataSource = _products;
            FormatGrid();

            // Mỗi khi danh sách thay đổi (Thêm/Sửa/Xóa/Import) -> cập nhật StatusStrip
            _products.ListChanged += (s, e) => UpdateStatus();
            UpdateStatus();
        }

        // Icon tạm lấy từ SystemIcons, có thể thay bằng icon riêng (Image.FromFile / Resources)
        private void LoadToolbarIcons()
        {
            toolOpen.Image = ToSmall(SystemIcons.Information);
            toolSaveReport.Image = ToSmall(SystemIcons.Shield);
            toolImport.Image = ToSmall(SystemIcons.Application);
            toolExport.Image = ToSmall(SystemIcons.Question);
            toolExit.Image = ToSmall(SystemIcons.Error);
        }

        private static Image ToSmall(Icon icon)
        {
            return new Bitmap(icon.ToBitmap(), new Size(20, 20));
        }

        private void FormatGrid()
        {
            dgvProducts.Columns[nameof(Product.Id)].HeaderText = "Mã SP";
            dgvProducts.Columns[nameof(Product.Name)].HeaderText = "Tên SP";
            dgvProducts.Columns[nameof(Product.Category)].HeaderText = "Loại";
            dgvProducts.Columns[nameof(Product.Price)].HeaderText = "Đơn giá";
            dgvProducts.Columns[nameof(Product.Quantity)].HeaderText = "Số lượng";
            dgvProducts.Columns[nameof(Product.ImagePath)].HeaderText = "Đường dẫn ảnh";
            dgvProducts.Columns[nameof(Product.TotalValue)].HeaderText = "Thành tiền";
            dgvProducts.Columns[nameof(Product.Price)].DefaultCellStyle.Format = "N0";
            dgvProducts.Columns[nameof(Product.TotalValue)].DefaultCellStyle.Format = "N0";
        }

        // =====================================================
        //  MENU / TOOLSTRIP EVENTS
        // =====================================================
        private void mnuOpen_Click(object sender, EventArgs e) { ImportCsv(); }
        private void mnuImport_Click(object sender, EventArgs e) { ImportCsv(); }
        private void mnuExport_Click(object sender, EventArgs e) { ExportCsv(); }
        private void mnuSaveReport_Click(object sender, EventArgs e) { SaveReport(); }
        private void mnuExit_Click(object sender, EventArgs e) { Close(); }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            lblClock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        // =====================================================
        //  XỬ LÝ HÌNH ẢNH (OpenFileDialog)
        // =====================================================
        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn ảnh sản phẩm";
                ofd.Filter = "Ảnh (*.jpg;*.png;*.bmp)|*.jpg;*.png;*.bmp";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                _currentImagePath = ofd.FileName;
                LoadImage(_currentImagePath);
            }
        }

        // Đọc ảnh qua MemoryStream để không khóa file trên đĩa
        private void LoadImage(string path)
        {
            if (picProductImage.Image != null)
            {
                picProductImage.Image.Dispose();
                picProductImage.Image = null;
            }

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    picProductImage.Image = Image.FromStream(ms);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở ảnh: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =====================================================
        //  THÊM / SỬA / XÓA
        // =====================================================
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtId.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã SP và Tên SP.", "Thiếu dữ liệu",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            if (_products.Any(p => p.Id.Equals(txtId.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Mã sản phẩm đã tồn tại.", "Trùng mã",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _products.Add(new Product
            {
                Id = txtId.Text.Trim(),
                Name = txtName.Text.Trim(),
                Category = cboCategory.Text,
                Price = numPrice.Value,
                Quantity = (int)numQuantity.Value,
                ImagePath = _currentImagePath
            });
            ClearInputs();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;
            if (!ValidateInput()) return;

            int index = dgvProducts.CurrentRow.Index;
            var p = _products[index];

            bool duplicate = _products.Where((x, i) => i != index)
                .Any(x => x.Id.Equals(txtId.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            if (duplicate)
            {
                MessageBox.Show("Mã sản phẩm đã tồn tại.", "Trùng mã",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            p.Id = txtId.Text.Trim();
            p.Name = txtName.Text.Trim();
            p.Category = cboCategory.Text;
            p.Price = numPrice.Value;
            p.Quantity = (int)numQuantity.Value;
            p.ImagePath = _currentImagePath;
            _products.ResetItem(index); // kích hoạt ListChanged -> cập nhật grid + StatusStrip
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            var result = MessageBox.Show("Bạn có chắc muốn xóa sản phẩm này?", "Xác nhận xóa",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                _products.RemoveAt(dgvProducts.CurrentRow.Index);
                ClearInputs();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        private void ClearInputs()
        {
            txtId.Clear();
            txtName.Clear();
            cboCategory.SelectedIndex = 0;
            numPrice.Value = 0;
            numQuantity.Value = 0;
            _currentImagePath = "";
            if (picProductImage.Image != null)
            {
                picProductImage.Image.Dispose();
                picProductImage.Image = null;
            }
            txtId.Focus();
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null || dgvProducts.CurrentRow.Index >= _products.Count) return;

            var p = _products[dgvProducts.CurrentRow.Index];
            txtId.Text = p.Id;
            txtName.Text = p.Name;
            cboCategory.Text = p.Category;
            numPrice.Value = Math.Min(p.Price, numPrice.Maximum);
            numQuantity.Value = Math.Min(p.Quantity, (int)numQuantity.Maximum);
            _currentImagePath = p.ImagePath;
            LoadImage(p.ImagePath);
        }

        // =====================================================
        //  STATUS STRIP
        // =====================================================
        private void UpdateStatus()
        {
            int totalQty = _products.Sum(p => p.Quantity);
            decimal totalValue = _products.Sum(p => p.TotalValue);
            lblCount.Text = $"Số lượng: {totalQty:N0} ({_products.Count} mặt hàng)";
            lblTotal.Text = $"Tổng giá trị kho: {totalValue:N0}";
        }

        // =====================================================
        //  EXPORT / IMPORT CSV
        // =====================================================
        private void ExportCsv()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Xuất dữ liệu CSV";
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.FileName = "kho_hang.csv";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(string.Join(CsvDelimiter.ToString(),
                        "Id", "Name", "Category", "Price", "Quantity", "ImagePath"));

                    foreach (var p in _products)
                    {
                        sb.AppendLine(string.Join(CsvDelimiter.ToString(),
                            Escape(p.Id),
                            Escape(p.Name),
                            Escape(p.Category),
                            p.Price.ToString(CultureInfo.InvariantCulture),
                            p.Quantity.ToString(CultureInfo.InvariantCulture),
                            Escape(p.ImagePath)));
                    }

                    // UTF-8 có BOM để Excel đọc đúng tiếng Việt
                    File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                    MessageBox.Show("Xuất CSV thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi ghi file: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ImportCsv()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn file CSV";
                ofd.Filter = "CSV (*.csv)|*.csv|Tất cả (*.*)|*.*";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                    if (lines.Length == 0) return;

                    // Tự nhận dạng dấu phân cách ; hoặc ,
                    char delim = lines[0].Contains(';') ? ';' : ',';

                    var imported = new List<Product>();
                    int errors = 0;

                    // Bỏ dòng tiêu đề (i = 1)
                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        var f = SplitCsvLine(lines[i], delim);
                        decimal price;
                        int qty;
                        if (f.Count < 5 ||
                            !decimal.TryParse(f[3], NumberStyles.Any, CultureInfo.InvariantCulture, out price) ||
                            !int.TryParse(f[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out qty))
                        {
                            errors++;
                            continue;
                        }

                        imported.Add(new Product
                        {
                            Id = f[0],
                            Name = f[1],
                            Category = f[2],
                            Price = price,
                            Quantity = qty,
                            ImagePath = f.Count > 5 ? f[5] : ""
                        });
                    }

                    // Nạp lại vào BindingList
                    _products.RaiseListChangedEvents = false;
                    _products.Clear();
                    foreach (var p in imported) _products.Add(p);
                    _products.RaiseListChangedEvents = true;
                    _products.ResetBindings();

                    ClearInputs();
                    MessageBox.Show($"Đã nhập {imported.Count} sản phẩm." + (errors > 0 ? $"\nBỏ qua {errors} dòng lỗi." : ""),
                        "Import CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi đọc file: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.IndexOfAny(new[] { ';', ',', '"', '\n', '\r' }) >= 0)
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        private static List<string> SplitCsvLine(string line, char delim)
        {
            var result = new List<string>();
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
                else
                {
                    if (c == '"') inQuotes = true;
                    else if (c == delim) { result.Add(sb.ToString()); sb.Clear(); }
                    else sb.Append(c);
                }
            }
            result.Add(sb.ToString());
            return result;
        }

        // =====================================================
        //  SAVE REPORT (báo cáo tổng hợp dạng text)
        // =====================================================
        private void SaveReport()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Lưu báo cáo tổng hợp";
                sfd.Filter = "Text (*.txt)|*.txt";
                sfd.FileName = "bao_cao_kho.txt";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("BÁO CÁO TỔNG HỢP KHO HÀNG");
                    sb.AppendLine("Thời gian: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    sb.AppendLine(new string('-', 70));
                    sb.AppendLine($"{"Mã",-10}{"Tên",-25}{"SL",8}{"Đơn giá",12}{"Thành tiền",15}");
                    foreach (var p in _products)
                        sb.AppendLine($"{p.Id,-10}{p.Name,-25}{p.Quantity,8}{p.Price,12:N0}{p.TotalValue,15:N0}");
                    sb.AppendLine(new string('-', 70));
                    sb.AppendLine($"Tổng số lượng : {_products.Sum(p => p.Quantity):N0}");
                    sb.AppendLine($"Tổng giá trị  : {_products.Sum(p => p.TotalValue):N0}");

                    File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                    MessageBox.Show("Đã lưu báo cáo!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi ghi file: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // =====================================================
        //  THOÁT CHƯƠNG TRÌNH (Yes/No)
        // =====================================================
        private void frmInventoryManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                e.Cancel = true;
        }
    }
}
