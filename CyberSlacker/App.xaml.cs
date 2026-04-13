using CyberSlacker.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;

namespace CyberSlacker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 使用 static 确保全局唯一，防止重复弹出窗口
        /// </summary>
        private static bool _isUpdateWindowOpen = false;

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


            InitNotify();

#if !DEBUG
            // 优化是否开机重启
            CyberSlacker.Util.StartupHelper.SetStartup(CyberSlacker.Properties.Settings.Default.IsAutoStart);
            CyberSlacker.Services.NativeUpdateService.StartUpdateFlow();
#endif


            base.OnStartup(e);
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


        /// <summary>
        /// 初始化消息提示
        /// </summary>
        private void InitNotify()
        {

            // 订阅激活事件
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // 异步解析点击时传递的参数
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // 必须回到 UI 线程操作窗口
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (args.Contains("action"))
                    {
                        string action = args["action"];

                        switch (action)
                        {
                            case "main":
                                var mainWin = Application.Current.MainWindow as MainWindow;
                                if (mainWin != null)
                                {
                                    // 如果窗口是隐藏的 (Collapsed)，重新显示
                                    if (mainWin.Visibility != Visibility.Visible)
                                    {
                                        mainWin.Visibility = Visibility.Visible;
                                    }

                                    // 强力置顶并获取焦点
                                    mainWin.Show();
                                    mainWin.Activate();
                                    mainWin.Focus();

                                    // 赛博反馈：可以让挂件闪烁一下，告诉用户“我在这儿”
                                    mainWin.PlayFlashAnimation();
                                }
                                return;
                            default:
                                break;
                        }
                    }
                });
            };

            // 启动时清理历史旧账
            // 保证每次启动，通知中心都是干净的
            ToastNotificationManagerCompat.History.Clear();
        }


    }

}
