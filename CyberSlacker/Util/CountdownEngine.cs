using CyberSlacker.Models;
using CyberSlacker.Properties;
using CyberSlacker.Services;
using System;
using System.Linq;

namespace CyberSlacker.Util
{
    public static class CountdownEngine
    {
        private static readonly Random _rng = new();

        /// <summary>
        /// 通用毒鸡汤
        /// </summary>
        private static readonly string[] _slackerQuotes = [
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
            "生活不只有眼前的工位，还有远方的外卖和快递",
            "虽然人在工位，但我的灵魂早已在五公里外的火锅店排队了",
            "少说话，多喝水，假装自己很忙，熬过今天就是胜利",
            "只要我足够透明，老板就看不见我在摸鱼",
            "会议的本质：一个带薪睡觉且不用担心被电话吵醒的地方",
            "电脑正在更新（1%），这是上天给我的带薪假",
            "薪水是老板给的，快乐是自己偷的",
            "如果你觉得累了，说明你正在给老板换库里南的路上加速",
            "所谓加班，就是用自己的生命，去圆老板的梦想",
            "在公司我只发挥 20% 的实力，剩下的 80% 用来防御老板的突袭",
            "不要问公司能为你做什么，要问你摸了多少鱼才对得起这工资",
            "如果你的工作让你不开心，那一定是摸鱼的力度不够",
        ];

        /// <summary>
        /// 普通下班提醒
        /// </summary>
        private static readonly (string Title, string Content)[] _offWorkCheers = [
            ("🎉 终于下班啦！", "钱是赚不完的，命是自己的。快跑，别回头！"),
            ("🏃 溜之大吉", "今天的砖就搬到这里，剩下的明天再说。"),
            ("🌆 晚霞真美", "放下鼠标，投身生活。去吃顿好的犒劳下自己！"),
            ("🍻 自由呼唤", "别划水了，直接上岸吧！自由的空气在招手。"),
            ("🛌 关机撤退", "你的灵魂已离线，肉体请尽快同步。"),
            ("🎮 副本开启", "打卡成功！生活这个大副本正在等待你开启。"),
            ("💃 自由之舞", "检测到下班指令，灵魂开始逃逸..."),
            ("🚀 弹射起步", "打卡成功！您已成功逃离工位，正在进入生活轨道。"),
            ("🍗 奖励自己", "今天表现满分！下班路上买个鸡腿犒劳下？"),
            ("🍹 摸鱼成功", "恭喜你！在老板眼皮底下又成功混过一天！"),
            ("🛸 瞬间移动", "打卡那一秒，我已完成从‘打工人’到‘自由人’的分子重组。"),
            ("📴 信号消失", "正在切断与钉钉的量子纠缠，有缘明天再见，无缘明天再说。"),
            ("🦸 英雄卸甲", "收起你的职业微笑，脱掉你的工装，现在是属于你自己的超级时间。"),
            ("🎬 本片终结", "今天的剧情到此为止，祝屏幕外的你生活愉快，晚安。"),
            ("🌬️ 随风而去", "下班的步伐要轻快，像一阵风，让老板想抓也抓不住。"),
         ];

        /// <summary>
        /// 点餐提醒
        /// </summary>
        private static readonly (string Title, string Content)[] _mealQuotes = [
            ("🍱 干饭时间到！", "干饭不积极，思想有问题！赶紧打开外卖App！"),
            ("🥘 别看了，吃饭去", "工作是老板的，胃是自己的。今天吃点好的犒劳下？"),
            ("🍜 补充能量", "检测到血糖偏低，灵魂请求开启干饭模式。"),
            ("🥗 摸鱼预警", "此时点餐，正好可以在下班时取到，计划通！"),
            ("🍖 肉食者强", "胃里的饥饿感是此时唯一的 KPI，请优先处理。"),
            ("🛒 决策困难", "选外卖的难度远超写代码，建议提前一个小时开启头脑风暴。"),
            ("🔥 炊烟起义", "肚子在闹革命，外卖在呼唤统帅。现在下单，时间刚好。"),
        ];

        /// <summary>
        /// 午休提醒
        /// </summary>
        private static readonly (string Title, string Content)[] _lunchQuotes = [
            ("💤 正在午休充能", "老板再急也得等我睡醒。拒绝接收任何 bug 修复指令。"),
            ("🛏️ 午睡是续命良方", "闭眼，做梦，别想代码。"),
            ("💆 此时不宜搬砖", "只宜躺平。身体是革命的本钱。"),
            ("🌑 进入沉睡", "我现在的状态是：除供氧外，其他系统功能已全部强制下线。"),
            ("🌠 梦境漫游", "在梦里，我刚刚把离职报告拍在老板桌上，别吵醒我。"),
            ("🔋 低功耗模式", "为了下午不打瞌睡，请立刻把头埋进枕头里。"),
        ];

