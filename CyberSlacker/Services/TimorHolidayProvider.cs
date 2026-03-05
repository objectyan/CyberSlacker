using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CyberSlacker.Models;

namespace CyberSlacker.Services
{
    public class TimorProvider : IHolidayProvider
    {
        private static readonly HttpClient _client = new HttpClient();

        public async Task<List<HolidayItem>> FetchYearDataAsync(int year)
        {
            try
            {
                var resp = await _client.GetStringAsync($"https://timor.tech/api/holiday/year/{year}");
                using var doc = JsonDocument.Parse(resp);

                if (doc.RootElement.GetProperty("code").GetInt32() != 0) return new List<HolidayItem>();

                var holidayObj = doc.RootElement.GetProperty("holiday");
                var list = new List<HolidayItem>();

                foreach (var prop in holidayObj.EnumerateObject())
                {
                    var item = prop.Value;

                    // 根据你截图的字段解析
                    bool isHoliday = item.GetProperty("holiday").GetBoolean();
                    string dateStr = item.GetProperty("date").GetString();
                    string name = item.GetProperty("name").GetString();

                    list.Add(new HolidayItem
                    {
                        Date = DateTime.Parse(dateStr),
                        Name = name,
                        // 核心逻辑：在这个列表里，holiday为true是节日，false是调休上班
                        Type = isHoliday ? DayType.Holiday : DayType.Tiaoxiu
                    });
                }
                return list.OrderBy(x => x.Date).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("解析失败: " + ex.Message);
                return new List<HolidayItem>();
            }
        }
    }
}