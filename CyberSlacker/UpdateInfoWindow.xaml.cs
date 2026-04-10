using AutoUpdaterDotNET;
using CyberSlacker.Properties;
using Microsoft.Web.WebView2.Core;
using Serilog;
using System.Windows;

namespace CyberSlacker
{
    /// <summary>
    /// UpdateInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateInfoWindow : Window
    {
        private UpdateInfoEventArgs _args;

        private string _lastVersion;

        public UpdateInfoWindow(UpdateInfoEventArgs args)
        {
            InitializeComponent();
            _args = args;

            if (System.Version.TryParse(args.CurrentVersion, out System.Version? remoteVersion))
            {
                if (remoteVersion.Revision > 0)
                {
                    TitleText.Text = "🛠️ 发现新基因 (Preview)";
                    TitleText.Foreground = System.Windows.Media.Brushes.Orange;
                }

                VersionFlow.Text = $"{args.InstalledVersion}  ➡  {remoteVersion}";

                _lastVersion = remoteVersion.ToString();
            }
            else
            {
                VersionFlow.Text = $"{args.InstalledVersion}  ➡  {args.CurrentVersion}";
                _lastVersion = args.CurrentVersion;
            }

            InitBrowser();
        }

        private async void InitBrowser()
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

                if (!string.IsNullOrEmpty(_args.ChangelogURL))
                {
                    WebView.Source = new Uri(_args.ChangelogURL);
                }
            }
            catch (Exception ex)
            {
                Log.Error("WebView2 初始化失败: ", ex);
            }
        }


        private void CoreWebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            // 如果当前的链接不是我们最初设置的 ChangelogURL (即用户点击了里面的链接)
            if (e.Uri != _args.ChangelogURL)
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
            if (AutoUpdater.DownloadUpdate(_args))
            {
                Settings.Default.LatestVersionSkipped = _lastVersion;
                Settings.Default.LastUpdateCheck = DateTime.Today;
                Settings.Default.Save();
                System.Windows.Application.Current.Shutdown();
                System.Environment.Exit(0);
            }
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
