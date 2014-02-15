using System;
using System.Windows.Data;
using System.Collections;

namespace SBXAThemeSupport.Converters
{
    public class CommandParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Hashtable && parameter is string && ((Hashtable) value).ContainsKey(((string)parameter).ToUpper());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