        /// <summary>
        /// 假前最后一天提醒
        /// </summary>
        private static readonly (string Title, string Content)[] _lastDayQuotes = [
            ("🚩 坚持住！", "这是放假前最后几个小时了！胜利就在前方！"),
            ("🔥 最后一搏", "灵魂已经提前放假了，肉体在工位上做最后的坚守。"),
            ("🎒 即将开启假期", "再坚持一下下，马上就能瞬移回家了。"),
            ("🕊️ 归心似箭", "身体还在工位坐着，心已经在几百公里外的家门口敲门了。"),
            ("🔌 提前断电", "由于对假期的渴望过于强烈，本人的工作系统已提前崩溃。"),
            ("🕯️ 黎明前的微光", "再坚持一下，你听，那是假期在敲击你的天灵盖。")
       ];

        /// <summary>
        /// 周期性休息提醒
        /// </summary>
        private static readonly (string Title, string Content)[] _restQuotes = [
            ("💧 吨吨吨时间", "你已经连续输出 {0} 分钟了，快喝口水，给 CPU 降降温。"),
            ("🧘 颈椎拯救计划", "别盯着屏幕了，站起来转转脖子，老板不会因为这 5 分钟变穷的。"),
            ("👀 保护视力", "你的眼睛请求执行“远眺”指令，哪怕是看看对面的美女/帅哥也好。"),
            ("🚶 走动一下", "生命在于运动，不在于久坐。去接杯水或者去趟洗手间吧。"),
            ("🌊 摸鱼回血", "工作是公司的，命是自己的。休息 5 分钟，回血一整天。"),
            ("🔋 能量格偏低", "检测到当前工时已达 {0} 分钟，身体电量不足，请执行物理休息。"),
            ("🌈 窗外探测", "站起来看看窗外，确认这个世界除了电子表格还有别的颜色。"),
            ("🌬️ 浊气排放", "深呼吸，吐掉被老板气出的那口老血，重新出发。"),
            ("🕺 赛博广播操", "活动一下腰椎，你的身体比你的项目更需要维护。"),
        ];

        /// <summary>
        /// 假期/节日狂欢语录
        /// </summary>
        private static readonly (string Title, string Content)[] _holidayQuotes = [
            ("🌴 假期模式开启", "手机关机，人间蒸发。老板是谁？不认识，没见过。"),
            ("💤 睡到自然醒", "恭喜你成功脱离五指山，请尽情挥霍这该死的自由。"),
            ("🍦 快乐不打折", "放假不摸鱼，脑子变木塔。今天的任务只有两个字：玩乐！"),
            ("🏰 被窝封印", "今日任务：在床上躺成一尊永恒的雕塑，拒绝任何光合作用。"),
            ("🚫 拒绝联络", "检测到非紧急工作信息，系统已自动将其投递至脑后垃圾桶。"),
            ("🎭 角色切换", "现在的我不是 XX 员工，是这颗星球上最伟大的流浪者。"),
        ];

        /// <summary>
        /// 发薪日专属动力语录
        /// </summary>
        private static readonly (string Title, string Content)[] _paydayQuotes = [
            ("💰 余额已回血", "今天的工作动力 100% 由人民币提供！老板看我的眼神都温柔了。"),
            ("🍖 晚饭加个腿", "工资到账，腰杆变硬。今晚必须犒劳一下这位辛苦搬砖的猛士！"),
            ("💸 暂时的富豪", "这笔钱虽然只是在我卡里路过，但那一秒钟，我确实是富有的。"),
            ("🎰 数字跳动", "看着卡里增加的余额，感觉我又能忍受老板五分钟了。"),
            ("🕯️ 奢华一夜", "今晚点外卖不看满减，这是发薪日给予我最后的底气。"),
            ("💎 散发光芒", "此时的我，走路带风，看隔壁桌的秃顶同事都觉得慈眉善目。"),
        ];

        /// <summary>
        /// 下班倒计时
        /// </summary>
        /// <param name="now"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static string GetOffWorkString(DateTime now, HolidayService service)
        {
            if (IsRestDay(now, service)) return "休息中";

            if (!TimeSpan.TryParse(Settings.Default.EndTime, out var offTime))
            {
                offTime = new(17, 30, 0);
            }

            DateTime target = DateTime.Today.Add(offTime);
            if (now >= target) return "已下班";

            // 计算总剩余秒数
            TimeSpan totalRemaining = target - now;

            return $"{(int)totalRemaining.TotalHours:D2}:{totalRemaining.Minutes:D2}:{totalRemaining.Seconds:D2}";

        }

