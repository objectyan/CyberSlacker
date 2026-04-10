using CyberSlacker.Properties;
using CyberSlacker.Services;
using Microsoft.Web.WebView2.Core;
using Serilog;
using System.Reflection;
using System.Windows;

namespace CyberSlacker
{
    /// <summary>
    /// UpdateInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateInfoWindow : Window
    {
        private string _lastVersion;

        public UpdateInfoWindow(UpdateInfo info)
        {
            InitializeComponent();

            // 获取本地版本
            var localVersion = Assembly.GetExecutingAssembly().GetName().Version;
            string localVerStr = localVersion != null ? $"{localVersion.Major}.{localVersion.Minor}.{localVersion.Build}" : "Unknown";
            if (localVersion?.Revision > 0) localVerStr += $".{localVersion.Revision}";

            // 赋值（现在红线应该消失了）
            CurrentVerTxt.Text = localVerStr;
            NewVerTxt.Text = info.RemoteVersion.ToString();

            // 动态标题判定
            if (info.RemoteVersion.Revision > 0)
            {
                TitleText.Text = "🛠️ 发现新基因 (Preview)";
                TitleText.Foreground = System.Windows.Media.Brushes.Orange;
            }

            if (!string.IsNullOrEmpty(info.Changelog))
            {
                InitBrowser(info.Changelog);
            }

            this.Tag = info.DownloadUrl;
        }

        private async void InitBrowser(string changLogUrl)
        {
            try
            {
                var userDataFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CyberSlacker", "WebView2Data");

                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await WebView.EnsureCoreWebView2Async(env);


                WebView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;

                WebView.NavigationCompleted += WebView_NavigationCompleted;

                if (!string.IsNullOrEmpty(changLogUrl))
                {
                    WebView.Source = new Uri(changLogUrl);
                }
            }
            catch (Exception ex)
            {
                Log.Error("WebView2 初始化失败: ", ex);
            }
        }


        private void CoreWebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            // 如果当前的链接不是我们最初设置的 Changelog (即用户点击了里面的链接)
            if (e.Uri != this.WebView.Source.ToString())
            {
                // A. 拦截 WebView2 内部的跳转
                e.Cancel = true;

                // B. 唤醒系统默认浏览器打开该链接
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri)
                    {
                        UseShellExecute = true // .NET 8 下必须设为 true 才能打开 URL
                    });
                }
                catch (Exception ex)
                {
                    Log.Error("无法打开浏览器: ", ex);
                }
            }
        }

        private async void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess) return;
            try
            {
                string jsCode = @"
        (function() {
            var content = document.querySelector('.markdown-body');
            if (content) {
                document.body.innerHTML = content.outerHTML;
                
                document.body.style.backgroundColor = '#1A1A1A'; 
                document.body.style.color = '#EEEEEE';          
                document.body.style.padding = '20px';
                document.body.style.overflowX = 'hidden';

                var style = document.createElement('style');
                style.innerHTML = '* { border-color: #333 !important; } .markdown-body { background: transparent !important; }';
                document.head.appendChild(style);
            }
            
            document.body.style.msOverflowStyle = 'none'; 
        })();
    ";

                await WebView.CoreWebView2.ExecuteScriptAsync(jsCode);
                LoadingStack.Visibility = Visibility.Collapsed;
                WebView.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Log.Error("WebView2 执行脚本失败: ", ex);
            }
        }


        private void OnUpdate(object sender, RoutedEventArgs e)
        {
            // 1. 关掉当前的更新明细窗口
            this.Close();

            var downloadUrl = this.Tag as string;
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                Log.Error("更新失败：下载地址为空。");
                NativeToastService.Show("错误", "无法获取更新下载地址。");
                return;
            }

            // 2. 启动自定义下载进度窗口
            // 传入下载地址
            var downloadWin = new DownloadWindow(downloadUrl);
            downloadWin.Show();
        }

        private void OnSkip(object sender, RoutedEventArgs e)
        {
            Settings.Default.LatestVersionSkipped = _lastVersion;
            Settings.Default.LastUpdateCheck = DateTime.Today;
            Settings.Default.Save();
            this.Close();
        }
    }
}
