using AutoUpdaterDotNET;
using CommunityToolkit.Mvvm.Messaging;
using CyberSlacker.Models;
using CyberSlacker.Properties;
using CyberSlacker.Services;
using CyberSlacker.Util;
using CyberSlacker.ViewModels;
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shell;
using System.Windows.Threading;
using static CyberSlacker.Util.Interop;

namespace CyberSlacker
{
    public partial class MainWindow : Window
    {
        private bool _isTopmost = false;
        IntPtr shellView = IntPtr.Zero;
        private double _windowsScalingFactor;
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            // 加载保存的值
            this.Left = Properties.Settings.Default.WindowLeft;
            this.Top = Properties.Settings.Default.WindowTop;
            this.Width = Properties.Settings.Default.WindowWidth;
            this.Height = Properties.Settings.Default.WindowHeight;

            // 注入并持有引用
            _vm = new MainViewModel();
            this.DataContext = _vm;

            AutoUpdater.DownloadPath = Path.Combine(Path.GetTempPath(), "CyberSlackerUpdates");
            string arch = RuntimeInformation.ProcessArchitecture.ToString().ToLower();
            AutoUpdater.Start($"https://raw.githubusercontent.com/objectyan/CyberSlacker/main/Update_{arch}.xml");
            AutoUpdater.ShowSkipButton = true;      // 允许跳过此版本
            AutoUpdater.ShowRemindLaterButton = true; // 允许稍后提醒
            AutoUpdater.RunUpdateAsAdmin = true;


