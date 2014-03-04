// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InfoButtonConverter.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Converters
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     The info button converter.
    /// </summary>
    public class InfoButtonConverter : IValueConverter
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        #endregion
    }

    /// <summary>
    ///     The multi binding to string array converter.
    /// </summary>
    public class MultiBindingToStringArrayConverter : IMultiValueConverter
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
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

            return returnValues;
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
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0] { };
        }

        #endregion
    }
}
