using AutoUpdaterDotNET;
using CyberSlacker.Properties;
using CyberSlacker.Services;
using CyberSlacker.Util;
using Microsoft.Toolkit.Uwp.Notifications;
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

            // 优化是否开机重启
            StartupHelper.SetStartup(Settings.Default.IsAutoStart);

#if !DEBUG
            AutoUpdaterDotNET.AutoUpdater.Start(App.GetUpdateUrl());      
#endif

            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;

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
                    if (_isUpdateWindowOpen) return;
                    _isUpdateWindowOpen = true;

                    // 动态寻找合适的 Owner
                    Window bestOwner = null;

                    // 按照窗口打开的逆序查找（通常最后打开的在最前面）
                    var windows = Application.Current.Windows.Cast<Window>().ToList();
                    for (int i = windows.Count - 1; i >= 0; i--)
                    {
                        var win = windows[i];
                        // 排除掉自己（防止死循环）且必须是可见的
                        if (win.IsVisible && win is not UpdateInfoWindow)
                        {
                            // 如果这个窗口正在被激活，那就是它了！
                            if (win.IsActive)
                            {
                                bestOwner = win;
                                break;
                            }
                            // 否则先保存在候选人里，以备没有 Active 窗口
                            bestOwner ??= win;
                        }
                    }

                    var updateWin = new UpdateInfoWindow(args);

                    if (bestOwner != null)
                    {
                        updateWin.Owner = bestOwner;
                        // 配合 CenterOwner，它会自动跳到 Owner 所在的那个显示器
                        updateWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    }
                    else
                    {
                        updateWin.Owner = Current.MainWindow;
                        // 如果连 MainWindow 都没有，就屏幕居中
                        updateWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    }

                    updateWin.Closed += (s, e) => { _isUpdateWindowOpen = false; };
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
