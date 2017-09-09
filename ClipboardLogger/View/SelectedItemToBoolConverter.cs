using System;
using System.Globalization;
using System.Windows.Data;
using ClipboardManager.Model;

namespace ClipboardManager.View
{
    class SelectedItemToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ClipboardElement element = (ClipboardElement) value;

            return (element != null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
