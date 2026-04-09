using AutoUpdaterDotNET;
using System.Diagnostics;
using System.Windows;
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
                MessageBox.Show("无法打开链接: " + ex.Message);
            }
        }

        private void OnCheckUpdate(object sender, RoutedEventArgs e)
        {
            this.updateBtn.IsEnabled = false;
            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;
            AutoUpdater.Start(App.GetUpdateUrl());
        }

        private void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            // 立即取消订阅，防止干扰其他地方的更新检查
            AutoUpdater.CheckForUpdateEvent -= AutoUpdater_CheckForUpdateEvent;
            this.updateBtn.IsEnabled = true;
        }
    }
}
