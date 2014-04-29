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

using System.Diagnostics;

namespace SBXAThemeSupport.Views
{
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;

    using SBXAThemeSupport.Models;

    /// <summary>
    ///     The sb string viewer cell template selector.
    /// </summary>
    public class AnalysisTreeItemTemplateSelector : DataTemplateSelector
    {
        #region Fields

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the alternate template.
        /// </summary>
        public DataTemplate ScreenDefinitionTemplate { get; set; }

        /// <summary>
        ///     Gets or sets the default template.
        /// </summary>
        public DataTemplate DefaultTemplate { get; set; }

        /// <summary>
        /// Gets or sets the field definition template.
        /// </summary>
        /// <value>
        /// The field definition template.
        /// </value>
        public DataTemplate FieldDefinitionTemplate { get; set; }

        /// <summary>
        /// Gets or sets the tree item template.
        /// </summary>
        /// <value>
        /// The tree item template.
        /// </value>
        public DataTemplate TreeItemTemplate { get; set; }

        /// <summary>
        /// Gets or sets the paragraph description template.
        /// </summary>
        /// <value>
        /// The paragraph description template.
        /// </value>
        public DataTemplate ParagraphDescriptionTemplate { get; set; }

        /// <summary>
        /// Gets or sets the basic program description template.
        /// </summary>
        /// <value>
        /// The basic program description template.
        /// </value>
        public DataTemplate BasicProgramDescriptionTemplate { get; set; }

        /// <summary>
        /// Gets or sets the file update description template.
        /// </summary>
        /// <value>
        /// The file update description template.
        /// </value>
        public DataTemplate FileUpdateDescriptionTemplate { get; set; }

        /// <summary>
        /// Gets or sets the sb expression template.
        /// </summary>
        /// <value>
        /// The sb expression template.
        /// </value>
        public DataTemplate SBExpressionTemplate { get; set; }

        /// <summary>
        /// Gets or sets the menu definition template.
        /// </summary>
        /// <value>
        /// The menu definition template.
        /// </value>
        public DataTemplate MenuDefinitionTemplate { get; set; }
        
        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The select template.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <returns>
        /// The <see cref="DataTemplate"/>.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
            {
                return this.DefaultTemplate;
            }
            Debug.WriteLine("[AnalysisTreeItemTemplateSelector.SelectTemplate(68)] " + item.GetType().Name);

            var dataUnit = item as TreeItem;

            if (dataUnit == null)
            {
                return this.DefaultTemplate;
            }

            Debug.WriteLine("[AnalysisTreeItemTemplateSelector.SelectTemplate(78)] " + dataUnit.GetType().Name);

            if (dataUnit is SBExpression)
            {
                return this.SBExpressionTemplate;
            }

            if (dataUnit is FieldDefinition)
            {
                return this.FieldDefinitionTemplate;
            }

            if (dataUnit is ProcessCall)
            {
                var processDescription = ((ProcessCall)dataUnit).ProcessDescription;
                if (processDescription == null)
                {
                    return this.DefaultTemplate;
                }
                Debug.WriteLine("[AnalysisTreeItemTemplateSelector.SelectTemplate(89)] " + processDescription.GetType().Name);
                //lets see what template we need to select according to the specified property value
                if (processDescription is ScreenDefintion)
                {
                    return this.ScreenDefinitionTemplate;
                }

                if (processDescription is ParagraphDescription)
                {
                    return this.ParagraphDescriptionTemplate;
                }

                if (processDescription is BasicProgramDescription)
                {
                    return this.BasicProgramDescriptionTemplate;
                }
                
                if (processDescription is FileUpdateDefinitionDescription)
                {
                    return this.FileUpdateDescriptionTemplate;
                }
                
                if (processDescription is MenuDefinitionDescription)
                {
                    return this.MenuDefinitionTemplate;
                }
                

                return this.DefaultTemplate;
            }

            return this.TreeItemTemplate;
        }

        #endregion
    }
}