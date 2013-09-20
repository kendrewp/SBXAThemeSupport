using System;
using System.Windows;
using System.Windows.Input;
using SBXA.UI.WPFControls;

namespace SBXAThemeSupport.Controls
{
    /// <summary>
    /// I subclassed SBTextBox as opposed to creating another control that is specifically used where needed in order to minimize the changes that would be needed
    /// if Rocket update SBTextBox.
    /// </summary>
    public class CustomSBTextBox : SBTextBox
    {
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

    }
}
