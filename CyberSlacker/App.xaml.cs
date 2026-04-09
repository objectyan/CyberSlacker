using AutoUpdaterDotNET;
using CyberSlacker.Properties;
using CyberSlacker.Services;
using CyberSlacker.Util;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Animation;

namespace CyberSlacker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LogService.Initialize();
            AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
            {
                Log.Fatal(ev.ExceptionObject as Exception, "【致命错误】非UI线程崩溃");
                Log.CloseAndFlush();
            };
            this.DispatcherUnhandledException += (s, ev) =>
            {
                Log.Error(ev.Exception, "【UI错误】拦截到未处理的异常");
                ev.Handled = true;
            };

            // 强制软件渲染（如果显卡驱动在跨屏时有 Bug，开启这个反而会变流畅）
            System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            // 强制降低非活动状态下的资源占用
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 20 } // 桌面挂件 20 帧足够，能省电
            );

            // 优化是否开机重启
            StartupHelper.SetStartup(Settings.Default.IsAutoStart);

            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;

#if !DEBUG
            AutoUpdaterDotNET.AutoUpdater.Start(App.GetUpdateUrl());      
#endif
            base.OnStartup(e);
        }

        /// <summary>
        /// 不自动弹出默认窗体
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {
                if (args.IsUpdateAvailable)
                {
                    var updateWin = new UpdateInfoWindow(args);

                    if (Current.MainWindow != null && Current.MainWindow.IsVisible)
                        updateWin.Owner = Current.MainWindow;

                    updateWin.ShowDialog();
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

        }

        /// <summary>
        /// 获取更新地址
        /// </summary>
        /// <returns></returns>
        public static string GetUpdateUrl()
        {
            AutoUpdater.DownloadPath = Path.Combine(Path.GetTempPath(), "CyberSlackerUpdates");
            AutoUpdater.RunUpdateAsAdmin = true;

            string arch = RuntimeInformation.ProcessArchitecture.ToString().ToLower();

            string prefix = Settings.Default.IsPreviewEnabled ? "Update_preview_" : "Update_";

            return $"https://raw.githubusercontent.com/objectyan/CyberSlacker/master/manifests/{prefix}{arch}.xml";
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information(">>> 赛博摸鱼员 正常关闭 <<<");
            Log.CloseAndFlush();
            base.OnExit(e);
        }



        // 提供一个手动清理内存的方法
        public static void FlushMemory()
        {
            try
            {
                using var process = Process.GetCurrentProcess();
                Util.Interop.EmptyWorkingSet(process.Handle);
            }
            catch { }
        }
    }

}
