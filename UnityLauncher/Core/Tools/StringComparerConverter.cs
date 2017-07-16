using System;
using System.Globalization;
using System.Windows.Data;

namespace UnityLauncher.Core
{
    public class StringComparerConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return false;
            if (values.Length < 2) return false;
            if (values[0] is string && values[1] is string)
            {
                return (string) values[0] == (string) values[1];
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }
    }
}