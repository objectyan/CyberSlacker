using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace CyberSlacker.Services
{
    public class HardwareService
    {
        private PerformanceCounter _cpuCounter;
        private List<PerformanceCounter> _gpuCounters = new List<PerformanceCounter>();

        private long _lastInBytes = 0;
        private long _lastOutBytes = 0;
        private DateTime _lastNetTime;

        public HardwareService()
        {
            try
            {
                // 1. 初始化 CPU 计数器
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // 初次采样预热

                // 2. 初始化 GPU 计数器 (仅支持 Win10/11)
                // 逻辑：扫描所有名为 "engtype_3D" 的实例，这是任务管理器显示的使用率来源
                var category = new PerformanceCounterCategory("GPU Engine");
                var instanceNames = category.GetInstanceNames();
                foreach (var name in instanceNames.Where(n => n.Contains("engtype_3D")))
                {
                    var pc = new PerformanceCounter("GPU Engine", "Utilization Percentage", name);
                    _gpuCounters.Add(pc);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("硬件计数器初始化失败: " + ex.Message);
            }

            // 3. 初始化网络采样
            InitNetwork();
        }

        /// <summary>
        /// 获取当前所有硬件指标
        /// </summary>
        /// <returns>(CPU使用率, 内存使用率, 下载速度, 上传速度, GPU使用率)</returns>
        public (float cpu, float ram, string netIn, string netOut, float gpu) GetStats()
        {
            // --- CPU ---
            float cpu = 0;
            try { cpu = _cpuCounter?.NextValue() ?? 0; } catch { }

            // --- RAM (使用 Win32 API 毫秒级获取) ---
            float ram = GetRamUsage();

            // --- GPU ---
            float gpu = 0;
            try
            {
                foreach (var counter in _gpuCounters)
                {
                    gpu += counter.NextValue();
                }
                if (gpu > 100) gpu = 100; // 防止多核计算溢出
            }
            catch { }

            // --- Network ---
            var (inStr, outStr) = CalculateNetworkSpeed();

            return (cpu, ram, inStr, outStr, gpu);
        }

        #region 内存逻辑 (免权限/极速)

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        private float GetRamUsage()
        {
            var memStatus = new MEMORYSTATUSEX();
            memStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            if (GlobalMemoryStatusEx(ref memStatus))
            {
                return memStatus.dwMemoryLoad; // 直接返回百分比
            }
            return 0;
        }

        #endregion

        #region 网速逻辑

        private void InitNetwork()
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up);

                _lastInBytes = interfaces.Sum(i => i.GetIPStatistics().BytesReceived);
                _lastOutBytes = interfaces.Sum(i => i.GetIPStatistics().BytesSent);
                _lastNetTime = DateTime.Now;
            }
            catch { }
        }

        private (string inSpeed, string outSpeed) CalculateNetworkSpeed()
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up);

                long currentIn = interfaces.Sum(i => i.GetIPStatistics().BytesReceived);
                long currentOut = interfaces.Sum(i => i.GetIPStatistics().BytesSent);
                DateTime now = DateTime.Now;

                double elapsedSec = (now - _lastNetTime).TotalSeconds;
                if (elapsedSec <= 0) return ("0B", "0B");

                string inStr = FormatSpeed((currentIn - _lastInBytes) / elapsedSec);
                string outStr = FormatSpeed((currentOut - _lastOutBytes) / elapsedSec);

                _lastInBytes = currentIn;
                _lastOutBytes = currentOut;
                _lastNetTime = now;

                return (inStr, outStr);
            }
            catch
            {
                return ("0B", "0B");
            }
        }

        private string FormatSpeed(double bytesPerSec)
        {
            if (bytesPerSec < 1024) return $"{bytesPerSec:F0}B/s";
            if (bytesPerSec < 1024 * 1024) return $"{(bytesPerSec / 1024.0):F1}K/s";
            return $"{(bytesPerSec / 1024.0 / 1024.0):F1}M/s";
        }

        #endregion
    }
}