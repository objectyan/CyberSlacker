using CyberSlacker.Models;
using CyberSlacker.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CyberSlacker.Services
{
    public class TimorProvider : IHolidayProvider
    {

        public async Task<List<HolidayItem>> FetchYearDataAsync(int year)
        {
            try
            {
                var resp = await HttpUtil.Client.GetStringAsync($"https://timor.tech/api/holiday/year/{year}");
                using var doc = JsonDocument.Parse(resp);

                if (doc.RootElement.GetProperty("code").GetInt32() != 0) return [];

                var holidayObj = doc.RootElement.GetProperty("holiday");
                var list = new List<HolidayItem>();

                foreach (var prop in holidayObj.EnumerateObject())
                {
                    var item = prop.Value;

                    bool isHoliday = item.GetProperty("holiday").GetBoolean();
                    string dateStr = item.GetProperty("date").GetString() ?? "";
                    string name = item.GetProperty("name").GetString() ?? "节假日";

                    list.Add(new HolidayItem
                    {
                        Date = DateTime.Parse(dateStr),
                        Name = name,
                        Type = isHoliday ? DayType.Holiday : DayType.Tiaoxiu
                    });
                }
                return [.. list.OrderBy(x => x.Date)];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("解析失败: " + ex.Message);
                return [];
            }
        }
    }
}