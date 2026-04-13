using CyberSlacker.Properties;
using CyberSlacker.Services;
using CyberSlacker.Util;
using CyberSlacker.ViewModels;
using Serilog;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using static CyberSlacker.Util.Interop;

namespace CyberSlacker
{
    public partial class MainWindow : Window
    {
        IntPtr shellView = IntPtr.Zero;
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();


            if (Settings.Default.IsUpgradeRequired) // 假设你在设置里定义了这个 bool
            {
                Settings.Default.Upgrade();
                Settings.Default.IsUpgradeRequired = false;
                Settings.Default.Save();
            }

            // 加载保存的值
            this.Left = Settings.Default.WindowLeft;
            this.Top = Settings.Default.WindowTop;
            this.Width = Settings.Default.WindowWidth;
            this.Height = Settings.Default.WindowHeight;

            // 注入并持有引用
            _vm = new MainViewModel();
            this.DataContext = _vm;

            this.Loaded += (s, e) =>
            {
                this.SetBinding(Window.OpacityProperty, new Binding("Opacity") { Source = Settings.Default });

                ApplyLockState(Settings.Default.IsLocked);
            };

            // 订阅窗口关闭事件进行资源释放
            this.Closed += (s, e) =>
            {
                _vm.Dispose();
            };
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                ReleaseCapture();
                SendMessage(hWnd, 0xA1, 0x2, 0);

                SaveCurrentPosition();
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
                if (Environment.UserInteractive)
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
                Log.Error("无桌面环境，跳过托盘初始化");
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

            var source = PresentationSource.FromVisual(this);
            double dpiX = source?.CompositionTarget.TransformToDevice.M11 ?? 1.0;
            double dpiY = source?.CompositionTarget.TransformToDevice.M22 ?? 1.0;

            Util.Interop.GetWindowRect(hwnd, out var parentRect);

            double virtualLeft = SystemParameters.VirtualScreenLeft;
            double virtualTop = SystemParameters.VirtualScreenTop;

            int width = (int)(Settings.Default.WindowWidth * dpiX);
            int height = (int)(Settings.Default.WindowHeight * dpiY);
            int x = (int)(Settings.Default.WindowLeft - virtualLeft * dpiX);
            int y = (int)(Settings.Default.WindowTop - virtualTop * dpiY);

            POINT pt = new()
            {
                X = x,
                Y = y
            };
            ScreenToClient(shellView, ref pt);


            Interop.SetWindowPos(hwnd, IntPtr.Zero, x, y, width, height, SWP_NOZORDER | SWP_SHOWWINDOW);

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


        /// <summary>
        /// 赛博闪烁动画：当用户点击通知唤醒挂件时调用
        /// </summary>
        public void PlayFlashAnimation()
        {
            // 更新提示语
            _vm.RefreshHolidayTip();

            // 定义发光强度动画 (从当前的 0.4 闪烁到 1.0 再回来)
            DoubleAnimation glowAnim = new()
            {
                From = 0.4,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(400),
                AutoReverse = true, // 自动往返
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            // 定义窗口轻微缩放动画 (增加“跳出来”的感觉)
            DoubleAnimation scaleAnim = new()
            {
                From = 1.0,
                To = 1.05,
                Duration = TimeSpan.FromMilliseconds(200),
                AutoReverse = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            // 执行动画 对阴影透明度执行动画
            MainShadow.BeginAnimation(DropShadowEffect.OpacityProperty, glowAnim);

            // 执行缩放动画 (同时应用到 X 和 Y 轴)
            MainScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            MainScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

            // 可以给窗口透明度也加
            this.BeginAnimation(Window.OpacityProperty, glowAnim);

        }

        /// <summary>
        /// 保存当前窗口位置的“保命”脚本
        /// </summary>
        private void SaveCurrentPosition()
        {
            IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            if (Util.Interop.GetWindowRect(hWnd, out Util.Interop.RECT rect))
            {
                var source = PresentationSource.FromVisual(this);
                double dpiX = source?.CompositionTarget.TransformToDevice.M11 ?? 1.0;
                double dpiY = source?.CompositionTarget.TransformToDevice.M22 ?? 1.0;

                // 将物理像素转回逻辑坐标再保存
                Settings.Default.WindowLeft = rect.Left / dpiX;
                Settings.Default.WindowTop = rect.Top / dpiY;
                Settings.Default.Save();
            }
        }

        #region 托盘事件
        /// <summary>
        /// 显示/隐藏小组件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnToggleWidget(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                SaveCurrentPosition();
                this.Visibility = Visibility.Collapsed;
                if (sender is MenuItem item) item.Header = "显示小组件";
            }
            else
            {
                this.Visibility = Visibility.Visible;
                if (sender is MenuItem item) item.Header = "隐藏小组件";
            }
        }

        /// <summary>
        /// 锁定/解锁挂件 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLockToggle(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {

                // 1. 获取当前状态（如果是 IsCheckable 模式）
                bool isLocked = item.IsChecked;

                Settings.Default.IsLocked = isLocked;
                Settings.Default.Save();

                // 3. 执行穿透逻辑
                ApplyLockState(isLocked);

                // 4. 发个彩色通知提醒一下（可选，更有仪式感）
                if (isLocked)
                {
                    NativeToastService.Show("🔒 挂件已锁定", "现在点击将直接穿透，防止摸鱼时误触。");
                }
            }
        }

        /// <summary>
        /// 打开设置中心 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenSettings(object sender, RoutedEventArgs e)
        {
            WindowManager.ShowUnique<SettingsWindow>(this);
        }

        /// <summary>
        /// 打开关于界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenAbout(object sender, RoutedEventArgs e)
        {
            WindowManager.ShowUnique<AboutWindow>(this);
        }

        /// <summary>
        /// 5. 退出程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExit(object sender, RoutedEventArgs e)
        {
            // 销毁托盘图标，防止残留
            MyNotifyIcon?.Dispose();

            // 释放 ViewModel 里的 Timer
            if (_vm is IDisposable disposable)
            {
                disposable.Dispose();
            }

            // 强制关闭所有进程线程，防止后台卡死
            System.Windows.Application.Current.Shutdown();
            System.Environment.Exit(0);
        }

        /// <summary>
        /// 辅助：点击穿透逻辑实现
        /// </summary>
        /// <param name="isLocked"></param>
        private void ApplyLockState(bool isLocked)
        {
            IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            nint exStyle = Util.Interop.GetWindowLong(hWnd, Util.Interop.GWL_EXSTYLE);

            if (isLocked)
            {
                // 增加 WS_EX_TRANSPARENT (0x20) 达到点击穿透效果
                Util.Interop.SetWindowLong(hWnd, Util.Interop.GWL_EXSTYLE, exStyle | 0x00000020);
            }
            else
            {
                // 移除穿透效果
                Util.Interop.SetWindowLong(hWnd, Util.Interop.GWL_EXSTYLE, exStyle & ~0x00000020);
            }
        }
        #endregion
    }
}