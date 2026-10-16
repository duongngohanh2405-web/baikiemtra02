using System.Windows.Forms;

namespace SuperMarketMDI;

/// <summary>Dữ liệu giả lập do tiến trình đồng bộ nền gửi về.</summary>
public record SyncData(DateTime Time, decimal UsdRate, int NewOrders);

public class frmMain : Form
{
    private readonly string _userName;

    private MenuStrip menuStrip;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblUser, lblSync;
    private ToolStripProgressBar progressBar;

    // Đếm số tác vụ nặng đang chạy -> ProgressBar dùng chung
    private int _busyCount;

    // Tiến trình đồng bộ nền
    private CancellationTokenSource _cts = new();

    /// <summary>Sự kiện phát ra từ THREAD PHỤ. Nơi nhận phải tự Invoke nếu đụng vào UI.</summary>
    public event Action<SyncData> SyncDataReceived;

    public frmMain(string userName)
    {
        _userName = userName;
        BuildUi();
        Load += (s, e) => StartSyncLoop();
        FormClosing += (s, e) => _cts.Cancel();
    }

    // ===================== UI =====================
    private void BuildUi()
    {
        Text = "Hệ thống quản lý siêu thị (MDI)";
        IsMdiContainer = true;
        WindowState = FormWindowState.Maximized;

        // --- MenuStrip ---
        menuStrip = new MenuStrip();

        var mnuSystem = new ToolStripMenuItem("Hệ thống");
        mnuSystem.DropDownItems.Add("Đăng xuất", null, (s, e) => Logout());
        mnuSystem.DropDownItems.Add(new ToolStripSeparator());
        mnuSystem.DropDownItems.Add("Thoát", null, (s, e) => Close());

        var mnuCatalog = new ToolStripMenuItem("Danh mục");
        mnuCatalog.DropDownItems.Add("Quản lý Sản phẩm", null, (s, e) => OpenChild<frmProductCatalog>());
        mnuCatalog.DropDownItems.Add("Quản lý Nhập xuất", null,
            (s, e) => MessageBox.Show("Chức năng sẽ mở rộng ở bài sau.", "Thông báo"));

        var mnuReport = new ToolStripMenuItem("Báo cáo");
        mnuReport.DropDownItems.Add("Thống kê Doanh thu", null, (s, e) => OpenChild<frmAsyncReport>());

        var mnuWindow = new ToolStripMenuItem("Cửa sổ");
        mnuWindow.DropDownItems.Add("Cascade", null, (s, e) => LayoutMdi(MdiLayout.Cascade));
        mnuWindow.DropDownItems.Add("Tile Horizontal", null, (s, e) => LayoutMdi(MdiLayout.TileHorizontal));
        mnuWindow.DropDownItems.Add("Tile Vertical", null, (s, e) => LayoutMdi(MdiLayout.TileVertical));
        menuStrip.MdiWindowListItem = mnuWindow; // hiện danh sách cửa sổ đang mở

        menuStrip.Items.AddRange(new ToolStripItem[] { mnuSystem, mnuCatalog, mnuReport, mnuWindow });
        MainMenuStrip = menuStrip;

        // --- StatusStrip ---
        statusStrip = new StatusStrip();
        lblUser = new ToolStripStatusLabel($"Người dùng: {_userName}") { BorderSides = ToolStripStatusLabelBorderSides.Right };
        lblSync = new ToolStripStatusLabel("Đồng bộ: đang chờ...") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
        progressBar = new ToolStripProgressBar
        {
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 30,
            Visible = false,
            Width = 150
        };
        statusStrip.Items.AddRange(new ToolStripItem[] { lblUser, lblSync, progressBar });

        Controls.Add(menuStrip);
        Controls.Add(statusStrip);
    }

    // ===================== Scenario A: điều hướng MDI =====================
    private void OpenChild<T>() where T : Form, new()
    {
        var existing = MdiChildren.OfType<T>().FirstOrDefault();
        if (existing != null)
        {
            if (existing.WindowState == FormWindowState.Minimized)
                existing.WindowState = FormWindowState.Normal;
            existing.Activate();
            return;
        }

        var child = new T { MdiParent = this };
        child.Show();
    }

    private void Logout()
    {
        if (MessageBox.Show("Bạn muốn đăng xuất?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        Application.Restart(); // quay về màn hình đăng nhập
    }

    // ===================== ProgressBar dùng chung =====================
    public void BeginBusy()
    {
        if (++_busyCount == 1) progressBar.Visible = true;
    }

    public void EndBusy()
    {
        if (--_busyCount <= 0)
        {
            _busyCount = 0;
            progressBar.Visible = false;
        }
    }

    // ===================== Scenario C: đồng bộ thời gian thực =====================
    private void StartSyncLoop()
    {
        var token = _cts.Token;

        // Chạy trên thread phụ, lặp mỗi 2 giây
        Task.Run(async () =>
        {
            var rnd = new Random();
            decimal rate = 25000m;

            while (!token.IsCancellationRequested)
            {
                try { await Task.Delay(2000, token); }
                catch (TaskCanceledException) { break; }

                rate += rnd.Next(-30, 31);
                var data = new SyncData(DateTime.Now, rate, rnd.Next(0, 8));

                // Cập nhật StatusStrip: BẮT BUỘC chuyển về UI Thread
                UpdateStatus(data);

                // Báo cho các Form con (chúng tự Invoke khi cập nhật DataGridView)
                SyncDataReceived?.Invoke(data);
            }
        }, token);
    }

    private void UpdateStatus(SyncData d)
    {
        if (IsDisposed) return;

        if (InvokeRequired)
        {
            try { Invoke(new Action(() => UpdateStatus(d))); }
            catch (ObjectDisposedException) { }  // form đã đóng
            catch (InvalidOperationException) { } // handle chưa/không còn tồn tại
            return;
        }

        lblSync.Text = $"Đồng bộ {d.Time:HH:mm:ss} | Tỷ giá USD: {d.UsdRate:N0} | Đơn mới: {d.NewOrders}";
    }
}
