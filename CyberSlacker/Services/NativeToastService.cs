using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;
using System;

namespace CyberSlacker.Services
{
    public static class NativeToastService
    {
        public static void Show(string title, string content)
        {
            try
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(content)
                    .Show();
            }
            catch (Exception ex)
            {
                Log.Error("通知发送失败: ", ex);
            }
        }

        /// <summary>
        /// 当用户点通知卡片时触发挂件动画
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void ShowGeneralNotification(string title, string content)
        {
            try
            {
                new ToastContentBuilder()
                .AddText(title)
                .AddText(content)
                .AddArgument("action", "main")
                .Show();
            }
            catch (Exception ex)
            {
                Log.Error("触发挂件通知发送失败: ", ex);
            }
        }
    }
}