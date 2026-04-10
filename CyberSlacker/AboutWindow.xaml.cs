using CyberSlacker.Services;
using CyberSlacker.Util;
using Serilog;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace CyberSlacker
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            // 1. 获取版本号
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            // 显示 3 位或 4 位版本
            if (version != null)
            {
                VersionText.Text = $"Version {version.Major}.{version.Minor}.{version.Build}";

                // 如果第四位（Revision）大于 0，说明是流水线自动生成的预览版
                if (version.Revision > 0)
                {
                    VersionText.Text += $".{version.Revision}"; // 补全第四位
                    PreviewBadge.Visibility = Visibility.Visible; // 显示 PREVIEW 标签
                    this.Title += "(Preview)";
                }
            }
        }

        private void OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                NativeToastService.Show("错误", "无法打开链接: " + ex.Message);
            }
        }

        private async void OnCheckUpdate(object sender, RoutedEventArgs e)
        {

            this.updateBtn.IsEnabled = false;
            this.updateBtn.Content = "正在检查基因中...";

            try
            {
                var info = await CyberSlacker.Services.NativeUpdateService.CheckForUpdateAsync();

                if (info != null)
                {
                    WindowManager.ShowUnique<UpdateInfoWindow>(
                           null,
                           () => new UpdateInfoWindow(info)
                    );
                }
            }
            catch (Exception ex)
            {
                // 网络报错处理
                Log.Error("赛博链路异常，无法连接 GitHub", ex);
            }
            finally
            {
                // 无论成功失败，恢复按钮状态
                this.updateBtn.IsEnabled = true;
                this.updateBtn.Content = "检 查 更 新";
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // 关闭按钮逻辑
        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
