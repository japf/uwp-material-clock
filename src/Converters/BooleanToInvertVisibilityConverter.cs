using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace UwpMaterialClock.Converters
{
    public class BooleanToInvertVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool) value)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}