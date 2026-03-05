using AutoUpdaterDotNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionText.Text = $"Version {version?.Major}.{version?.Minor}.{version?.Build}";
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

            if (args.Error == null)
            {
                if (args.IsUpdateAvailable)
                {
                    AutoUpdater.ShowUpdateForm(args);
                }
                else
                {
                    MessageBox.Show("当前已是最新版本，摸鱼愉快！", "检查更新",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("网络连接失败，请检查 GitHub 访问是否正常。", "提示");
            }
            this.updateBtn.IsEnabled = true;
        }
    }
}
