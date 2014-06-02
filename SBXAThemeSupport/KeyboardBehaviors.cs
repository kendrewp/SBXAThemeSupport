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

        #region Public Properties

        /// <summary>
        ///     Gets or sets the command to execute when Ctrl-Shift-O is executed.
        /// </summary>
        public ICommand CtrlShiftDKeyUpCommand { get; set; }

        /// <summary>
        ///     Gets or sets the command to execute when Ctrl-Shift-G is executed.
        /// </summary>
        public ICommand CtrlShiftGKeyUpCommand { get; set; }

        /// <summary>
        ///     Gets or sets the command to execute when Ctrl-Shift-K is executed.
        /// </summary>
        public ICommand CtrlShiftKKeyUpCommand { get; set; }

        /// <summary>
        ///     Gets or sets the command to execute when Ctrl-Shift-L is executed.
        /// </summary>
        public ICommand CtrlShiftLKeyUpCommand { get; set; }

        /// <summary>
        ///     Gets or sets the command to execute when Ctrl-Shift-T is executed.
        /// </summary>
        public ICommand CtrlShiftOKeyUpCommand { get; set; }

        /// <summary>
        ///     Gets or sets the command to execute when Ctrl-Shift-T is executed.
        /// </summary>
        public ICommand CtrlShiftTKeyUpCommand { get; set; }

        /// <summary>
        ///     Gets or sets the Ctrl-X key up command.
        /// </summary>
        /// <value>
        ///     The control x key up command.
        /// </value>
        public ICommand CtrlXKeyUpCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     The on attached.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.KeyUp += this.HandleAssociatedObjectKeyUp;
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
            this.AssociatedObject.KeyUp -= this.HandleAssociatedObjectKeyUp;
        }

        /// <summary>
        /// Handles the associated object key up.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="KeyEventArgs"/> instance containing the event data.
        /// </param>
        private void HandleAssociatedObjectKeyUp(object sender, KeyEventArgs e)
        {
            // first check to see if there is a command registered for the key combination.
            var keyEventArgs = e;
            if (keyEventArgs == null)
            {
                return;
            }

            var isCtrlShift = (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift))
                              == (ModifierKeys.Control | ModifierKeys.Shift);

            if (!isCtrlShift)
            {
                // Check if the user hit Ctrl-X, if so send a Ctr-X to the server.
                if (keyEventArgs.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control && !keyEventArgs.Handled
                    && this.CtrlXKeyUpCommand != null)
                {
                    this.CtrlXKeyUpCommand.Execute(e);
                }

                return;
            }

            switch (e.Key)
            {
                case Key.T:
                    if (this.CtrlShiftTKeyUpCommand != null)
                    {
                        this.CtrlShiftTKeyUpCommand.Execute(e);
                        return;
                    }

                    break;
                case Key.O:
                    if (this.CtrlShiftOKeyUpCommand != null)
                    {
                        this.CtrlShiftOKeyUpCommand.Execute(e);
                        return;
                    }

                    break;
                case Key.D:
                    if (this.CtrlShiftDKeyUpCommand != null)
                    {
                        this.CtrlShiftDKeyUpCommand.Execute(e);
                        return;
                    }

                    break;
                case Key.K:
                    if (this.CtrlShiftKKeyUpCommand != null)
                    {
                        this.CtrlShiftKKeyUpCommand.Execute(e);
                        return;
                    }

                    break;
                case Key.L:
                    if (this.CtrlShiftLKeyUpCommand != null)
                    {
                        this.CtrlShiftLKeyUpCommand.Execute(e);
                        return;
                    }

                    break;
                case Key.G:
                    if (this.CtrlShiftGKeyUpCommand != null)
                    {
                        this.CtrlShiftGKeyUpCommand.Execute(e);
                        return;
                    }

                    break;
            }
        }

        /// <summary>
        /// The handle main window key up.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleMainWindowKeyUp(object sender, KeyEventArgs e)
        {
            this.HandleAssociatedObjectKeyUp(sender, e);
            e.Handled = true;
        }

        /// <summary>
        /// The handle sb window closed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleSBWindowClosed(object sender, EventArgs e)
        {
            try
            {
                var window = sender as SBWindow;
                if (window != null)
                {
                    window.Closed -= this.HandleSBWindowClosed;
                    this.AssociatedObject.KeyUp -= this.HandleAssociatedObjectKeyUp;
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

        /// <summary>
        /// The listen to window close.
        /// </summary>
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