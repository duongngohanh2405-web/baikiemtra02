using System.ComponentModel;
using System.Globalization;

namespace DeviceManager
{
    public partial class frmDeviceManager : Form
    {
        // ===== Dữ liệu =====
        private readonly BindingList<Device> _devices = new();
        private readonly BindingSource _bs = new();
        private int _counter = 0;

        public frmDeviceManager()
        {
            InitializeComponent();          // phần giao diện nằm trong frmDeviceManager.Designer.cs

            ConfigureGrid();                // cấu hình cột hoàn toàn bằng code
            LoadSampleData();

            // Ràng buộc dữ liệu: BindingList -> BindingSource -> DataGridView
            _bs.DataSource = _devices;
            dgvDevices.DataSource = _bs;

            ClearForm();
        }

        // ------------------------------------------------------------------
        // 1. CẤU HÌNH DATAGRIDVIEW BẰNG CODE (không dùng Designer sinh cột)
        // ------------------------------------------------------------------
        private void ConfigureGrid()
        {
            dgvDevices.AutoGenerateColumns = false;
            dgvDevices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDevices.MultiSelect = false;
            dgvDevices.ReadOnly = true;
            dgvDevices.AllowUserToAddRows = false;
            dgvDevices.AllowUserToDeleteRows = false;
            dgvDevices.RowHeadersVisible = false;
            dgvDevices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDevices.BackgroundColor = SystemColors.Window;

            dgvDevices.Columns.Clear();
            dgvDevices.Columns.Add(MakeColumn(nameof(Device.Id), "Mã thiết bị", 15));
            dgvDevices.Columns.Add(MakeColumn(nameof(Device.Name), "Tên thiết bị", 35));
            dgvDevices.Columns.Add(MakeColumn(nameof(Device.Category), "Loại", 20));

            var colPrice = MakeColumn(nameof(Device.Price), "Đơn giá", 20);
            colPrice.DefaultCellStyle.Format = "#,##0 'đ'";
            colPrice.DefaultCellStyle.FormatProvider = CultureInfo.GetCultureInfo("vi-VN");
            colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvDevices.Columns.Add(colPrice);

            var colQty = MakeColumn(nameof(Device.Quantity), "Tồn kho", 10);
            colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDevices.Columns.Add(colQty);
        }

        private static DataGridViewTextBoxColumn MakeColumn(string prop, string header, float weight) =>
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = prop,
                Name = "col" + prop,
                HeaderText = header,
                FillWeight = weight
            };

        private void LoadSampleData()
        {
            AddDevice("iPhone 15", "Điện thoại", 22000000, 10);
            AddDevice("Dell XPS 13", "Laptop", 32000000, 5);
            AddDevice("iPad Air", "Máy tính bảng", 16500000, 8);
        }

        private void AddDevice(string name, string cat, decimal price, int qty) =>
            _devices.Add(new Device { Id = NextId(), Name = name, Category = cat, Price = price, Quantity = qty });

        private string NextId() => $"TB{++_counter:000}";

        // ------------------------------------------------------------------
        // 2. VALIDATION + ERRORPROVIDER
        // ------------------------------------------------------------------
        private bool ValidateInput(out decimal price)
        {
            bool ok = true;
            price = 0;
            errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(txtDeviceName.Text))
            {
                errorProvider.SetError(txtDeviceName, "Tên thiết bị không được để trống!");
                ok = false;
            }

            if (cboCategory.SelectedIndex < 0)
            {
                errorProvider.SetError(cboCategory, "Vui lòng chọn loại thiết bị!");
                ok = false;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out price) || price < 0)
            {
                errorProvider.SetError(txtPrice, "Đơn giá phải là số hợp lệ và không nhỏ hơn 0!");
                ok = false;
            }

            return ok;
        }

        // ------------------------------------------------------------------
        // 3. ĐỒNG BỘ DỮ LIỆU 2 CHIỀU
        // ------------------------------------------------------------------
        // Grid -> Form
        private void dgvDevices_SelectionChanged(object sender, EventArgs e)
        {
            if (_bs.Current is Device d && dgvDevices.SelectedRows.Count > 0)
            {
                txtDeviceId.Text = d.Id;
                txtDeviceName.Text = d.Name;
                cboCategory.SelectedItem = d.Category;
                txtPrice.Text = d.Price.ToString("0.##", CultureInfo.CurrentCulture);
                numQuantity.Value = d.Quantity;
                errorProvider.Clear();
            }
        }

        // Thêm
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out decimal price)) return;

            _devices.Add(new Device
            {
                Id = NextId(),
                Name = txtDeviceName.Text.Trim(),
                Category = cboCategory.SelectedItem.ToString(),
                Price = price,
                Quantity = (int)numQuantity.Value
            });

            MessageBox.Show("Thêm thiết bị thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearForm();
        }

        // Cập nhật
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_bs.Current is not Device current || dgvDevices.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn thiết bị cần cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateInput(out decimal price)) return;

            current.Name = txtDeviceName.Text.Trim();
            current.Category = cboCategory.SelectedItem.ToString();
            current.Price = price;
            current.Quantity = (int)numQuantity.Value;

            MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Xóa
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_bs.Current is not Device current || dgvDevices.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn thiết bị cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rs = MessageBox.Show($"Bạn có chắc muốn xóa thiết bị \"{current.Name}\" không?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (rs == DialogResult.Yes)
            {
                _bs.Remove(current);
                ClearForm();
            }
        }

        // Làm mới / Hủy
        private void btnClear_Click(object sender, EventArgs e) => ClearForm();

        private void ClearForm()
        {
            errorProvider.Clear();
            dgvDevices.ClearSelection();
            txtDeviceId.Text = $"TB{_counter + 1:000}";
            txtDeviceName.Clear();
            cboCategory.SelectedIndex = -1;
            txtPrice.Clear();
            numQuantity.Value = 0;
            txtDeviceName.Focus();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ClearForm();    // bỏ chọn dòng đầu tiên khi mới mở form
        }
    }
}
