using System;
using System.Globalization;
using System.Windows.Data;

namespace CyberSlacker.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 增加空检查
            if (value == null || parameter == null) return false;
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 使用 null-forgiving 运算符 ! 或者 check
            if (value is bool b && b && parameter != null)
            {
                return int.Parse(parameter.ToString()!);
            }
            return Binding.DoNothing;
        }
    }
}
