using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyberSlacker.Models;
using CyberSlacker.Services;

namespace CyberSlacker.Util
{
    public static class CountdownEngine
    {
        private static readonly Random _rng = new Random();
        private static readonly string[] _slackerQuotes = {
            "小心点，你老板在你背后",
            "工作再累，也别忘了摸鱼哦，钱是老板的，命是自己的",
            "我毕生的梦想，就是可以准点下班",
            "你上会班吧，我替你老板求求你了",
            "别划水了，上岸换口气吧",
            "愿你的烦恼，像你的头发一样，越来越少",
            "只要我够努力，老板明年就能换辆库里南",
            "哪怕是生产队的驴，也没你这么能干",
            "摸鱼不是偷懒，是打工人对资本最后的倔强",
            "进公司那一刻，我就开始期待下班了",
            "如果工作能变现，我希望是变成现成的假期",
            "三点几嘞，饮茶先啦！做这么多没用的",
            "努力不一定会成功，但不努力一定会很舒服",
            "别问我为什么还没走，问就是我在等灵魂跟上肉体",
            "生活不只有眼前的工位，还有远方的外卖和快递"
        };

        private static readonly (string Title, string Content)[] _offWorkCheers = {
            ("🎉 终于下班啦！", "钱是赚不完的，命是自己的。快跑，别回头！"),
            ("🏃 溜之大吉", "今天的砖就搬到这里，剩下的明天再说。"),
            ("🌆 晚霞真美", "放下鼠标，投身生活。去吃顿好的犒劳下自己！"),
            ("🍻 自由呼唤", "别划水了，直接上岸吧！自由的空气在招手。"),
            ("🛌 关机撤退", "你的灵魂已离线，肉体请尽快同步。"),
            ("🎮 副本开启", "打卡成功！生活这个大副本正在等待你开启。"),
            ("💃 自由之舞", "检测到下班指令，灵魂开始逃逸..."),
            ("🚀 弹射起步", "打卡成功！您已成功逃离工位，正在进入生活轨道。"),
            ("🍗 奖励自己", "今天表现满分！下班路上买个鸡腿犒劳下？"),
            ("🍹 摸鱼成功", "恭喜你！在老板眼皮底下又成功混过一天！")
         };


        private static readonly (string Title, string Content)[] _mealQuotes = {
            ("🍱 干饭时间到！", "干饭不积极，思想有问题！赶紧打开外卖App！"),
            ("🥘 别看了，吃饭去", "工作是老板的，胃是自己的。今天吃点好的犒劳下？"),
            ("🍜 补充能量", "检测到血糖偏低，灵魂请求开启干饭模式。"),
            ("🥗 摸鱼预警", "此时点餐，正好可以在下班时取到，计划通！")
        };

        private static readonly (string Title, string Content)[] _restQuotes = {
            ("💧 吨吨吨时间", "你已经连续输出 {0} 分钟了，快喝口水，给 CPU 降降温。"),
            ("🧘 颈椎拯救计划", "别盯着屏幕了，站起来转转脖子，老板不会因为这 5 分钟变穷的。"),
            ("👀 保护视力", "你的眼睛请求执行“远眺”指令，哪怕是看看对面的美女/帅哥也好。"),
            ("🚶 走动一下", "生命在于运动，不在于久坐。去接杯水或者去趟洗手间吧。"),
            ("🌊 摸鱼回血", "工作是公司的，命是自己的。休息 5 分钟，回血一整天。"),
            ("🔋 能量格偏低", "检测到当前工时已达 {0} 分钟，身体电量不足，请执行物理休息。")
        };

        // 1. 下班倒计时
        public static string GetOffWorkString(DateTime now, HolidayService service)
        {
            if (IsRestDay(now, service)) return "休息中";

            TimeSpan.TryParse(Properties.Settings.Default.EndTime, out var offTime);
            TimeSpan.TryParse(Properties.Settings.Default.LunchStart, out var lStart);
            TimeSpan.TryParse(Properties.Settings.Default.LunchEnd, out var lEnd);

            DateTime target = DateTime.Today.Add(offTime);
            if (now >= target) return "已下班";

            // 计算总剩余秒数
            TimeSpan totalRemaining = target - now;

            return totalRemaining.ToString(@"hh\:mm\:ss");
        }

