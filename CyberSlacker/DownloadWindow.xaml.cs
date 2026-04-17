using CyberSlacker.Services;
using CyberSlacker.Util;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace CyberSlacker
{
    public partial class DownloadWindow : Window
    {
        private readonly string _downloadUrl;
        private readonly string _tempFilePath;

        private bool _isDownloadFinished = false;

        public DownloadWindow(string url)
        {
            InitializeComponent();
            _downloadUrl = url;
            // 存放在系统临时目录，避免权限问题
            _tempFilePath = Path.Combine(Path.GetTempPath(), "CyberSlacker_Update.msi");

            this.Loaded += (s, e) => StartDownload();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // 如果下载还没结束，就点不动 X，也按不了 Alt+F4
            if (!_isDownloadFinished)
            {
                e.Cancel = true;
                // 提示一下（可选，更人性化）
                // ShowMsg("正在注入基因，请勿中断..."); 
            }

            base.OnClosing(e);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            // 获取系统菜单句柄
            IntPtr hMenu = Util.Interop.GetSystemMenu(hWnd, false);
            if (hMenu != IntPtr.Zero)
            {
                // 禁用关闭按钮 (SC_CLOSE = 0xF060)
                Interop.EnableMenuItem(hMenu, Interop.SC_CLOSE, Interop.MF_BYCOMMAND | Interop.MF_DISABLED | Interop.MF_GRAYED);
            }
        }

        private async void StartDownload()
        {
            try
            {
                // 获取文件大小
                var response = await HttpUtil.Client.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;

                using var downloadStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(_tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

                byte[] buffer = new byte[8192];
                long totalRead = 0;
                int read;

                while ((read = await downloadStream.ReadAsync(buffer.AsMemory())) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, read));
                    totalRead += read;

                    // 实时更新 WPF 界面
                    if (totalBytes != -1)
                    {
                        double progress = (double)totalRead / totalBytes;
                        ProgressBar.Width = (this.ActualWidth - 60) * progress; // 动态计算宽度
                        PercentTxt.Text = $"{(progress * 100):F0}%";
                        StatusTxt.Text = $"已下载: {(totalRead / 1024.0 / 1024.0):F2}MB / {(totalBytes / 1024.0 / 1024.0):F2}MB";
                    }
                }

                fileStream.Close();

                _isDownloadFinished = true;

                // 下载完成：启动安装包并闪人
                Process.Start(new ProcessStartInfo(_tempFilePath) { UseShellExecute = true });
                System.Windows.Application.Current.Shutdown();
                System.Environment.Exit(0);
            }
            catch (Exception ex)
            {
                _isDownloadFinished = true;
                NativeToastService.Show("更新失败", "下载失败，请重试: " + ex.Message);
                this.Close();
            }
        }
    }
}