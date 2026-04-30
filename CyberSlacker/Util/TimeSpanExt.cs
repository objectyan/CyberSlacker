using System;

namespace CyberSlacker.Util
{
    public static class TimeSpanExt
    {
        /// <summary>
        /// 转化一下时分秒
        /// </summary>
        /// <param name="diff"></param>
        /// <returns></returns>
        public static string ToSmartFormat(this TimeSpan diff)
        {
            int days = (int)diff.TotalDays;
            if (days > 0)
                return $"{days}天 {diff.Hours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";

            return $"{diff.Hours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";

        }
    }
}
