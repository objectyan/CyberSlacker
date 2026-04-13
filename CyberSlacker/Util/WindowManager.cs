using System;
using System.Linq;
using System.Windows;

namespace CyberSlacker.Util
{
    public static class WindowManager
    {
        /// <summary>
        /// 显示唯一窗口（幂等逻辑）
        /// </summary>
        /// <typeparam name="T">窗口类型</typeparam>
        /// <param name="owner">父窗口</param>
        /// <param name="factory">创建新窗口的工厂方法（可选）</param>
        public static void ShowUnique<T>(Window? owner = null, Func<T>? factory = null) where T : Window
        {
            // 1. 在当前所有打开的窗口中寻找是否已有该类型的窗口
            var existingWindow = Application.Current.Windows.OfType<T>().FirstOrDefault();

            if (existingWindow != null)
            {
                // 2. 如果窗口已存在：唤醒它
                if (existingWindow.Visibility != Visibility.Visible)
                {
                    existingWindow.Visibility = Visibility.Visible;
                }

                // 强行带到最前并闪烁一下（如果之前写了动画逻辑）
                existingWindow.Activate();
                existingWindow.Focus();
                existingWindow.Topmost = true;
                return;
            }

            // 3. 如果窗口不存在：创建它
            T newWindow = factory != null ? factory() : Activator.CreateInstance<T>();

            if (owner != null && owner.IsVisible)
            {
                newWindow.Owner = owner;
            }
            newWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            newWindow.Show();
        }
    }
}