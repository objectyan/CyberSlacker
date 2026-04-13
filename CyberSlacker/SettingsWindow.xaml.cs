using CyberSlacker.Util;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CyberSlacker
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly bool _initialAutoStart;


        public SettingsWindow()
        {
            InitializeComponent();
            this.DataContext = Properties.Settings.Default;

            _initialAutoStart = Properties.Settings.Default.IsAutoStart;


            var hours = Enumerable.Range(0, 24).Select(i => i.ToString("D2")).ToList();
            var mins = Enumerable.Range(0, 60).Select(i => i.ToString("D2")).ToList();


            SetItemsSource(hours, StartHour, EndHour, MealHour, LunchStartHour, LunchEndHour);
            SetItemsSource(mins, StartMin, EndMin, MealMin, LunchStartMin, LunchEndMin);

            StartHour.ItemsSource = hours;
            EndHour.ItemsSource = hours;
            StartMin.ItemsSource = mins;
            EndMin.ItemsSource = mins;

            // 2. 解析当前存储的时间字符串 (例如 "08:30")
            ParseAndSetTime(Properties.Settings.Default.StartTime, StartHour, StartMin);
            ParseAndSetTime(Properties.Settings.Default.EndTime, EndHour, EndMin);

            ParseAndSetTime(Properties.Settings.Default.MealTime, MealHour, MealMin);

            ParseAndSetTime(Properties.Settings.Default.LunchStart, LunchStartHour, LunchStartMin);
            ParseAndSetTime(Properties.Settings.Default.LunchEnd, LunchEndHour, LunchEndMin);
        }

        /// <summary>
        /// 辅助方法：批量设置数据源
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cmbs"></param>
        private static void SetItemsSource(System.Collections.Generic.List<string> data, params ComboBox[] cmbs)
        {
            foreach (var cb in cmbs)
            {
                if (cb != null) cb.ItemsSource = data;
            }
        }


        [GeneratedRegex("[^0-9]+")]
        private static partial Regex NumericRegex();

        /// <summary>
        /// 仅允许输入数字的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) => e.Handled = NumericRegex().IsMatch(e.Text);


        private static void ParseAndSetTime(string timeStr, System.Windows.Controls.ComboBox hBox, System.Windows.Controls.ComboBox mBox)
        {
            try
            {
                var parts = timeStr.Split(':');
                hBox.SelectedItem = parts[0];
                mBox.SelectedItem = parts.Length > 1 ? parts[1] : "00";
            }
            catch { hBox.SelectedIndex = 0; mBox.SelectedIndex = 0; }
        }

        private async void OnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.SaveButton.IsEnabled = false;
                // --- 1. 非空与合法性校验 ---

                // 检查 ComboBox 是否选择了值
                if (StartHour.SelectedItem == null || StartMin.SelectedItem == null ||
                    EndHour.SelectedItem == null || EndMin.SelectedItem == null)
                {
                    ShowMsg("请选择完整的考勤时间！");
                    return;
                }

                if (MealHour.SelectedItem == null || MealMin.SelectedItem == null)
                {
                    ShowMsg("请选择干饭时间！");
                    return;
                }

                if (LunchStartHour.SelectedItem == null || LunchStartMin.SelectedItem == null ||
                   LunchEndHour.SelectedItem == null || LunchEndMin.SelectedItem == null)
                {
                    ShowMsg("请选择完整的午休时间！");
                    return;
                }

                // 检查发薪日是否为空或非数字
                if (string.IsNullOrWhiteSpace(PayDayInput.Text))
                {
                    ShowMsg("发薪日不能为空！");
                    return;
                }

                if (int.TryParse(PayDayInput.Text, out int payday))
                {
                    if (payday < 1 || payday > 31)
                    {
                        ShowMsg("发薪日必须在 1 到 31 号之间！");
                        return;
                    }
                }
                else
                {
                    ShowMsg("发薪日格式不正确！");
                    return;
                }

                // --- 2. 逻辑校验 (可选) ---
                string sTime = $"{StartHour.SelectedItem}:{StartMin.SelectedItem}";
                string eTime = $"{EndHour.SelectedItem}:{EndMin.SelectedItem}";

                string lsTime = $"{LunchStartHour.SelectedItem}:{LunchStartMin.SelectedItem}";
                string leTime = $"{LunchEndHour.SelectedItem}:{LunchEndMin.SelectedItem}";

                if (TimeSpan.Parse(sTime) >= TimeSpan.Parse(eTime))
                {
                    ShowMsg("下班时间不能早于上班时间！");
                    return;
                }

                if (TimeSpan.Parse(lsTime) >= TimeSpan.Parse(leTime))
                {
                    ShowMsg("午休结束时间必须晚于开始时间！");
                    return;
                }

                if (TimeSpan.Parse(leTime) <= TimeSpan.Parse(sTime))
                {
                    ShowMsg("午休结束时间必须晚于上班时间！");
                    return;
                }

                // --- 3. 执行保存 ---
                Properties.Settings.Default.StartTime = sTime;
                Properties.Settings.Default.EndTime = eTime;
                Properties.Settings.Default.MealTime = $"{MealHour.SelectedItem}:{MealMin.SelectedItem}";

                Properties.Settings.Default.LunchStart = lsTime;
                Properties.Settings.Default.LunchEnd = leTime;


                if (Properties.Settings.Default.IsAutoStart != _initialAutoStart)
                {
                    StartupHelper.SetStartup(Properties.Settings.Default.IsAutoStart);
                }

                Properties.Settings.Default.Save();

                await ShowStatus("同步成功，赛博核心已更新！💾", isSuccess: true, CloseWindowWithFade);

            }
            finally
            {
                this.SaveButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// 🌟 独立的窗口退出特效，不再跟提示条耦合
        /// </summary>
        private void CloseWindowWithFade()
        {
            DoubleAnimation windowFade = new(1, 0, TimeSpan.FromMilliseconds(100));
            windowFade.Completed += (s, ev) => this.Close();
            this.BeginAnimation(OpacityProperty, windowFade);
        }


        // 标题栏拖动
        private void OnHeaderDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
        }

        // 关闭按钮
        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool _isShowingMsg = false;

        /// <summary>
        /// 🌟 通用提示：支持成功(绿)和错误(红)
        /// </summary>
        private async Task ShowStatus(string message, bool isSuccess = false, Action? onFinished = null)
        {
            if (_isShowingMsg) return;
            _isShowingMsg = true;

            // 1. 根据状态切换颜色和图标
            if (isSuccess)
            {
                // 赛博绿 (#00E676)
                MsgBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F200E676"));
                MsgGlow.Color = (Color)ColorConverter.ConvertFromString("#00E676");
                MsgIcon.Text = "✅";
            }
            else
            {
                // 赛博红 (#FF5252)
                MsgBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2FF5252"));
                MsgGlow.Color = (Color)ColorConverter.ConvertFromString("#FF5252");
                MsgIcon.Text = "⚠️";
            }

            MsgText.Text = message;

            // 2. 动画显示 (保持之前的 BackEase 效果)
            DoubleAnimation fadeIn = new(0, 1, TimeSpan.FromMilliseconds(300));
            DoubleAnimation moveUp = new(30, 0, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new BackEase { Amplitude = 0.8, EasingMode = EasingMode.EaseOut }
            };
            MsgBar.BeginAnimation(OpacityProperty, fadeIn);
            MsgTransform.BeginAnimation(TranslateTransform.YProperty, moveUp);

            // 3. 无论成功失败，都只负责显示提示
            await Task.Delay(2500);

            // 4. 播放滑出动画
            DoubleAnimation fadeOut = new(1, 0, TimeSpan.FromMilliseconds(300));
            MsgBar.BeginAnimation(OpacityProperty, fadeOut);

            await Task.Delay(300);
            _isShowingMsg = false;

            onFinished?.Invoke();
        }

        private async void ShowMsg(string message)
        {
            _ = Task.Run(async () => await ShowStatus(message, false));
        }
    }
}
