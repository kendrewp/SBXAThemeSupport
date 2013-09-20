using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;
using SBXA.UI.WPFControls;

namespace SBXAThemeSupport
{

    // Behavior<ContentControl>
    public class KeyboardBehaviors : Behavior<ContentControl>
    {
        public KeyboardBehaviors()
        {
            Application.Current.MainWindow.KeyUp += HandleMainWindowKeyUp;
        }

        private void HandleMainWindowKeyUp(object sender, KeyEventArgs e)
        {
            AssociatedObject_KeyUp(sender, e);
            e.Handled = true;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyUp += AssociatedObject_KeyUp;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(delegate
                                                                                                 {
                                                                                                     ListenToWindowClose
                                                                                                         ();
                                                                                                     return null;
                                                                                                 }), null);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
        }

        private void ListenToWindowClose()
        {
            var sbWindow = AssociatedObject.FindAncestor<SBWindow>();
            if (sbWindow != null)
            {
                sbWindow.Closed += HandleSBWindowClosed;
            }
        }

        private void HandleSBWindowClosed(object sender, EventArgs e)
        {
            var window = sender as SBWindow;
            if (window != null)
            {
                window.Closed -= HandleSBWindowClosed;
                AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
            }
            Application.Current.MainWindow.KeyUp -= HandleMainWindowKeyUp;

        }

        private void AssociatedObject_KeyUp(object sender, KeyEventArgs e)
        {
            if (SBXAThemeSupport.UiAssistant.Current.KeyUpCommand != null && SBXAThemeSupport.UiAssistant.Current.KeyUpCommand.CanExecute(e))
                SBXAThemeSupport.UiAssistant.Current.KeyUpCommand.Execute(e);
        }
    }
}