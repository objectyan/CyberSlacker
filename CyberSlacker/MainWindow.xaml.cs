using CommunityToolkit.Mvvm.Messaging;
using CyberSlacker.Properties;
using CyberSlacker.Util;
using CyberSlacker.ViewModels;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using static CyberSlacker.Util.Interop;

namespace CyberSlacker
{
    public partial class MainWindow : Window
    {
        IntPtr shellView = IntPtr.Zero;
        private double _windowsScalingFactor;
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            // 加载保存的值
            this.Left = Settings.Default.WindowLeft;
            this.Top = Settings.Default.WindowTop;
            this.Width = Settings.Default.WindowWidth;
            this.Height = Settings.Default.WindowHeight;

            // 注入并持有引用
            _vm = new MainViewModel();
            this.DataContext = _vm;

#if !DEBUG
            AutoUpdaterDotNET.AutoUpdater.Start(App.GetUpdateUrl());      
#endif
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

            this.Loaded += (s, e) =>
            {
                this.SetBinding(Window.OpacityProperty, new Binding("Opacity") { Source = Settings.Default });
            };

            // 订阅窗口关闭事件进行资源释放
            this.Closed += (s, e) =>
            {
                _vm.Dispose();
                WeakReferenceMessenger.Default.UnregisterAll(this);
            };
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new() { Owner = this };
            sw.ShowDialog();
        }

        private void OnOpenAbout(object sender, RoutedEventArgs e)
        {
            AboutWindow aw = new() { Owner = this };
            aw.ShowDialog();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Shutdown();
                System.Environment.Exit(0);
            }
            catch
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

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
                    Settings.Default.WindowLeft = rect.Left;
                    Settings.Default.WindowTop = rect.Top;
                    Settings.Default.Save();
                }
            }
        }

        private void Window_OnRefreshQuote(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                // 调用 ViewModel 的方法
                vm.RefreshHolidayTip();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            try
            {
                if (System.Windows.Forms.SystemInformation.TerminalServerSession == false)
                {
                    try
                    {
                        MyNotifyIcon.Icon = Properties.Resources.app;
                    }
                    catch
                    {
                        MyNotifyIcon.Icon = System.Drawing.SystemIcons.Application;
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("无桌面环境，跳过托盘初始化");
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
            IntPtr HWND_BOTTOM = new(1);
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
                        IntPtr shellViewIntPtr = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null!);
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
            POINT pt = new()
            {
                X = (int)(Settings.Default.WindowLeft * _windowsScalingFactor),
                Y = (int)(Settings.Default.WindowTop * _windowsScalingFactor)
            };
            ScreenToClient(shellView, ref pt);

            int width = (int)(Settings.Default.WindowWidth * _windowsScalingFactor);
            int height = (int)(Settings.Default.WindowHeight * _windowsScalingFactor);

            Interop.SetWindowPos(hwnd, IntPtr.Zero, pt.X, pt.Y, width, height, SWP_NOZORDER | SWP_SHOWWINDOW);

        }

        public void SetNoActivate()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            IntPtr style = Interop.GetWindowLong(hwnd, Interop.GWL_EXSTYLE);
            IntPtr newStyle = new(style.ToInt64() | Interop.WS_EX_NOACTIVATE);
            Interop.SetWindowLong(hwnd, Interop.GWL_EXSTYLE, newStyle);
        }

        public void SetAsToolWindow()
        {
            WindowInteropHelper wih = new(this);
            IntPtr dwNew = new(((long)Interop.GetWindowLong(wih.Handle, Interop.GWL_EXSTYLE).ToInt32() | 128L | 0x00200000L) & 4294705151L);
            Interop.SetWindowLong((nint)new HandleRef(this, wih.Handle), Interop.GWL_EXSTYLE, dwNew);
        }
    }
}