using Serilog;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace CyberSlacker.Util
{
    public static class HttpUtil
    {
        private static readonly HttpClient _client;

        static HttpUtil()
        {
            // 配置高性能处理器
            var handler = new SocketsHttpHandler
            {
                // 解决 DNS 缓存问题：每 2 分钟强行刷新连接
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                // 自动处理 GZip 解压，节省流量
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            // 初始化全局唯一的 HttpClient
            _client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(20) // 全局默认超时
            };

            // 动态设置 User-Agent
            var assembly = Assembly.GetExecutingAssembly();
            string appName = assembly.GetName().Name ?? "CyberSlacker.App";
            string appVersion = assembly.GetName().Version?.ToString(3) ?? "1.0.0";
            _client.DefaultRequestHeaders.Add("User-Agent", $"{appName}/{appVersion}");
        }

        /// <summary>
        /// 通用的 GET 请求方法（带自动重试）
        /// </summary>
        public static async Task<string?> GetStringWithRetryAsync(string url, int maxRetries = 3)
        {
            int delay = 2;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // 不要在 GetStringAsync 外面套 using
                    return await _client.GetStringAsync(url);
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1)
                    {
                        // 只有最后一次失败才记录日志
                        Log.Error($"请求失败: {url}", ex);
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                    delay *= 2; // 指数退避
                }
            }
            return null;
        }

        /// <summary>
        /// 如果其他地方需要直接访问 HttpClient 原生方法，也可以暴露出来
        /// </summary>
        public static HttpClient Client => _client;
    }
}
