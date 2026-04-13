using Serilog;
using System;
using System.IO;

namespace CyberSlacker.Services
{
    public static class LogService
    {
        // 存放路径：C:\Users\用户名\AppData\Local\CyberSlacker\Logs
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CyberSlacker", "Logs", "log-.txt");

        public static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    LogPath,
                    rollingInterval: RollingInterval.Day, // 每天一个新文件
                    retainedFileCountLimit: 7,            // 只保留最近7天
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            Log.Information(">>> 赛博摸鱼员 启动 <<<");
        }
    }
}
