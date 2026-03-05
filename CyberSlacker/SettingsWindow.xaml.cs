using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CyberSlacker.Util;

namespace CyberSlacker
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private bool _initialAutoStart;


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
        private void SetItemsSource(System.Collections.Generic.List<string> data, params ComboBox[] cmbs)
        {
            foreach (var cb in cmbs)
            {
                if (cb != null) cb.ItemsSource = data;
            }
        }

        // 仅允许输入数字的事件
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ParseAndSetTime(string timeStr, System.Windows.Controls.ComboBox hBox, System.Windows.Controls.ComboBox mBox)
        {
            try
            {
                var parts = timeStr.Split(':');
                hBox.SelectedItem = parts[0];
                mBox.SelectedItem = parts.Length > 1 ? parts[1] : "00";
            }
            catch { hBox.SelectedIndex = 0; mBox.SelectedIndex = 0; }
        }

        private void OnSave_Click(object sender, RoutedEventArgs e)
        {
            // --- 1. 非空与合法性校验 ---

            // 检查 ComboBox 是否选择了值
            if (StartHour.SelectedItem == null || StartMin.SelectedItem == null ||
                EndHour.SelectedItem == null || EndMin.SelectedItem == null)
            {
                MessageBox.Show("请选择完整的考勤时间！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MealHour.SelectedItem == null || MealMin.SelectedItem == null)
            {
                MessageBox.Show("请选择干饭时间！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (LunchStartHour.SelectedItem == null || LunchStartMin.SelectedItem == null ||
               LunchEndHour.SelectedItem == null || LunchEndMin.SelectedItem == null)
            {
                MessageBox.Show("请选择完整的午休时间！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 检查发薪日是否为空或非数字
            if (string.IsNullOrWhiteSpace(PayDayInput.Text))
            {
                MessageBox.Show("发薪日不能为空！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (int.TryParse(PayDayInput.Text, out int payday))
            {
                if (payday < 1 || payday > 31)
                {
                    MessageBox.Show("发薪日必须在 1 到 31 号之间！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                MessageBox.Show("发薪日格式不正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- 2. 逻辑校验 (可选) ---
            string sTime = $"{StartHour.SelectedItem}:{StartMin.SelectedItem}";
            string eTime = $"{EndHour.SelectedItem}:{EndMin.SelectedItem}";

            string lsTime = $"{LunchStartHour.SelectedItem}:{LunchStartMin.SelectedItem}";
            string leTime = $"{LunchEndHour.SelectedItem}:{LunchEndMin.SelectedItem}";

            if (TimeSpan.Parse(sTime) >= TimeSpan.Parse(eTime))
            {
                MessageBox.Show("下班时间不能早于上班时间！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TimeSpan.Parse(lsTime) >= TimeSpan.Parse(leTime))
            {
                MessageBox.Show("午休结束时间必须晚于开始时间！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TimeSpan.Parse(leTime) <= TimeSpan.Parse(sTime))
            {
                MessageBox.Show("午休结束时间必须晚于上班时间！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            this.Close();
        }
    }
}
