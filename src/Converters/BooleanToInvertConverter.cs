using System;
using Windows.UI.Xaml.Data;

namespace UwpMaterialClock.Converters
{
    public class BooleanToInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool) value)
                return false;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}