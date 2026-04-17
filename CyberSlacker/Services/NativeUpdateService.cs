using CyberSlacker.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            try
            {
                // 获取基础信息
                string arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower();
                var localVer = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
                bool isPreviewEnabled = Properties.Settings.Default.IsPreviewEnabled;

                // 准备检查任务 总是检查“正式版”，如果开启了预览开关，则同时检查“预览版”
                var tasks = new List<(string Channel, Task<UpdateInfo?> Task)>();

                tasks.Add(("Stable", FetchRemoteUpdateInfoAsync("Update_", arch)));

                if (isPreviewEnabled)
                {
                    tasks.Add(("Preview", FetchRemoteUpdateInfoAsync("Update_preview_", arch)));
                }

                // 并行执行网络请求
                await Task.WhenAll(tasks.Select(t => t.Task));

                // 获取结果
                UpdateInfo? stableInfo = tasks.First(t => t.Channel == "Stable").Task.Result;
                UpdateInfo? previewInfo = isPreviewEnabled
                    ? tasks.First(t => t.Channel == "Preview").Task.Result
                    : null;

                // 核心优先级逻辑判断
                UpdateInfo? targetRemote = null;

                if (isPreviewEnabled)
                {
                    // 如果开启了预览：取两者中版本号最高的一个
                    if (stableInfo != null && previewInfo != null)
                    {
                        // 如果版本号一样，优先选正式版 (Stable)
                        targetRemote = (previewInfo.RemoteVersion > stableInfo.RemoteVersion) ? previewInfo : stableInfo;
                    }
                    else
                    {
                        // 哪个请求成功了就用哪个
                        targetRemote = previewInfo ?? stableInfo;
                    }
                }
                else
                {
                    // 如果没开预览：只认正式版
                    targetRemote = stableInfo;
                }

                // 最终对比：只有远程版本 > 本地版本时才提示更新
                if (targetRemote != null && targetRemote.RemoteVersion > localVer)
                {
                    return targetRemote;
                }
            }
            catch (Exception ex)
            {
                // 记录错误日志
                Log.Error($"检查更新时发生异常:", ex);
                throw;
            }

            return null;
        }


        /// <summary>
        /// 私有辅助方法：负责具体的下载和解析
        /// </summary>
        private static async Task<UpdateInfo?> FetchRemoteUpdateInfoAsync(string prefix, string arch)
        {
            try
            {
                string url = $"https://github.com/objectyan/CyberSlacker/raw/refs/heads/master/manifests/{prefix}{arch}.xml";

                // 加随机数参数绕过 GitHub/CDN 缓存
                string finalUrl = $"{url}?t={DateTime.Now.Ticks}";

                string? xmlContent = await HttpUtil.GetStringWithRetryAsync(url, 2);

                if (xmlContent == null)
                {
                    Log.Warning($"未能获取更新信息 (URL: {finalUrl})");
                    return null;
                }

                // 解析 XML
                var doc = XDocument.Parse(xmlContent);
                var item = doc.Element("item");
                if (item == null) return null;

                var versionStr = item.Element("version")?.Value;
                if (Version.TryParse(versionStr, out Version? remoteVer))
                {
                    return new UpdateInfo
                    {
                        RemoteVersion = remoteVer,
                        DownloadUrl = item.Element("url")?.Value ?? string.Empty,
                        Changelog = item.Element("changelog")?.Value ?? string.Empty,
                        IsMandatory = bool.Parse(item.Element("mandatory")?.Value ?? "false")
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Error($"获取远程更新信息失败 (URL: {prefix}{arch}.xml):", ex);
                throw;
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