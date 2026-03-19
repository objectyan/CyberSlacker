using AutoUpdaterDotNET;
using CyberSlacker.Services;
using Serilog;
using System.Configuration;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

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
            return $"https://raw.githubusercontent.com/objectyan/CyberSlacker/master/Update_{arch}.xml";
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information(">>> 赛博摸鱼员 正常关闭 <<<");
            Log.CloseAndFlush(); 
            base.OnExit(e);
        }
    }

}
