using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Timers;
using CyberSlacker.Models;
using CyberSlacker.Services;
using CyberSlacker.Util;
using Timer = System.Timers.Timer;

namespace CyberSlacker.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly HolidayService _holidayService;
        private readonly Timer _timer;
        private readonly Random _rng = new Random();

        private bool _isDisposed = false;

        private bool _hasNotifiedMeal = false;
        private bool _hasNotifiedToday = false;
        private DateTime _lastRestNotifyTime = DateTime.Now;

        [ObservableProperty] private string _offWorkCountdown = "计算中...";
        [ObservableProperty] private string _weekendCountdown = "同步中...";
        [ObservableProperty] private string _payDayCountdown;
        [ObservableProperty] private string _holidayTip = "正在加载...";
        [ObservableProperty] private string _nextHolidayName = "加载中";
        [ObservableProperty] private string _nextHolidayCountdown;
        [ObservableProperty] private string _currentTime;
        [ObservableProperty] private string _taskbarTooltip;

        public MainViewModel()
        {
            // 1. 初始化服务
            _holidayService = new HolidayService(new TimorProvider());

            // 2. 初始化定时器 (1秒)
            _timer = new Timer(1000);
            _timer.Elapsed += (s, e) => UpdateAllProperties();
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void UpdateAllProperties()
        {
            if (_isDisposed) return;

            DateTime now = DateTime.Now;
            var todayInfo = _holidayService.GetDateInfo(now);

            _ = _holidayService.InitializeAsync(now.Year);

            // 1. 时间
            CurrentTime = now.ToString("HH:mm:ss");

            // 2. 下班（内部有默认值兜底）
            OffWorkCountdown = CountdownEngine.GetOffWorkString(now, _holidayService);

            // 3. 周末（现在内部有离线判定兜底，不会一直卡在“同步中”）
            WeekendCountdown = CountdownEngine.GetWeekendString(now, _holidayService);

            // 4. 发薪日
            PayDayCountdown = CountdownEngine.GetPaydayString(now, _holidayService);

            // 5. 下一个节日
            var holidayInfo = CountdownEngine.GetNextHolidayInfo(now, _holidayService);
            NextHolidayName = holidayInfo.Name;
            NextHolidayCountdown = holidayInfo.Countdown;

            DateTime startOfDay = new DateTime(now.Year, now.Month, now.Day);
            TimeSpan timeSpan = now - startOfDay;
            int seconds = (int)timeSpan.TotalSeconds;
            int step = _rng.Next(30, 300);

            // 6. 动态提示语 (每30秒换一次)
            if (seconds % step == 0 || string.IsNullOrEmpty(HolidayTip) || HolidayTip == "正在加载...")
            {
                HolidayTip = CountdownEngine.GetDynamicTip(now, todayInfo, _holidayService, _lastRestNotifyTime);
            }

            CheckOffWorkNotification(now);

            CheckMealNotification(now);

            CheckCyclicRestNotification(now);

            TaskbarTooltip = $"{HolidayTip}\n" +
                             $"下班倒计时: {OffWorkCountdown}\n" +
                             $"周末倒计时: {WeekendCountdown}\n" +
                             $"发薪日倒计时: {PayDayCountdown}\n" +
                             $"下一个节日: {NextHolidayName} ({NextHolidayCountdown})";
        }

        private void CheckOffWorkNotification(DateTime now)
        {
            // 1. 每天凌晨重置通知状态
            if (now.Hour == 0 && now.Minute == 0 && now.Second == 0)
            {
                _hasNotifiedToday = false;
            }

            // 2. 如果今天已经弹过窗了，直接退出
            if (_hasNotifiedToday) return;

            // 3. 【最重要】如果今天是休息日（含法定节假日和普通周末），不弹窗
            // 这里调用我们之前写在 Engine 里的 IsRestDay
            if (CountdownEngine.IsRestDay(now, _holidayService)) return;

            // 4. 判断是否到达下班点
            if (TimeSpan.TryParse(Properties.Settings.Default.EndTime, out var offTime))
            {
                DateTime target = DateTime.Today.Add(offTime);

                // 如果现在时间已经超过或等于下班时间
                if (now >= target)
                {
                    _hasNotifiedToday = true; // 标记今日已完成通知

                    var cheer = CountdownEngine.GetRandomOffWorkCheer();
                    // 发送一条“下班啦”的消息给 MainWindow
                    // 这种方式不需要 ViewModel 引用 MainWindow，非常解耦
                    WeakReferenceMessenger.Default.Send(new string[] { cheer.Title, cheer.Content }, "NotifyOffWork");
                }
            }
        }

        private void CheckMealNotification(DateTime now)
        {
            // 凌晨重置
            if (now.Hour == 0 && now.Minute == 0 && now.Second == 0) _hasNotifiedMeal = false;
            if (_hasNotifiedMeal) return;

            // 如果今天是休息日，不弹窗
            if (CountdownEngine.IsRestDay(now, _holidayService)) return;

            if (TimeSpan.TryParse(Properties.Settings.Default.MealTime, out var mealTime))
            {
                // 只要当前时间到了设定的提醒时间（精确到分）
                if (now.Hour == mealTime.Hours && now.Minute == mealTime.Minutes)
                {
                    _hasNotifiedMeal = true;
                    var cheer = CountdownEngine.GetRandomMealCheer();
                    WeakReferenceMessenger.Default.Send(new string[] { cheer.Title, cheer.Content }, "NotifyMeal");
                }
            }
        }

        private void CheckCyclicRestNotification(DateTime now)
        {
            if (!Properties.Settings.Default.IsRestEnabled) return;

            // 如果是休息日，不提醒
            if (CountdownEngine.IsRestDay(now, _holidayService)) return;

            // 计算距离上次提醒过了多少分钟
            double elapsedMinutes = (now - _lastRestNotifyTime).TotalMinutes;

            if (elapsedMinutes >= Properties.Settings.Default.RestInterval)
            {
                _lastRestNotifyTime = now; // 重置时间

                var cheer = CountdownEngine.GetRandomRestCheer(Properties.Settings.Default.RestInterval);

                // 2. 发送彩色通知消息
                WeakReferenceMessenger.Default.Send(new string[] {
                    cheer.Title,
                    cheer.Content
                }, "NotifyRest");
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _timer?.Stop();
                _timer?.Dispose();
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}