        // 2. 周末倒计时（核心补丁：识别下班即周末）
        public static string GetWeekendString(DateTime now, HolidayService service)
        {
            if (!service.IsDataReady) return "正在同步假表...";

            TimeSpan.TryParse(Properties.Settings.Default.EndTime, out var offTime);
            DateTime today = DateTime.Today;

            // 1. 判断今天是否在放假
            if (IsRestDay(today, service))
            {
                return "享受假期中！";
            }

            // 2. 寻找切换点
            DateTime? weekendStart = null;
            for (int i = 0; i < 15; i++)
            {
                DateTime curr = today.AddDays(i);
                DateTime next = today.AddDays(i + 1);

                if (!IsRestDay(curr, service) && IsRestDay(next, service))
                {
                    weekendStart = curr.Add(offTime);
                    if (weekendStart > now) break;
                    else return "享受假期中！";
                }
            }

            if (weekendStart.HasValue)
            {
                TimeSpan diff = weekendStart.Value - now;
                return $"{(int)diff.TotalDays}天 {diff.Hours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";
            }

            // 3. 【终极兜底】如果 service 没数据且没算出来，显示搬砖中
            return "努力搬砖中";
        }

        /// <summary>
        /// 获取经过策略调整后的发薪日期
        /// </summary>
        public static DateTime GetAdjustedPayday(DateTime baseDate, HolidayService service)
        {
            int pd = Properties.Settings.Default.PayDay;
            int daysInMonth = DateTime.DaysInMonth(baseDate.Year, baseDate.Month);
            DateTime payDate = new DateTime(baseDate.Year, baseDate.Month, Math.Min(pd, daysInMonth));

            // 如果发薪日是休息日
            if (IsRestDay(payDate, service))
            {
                int strategy = Properties.Settings.Default.PaydayStrategy; // 0:提前, 1:延后
                int step = (strategy == 0) ? -1 : 1;

                // 循环查找，直到找到工作日
                while (IsRestDay(payDate, service))
                {
                    payDate = payDate.AddDays(step);
                    // 安全出口：防止死循环（理论上不可能，除非一年都在放假）
                    if (payDate.Year != baseDate.Year && Math.Abs(payDate.Month - baseDate.Month) > 1) break;
                }
            }
            return payDate;
        }

        // 3. 发薪日倒计时
        public static string GetPaydayString(DateTime now, HolidayService service)
        {
            if (!service.IsDataReady) return "同步中...";

            // 1. 先算本月的调整后发薪日
            DateTime currentPayday = GetAdjustedPayday(now, service);

            // 2. 如果今天已经过了本月发薪日（假设17点后算第二天），算下个月的
            if (now > currentPayday.AddHours(17))
            {
                currentPayday = GetAdjustedPayday(now.AddMonths(1), service);
            }

            int diff = (currentPayday.Date - now.Date).Days;

            if (diff == 0) return "💰 就在今天！";
            return $"{diff} 天";
        }

        // 4. 动态提示语
        public static string GetDynamicTip(DateTime now, HolidayItem todayInfo, HolidayService service, DateTime lastRestTime)
        {
            // 1. 节假日和补班逻辑优先
            if (todayInfo?.Type == DayType.Holiday) return $"🎉 {todayInfo.Name}快乐！好好休息~";
            if (todayInfo?.Type == DayType.Tiaoxiu) return "😫 补班中... 报警电话是110。";

            // 2. 休息日逻辑
            if (IsRestDay(now, service)) return "🍵 享受周末，远离钉钉。";

            // 3. 核心时间段逻辑 (干饭/午休/休息)
            TimeSpan.TryParse(Properties.Settings.Default.MealTime, out var mealTime);
            TimeSpan.TryParse(Properties.Settings.Default.LunchStart, out var lStart);
            TimeSpan.TryParse(Properties.Settings.Default.LunchEnd, out var lEnd);

            TimeSpan currentTime = now.TimeOfDay;

            // 午饭时间
            if (currentTime >= mealTime && currentTime < lStart)
                return "🍱 饭点到了！干饭不积极，思想有问题。";

            // 午休时间
            if (currentTime >= lStart && currentTime < lEnd)
                return "💤 正在午休中... 别卷了，快睡会儿。";

            // --- 临近下班 ---
            TimeSpan.TryParse(Properties.Settings.Default.EndTime, out var offTime);
            int preMins = Properties.Settings.Default.PreOffWorkMins;
            if (preMins <= 0) preMins = 30; // 容错处理
            if (currentTime >= offTime.Subtract(TimeSpan.FromMinutes(preMins)) && currentTime < offTime)
            {
                // 这里可以根据剩余时间长短返回不同的提示
                double remaining = (offTime - currentTime).TotalMinutes;
                if (remaining <= 5)
                    return "🚀 倒计时 "+ Math.Ceiling(remaining) + " 分钟！起跑架已准备好，随时准备弹射！";

                return "🎒 收拾书包，准备冲刺，姿势要帅！";
            }

            // 久坐提醒（如果接近间隔时间，强制变更为提醒状态）
            if (Properties.Settings.Default.IsRestEnabled)
            {
                double workMins = (now - lastRestTime).TotalMinutes;
                if (workMins >= Properties.Settings.Default.RestInterval)
                    return "💧 提醒：你已经坐很久了，快去喝水走动下！";
            }

            // 4. 默认毒鸡汤
            return GetRandomSlackerQuote(now);
        }


