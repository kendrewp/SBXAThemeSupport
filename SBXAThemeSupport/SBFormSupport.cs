// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SBFormSupport.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="SBFormSupport.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
    using System;
    using System.Diagnostics;
    using System.Windows.Input;

    using SBXA.Shared;
    using SBXA.Shared.Definitions;
    using SBXA.UI.Client;
    using SBXA.UI.WPFControls;

    /// <summary>
    ///     The sb form support.
    /// </summary>
    public class SBFormSupport
    {
        #region Public Methods and Operators

        /// <summary>
        ///     The add handlers.
        /// </summary>
        public static void AddHandlers()
        {
            SBPlusClient.SBFormCreated += HandleSBPlusClientSBFormCreated;
        }

        /// <summary>
        /// The send response.
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        public static void SendResponse(SBString response)
        {
            SBPlus.Current.SetInputState(SBInputState.Dormant, "Sending response to SB/XA server.");
            SBPlusClient.LogInformation(
                "Responded to message." + response.GetStandardString() + "(" + Convert.ToString(DateTime.Now.Millisecond) + ")");
            try
            {
                SBPlus.Current.SBPlusRuntime.RespondToLastMessage(response);
            }
            catch (SBApplicationException exception)
            {
                if (exception.ErrorCode.Equals("CS0049"))
                {
                    SBDisplay.Display(SBDisplayTypes.Fatal, "An Encoding error occurred handing a message", exception);
                    SBPlus.Current.SetInputState(SBInputState.WaitingForInput, "Encoding error.");
                }
            }
            catch (Exception exception2)
            {
                SBPlusClient.LogError("An error occurred handing a message.", exception2);
                SBPlus.Current.SetInputState(SBInputState.WaitingForInput, "An error occurred handing a message.");
            }
        }

        /// <summary>
        /// The key up handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public static void KeyUpHandler(object sender, KeyEventArgs args)
        {
            if (args.Key != Key.X || Keyboard.Modifiers != ModifierKeys.Control || args.Handled)
            {
                return;
            }

            Debug.WriteLine("[SBFormSupport.keyUpHandler(30)] Sending Ctrl-X back to the server. ");
            SendControlX();
        }

        #endregion

        #region Methods

        private static void HandleSBPlusClientSBFormCreated(object sender, SBFormCreatedEventArgs e)
        {
            var sbForm = e.Form as SBForm;
            if (sbForm != null)
            {
                Keyboard.AddKeyUpHandler(sbForm, KeyUpHandler);
            }
        }

        private static void SendControlX()
        {
            var field2 = SBFocusManager.FocusedControl as ISBField;
            SendResponse(
                field2 != null
                    ? new GuiInputEvent(
                          field2.SBValue, 
                          SBCommands.SBCtrlxCommand.SBEvent, 
                          SBCommands.SBCtrlxCommand.SBKeyValue, 
                          field2.CursorPosition + 1).ResponseString
                    : new GuiInputEvent(string.Empty, SBCommands.SBCtrlxCommand.SBEvent, SBCommands.SBCtrlxCommand.SBKeyValue, 1)
                          .ResponseString);
        }

        #endregion
    }
}
