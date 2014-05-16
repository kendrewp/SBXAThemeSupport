// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeConverter.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     Converts the a DateTime into a time string including milliseconds.
    /// </summary>
    public class TimeConverter : IValueConverter
    {
        #region Public Methods and Operators

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">
        /// The value produced by the binding source.
        /// </param>
        /// <param name="targetType">
        /// The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is DateTime))
            {
                return value;
            }

            var time = (DateTime)value;
            return string.Format("{0}:{1}:{2}:{3}", time.Hour, time.Minute, time.Second, time.Millisecond);
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">
        /// The value that is produced by the binding target.
        /// </param>
        /// <param name="targetType">
        /// The type to convert to.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        #endregion
    }

    /// <summary>
    ///     Calculates the difference between two times.
    /// </summary>
    public class TimeDifferenceConverter : IMultiValueConverter
    {
        #region Public Methods and Operators

        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it
        ///     propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">
        /// The array of values that the source bindings in the
        ///     <see cref="T:System.Windows.Data.MultiBinding"/> produces. The value
        ///     <see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the source binding has no value to
        ///     provide for conversion.
        /// </param>
        /// <param name="targetType">
        /// The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of
        ///     <see cref="T:System.Windows.DependencyProperty"/>.<see cref="F:System.Windows.DependencyProperty.UnsetValue"/>
        ///     indicates that the converter did not produce a value, and that the binding will use the
        ///     <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> if it is available, or else will use the default
        ///     value.A return value of <see cref="T:System.Windows.Data.Binding"/>.
        ///     <see cref="F:System.Windows.Data.Binding.DoNothing"/> indicates that the binding does not transfer the value or
        ///     use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values[0] is DateTime) && !(values[1] is DateTime))
            {
                if (!(values[0] is int) && !(values[1] is int))
                {
                    return DependencyProperty.UnsetValue;
                }
            }

            if (values[0] is DateTime)
            {
                var start = (DateTime)values[0];
                var end = (DateTime)values[1];

                var time = end.Subtract(start);

                return time.Ticks < 0
                           ? string.Empty
                           : string.Format("{0}:{1}:{2}:{3}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            }

            return (int)values[1] < (int)values[0] ? string.Empty : string.Format("{0} ms", (int)values[1] - (int)values[0]);
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">
        /// The value that the binding target produces.
        /// </param>
        /// <param name="targetTypes">
        /// The array of types to convert to. The array length indicates the number and types of values
        ///     that are suggested for the method to return.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new[] { DependencyProperty.UnsetValue };
        }

        #endregion
    }
}