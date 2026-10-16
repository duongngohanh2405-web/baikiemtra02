namespace DeviceManager
{
    partial class frmDeviceManager
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            grpInput = new GroupBox();
            lblId = new Label();
            lblName = new Label();
            lblCategory = new Label();
            lblPrice = new Label();
            lblQuantity = new Label();
            txtDeviceId = new TextBox();
            txtDeviceName = new TextBox();
            cboCategory = new ComboBox();
            txtPrice = new TextBox();
            numQuantity = new NumericUpDown();
            btnAdd = new Button();
            btnUpdate = new Button();
            btnDelete = new Button();
            btnClear = new Button();
            dgvDevices = new DataGridView();
            errorProvider = new ErrorProvider(components);
            grpInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numQuantity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvDevices).BeginInit();
            ((System.ComponentModel.ISupportInitialize)errorProvider).BeginInit();
            SuspendLayout();
            // 
            // grpInput
            // 
            grpInput.Controls.Add(lblId);
            grpInput.Controls.Add(lblName);
            grpInput.Controls.Add(lblCategory);
            grpInput.Controls.Add(lblPrice);
            grpInput.Controls.Add(lblQuantity);
            grpInput.Controls.Add(txtDeviceId);
            grpInput.Controls.Add(txtDeviceName);
            grpInput.Controls.Add(cboCategory);
            grpInput.Controls.Add(txtPrice);
            grpInput.Controls.Add(numQuantity);
            grpInput.Location = new Point(12, 10);
            grpInput.Name = "grpInput";
            grpInput.Size = new Size(780, 150);
            grpInput.TabIndex = 0;
            grpInput.TabStop = false;
            grpInput.Text = "Thông tin thiết bị";
            // 
            // lblId
            // 
            lblId.AutoSize = true;
            lblId.Location = new Point(20, 33);
            lblId.Name = "lblId";
            lblId.Size = new Size(88, 23);
            lblId.TabIndex = 0;
            lblId.Text = "Mã thiết bị:";
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(20, 68);
            lblName.Name = "lblName";
            lblName.Size = new Size(94, 23);
            lblName.TabIndex = 1;
            lblName.Text = "Tên thiết bị:";
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(20, 103);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(100, 23);
            lblCategory.TabIndex = 2;
            lblCategory.Text = "Loại thiết bị:";
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Location = new Point(400, 33);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(66, 23);
            lblPrice.TabIndex = 3;
            lblPrice.Text = "Đơn giá:";
            // 
            // lblQuantity
            // 
            lblQuantity.AutoSize = true;
            lblQuantity.Location = new Point(400, 68);
            lblQuantity.Name = "lblQuantity";
            lblQuantity.Size = new Size(104, 23);
            lblQuantity.TabIndex = 4;
            lblQuantity.Text = "Số lượng tồn:";
            // 
            // txtDeviceId
            // 
            txtDeviceId.Location = new Point(130, 30);
            txtDeviceId.Name = "txtDeviceId";
            txtDeviceId.ReadOnly = true;
            txtDeviceId.Size = new Size(220, 30);
            txtDeviceId.TabIndex = 5;
            // 
            // txtDeviceName
            // 
            txtDeviceName.Location = new Point(130, 65);
            txtDeviceName.Name = "txtDeviceName";
            txtDeviceName.Size = new Size(220, 30);
            txtDeviceName.TabIndex = 0;
            // 
            // cboCategory
            // 
            cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategory.FormattingEnabled = true;
            cboCategory.Items.AddRange(new object[] { "Điện thoại", "Laptop", "Máy tính bảng", "Phụ kiện", "Tivi" });
            cboCategory.Location = new Point(130, 100);
            cboCategory.Name = "cboCategory";
            cboCategory.Size = new Size(220, 31);
            cboCategory.TabIndex = 1;
            // 
            // txtPrice
            // 
            txtPrice.Location = new Point(520, 30);
            txtPrice.Name = "txtPrice";
            txtPrice.Size = new Size(220, 30);
            txtPrice.TabIndex = 2;
            // 
            // numQuantity
            // 
            numQuantity.Location = new Point(520, 65);
            numQuantity.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numQuantity.Name = "numQuantity";
            numQuantity.Size = new Size(120, 30);
            numQuantity.TabIndex = 3;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(12, 172);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(120, 36);
            btnAdd.TabIndex = 1;
            btnAdd.Text = "Thêm mới";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(142, 172);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(120, 36);
            btnUpdate.TabIndex = 2;
            btnUpdate.Text = "Cập nhật";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(272, 172);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(120, 36);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Xóa";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(402, 172);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(150, 36);
            btnClear.TabIndex = 4;
            btnClear.Text = "Làm mới / Hủy";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // dgvDevices
            // 
            dgvDevices.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDevices.Location = new Point(12, 220);
            dgvDevices.Name = "dgvDevices";
            dgvDevices.Size = new Size(780, 325);
            dgvDevices.TabIndex = 5;
            dgvDevices.SelectionChanged += dgvDevices_SelectionChanged;
            // 
            // errorProvider
            // 
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProvider.ContainerControl = this;
            // 
            // frmDeviceManager
            // 
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(804, 561);
            Controls.Add(grpInput);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(btnClear);
            Controls.Add(dgvDevices);
            Font = new Font("Segoe UI", 10F);
            Name = "frmDeviceManager";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản lý danh mục thiết bị điện tử";
            grpInput.ResumeLayout(false);
            grpInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numQuantity).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvDevices).EndInit();
            ((System.ComponentModel.ISupportInitialize)errorProvider).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpInput;
        private Label lblId;
        private Label lblName;
        private Label lblCategory;
        private Label lblPrice;
        private Label lblQuantity;
        private TextBox txtDeviceId;
        private TextBox txtDeviceName;
        private ComboBox cboCategory;
        private TextBox txtPrice;
        private NumericUpDown numQuantity;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private DataGridView dgvDevices;
        private ErrorProvider errorProvider;
    }
}
