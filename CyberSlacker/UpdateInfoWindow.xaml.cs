using CyberSlacker.Properties;
using CyberSlacker.Services;
using Microsoft.Web.WebView2.Core;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace CyberSlacker
{
    /// <summary>
    /// UpdateInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateInfoWindow : Window
    {
        private readonly string _lastVersion = string.Empty;

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

                // 禁用左下角 URL 显示
                WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;

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


        private void CoreWebView2_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
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

        private async void WebView_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess) return;
            try
            {
                var resourceInfo = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/CyberInject.js"));
                using (var reader = new StreamReader(resourceInfo.Stream))
                {
                    string styleHide = @"(function () {
                                            const head = document.head || document.getElementsByTagName('head')[0];
                                            const fastHideStyle = document.createElement('style');
                                            fastHideStyle.innerHTML = 'body > *:not(#cyber-stage) { display: none !important; }';
                                            head.appendChild(fastHideStyle);
                                        })();";
                    await WebView.CoreWebView2.ExecuteScriptAsync(styleHide);
                    await Task.Delay(100);
                    string jsCode = reader.ReadToEnd();
                    WebView.Visibility = Visibility.Visible;
                    await WebView.CoreWebView2.ExecuteScriptAsync(jsCode);
                    await Task.Delay(100);
                    DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
                    fadeOut.Completed += (s, ev) => LoadingStack.Visibility = Visibility.Collapsed;
                    LoadingStack.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                }
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
