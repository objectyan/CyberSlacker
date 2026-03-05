using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberSlacker.Models
{
    public enum DayType
    {
        Workday = 0, // 普通工作日
        Weekend = 1, // 普通周末
        Holiday = 2, // 法定节假日
        Tiaoxiu = 3  // 调休补班日
    }

    public class HolidayItem
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public DayType Type { get; set; }
    }

    public interface IHolidayProvider
    {
        // 获取指定年份的所有节日（用于倒计时）
        Task<List<HolidayItem>> FetchYearDataAsync(int year);
    }
}
