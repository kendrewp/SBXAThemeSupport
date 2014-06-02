// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationHelper.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
    using System;
    using System.Windows;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.UI.Client;
    using SBXA.UI.WPFControls;

    /// <summary>
    ///     The platform.
    /// </summary>
    public enum Platform
    {
        /// <summary>
        ///     The uni data.
        /// </summary>
        UniData, 

        /// <summary>
        ///     The uni verse.
        /// </summary>
        UniVerse
    }

    /// <summary>
    ///     The application helper.
    /// </summary>
    public static class ApplicationHelper
    {
        #region Static Fields

        /// <summary>
        /// The last can send value.
        /// </summary>
        private static bool lastCanSendValue; // To hold the last value of CanSend, so I can only raise the event when it changes.

        /// <summary>
        /// The platform.
        /// </summary>
        private static Platform platform;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="ApplicationHelper" /> class.
        /// </summary>
        static ApplicationHelper()
        {
            if (SBPlusRuntime.Current != null)
            {
                SBPlusRuntime.Current.DisConnected += SBPlusDisconnected;
                SBPlusClientOnConnected(null, null);
            }
            else
            {
                SBPlusClient.Connected += SBPlusClientOnConnected;
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     Register a listener for the ExecuteSBProcess Event.
        /// </summary>
        public static event CanSendCommandChangedEventHandler CanSendCommandChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the current account name.
        /// </summary>
        public static string CurrentAccountName
        {
            get
            {
                return GetCurrentAccountName();
            }
        }

        /// <summary>
        ///     Gets the current form sb handle.
        /// </summary>
        public static string CurrentFormSbHandle
        {
            get
            {
                if (SBPlus.Current != null && SBPlus.Current.CurrentForm != null)
                {
                    return SBPlus.Current.CurrentForm.SBObjectHandle;
                }

                return string.Empty;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether has server pending jobs to process.
        /// </summary>
        public static bool HasServerPendingJobsToProcess
        {
            get
            {
                return SbProcessRunner.Instance.HasJobsToProcess || SBPlusRuntime.Current.GuiInputQueue.Count > 0
                       || !CanSendServerCommands() || !SBPlusRuntime.Current.CommandProcessor.IsServerWaiting;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether is current account null.
        /// </summary>
        public static bool IsCurrentAccountNull
        {
            get
            {
                return string.IsNullOrEmpty(CurrentAccountName);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether is sb dialog box open.
        /// </summary>
        public static bool IsSbDialogBoxOpen
        {
            get
            {
                if (SBPlus.Current == null || SBPlus.Current.CurrentForm == null)
                {
                    return false;
                }

                return SBPlus.Current.CurrentForm is SBDialog;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether is startgui.
        /// </summary>
        public static bool IsStartgui
        {
            get
            {
                if (SBPlus.Current != null && !SBPlus.Current.CheckAccess())
                {
                    var operation = SBPlus.Current.Dispatcher.Invoke(new Func<bool>(() => IsStartgui));
                    if (operation == null)
                    {
                        return false;
                    }

                    return Convert.ToBoolean(operation);
                }

                return SBPlus.Current != null && SBPlus.Current.CurrentSystemId == "YS";
            }
        }

        /// <summary>
        ///     Gets a value indicating whether is startgui ready.
        /// </summary>
        public static bool IsStartguiReady
        {
            get
            {
                if (!IsStartgui || SBPlus.Current.ConnectionStatus != ConnectionStatuses.Connected)
                {
                    return false;
                }

                return SBPlus.Current.CurrentSystemId == "YS" && CanSendServerCommands(false);
            }
        }

        /// <summary>
        ///     Gets or sets the platform.
        /// </summary>
        /// <value>
        ///     The platform.
        /// </value>
        public static Platform Platform
        {
            get
            {
                return platform;
            }

            set
            {
                platform = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The can send server commands.
        /// </summary>
        /// <param name="commandCouldCauseUiAction">
        /// The command could cause ui action.
        /// </param>
        /// <param name="doSendEventsBeforeCheck">
        /// The do send events before check.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool CanSendServerCommands(bool commandCouldCauseUiAction = true, bool doSendEventsBeforeCheck = false)
        {
#if SHOW_DEBUG
            CustomLogger.LogDebug(() => string.Format("commandCouldCauseUiAction {0}", commandCouldCauseUiAction));
#endif
            if (Application.Current == null || Application.Current.Dispatcher == null || SBPlus.Current == null
                || SBPlusRuntime.Current == null)
            {
                return false;
            }

            bool canSend = false;
            if (commandCouldCauseUiAction)
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    JobManager.RunSyncInUIThread(
                        DispatcherPriority.Input, 
                        () => { canSend = CheckCanSendServerCommands(doSendEventsBeforeCheck); });
                }
                else
                {
                    canSend = CheckCanSendServerCommands(doSendEventsBeforeCheck);
                }
            }
            else
            {
                if (SBPlus.Current != null)
                {
#if SHOW_DEBUG
                    CustomLogger.LogDebug(
                        () => string.Format("IsServerWaiting {0}", SBPlusRuntime.Current.CommandProcessor.IsServerWaiting));
#endif
                }

                if (SBPlusRuntime.Current != null && SBPlusRuntime.Current.CommandProcessor.IsServerWaiting)
                {
                    canSend = true;
                }
            }

#if SHOW_DEBUG
            CustomLogger.LogDebug(
                () => string.Format("commandCouldCauseUiAction:{0}, canSend result: {1} ", commandCouldCauseUiAction, canSend));
#endif
            return canSend;
        }

        /// <summary>
        /// The has server pending jobs to process only with name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool HasServerPendingJobsToProcessOnlyWithName(string name)
        {
            int countWithName = SbProcessRunner.Instance.Count(name);
            int totalCount = SbProcessRunner.Instance.Count();
            return countWithName == totalCount;
        }

        /// <summary>
        /// The invoke can send command changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        public static void InvokeCanSendCommandChanged(CanSendCommandChangedEventArgs e)
        {
            CanSendCommandChangedEventHandler handler = CanSendCommandChanged;
            if (handler != null)
            {
                handler(null, e);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The check can send server commands.
        /// </summary>
        /// <param name="doSendEventsBeforeCheck">
        /// The do send events before check.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool CheckCanSendServerCommands(bool doSendEventsBeforeCheck)
        {
            // let the client process the responses before we check the state of the server
            if (doSendEventsBeforeCheck)
            {
                Extensions.DoEvents();
            }

            bool canSend = false;
            try
            {
                // Check I am connected and that there is a system selected.
                if (SBPlusClient.Current.IsConnected && SBPlusClient.Current.IsSystemSelected)
                {
#if SHOW_DEBUG
                    CustomLogger.LogDebug(() => "SBPlus is connected and the system is selected");
#endif
                    // now check if the server is ready
                    if (SBPlusRuntime.Current.CommandProcessor.IsServerWaiting)
                    {
#if SHOW_DEBUG
                        CustomLogger.LogDebug(() => "Server is waiting :-) for commands");

                        CustomLogger.LogDebug(() => string.Format("SBPlus.Current.InputState is {0}", SBPlus.Current.InputState));
#endif

                        // make sure the UI is waiting for input.
                        if (SBPlus.Current.InputState == SBInputState.WaitingForInput)
                        {
#if SHOW_DEBUG
                            CustomLogger.LogDebug(() => "State is on WaitingForInput");
#endif

                            // now I need to check if the command waiting is an SBGuiCommand.
                            SBPlusServerMessage sbPlusServerMessage = SBPlusRuntime.Current.CommandProcessor.GetLastMessage(false);
                            if (sbPlusServerMessage != null && sbPlusServerMessage.Command != null
                                && sbPlusServerMessage.Command is GuiInputCommand)
                            {
#if SHOW_DEBUG
                                CustomLogger.LogDebug(() => "Client can send commands, because the server is ready");
#endif
                                canSend = true;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                CustomLogger.LogException(exception);
                throw;
            }

#if SHOW_DEBUG
            CustomLogger.LogDebug(() => string.Format("CheckCanSendServerCommands result is {0}", canSend));
#endif
            return canSend;
        }

        /// <summary>
        /// The get current account name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetCurrentAccountName()
        {
            if (SBPlus.Current == null)
            {
                return string.Empty;
            }

            if (!SBPlus.Current.CheckAccess())
            {
                var operation = SBPlus.Current.Dispatcher.Invoke(new Func<string>(GetCurrentAccountName));
                if (operation == null)
                {
                    return string.Empty;
                }

                return operation.ToString();
            }

            return SBPlus.Current.CurrentAccountName;
        }

        /// <summary>
        /// The handle input state changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void HandleInputStateChanged(object sender, InputStateChangedEventArgs e)
        {
            bool checkCanSend = CanSendServerCommands();
            if (checkCanSend == lastCanSendValue)
            {
                return;
            }

            lastCanSendValue = checkCanSend;
            InvokeCanSendCommandChanged(new CanSendCommandChangedEventArgs(checkCanSend));
        }

        /// <summary>
        /// The read control record completed.
        /// </summary>
        /// <param name="subroutineName">
        /// The subroutine name.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="userState">
        /// The user state.
        /// </param>
        private static void ReadControlRecordCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {
                var status = parameters[5];
                if (status.Count != 1 || !status.Value.Equals("0"))
                {
                    return;
                }

                var controlRec = parameters[3];
                switch (controlRec.Extract(9).Value)
                {
                    case "UV":
                        Platform = Platform.UniVerse;
                        break;
                    case "UDATA":
                        Platform = Platform.UniData;
                        break;
                }
            }
            catch (Exception exception)
            {
                CustomLogger.LogException(exception, "There was a problem reading the control record.");
            }
        }

        /// <summary>
        /// The sb plus client on connected.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        private static void SBPlusClientOnConnected(object sender, EventArgs eventArgs)
        {
            SBPlus.Current.InputStateChanged += HandleInputStateChanged;
            JobManager.RunInUIThread(
                DispatcherPriority.Input, 
                () => SBFile.Read("DMCONT", "SB.CONTROL", ReadControlRecordCompleted, new object()));
        }

        /// <summary>
        /// The sb plus disconnected.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void SBPlusDisconnected(object sender, EventArgs args)
        {
            SBPlus.Current.InputStateChanged -= HandleInputStateChanged;
            SBPlusClient.Connected += SBPlusClientOnConnected;
        }

        #endregion
    }
}