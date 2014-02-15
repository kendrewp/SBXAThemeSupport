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
            HandleAssociatedObjectKeyUp(sender, e);
            e.Handled = true;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyUp += HandleAssociatedObjectKeyUp;
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
            AssociatedObject.KeyUp -= HandleAssociatedObjectKeyUp;
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
                AssociatedObject.KeyUp -= HandleAssociatedObjectKeyUp;
            }
            Application.Current.MainWindow.KeyUp -= HandleMainWindowKeyUp;

        }

        private static void HandleAssociatedObjectKeyUp(object sender, KeyEventArgs e)
        {
            if (UiAssistant.Current.KeyUpCommand != null && UiAssistant.Current.KeyUpCommand.CanExecute(e))
                UiAssistant.Current.KeyUpCommand.Execute(e);
        }
    }
}