using AutoUpdaterDotNET;
using CyberSlacker.Properties;
using CyberSlacker.Services;
using Serilog;
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

            base.OnStartup(e);
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
    }

}
