using Microsoft.Win32;

namespace CyberSlacker.Util
{
    public static class StartupHelper
    {
        private const string AppName = "CyberSlackerWidget";

        public static void SetStartup(bool enable)
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key != null)
            {
                if (enable)
                {
                    string? exePath = Environment.ProcessPath;
                    key.SetValue(AppName, $"\"{exePath ?? ""}\"");
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
        }
    }
}
