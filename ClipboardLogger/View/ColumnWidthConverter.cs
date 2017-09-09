using System;
using System.Globalization;
using System.Windows.Data;

namespace ClipboardManager.View
{
    public class ColumnWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double totalWidth = (double)value;

            return totalWidth / 3;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