        /// <summary>
        /// 周末倒计时
        /// </summary>
        /// <param name="now"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static string GetWeekendString(DateTime now, HolidayService service)
        {
            if (!service.IsDataReady) return "正在同步假表...";

            if (!TimeSpan.TryParse(Settings.Default.EndTime, out var offTime))
            {
                offTime = new(17, 30, 0);
            }
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
            int pd = Settings.Default.PayDay;
            int daysInMonth = DateTime.DaysInMonth(baseDate.Year, baseDate.Month);
            DateTime payDate = new(baseDate.Year, baseDate.Month, Math.Min(pd, daysInMonth));

            // 如果发薪日是休息日
            if (IsRestDay(payDate, service))
            {
                int strategy = Settings.Default.PaydayStrategy; // 0:提前, 1:延后
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

        /// <summary>
        /// 发薪日倒计时
        /// </summary>
        /// <param name="now"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static string GetPaydayString(DateTime now, HolidayService service)
        {
            if (!service.IsDataReady) return "同步中...";

            if (!TimeSpan.TryParse(Settings.Default.EndTime, out var offTime))
            {
                offTime = new TimeSpan(18, 0, 0);
            }

            // 1. 先算本月的调整后发薪日
            DateTime currentPayday = GetAdjustedPayday(now, service);

            // 2. 如果今天已经过了本月发薪日（假设17点后算第二天），算下个月的
            if (now > currentPayday.Add(offTime))
            {
                currentPayday = GetAdjustedPayday(now.AddMonths(1), service);
            }

            int diff = (currentPayday.Date - now.Date).Days;

            if (diff == 0) return Merge(_paydayQuotes[_rng.Next(_paydayQuotes.Length)]);
            return $"{diff} 天";
        }

        // 🌟 将元组内容合并为一行字符串
        private static string Merge((string Title, string Content) quote) => $"{quote.Title} {quote.Content}";

        private static string Merge(string prefix, string context) => $"{prefix} {context}";

        private static string Merge(string prefix, (string Title, string Content) quote) => $"{prefix} {quote.Title} {quote.Content}";

        /// <summary>
        /// 动态提示语
        /// </summary>
        /// <param name="now"></param>
        /// <param name="todayInfo"></param>
        /// <param name="service"></param>
        /// <param name="lastRestTime"></param>
        /// <returns></returns>
        public static string GetDynamicTip(DateTime now, HolidayItem todayInfo, HolidayService service, DateTime lastRestTime)
        {
            // 节假日、休息日
            if (todayInfo?.Type == DayType.Holiday || IsRestDay(now, service)) return Merge(_holidayQuotes[_rng.Next(_holidayQuotes.Length)]);

            // 核心时间段逻辑 (干饭/午休/休息)
            TimeSpan currentTime = now.TimeOfDay;
            if (!TimeSpan.TryParse(Settings.Default.MealTime, out var mealTime))
            {
                mealTime = new(11, 45, 0);
            }
            if (!TimeSpan.TryParse(Settings.Default.LunchStart, out var lStart))
            {
                lStart = new(12, 00, 0);
            }
            if (!TimeSpan.TryParse(Settings.Default.LunchEnd, out var lEnd))
            {
                lEnd = new(13, 00, 0);
            }
            if (!TimeSpan.TryParse(Settings.Default.EndTime, out var offTime))
            {
                offTime = new(17, 30, 0);
            }

            // 鸡汤前缀
            string prefix = "";
            if (todayInfo?.Type == DayType.Tiaoxiu)
            {
                prefix = "😫 补班中 | ";
            }
            else if (now.Date == GetAdjustedPayday(now, service).Date && currentTime < offTime)
            {
                prefix = "💰 发薪日 | ";
            }

            // 午饭时间
            if (currentTime >= mealTime && currentTime < lStart)
                return Merge(prefix, GetRandomMealCheer());

            // 午休时间
            if (currentTime >= lStart && currentTime < lEnd)
                return Merge(prefix, _lunchQuotes[_rng.Next(_lunchQuotes.Length)]);

            // 临近下班逻辑
            int preMins = Settings.Default.PreOffWorkMins > 0 ? Settings.Default.PreOffWorkMins : 30;
            if (currentTime >= offTime.Subtract(TimeSpan.FromMinutes(preMins)) && currentTime < offTime)
            {
                double remaining = (offTime - currentTime).TotalMinutes;
                if (remaining <= 5)
                    prefix += "🚀 还有 " + Math.Ceiling(remaining) + " 分钟！随时弹射！";

                if (IsRestDay(DateTime.Today.AddDays(1), service))
                    return Merge(prefix, _lastDayQuotes[_rng.Next(_lastDayQuotes.Length)]);

                return Merge(prefix, GetRandomOffWorkCheer());
            }


            // 久坐提醒（如果接近间隔时间，强制变更为提醒状态）
            if (Settings.Default.IsRestEnabled)
            {
                int workMins = (int)(now - lastRestTime).TotalMinutes;
                if (workMins >= Settings.Default.RestInterval)
                    return Merge(prefix, GetRandomRestCheer(workMins));
            }

            // 4. 默认毒鸡汤
            return Merge(prefix, GetRandomSlackerQuote(now));
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
        /// 获取下一个节假日信息
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
                return (nextH.Name ?? "节假日", $"还有 {days} 天");
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
            int mode = Settings.Default.WorkMode; // 0:双休, 1:大小周, 2:单休
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
            var (title, content) = _restQuotes[_rng.Next(_restQuotes.Length)];
            // 将间隔时间动态注入到文案中
            string formattedContent = string.Format(content, interval);
            return (title, formattedContent);
        }
    }
}