            WeakReferenceMessenger.Default.Register<string[], string>(this, "NotifyOffWork", (r, m) =>
            {
                // m[0] 是标题，m[1] 是内容
                string title = m[0];
                string content = m[1];

                this.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        MyNotifyIcon.ShowNotification(
                            title,
                            content,
                            H.NotifyIcon.Core.NotificationIcon.None);
                    }
                    catch
                    {
                    }
                });
            });

            WeakReferenceMessenger.Default.Register<string[], string>(this, "NotifyMeal", (r, m) =>
            {
                // m[0] 是标题，m[1] 是内容
                string title = m[0];
                string content = m[1];

                this.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        MyNotifyIcon.ShowNotification(
                            title,
                            content,
                            H.NotifyIcon.Core.NotificationIcon.None);
                    }
                    catch
                    {
                    }
                });
            });

            WeakReferenceMessenger.Default.Register<string[], string>(this, "NotifyRest", (r, m) =>
            {
                // m[0] 是标题，m[1] 是内容
                string title = m[0];
                string content = m[1];

                this.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        MyNotifyIcon.ShowNotification(
                            title,
                            content,
                            H.NotifyIcon.Core.NotificationIcon.None);
                    }
                    catch
                    {
                    }
                });
            });

            this.Loaded += MainWindow_Loaded;

            // 订阅窗口关闭事件进行资源释放
            this.Closed += (s, e) =>
            {
                _vm.Dispose();
                WeakReferenceMessenger.Default.UnregisterAll(this);
            };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.WindowLeft > 0)
            {
                this.Left = Settings.Default.WindowLeft;
                this.Top = Settings.Default.WindowTop;
            }
            this.SetBinding(Window.OpacityProperty, new Binding("Opacity") { Source = Properties.Settings.Default });
            EnsureWindowIsVisible();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.Owner = this;
            sw.ShowDialog();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                ReleaseCapture();
                SendMessage(hWnd, 0xA1, 0x2, 0);

                // 获取精确的物理位置
                if (GetWindowRect(hWnd, out RECT rect))
                {
                    Properties.Settings.Default.WindowLeft = rect.Left;
                    Properties.Settings.Default.WindowTop = rect.Top;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void EnsureWindowIsVisible()
        {
            double vLeft = SystemParameters.VirtualScreenLeft;
            double vTop = SystemParameters.VirtualScreenTop;
            double vWidth = SystemParameters.VirtualScreenWidth;
            double vHeight = SystemParameters.VirtualScreenHeight;

            this.Left = Properties.Settings.Default.WindowLeft;
            this.Top = Properties.Settings.Default.WindowTop;

            // 检查当前坐标是否在所有显示器围成的“大画布”内
            bool isVisible = (this.Left >= vLeft &&
                              this.Left < (vLeft + vWidth - 50) &&
                              this.Top >= vTop &&
                              this.Top < (vTop + vHeight - 50));

            if (!isVisible)
            {
                // 跑丢了，重置回主屏幕坐标
                this.Left = 100;
                this.Top = 100;

                // 同步更新保存的设置
                Properties.Settings.Default.WindowLeft = 100;
                Properties.Settings.Default.WindowTop = 100;
                Properties.Settings.Default.Save();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            try
            {
                MyNotifyIcon.Icon = Properties.Resources.app;
            }
            catch
            {
                MyNotifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }


            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int exStyle = (int)Interop.GetWindowLong(hwnd, Interop.GWL_EXSTYLE);
            Interop.SetWindowLong(hwnd, Interop.GWL_EXSTYLE, exStyle | Interop.WS_EX_NOACTIVATE);
            KeepWindowBehind();
            SetAsDesktopChild();
            SetNoActivate();
            SetAsToolWindow();
        }

        private void KeepWindowBehind()
        {
            if (_isTopmost)
            {
                return;
            }
            IntPtr HWND_BOTTOM = new IntPtr(1);
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            Interop.SetWindowPos(hwnd, HWND_BOTTOM, 0, 0, 0, 0, Interop.SWP_NOREDRAW | Interop.SWP_NOACTIVATE | Interop.SWP_NOMOVE | Interop.SWP_NOSIZE);
        }

        private void SetAsDesktopChild()
        {
            while (true)
            {
                while (shellView == IntPtr.Zero)
                {
                    EnumWindows((tophandle, _) =>
                    {
                        IntPtr shellViewIntPtr = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null);
                        if (shellViewIntPtr != IntPtr.Zero)
                        {
                            shellView = shellViewIntPtr;
                            return false;
                        }
                        return true;
                    }, IntPtr.Zero);
                }
                if (shellView == IntPtr.Zero) Thread.Sleep(1000);
                else break;
            }
            if (shellView == IntPtr.Zero) throw new InvalidOperationException("SHELLDLL_DefView not found.");

            var interopHelper = new WindowInteropHelper(this);
            interopHelper.EnsureHandle();
            IntPtr hwnd = interopHelper.Handle;
            SetParent(hwnd, shellView);

            int style = (int)GetWindowLong(hwnd, GWL_STYLE);
            style &= ~WS_POPUP; // remove flag, to make sure it doesn't interfere
            style |= WS_CHILD; // add flag
            SetWindowLong(hwnd, GWL_STYLE, style);

            // convert coords to parent-relative coords
            uint dpi = GetDpiForWindow(hwnd);
            _windowsScalingFactor = dpi / 96.0;
            POINT pt = new POINT
            {
                X = (int)(100 * _windowsScalingFactor),
                Y = (int)(100 * _windowsScalingFactor)
            };
            ScreenToClient(shellView, ref pt);
        }

        public void SetNoActivate()
        {
            if (_isTopmost)
            {
                return;
            }
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            IntPtr style = Interop.GetWindowLong(hwnd, Interop.GWL_EXSTYLE);
            IntPtr newStyle = new IntPtr(style.ToInt64() | Interop.WS_EX_NOACTIVATE);
            Interop.SetWindowLong(hwnd, Interop.GWL_EXSTYLE, newStyle);
        }

        public void SetAsToolWindow()
        {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            IntPtr dwNew = new IntPtr(((long)Interop.GetWindowLong(wih.Handle, Interop.GWL_EXSTYLE).ToInt32() | 128L | 0x00200000L) & 4294705151L);
            Interop.SetWindowLong((nint)new HandleRef(this, wih.Handle), Interop.GWL_EXSTYLE, dwNew);
        }
    }
}