using CyberSlacker.Util;
using Serilog;
using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CyberSlacker.Services
{
    public class UpdateInfo
    {
        public required Version RemoteVersion { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string Changelog { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
    }

    public static class NativeUpdateService
    {
        public static async Task<UpdateInfo?> CheckForUpdateAsync()
        {
            // 1. 获取架构和配置路径
            string arch = RuntimeInformation.ProcessArchitecture.ToString().ToLower();
            string prefix = Properties.Settings.Default.IsPreviewEnabled ? "Update_preview_" : "Update_";
            string url = $"https://raw.githubusercontent.com/objectyan/CyberSlacker/master/manifests/{prefix}{arch}.xml";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            // 增加随机参数防止 GitHub CDN 缓存
            string xmlContent = await client.GetStringAsync($"{url}?t={DateTime.Now.Ticks}");

            // 2. 解析 XML
            var doc = XDocument.Parse(xmlContent);
            var item = doc.Element("item");
            if (item == null) return null;

            var remoteVerStr = item.Element("version")?.Value;
            var remoteVer = new Version(remoteVerStr ?? "0.0.0.0");

            // 3. 获取本地版本
            var localVer = Assembly.GetExecutingAssembly().GetName().Version;

            // 4. 比对版本
            if (remoteVer > localVer)
            {
                return new UpdateInfo
                {
                    RemoteVersion = remoteVer,
                    DownloadUrl = item.Element("url")?.Value ?? string.Empty,
                    Changelog = item.Element("changelog")?.Value ?? string.Empty,
                    IsMandatory = bool.Parse(item.Element("mandatory")?.Value ?? "false")
                };
            }
            return null;
        }


        public static async Task StartUpdateFlow()
        {
            try
            {
                // 检查更新
                var info = await CheckForUpdateAsync();

                if (info != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                     {
                         WindowManager.ShowUnique<UpdateInfoWindow>(
                                null,
                                () => new UpdateInfoWindow(info)
                         );
                     });
                }
            }
            catch (Exception ex)
            {
                Log.Error("自动检查更新静默失败:", ex);
            }
        }
    }
}