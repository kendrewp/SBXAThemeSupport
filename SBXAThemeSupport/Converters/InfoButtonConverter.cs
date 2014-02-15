using System;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace SBXAThemeSupport.Converters
{
    public class InfoButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var tooltip = new StringBuilder();
            var vals = value as string[];
            var ctr = 0;
            if (vals != null)
            {
                foreach (var var in vals)
                {
                    tooltip.AppendLine(ctr == 0 ? string.Format("Process Name : {0}", var) : string.Format(var));
                    ctr++;
                }
            }
            

            var version = Application.Current.GetType().Assembly.GetName().Version;

            tooltip.AppendLine(string.Format("Theme Version : {0}", version));

            
            return tooltip.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class MultiBindingToStringArrayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var returnValues = new string[values.Length];
            var ctr = 0;
            foreach (var val in values)
            {
                if (val == DependencyProperty.UnsetValue)
                {
                    returnValues[ctr++] = string.Empty;
                }
                else
                {
                    returnValues[ctr++] = System.Convert.ToString(val);
                }
            }

            
            return (returnValues);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
