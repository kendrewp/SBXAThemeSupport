// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyboardBehaviors.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="KeyboardBehaviors.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interactivity;
    using System.Windows.Threading;

    using SBXA.UI.Client;
    using SBXA.UI.WPFControls;

    /// <summary>
    ///     The keyboard behaviors.
    /// </summary>
    public class KeyboardBehaviors : Behavior<ContentControl>
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyboardBehaviors" /> class.
        /// </summary>
        public KeyboardBehaviors()
        {
            Application.Current.MainWindow.KeyUp += this.HandleMainWindowKeyUp;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The on attached.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.KeyUp += HandleAssociatedObjectKeyUp;
            this.Dispatcher.BeginInvoke(
                DispatcherPriority.Input, 
                new DispatcherOperationCallback(
                    delegate
                        {
                            this.ListenToWindowClose();
                            return null;
                        }), 
                null);
        }

        /// <summary>
        ///     The on detaching.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.KeyUp -= HandleAssociatedObjectKeyUp;
        }

        private static void HandleAssociatedObjectKeyUp(object sender, KeyEventArgs e)
        {
            if (UiAssistant.Current.KeyUpCommand != null && UiAssistant.Current.KeyUpCommand.CanExecute(e))
            {
                UiAssistant.Current.KeyUpCommand.Execute(e);
            }
        }

        private void HandleMainWindowKeyUp(object sender, KeyEventArgs e)
        {
            HandleAssociatedObjectKeyUp(sender, e);
            e.Handled = true;
        }

        private void HandleSBWindowClosed(object sender, EventArgs e)
        {
            try
            {
                var window = sender as SBWindow;
                if (window != null)
                {
                    window.Closed -= this.HandleSBWindowClosed;
                    this.AssociatedObject.KeyUp -= HandleAssociatedObjectKeyUp;
                }

                if (Application.Current != null && Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.KeyUp -= this.HandleMainWindowKeyUp;
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Problem cleaning up KeyboardBehaviours.", exception);
            }
        }

        private void ListenToWindowClose()
        {
            var sbWindow = this.AssociatedObject.FindAncestor<SBWindow>();
            if (sbWindow != null)
            {
                sbWindow.Closed += this.HandleSBWindowClosed;
            }
        }

        #endregion
    }
}