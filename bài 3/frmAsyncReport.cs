using System.Data;
using System.Windows.Forms;

namespace SuperMarketMDI;

public class frmAsyncReport : Form
{
    private Button btnLoad;
    private Label lblState;
    private DataGridView dgvReport, dgvSync;
    private frmMain _main;

    public frmAsyncReport()
    {
        Text = "Báo cáo Thống kê & Đồng bộ";
        Width = 800;
        Height = 560;
        BuildUi();

        Load += (s, e) =>
        {
            _main = MdiParent as frmMain;
            if (_main != null) _main.SyncDataReceived += OnSyncDataReceived;
        };
        // Hủy đăng ký để thread nền không gọi vào Form đã đóng
        FormClosing += (s, e) =>
        {
            if (_main != null) _main.SyncDataReceived -= OnSyncDataReceived;
        };
    }

    private void BuildUi()
    {
        btnLoad = new Button { Text = "Tải Báo Cáo Doanh Thu", Left = 10, Top = 10, Width = 200, Height = 32 };
        btnLoad.Click += btnLoad_Click;

        lblState = new Label { Left = 220, Top = 18, Width = 500, Text = "Sẵn sàng." };

        var grpReport = new GroupBox { Text = "Doanh thu theo ngành hàng", Left = 10, Top = 50, Width = 760, Height = 220, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        dgvReport = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        grpReport.Controls.Add(dgvReport);

        var grpSync = new GroupBox { Text = "Nhật ký đồng bộ thời gian thực (mỗi 2 giây)", Left = 10, Top = 280, Width = 760, Height = 230, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
        dgvSync = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        dgvSync.Columns.Add("colTime", "Thời gian");
        dgvSync.Columns.Add("colRate", "Tỷ giá USD");
        dgvSync.Columns.Add("colOrders", "Đơn hàng mới");
        grpSync.Controls.Add(dgvSync);

        Controls.AddRange(new Control[] { btnLoad, lblState, grpReport, grpSync });
    }

    // ===================== Scenario B: async/await + Task.Run =====================
    private async void btnLoad_Click(object sender, EventArgs e)
    {
        btnLoad.Enabled = false;
        lblState.Text = "Đang tính toán, vui lòng chờ...";
        _main?.BeginBusy();                       // bật ProgressBar ở StatusStrip Form cha

        try
        {
            // Đẩy việc nặng xuống thread pool; UI Thread được giải phóng tại 'await'
            DataTable table = await Task.Run(() => BuildRevenueReport());

            // Sau await, code tự quay lại UI Thread -> gán trực tiếp an toàn
            dgvReport.DataSource = table;
            lblState.Text = $"Hoàn tất lúc {DateTime.Now:HH:mm:ss}.";
        }
        catch (Exception ex)
        {
            lblState.Text = "Lỗi: " + ex.Message;
        }
        finally
        {
            _main?.EndBusy();                     // tắt ProgressBar
            btnLoad.Enabled = true;
        }
    }

    /// <summary>Chạy trên thread phụ: KHÔNG được đụng vào Control ở đây.</summary>
    private static DataTable BuildRevenueReport()
    {
        // Giả lập truy xuất CSDL / tính toán nặng ~4 giây
        Thread.Sleep(2500);
        double acc = 0;
        for (int i = 1; i < 60_000_000; i++) acc += Math.Sqrt(i) % 7;   // tính toán lặp nặng
        Thread.Sleep(1000);

        var rnd = new Random();
        var dt = new DataTable();
        dt.Columns.Add("Ngành hàng", typeof(string));
        dt.Columns.Add("Số đơn", typeof(int));
        dt.Columns.Add("Doanh thu (VNĐ)", typeof(decimal));

        foreach (var name in new[] { "Thực phẩm tươi sống", "Đồ uống", "Bánh kẹo", "Hóa mỹ phẩm", "Gia dụng" })
            dt.Rows.Add(name, rnd.Next(200, 2000), rnd.Next(20, 500) * 1_000_000m);

        return dt;
    }

    // ===================== Scenario C: cập nhật DataGridView an toàn =====================
    // Hàm này được gọi từ THREAD PHỤ (do frmMain phát sự kiện)
    private void OnSyncDataReceived(SyncData d)
    {
        if (IsDisposed || !IsHandleCreated) return;

        if (InvokeRequired)                       // đang ở thread phụ?
        {
            try { Invoke(new Action(() => OnSyncDataReceived(d))); }   // chuyển về UI Thread
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            return;
        }

        // Từ đây là UI Thread -> an toàn
        dgvSync.Rows.Insert(0, d.Time.ToString("HH:mm:ss"), d.UsdRate.ToString("N0"), d.NewOrders);
        if (dgvSync.Rows.Count > 50) dgvSync.Rows.RemoveAt(dgvSync.Rows.Count - 1); // giữ tối đa 50 dòng
    }
}
