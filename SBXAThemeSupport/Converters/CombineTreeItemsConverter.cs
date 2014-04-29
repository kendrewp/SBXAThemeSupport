using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Data;
using SBXA.Shared;
using SBXAThemeSupport.Models;

namespace SBXAThemeSupport.Converters
{
    using System.Windows;

    /// <summary>
    /// This converter will combine multiple <see cref="IEnumerable"/> collections into a single ObservableCollection/>.
    /// </summary>
    public class CombineTreeItemsConverter :IMultiValueConverter
    {
        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<ITreeItem> newCollection = new ObservableCollection<ITreeItem>();
            try
            {
                var text = parameter as string;
                var descriptions = new string[values.Length];
                if (text != null)
                {
                    descriptions = text.Split(GenericConstants.CHAR_ARRAY_COMMA);
                }
                var collCtr = 0;
                
                foreach (var sourceColl in values)
                {
                    if (!(sourceColl is IEnumerable))
                    {
                        continue;
                    }
                    TreeItem parentTreeItem = new TreeItem(string.Empty) { Description = (string.IsNullOrEmpty(descriptions[collCtr]) ? string.Empty : descriptions[collCtr]) };

                    parentTreeItem.Children = sourceColl as IEnumerable; // must assign it here otherwise when the children are added they are not updated.

                    collCtr++;
                    newCollection.Add(parentTreeItem);
                }
            }
            catch (Exception exception)
            {
                CustomLogger.LogException(exception, "Problem combining collections.");
                return (DependencyProperty.UnsetValue);
            }
            return newCollection;
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new object[0];
        }
    }
}
