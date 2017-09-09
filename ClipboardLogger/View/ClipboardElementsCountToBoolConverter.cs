using System;
using System.Globalization;
using System.Windows.Data;

namespace ClipboardManager.View
{
    public class ClipboardElementsCountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int) value > 0)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