        /// <summary>
        /// 获取毒鸡汤
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        private static string GetRandomSlackerQuote(DateTime now)
        {
            string timeGreeting = now.Hour < 9 ? "早安" : (now.Hour < 12 ? "上午好" : (now.Hour < 18 ? "下午好" : "晚上好"));
            return $"{timeGreeting}！{_slackerQuotes[_rng.Next(_slackerQuotes.Length)]}";
        }

        /// <summary>
        /// 5. 获取下一个节假日信息
        /// </summary>
        public static (string Name, string Countdown) GetNextHolidayInfo(DateTime now, HolidayService service)
        {
            // 如果数据还没准备好
            if (!service.IsDataReady)
                return ("同步中...", "");

            // 逻辑搬迁到这里
            var nextH = service.AllItems
                .Where(x => x.Date.Date > now.Date && x.Type == DayType.Holiday)
                .OrderBy(x => x.Date)
                .FirstOrDefault();

            if (nextH != null)
            {
                int days = (nextH.Date.Date - now.Date).Days;
                return (nextH.Name, $"还有 {days} 天");
            }

            return ("今年没假了", "");
        }

        // 判定某天是否真的是休息日（考虑调休优先级）
        public static bool IsRestDay(DateTime date, HolidayService service)
        {
            // 1. 尝试从 API 缓存中获取这一天的特殊定义
            // 注意：Timor API 的 year 列表只包含“被改动过”的日子（节日或调休上班）
            var info = service.GetDateInfo(date);

            if (info != null)
            {
                // 只要 API 列表里有这一天，就必须听 API 的
                if (info.Type == DayType.Holiday) return true;  // 明确放假
                if (info.Type == DayType.Tiaoxiu) return false; // 明确补班
            }

            // 2. 如果 API 列表里没这一天，说明它是“普通日子”，按星期几判断
            int mode = Properties.Settings.Default.WorkMode; // 0:双休, 1:大小周, 2:单休
            DayOfWeek dow = date.DayOfWeek;

            switch (mode)
            {
                case 0: // 双休
                    return (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday);

                case 2: // 单休 (仅周日休)
                    return (dow == DayOfWeek.Sunday);

                case 1: // 大小周 (关键逻辑)
                        // 获取当前日期是这一年的第几周
                    int weekNum = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);

                    // 假设：偶数周是大周(双休)，奇数周是小周(单休)
                    // 2026年3月1日是第9周(奇数)，按小周算，只有周日休。
                    bool isDoubleWeek = (weekNum % 2 == 0);

                    if (isDoubleWeek)
                        return (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday);
                    else
                        return (dow == DayOfWeek.Sunday); // 小周周日依然要休！

                default:
                    return (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday);
            }
        }


        /// <summary>
        /// 获取下班提示语
        /// </summary>
        /// <returns></returns>
        public static (string Title, string Content) GetRandomOffWorkCheer()
        {
            return _offWorkCheers[_rng.Next(_offWorkCheers.Length)];
        }

        /// <summary>
        /// 获取干饭提示语
        /// </summary>
        /// <returns></returns>
        public static (string Title, string Content) GetRandomMealCheer()
        {
            return _mealQuotes[_rng.Next(_mealQuotes.Length)];
        }

        /// <summary>
        /// 获取周期性休息提示语
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static (string Title, string Content) GetRandomRestCheer(int interval)
        {
            var cheer = _restQuotes[_rng.Next(_restQuotes.Length)];
            // 将间隔时间动态注入到文案中
            string formattedContent = string.Format(cheer.Content, interval);
            return (cheer.Title, formattedContent);
        }
    }
}