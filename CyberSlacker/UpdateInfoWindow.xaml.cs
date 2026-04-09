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
                Application.Current.Shutdown();
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
