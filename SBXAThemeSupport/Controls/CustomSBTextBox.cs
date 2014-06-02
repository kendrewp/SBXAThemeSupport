// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomSBTextBox.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="CustomSBTextBox.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    using SBXA.UI.Client;
    using SBXA.UI.WPFControls;

    using SBXAThemeSupport.ViewModels;

    /// <summary>
    ///     I subclassed SBTextBox as opposed to creating another control that is specifically used where needed in order to
    ///     minimize the changes that would be needed
    ///     if Rocket update SBTextBox.
    /// </summary>
    public class CustomSBTextBox : SBTextBox
    {
        #region Methods

        /// <summary>
        /// The on preview key down.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            try
            {
                // Find the field
                var sbField = SBUISupport.FindParentByType(this, typeof(SBField)) as SBField;
                if (sbField == null || sbField.GuiObjectDefinition == null || sbField.GuiObjectDefinition.FieldDefinition == null)
                {
                    return;
                }

                if (UiViewModel.Current.IsKeyDisabled(e, sbField.GuiObjectDefinition.FieldDefinition.FieldName))
                {
                    // Find SBMvEditControl
                    var sbMvEditControl = SBUISupport.FindParentByType(this, typeof(SBMvEditControl)) as SBMvEditControl;
                    if (sbMvEditControl == null)
                    {
                        return;
                    }

                    // Find the window
                    var sbWindow = SBUISupport.FindParentByType(this, typeof(SBWindow)) as SBWindow;
                    if (sbWindow == null)
                    {
                        return;
                    }

                    SBControl.SetShouldSendSBCommands(sbMvEditControl, false);

                    sbWindow.PreviewMouseDown += this.HandleSBWindowPreviewMouseDown;
                    e.Handled = true;

                    return;
                }
                else
                {
                    var sbMvEditControl = SBUISupport.FindParentByType(this, typeof(SBMvEditControl)) as SBMvEditControl;
                    if (sbMvEditControl != null)
                    {
                        SBControl.SetShouldSendSBCommands(sbMvEditControl, true);
                    }
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("An exception was caught while looking for disabled keys.", exception);
            }

            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// The on preview mouse double click.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            var sbTextBox = e.Source as SBTextBox;
            // If the SBTextBox has been created via SBRun, the FindParentByType throws an exception. So in order to prevent it I catch it and return.
            if (sbTextBox == null)
            {
                try
                {
                    // ReSharper disable once UnusedVariable
                    var textBox = SBUISupport.FindParentByType(e.Source as DependencyObject, typeof(SBTextBox)) as SBTextBox;
                }
                catch (Exception)
                {
                    e.Handled = true;
                    return;
                }
            }

            base.OnPreviewMouseDoubleClick(e);
        }

        /// <summary>
        /// The on preview mouse down.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            var sbMvEditControl = SBUISupport.FindParentByType(this, typeof(SBMvEditControl)) as SBMvEditControl;
            if (sbMvEditControl != null)
            {
                SBControl.SetShouldSendSBCommands(sbMvEditControl, true);
            }

            base.OnPreviewMouseDown(e);
        }

        /// <summary>
        /// The handle sb window preview mouse down.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleSBWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Find SBMvEditControl
            var sbMvEditControl = SBUISupport.FindParentByType(this, typeof(SBMvEditControl)) as SBMvEditControl;
            if (sbMvEditControl == null)
            {
                return;
            }

            // Find the window
            var sbWindow = SBUISupport.FindParentByType(this, typeof(SBWindow)) as SBWindow;
            if (sbWindow == null)
            {
                return;
            }

            sbWindow.PreviewMouseDown -= this.HandleSBWindowPreviewMouseDown;
            SBControl.SetShouldSendSBCommands(sbMvEditControl, true);
        }

        #endregion
    }
}