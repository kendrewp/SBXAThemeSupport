using System;
using System.Diagnostics;
using System.Windows.Input;
using SBXA.Shared;
using SBXA.Shared.Definitions;
using SBXA.UI.Client;
using SBXA.UI.WPFControls;

namespace SBXAThemeSupport
{
    public class SBFormSupport
    {
        public static void AddHandlers()
        {
            SBPlusClient.SBFormCreated += HandleSBPlusClientSBFormCreated;
        }

        static void HandleSBPlusClientSBFormCreated(object sender, SBFormCreatedEventArgs e)
        {
            SBForm sbForm = e.Form as SBForm;
            if (sbForm != null)
            {
                Keyboard.AddKeyUpHandler(sbForm, keyUpHandler);
            }
        }

        public static void keyUpHandler(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control && !args.Handled)
            {
                Debug.WriteLine("[SBFormSupport.keyUpHandler(30)] Sending Ctrl-X back to the server. ");
                SendControlX();
            }
        }

        public static void SendResponse(SBString response)
        {
            SBPlus.Current.SetInputState(SBInputState.Dormant, "Sending response to SB/XA server.");
            SBPlusClient.LogInformation("Responded to message." + response.GetStandardString() + "(" + Convert.ToString(DateTime.Now.Millisecond) + ")");
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

        private static void SendControlX()
        {
            ISBField field2 = SBFocusManager.FocusedControl as ISBField;
            SendResponse(field2 != null
                             ? new GuiInputEvent(field2.SBValue, SBCommands.SBCtrlxCommand.SBEvent, SBCommands.SBCtrlxCommand.SBKeyValue, field2.CursorPosition + 1).ResponseString
                             : new GuiInputEvent(string.Empty, SBCommands.SBCtrlxCommand.SBEvent, SBCommands.SBCtrlxCommand.SBKeyValue, 1).ResponseString);
        }
    }
}
