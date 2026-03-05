using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberSlacker.Util
{
    public static class StartupHelper
    {
        private const string AppName = "WorkCountdownWidget";

        public static void SetStartup(bool enable)
        {
            // 获取当前可执行文件的路径
            string exePath = Environment.ProcessPath;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (enable)
                    key.SetValue(AppName, $"\"{exePath}\"");
                else
                    key.DeleteValue(AppName, false);
            }
        }
    }
}
