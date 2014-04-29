// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SBStringViewerCellTemplateSelector.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="SBStringViewerCellTemplateSelector.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="SBStringViewerCellTemplateSelector.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.Views
{
    using System.Windows;
    using System.Windows.Controls;

    using SBXAThemeSupport.Models;

    /// <summary>
    ///     The sb string viewer cell template selector.
    /// </summary>
    public class SBStringViewerCellTemplateSelector : DataTemplateSelector
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the alternate template.
        /// </summary>
        public DataTemplate AlternateTemplate { get; set; }

        /// <summary>
        ///     Gets or sets the default template.
        /// </summary>
        public DataTemplate DefaultTemplate { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The select template.
        /// </summary>
        /// <param name="item">
        ///     The item.
        /// </param>
        /// <param name="container">
        ///     The container.
        /// </param>
        /// <returns>
        ///     The <see cref="DataTemplate" />.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var dataUnit = item as NestedAttribute;

            if (dataUnit == null)
            {
                return this.DefaultTemplate;
            }

            //lets see what template we need to select according to the specified property value
            if (dataUnit.IsNested)
            {
                return this.AlternateTemplate;
            }

            return this.DefaultTemplate;
        }

        #endregion
    }
}