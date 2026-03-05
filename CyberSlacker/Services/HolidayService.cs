using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CyberSlacker.Models;

namespace CyberSlacker.Services
{
    public class HolidayService
    {
        private bool _isUpdating = false; // 状态锁：防止重叠请求

        private readonly IHolidayProvider _provider;
        private List<HolidayItem> _cache = new();

        public bool IsDataReady => _cache != null && _cache.Count > 0;


        public HolidayService(IHolidayProvider provider)
        {
            _provider = provider;
        }

        public async Task InitializeAsync(int year)
        {

            // --- 1. 防止并发锁 ---
            if (_isUpdating) return;

            // --- 2. 预检查：如果今天已经更新过了，且内存里有数据，直接跳过 ---
            if (IsDataReady && Properties.Settings.Default.LastHolidayUpdate.Date == DateTime.Today)
            {
                return;
            }

            _isUpdating = true; // 上锁

            try
            {
                // 尝试加载本地缓存
                if (!IsDataReady)
                {
                    string cachedJson = Properties.Settings.Default.HolidayCacheJson;
                    if (!string.IsNullOrEmpty(cachedJson))
                    {
                        var data = JsonSerializer.Deserialize<List<HolidayItem>>(cachedJson);
                        if (data != null && data.Any() && data[0].Date.Year == year)
                        {
                            _cache = data;
                            // 如果缓存就是今天的，那就直接 return 释放锁即可
                            if (Properties.Settings.Default.LastHolidayUpdate.Date == DateTime.Today) return;
                        }
                    }
                }

                // 执行联网更新
                var newData = await _provider.FetchYearDataAsync(year);
                if (newData != null && newData.Any())
                {
                    _cache = newData;
                    Properties.Settings.Default.HolidayCacheJson = JsonSerializer.Serialize(_cache);
                    Properties.Settings.Default.LastHolidayUpdate = DateTime.Today;
                    Properties.Settings.Default.Save();
                }
            }
            finally
            {
                _isUpdating = false; // 无论成功失败，最终都要释放锁
            }
        }

        public HolidayItem GetDateInfo(DateTime date)
        {
            return _cache.FirstOrDefault(x => x.Date.Date == date.Date)
                   ?? new HolidayItem { Date = date, Type = DayType.Workday };
        }

        public List<HolidayItem> AllItems => _cache;
    }
}