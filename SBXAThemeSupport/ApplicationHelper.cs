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
    ///     The application helper.
    /// </summary>
    public static class ApplicationHelper
    {
        #region Static Fields

        private static bool lastCanSendValue; // To hold the last value of CanSend, so I can only raise the event when it changes.

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

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The can send server commands.
        /// </summary>
        /// <param name="commandCouldCauseUiAction">
        ///     The command could cause ui action.
        /// </param>
        /// <param name="doSendEventsBeforeCheck">
        ///     The do send events before check.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool CanSendServerCommands(bool commandCouldCauseUiAction = true, bool doSendEventsBeforeCheck = false)
        {
            CustomLogger.LogDebug(() => string.Format("commandCouldCauseUiAction {0}", commandCouldCauseUiAction));

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
                    CustomLogger.LogDebug(
                        () => string.Format("IsServerWaiting {0}", SBPlusRuntime.Current.CommandProcessor.IsServerWaiting));
                }

                if (SBPlusRuntime.Current != null && SBPlusRuntime.Current.CommandProcessor.IsServerWaiting)
                {
                    canSend = true;
                }
            }

            CustomLogger.LogDebug(
                () => string.Format("commandCouldCauseUiAction:{0}, canSend result: {1} ", commandCouldCauseUiAction, canSend));

            return canSend;
        }

        /// <summary>
        ///     The has server pending jobs to process only with name.
        /// </summary>
        /// <param name="name">
        ///     The name.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool HasServerPendingJobsToProcessOnlyWithName(string name)
        {
            int countWithName = SbProcessRunner.Instance.Count(name);
            int totalCount = SbProcessRunner.Instance.Count();
            return countWithName == totalCount;
        }

        /// <summary>
        ///     The invoke can send command changed.
        /// </summary>
        /// <param name="e">
        ///     The e.
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
                    CustomLogger.LogDebug(() => "SBPlus is connected and the system is selected");

                    // now check if the server is ready
                    if (SBPlusRuntime.Current.CommandProcessor.IsServerWaiting)
                    {
                        CustomLogger.LogDebug(() => "Server is waiting :-) for commands");

                        CustomLogger.LogDebug(() => string.Format("SBPlus.Current.InputState is {0}", SBPlus.Current.InputState));

                        // make sure the UI is waiting for input.
                        if (SBPlus.Current.InputState == SBInputState.WaitingForInput)
                        {
                            CustomLogger.LogDebug(() => "State is on WaitingForInput");

                            // now I need to check if the command waiting is an SBGuiCommand.
                            SBPlusServerMessage sbPlusServerMessage = SBPlusRuntime.Current.CommandProcessor.GetLastMessage(false);
                            if (sbPlusServerMessage != null && sbPlusServerMessage.Command != null
                                && sbPlusServerMessage.Command is GuiInputCommand)
                            {
                                CustomLogger.LogDebug(() => "Client can send commands, because the server is ready");
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

            CustomLogger.LogDebug(() => string.Format("CheckCanSendServerCommands result is {0}", canSend));
            return canSend;
        }

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

        private static void SBPlusClientOnConnected(object sender, EventArgs eventArgs)
        {
            SBPlus.Current.InputStateChanged += HandleInputStateChanged;
        }

        private static void SBPlusDisconnected(object sender, EventArgs args)
        {
            SBPlus.Current.InputStateChanged -= HandleInputStateChanged;
            SBPlusClient.Connected += SBPlusClientOnConnected;
        }

        #endregion
    }
}