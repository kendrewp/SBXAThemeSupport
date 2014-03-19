// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SBProcessRunner.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.Shared.Definitions;
    using SBXA.UI.Client;
    using SBXA.UI.WPFControls;

    /// <summary>
    ///     The can send command changed event handler.
    /// </summary>
    /// <param name="sender">
    ///     The sender.
    /// </param>
    /// <param name="e">
    ///     The e.
    /// </param>
    public delegate void CanSendCommandChangedEventHandler(object sender, EventArgs e);

    /// <summary>
    ///     The sb process runner.
    /// </summary>
    public class SBProcessRunner
    {
        #region Static Fields

        private static readonly SBProcessRunner ProcessRunner = new SBProcessRunner();

        #endregion

        #region Fields

        private readonly ConcurrentQueue<IActionDefinition> processes = new ConcurrentQueue<IActionDefinition>();

        private bool isRunningProcesses;

        private bool lastCanSendValue;

        #endregion

        #region Constructors and Destructors

        private SBProcessRunner()
        {
            SBPlusRuntime.Current.DisConnected += this.SBPlusDisconnected;
            this.SBPlusClientOnConnected(null, null);
        }

        #endregion

        #region Delegates

        /// <summary>
        ///     The sb plus process.
        /// </summary>
        /// <param name="parameter">
        ///     The parameter.
        /// </param>
        /// <param name="currentFormHandle">
        ///     The current form handle.
        /// </param>
        /// <param name="target">
        ///     The target.
        /// </param>
        /// <param name="isInContext">
        ///     The is in context.
        /// </param>
        /// <param name="serverProcessFailed">
        ///     The server process failed.
        /// </param>
        public delegate void SBPlusProcess(
            object parameter, 
            string currentFormHandle, 
            IInputElement target, 
            bool isInContext, 
            ServerProcessFailed serverProcessFailed = null);

        #endregion

        #region Public Events

        /// <summary>
        ///     Register a listener for the ExecuteSBProcess Event.
        /// </summary>
        public event CanSendCommandChangedEventHandler CanSendCommandChanged;

        #endregion

        #region Interfaces

        /// <summary>
        ///     The ActionDefinition interface.
        /// </summary>
        public interface IActionDefinition
        {
            #region Public Properties

            /// <summary>
            ///     Gets the can send command to server.
            /// </summary>
            Func<bool> CanSendCommandToServer { get; }

            /// <summary>
            ///     Gets the name.
            /// </summary>
            string Name { get; }

            #endregion
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the current form sb handle.
        /// </summary>
        public static string CurrentFormSBHandle
        {
            get
            {
                if (SBPlus.Current != null && SBPlus.Current.CurrentForm != null)
                {
                    return SBFocusManager.FormWithFocus.SBObjectHandle;
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
                return Instance.HasJobsToProcess || !CanSendServerCommands(false);
            }
        }

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        public static SBProcessRunner Instance
        {
            get
            {
                return ProcessRunner;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether there are jobs in the queue waiting to be processed.
        /// </summary>
        public bool HasJobsToProcess
        {
            get
            {
                return this.processes.Count > 0;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The call process.
        /// </summary>
        /// <param name="processName">
        /// The process name.
        /// </param>
        /// <param name="isInContext">
        /// The is in context.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public static void CallProcess(string processName, bool isInContext, SBString parameter, string name = null)
        {
            string param = parameter != null ? parameter.GetRawString() : string.Empty;

            string procIncludingParam = processName;
            if (!string.IsNullOrEmpty(param))
            {
                procIncludingParam = string.Format("{0},{1}", processName, param);
            }

            SBPlusClient.LogInformation(
                string.Format("CheckIsServerReady: {0}, process call: {1}", CanSendServerCommands(false), procIncludingParam));

            ExecuteSbPlusProcess(procIncludingParam, isInContext);

            // Instance.ExecuteSbPlusProcess(CallProcessInternal, isInContext, procIncludingParam, SBPlus.Current, null, name);
        }

        /// <summary>
        /// The call process.
        /// </summary>
        /// <param name="processName">
        /// The process name.
        /// </param>
        /// <param name="isInContext">
        /// The is in context.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="serverProcessFailed">
        /// The server process failed.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public static void CallProcess(
            string processName, 
            bool isInContext, 
            SBString parameter, 
            ServerProcessFailed serverProcessFailed, 
            string name = null)
        {
            string param = parameter.GetRawString();

            SBPlusClient.LogInformation(string.Format("Check Application.IsServerReady: {0}", CanSendServerCommands(false)));

            string procIncludingParam = processName;
            if (!string.IsNullOrEmpty(param))
            {
                procIncludingParam = string.Format("{0},{1}", processName, param);
            }

            ExecuteSbPlusProcess(procIncludingParam, isInContext);

            // Instance.ExecuteSbPlusProcess(CallProcessInternal, isInContext, procIncludingParam, SBPlus.Current, serverProcessFailed, name);
        }

        /// <summary>
        /// This method will execute a subroutine on the server synchronously. If the server is busy, then based on the ignore
        ///     if busy flag will either ignore the call or throw an exception.
        /// </summary>
        /// <param name="subroutineName">
        /// The name of the subroutine
        /// </param>
        /// <param name="parCount">
        /// The numbr of parameters
        /// </param>
        /// <param name="parameter">
        /// The actual parameters being passed to the subroutine
        /// </param>
        /// <param name="ignoreIfBusy">
        /// If this is true, then the subroutine call will be ignored if the server is busy. If not then
        ///     an exception will be thrown if the server is busy.
        /// </param>
        /// <param name="onlyServerSide">
        /// If this is true it means that the basic subroutine will not do anything to cause the
        ///     server to make a call to the client and therefore we do not have to worry about checking if the UI is busy.
        /// </param>
        /// <returns>
        /// The values that were passed back from the subroutine.
        /// </returns>
        public static SBString[] CallSubroutine(
            string subroutineName, 
            int parCount, 
            SBString[] parameter, 
            bool ignoreIfBusy = true, 
            bool onlyServerSide = false)
        {
            if (subroutineName == null)
            {
                throw new ArgumentNullException("subroutineName");
            }

            if (parameter.Length < parCount)
            {
                Array.Resize(ref parameter, parCount);
            }

            if (!ignoreIfBusy && !CanSendServerCommands(false))
            {
                throw new Exception("The server is not able to accept requests at this time.");
            }

            if (!ignoreIfBusy && !CanSendServerCommands())
            {
                throw new Exception("The server is not able to accept requests at this time.");
            }

            SBString[] retunSBStrings = null;
            if (Application.Current.Dispatcher.CheckAccess())
            {
                retunSBStrings = onlyServerSide
                                     ? ExecuteSubroutineIfNoUi(subroutineName, parameter)
                                     : ExecuteSubroutine(subroutineName, parameter);
            }
            else
            {
                if (onlyServerSide)
                {
                    JobManager.RunSyncInUIThread(
                        DispatcherPriority.Normal, 
                        () => retunSBStrings = ExecuteSubroutineIfNoUi(subroutineName, parameter));
                }
                else
                {
                    JobManager.RunSyncInUIThread(
                        DispatcherPriority.Normal, 
                        () => retunSBStrings = ExecuteSubroutine(subroutineName, parameter));
                }
            }

            return retunSBStrings;
        }

        /// <summary>
        /// The can send server commands.
        /// </summary>
        /// <param name="commandCouldCauseUiAction">
        /// The command could cause ui action.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool CanSendServerCommands(bool commandCouldCauseUiAction = true)
        {
            var canSend = false;
            if (commandCouldCauseUiAction)
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    JobManager.RunSyncInUIThread(DispatcherPriority.Input, () => { canSend = CheckCanSendServerCommands(); });
                }
                else
                {
                    canSend = CheckCanSendServerCommands();
                }
            }
            else
            {
                if (SBPlusRuntime.Current.CommandProcessor.IsServerWaiting)
                {
                    canSend = true;
                }
            }

            return canSend;
        }

        /// <summary>
        /// Executes an SB/XA process when it gets a chance.
        /// </summary>
        /// <param name="processName">
        /// The name of the process.
        /// </param>
        /// <param name="inContext">
        /// True if the command should be executed in context.
        /// </param>
        public static void ExecuteSbPlusProcess(string processName, bool inContext = false)
        {
            SBProcessCallAction actionDefinition = inContext
                                                       ? new SBProcessCallAction(processName, true, ExecuteSBProcessInContext)
                                                       : new SBProcessCallAction(processName, true, ExecuteSBProcess);
            Instance.processes.Enqueue(actionDefinition);
            Instance.RunProcess();
        }

        /// <summary>
        /// Executes an SB/XA process with parameters, i.e. ProcessName,P1,P2. The process call will wait until the server is
        ///     available.
        /// </summary>
        /// <param name="processName">
        /// The name of the process
        /// </param>
        /// <param name="parameters">
        /// The array with parameters
        /// </param>
        public static void ExecuteSbPlusProcess(string processName, string[] parameters = null)
        {
            var processCall = processName;
            if (parameters != null)
            {
                processCall = parameters.Aggregate(processCall, (current, param) => current + ("," + param));
            }

            ExecuteSbPlusProcess(processCall, false);
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
            int countWithName = Instance.Count(j => j.Name == name);
            int totalCount = Instance.Count();
            return countWithName == totalCount;
        }

        /// <summary>
        /// This method will read a record from the server when it gets the opportunity. After the record has been read the
        ///     <see cref="SubroutineCallCompleted"/> is called.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file to read the record from.
        /// </param>
        /// <param name="itemName">
        /// The item name to read
        /// </param>
        /// <param name="userState">
        /// And user defined state that should be passed to the <see cref="SubroutineCallCompleted"/>.
        /// </param>
        /// <param name="subroutineCallCompleted">
        /// The <see cref="SubroutineCallCompleted"/> to call when the read has occurred.
        /// </param>
        public static void ReadRecord(string fileName, string itemName, object userState, SubroutineCallCompleted subroutineCallCompleted)
        {
            Instance.CallSubroutine(
                "UT.XUI.READ", 
                6, 
                new[] { new SBString(fileName), new SBString(itemName), new SBString(), new SBString(), new SBString("0"), new SBString() }, 
                userState, 
                subroutineCallCompleted, 
                true);
        }

        /// <summary>
        /// This method will wait for the server to be in a state to call the subroutine, then call it. It is an AsyncCall,
        ///     otherwise the UI thread could be hung.
        /// </summary>
        /// <param name="subroutineName">
        /// The name of the subroutine to execute.
        /// </param>
        /// <param name="parCount">
        /// The number of parameters in the subroutine.
        /// </param>
        /// <param name="parameters">
        /// The parameters to pass to the subroutine.
        /// </param>
        /// <param name="userState">
        /// Any state values that will be passed to the callbacks.
        /// </param>
        /// <param name="subroutineCallCompleted">
        /// The method that will be called when the subroutine completes.
        /// </param>
        /// <param name="onlyServerSide">
        /// If this subroutine has no interaction with the client, this can be set to True.
        /// </param>
        public void CallSubroutine(
            string subroutineName, 
            int parCount, 
            SBString[] parameters, 
            object userState, 
            SubroutineCallCompleted subroutineCallCompleted, 
            bool onlyServerSide = false)
        {
            if (subroutineName == null)
            {
                throw new ArgumentNullException("subroutineName");
            }

            if (parameters.Length < parCount)
            {
                Array.Resize(ref parameters, parCount);
            }

            var subroutineCallAction = new SubroutineCallAction(
                subroutineName, 
                parameters, 
                userState, 
                subroutineCallCompleted, 
                ExecuteSubroutine, 
                onlyServerSide);
            this.processes.Enqueue(subroutineCallAction);
            this.RunProcess();
        }

        /// <summary>
        ///     Returns the number of job currently in the queue.
        /// </summary>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public int Count()
        {
            return this.processes.Count;
        }

        /// <summary>
        /// The count.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int Count(string name)
        {
            return this.processes.ToArray().Cast<ActionDefinition>().Count(a => a.Name == name);
        }

        /// <summary>
        /// The count.
        /// </summary>
        /// <param name="predicate">
        /// The predicate.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int Count(Func<SBProcessDefinition, bool> predicate)
        {
            return // ReSharper disable SuspiciousTypeConversion.Global
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                this.processes.ToArray().Where(o => o is SBProcessDefinition).Cast<SBProcessDefinition>().Where(predicate).Count();
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            // ReSharper restore SuspiciousTypeConversion.Global
        }

        /// <summary>
        /// The execute method.
        /// </summary>
        /// <param name="myAction">
        /// The my action.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public void ExecuteMethod(Action myAction, string name)
        {
            this.ExecuteMethod(myAction, false, name);
        }

        /// <summary>
        /// The execute method.
        /// </summary>
        /// <param name="myAction">
        /// The my action.
        /// </param>
        /// <param name="canCauseUnexpectedResponsesToServer">
        /// The can cause unexpected responses to server.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public void ExecuteMethod(Action myAction, bool canCauseUnexpectedResponsesToServer, string name = null)
        {
            var actionDefinition = new ActionDefinition(canCauseUnexpectedResponsesToServer, myAction);
            this.processes.Enqueue(actionDefinition);
            this.RunProcess();
        }

        /// <summary>
        /// Executes a process via an Action.
        /// </summary>
        /// <param name="myAction">
        /// The <see cref="Action"/> that defines the method to be executed.
        /// </param>
        /// <example>
        /// <code lang="c#">
        /// 
        /// ...
        /// ...
        /// ...
        /// SbProcessRunner.Instance.ExecuteMethod(() =&gt; InitializeStartguiInternal(options));
        /// ...
        /// ...
        /// ...
        /// </code>
        /// </example>
        public void ExecuteMethod(Action myAction)
        {
            this.ExecuteMethod(myAction, false);
        }

        /// <summary>
        /// The execute sb plus process.
        /// </summary>
        /// <param name="sbPlusProcess">
        /// The sb plus process.
        /// </param>
        /// <param name="isInContext">
        /// The is in context.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="serverProcessFailed">
        /// The server process failed.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
/*
        internal void ExecuteSbPlusProcess(
            SBPlusProcess sbPlusProcess, 
            bool isInContext, 
            object parameter, 
            IInputElement target, 
            ServerProcessFailed serverProcessFailed, 
            string name = null)
        {
            this.RunProcess();
        }
*/

        /// <summary>
        /// The invoke can send command changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        public void InvokeCanSendCommandChanged(CanSendCommandChangedEventArgs e)
        {
            CanSendCommandChangedEventHandler handler = this.CanSendCommandChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region Methods

        private static void CallProcessInternal(
            object parameter, 
            string currentFormHandle, 
            IInputElement target, 
            bool isInContext, 
            ServerProcessFailed serverProcessFailed = null)
        {
            if (parameter == null)
            {
                SBPlusClient.LogInformation("parameter is null. This should never be the case");
                return;
            }

            string myLogParameter = parameter.ToString();

            // if the current form is no longer the current from so ignore the call
            if (isInContext && CurrentFormSBHandle != currentFormHandle)
            {
                SBPlusClient.LogWarning(
                    string.Format("Ignore the call before the form is no longer the current form. Parameter: {0}", myLogParameter));
                SBPlusClient.LogWarning("Ignore the call before the form is no longer the current form.");
                return;
            }

            // change the incontext);
            if (isInContext && SBPlus.Current != null && SBPlus.Current.CurrentForm != null)
            {
                var form = SBFocusManager.FormWithFocus as SBMultiForm;
                GuiObjectDefinition guiObjectDefinition = form != null
                                                              ? ((SBForm)form.CurrentForm).GuiObjectDefinition
                                                              : SBFocusManager.FormWithFocus.GuiObjectDefinition;

                var formObjectDefinition = guiObjectDefinition as FormObjectDefinition;
                isInContext = formObjectDefinition != null && formObjectDefinition.ProcessType == ProcessTypes.I;
            }

            SBPlusClient.LogInformation(string.Format("CallProcessInternal isInContext: {0} Parameter: {1}", isInContext, myLogParameter));

            SBPlusClient.LogInformation(string.Format("Der Process : {0} wird aufgerufen.", myLogParameter));

            if (isInContext)
            {
                SBPlusRuntime.Current.ExecuteInContextServerProcess(myLogParameter, serverProcessFailed ?? ServerProcessFailed);
            }
            else
            {
                SBPlusRuntime.Current.ExecuteServerProcess(myLogParameter, serverProcessFailed ?? ServerProcessFailed);
            }

            SBPlusClient.LogInformation(string.Format("Der Process : {0} wurde aufgerufen.", myLogParameter));
        }

        private static bool CheckCanSendServerCommands()
        {
            var canSend = false;
            // Check I am connected and that there is a system selected.
            if (SBPlusClient.Current.IsConnected && SBPlusClient.Current.IsSystemSelected)
            {
                // now check if the server is ready
                if (SBPlusRuntime.Current.CommandProcessor.IsServerWaiting)
                {
                    // make sure the UI is waiting for input.
                    if (SBPlus.Current.InputState == SBInputState.WaitingForInput)
                    {
                        // now I need to check if the command waiting is an SBGuiCommand.
                        var sbPlusServerMessage = SBPlusRuntime.Current.CommandProcessor.GetLastMessage(false);
                        /*
                        if (sbPlusServerMessage != null && sbPlusServerMessage.Command != null)
                        {
                            Debug.WriteLine("[SBProcessRunner.CheckCanSendServerCommands(412)] Command is " + sbPlusServerMessage.Command.GetType().Name);
                        }
                        else
                        {
                            Debug.WriteLine("[SBProcessRunner.CheckCanSendServerCommands(412)] Command is null.");
                        }
*/
                        if (sbPlusServerMessage != null && sbPlusServerMessage.Command != null
                            && sbPlusServerMessage.Command is GuiInputCommand)
                        {
                            canSend = true;
                        }
                    }
                }
            }

            return canSend;
        }

        private static void ExecuteSBProcess(string processName)
        {
            SBPlusRuntime.Current.ExecuteServerProcess(processName, ServerProcessFailed);
        }

        private static void ExecuteSBProcessInContext(string processName)
        {
            SBPlusRuntime.Current.ExecuteInContextServerProcess(processName, InContextServerProcessFailed);
        }

        private static void ExecuteSubroutine(
            string subroutineName, 
            SBString[] parameters, 
            object userState, 
            SubroutineCallCompleted subroutineCallCompleted)
        {
            SBPlusClient.Current.ExecuteSubroutine(subroutineName, parameters, userState, subroutineCallCompleted);
        }

        private static SBString[] ExecuteSubroutine(string subroutineName, SBString[] arguments)
        {
            SBString[] retunSBStrings = null;

            if (CanSendServerCommands() && SBPlusRuntime.Current.CommandProcessor.IsServerWaiting)
            {
                try
                {
                    retunSBStrings = SBPlusClient.Current.ExecuteSubroutineSynchronous(subroutineName, arguments);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("[SbProcessHandler.CallSubroutine(174)] Exception calling subroutine " + exception.Message);
                }
            }
            else
            {
                Debug.WriteLine("[SbProcessHandler.CallSubroutine(181)] skipped call because server was not ready.");
            }

            return retunSBStrings;
        }

        private static SBString[] ExecuteSubroutineIfNoUi(string subroutineName, SBString[] arguments)
        {
            SBString[] retunSBStrings = null;

            if (SBPlusRuntime.Current.CommandProcessor.IsServerWaiting)
            {
                try
                {
                    retunSBStrings = SBPlusClient.Current.ExecuteSubroutineSynchronous(subroutineName, arguments);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("[SbProcessHandler.CallSubroutine(174)] Exception calling subroutine " + exception.Message);
                }
            }
            else
            {
                Debug.WriteLine("[SbProcessHandler.CallSubroutine(181)] skipped call because server was not ready.");
            }

            return retunSBStrings;
        }

        private static void InContextServerProcessFailed(string processName, object stateObject, Exception exception)
        {
            Debug.WriteLine("[SBProcessRunner.ServerProcessFailed(37)] Failed to execute " + processName);
        }

        private static void ServerProcessFailed(string processName, object stateObject, Exception exception)
        {
            Debug.WriteLine("[SBProcessRunner.ServerProcessFailed(37)] Failed to execute " + processName);
        }

        private void DoRunProcess()
        {
            if (this.isRunningProcesses)
            {
                return;
            }

            this.isRunningProcesses = true;
            try
            {
                while (this.processes.Count > 0 && CanSendServerCommands(false))
                {
                    SBPlusClient.LogInformation(string.Format("Jobs to process {0}", this.processes.Count));

                    IActionDefinition targetAction;
                    if (!this.processes.TryPeek(out targetAction))
                    {
                        break; // if there is nothing in the queue break out.
                    }

                    // In theory, the value on CanSendCommandToServer can change between the check and the call.
                    if (!targetAction.CanSendCommandToServer())
                    {
                        break; // Nothing can be sent to the server so break out.
                    }

                    try
                    {
                        if (targetAction is ActionDefinition)
                        {
                            SBPlusClient.LogInformation("Executing action. Is on UI thread " + Application.Current.Dispatcher.CheckAccess());

                            ((ActionDefinition)targetAction).Action.Invoke();
                            this.processes.TryDequeue(out targetAction); // Only remove the process from the queue if it runs sucessfully.

                            SBPlusClient.LogInformation(
                                "Dequeued " + targetAction.Name + ". Number of processes left = " + this.processes.Count);
                        }
                        else if (targetAction is SubroutineCallAction)
                        {
                            SBPlusClient.LogInformation("Executing action. Is on UI thread " + Application.Current.Dispatcher.CheckAccess());

                            // If the action is only to run a process on the server, then inclose the check and the call in a single block so as to prevent any action on the UI from getting in between and preventing 
                            // the server call.
                            JobManager.RunSyncInUIThread(
                                DispatcherPriority.Normal, 
                                () =>
                                    {
                                        var subroutineCallAction = targetAction as SubroutineCallAction;
                                        if (subroutineCallAction == null)
                                        {
                                            return;
                                        }

                                        if (!subroutineCallAction.CanSendCommandToServer())
                                        {
                                            return;
                                        }

                                        try
                                        {
                                            SBPlusClient.LogInformation(
                                                "Executing action. Is on UI thread " + Application.Current.Dispatcher.CheckAccess());
                                            subroutineCallAction.Action.Invoke(
                                                subroutineCallAction.SubroutineName, 
                                                subroutineCallAction.Parameters, 
                                                subroutineCallAction.UserState, 
                                                subroutineCallAction.SubroutineCallCompleted);
                                            this.processes.TryDequeue(out targetAction);
                                            // Only remove the process from the queue if it runs sucessfully.
                                            SBPlusClient.LogInformation(
                                                "Executed action. Number of processes left = " + this.processes.Count);
                                        }
                                        catch (Exception exception)
                                        {
                                            SBPlusClient.LogError("Exception while executing SB+ process.", exception);
                                        }
                                    });
                            // Should have already been dequeued in the try/catch bloxk, _Processes.TryDequeue(out targetAction); // Only remove the process from the queue if it runs sucessfully.
                            SBPlusClient.LogInformation("Executed action. Number of processes left = " + this.processes.Count);
                        }
                        else
                        {
                            // If the action is only to run a process on the server, then inclose the check and the call in a single block so as to prevent any action on the UI from getting in between and preventing 
                            // the server call.
                            IActionDefinition action = targetAction;
                            JobManager.RunSyncInUIThread(
                                DispatcherPriority.Normal, 
                                () =>
                                    {
                                        if (!CanSendServerCommands())
                                        {
                                            return;
                                        }

                                        try
                                        {
                                            var sbProcessCallAction = action as SBProcessCallAction;
                                            if (sbProcessCallAction == null)
                                            {
                                                return;
                                            }

                                            SBPlusClient.LogInformation(
                                                "Executing action. Is on UI thread " + Application.Current.Dispatcher.CheckAccess());

                                            sbProcessCallAction.Action.Invoke(sbProcessCallAction.ProcessName);
                                            this.processes.TryDequeue(out action);
                                            // Only remove the process from the queue if it runs sucessfully.
                                            var disposable = action as IDisposable;
                                            if (disposable != null)
                                            {
                                                disposable.Dispose();
                                            }

                                            SBPlusClient.LogInformation(
                                                "Executed action. Number of processes left = " + this.processes.Count);
                                        }
                                        catch (Exception exception)
                                        {
                                            SBPlusClient.LogError("Exception while executing SB+ process.", exception);
                                        }
                                    });
                        }

                        SBPlusClient.LogInformation("Executed action");
                    }
                    catch (SBPlusApplicationException exception)
                    {
                        Debug.WriteLine("[SBProcessRunner.RunProcess(186)] Exception " + exception.Message);
                        if (!CanSendServerCommands(false)
                            || exception.Message.Contains("The server is currently busy and cannot accept requests."))
                        {
                            if (CanSendServerCommands(false))
                            {
                                // this can cause executing of the action in wrong order, but should never be executed
                                // do not use above peek because the Invoke of the action could cause the process runner to run again
                                SBPlusClient.LogError(
                                    string.Format(
                                        "Exception is caught. Server is waiting: {0}", 
                                        SBPlusRuntime.Current.CommandProcessor.IsServerWaiting), 
                                    exception);
                                Thread.Sleep(300);
                                Extensions.DoEvents();
                                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(this.RunProcess));
                                return;
                            }
                        }
                        else
                        {
                            SBPlusClient.LogError(
                                string.Format(
                                    "Exception is caught. Server is waiting: {0}", 
                                    SBPlusRuntime.Current.CommandProcessor.IsServerWaiting), 
                                exception);
                            throw;
                        }
                    }
                    catch (Exception exception)
                    {
                        SBPlusClient.LogError(
                            string.Format(
                                "Exception is caught. Server is waiting: {0}", 
                                SBPlusRuntime.Current.CommandProcessor.IsServerWaiting), 
                            exception);
                        throw;
                    }
                }
            }
            finally
            {
                this.isRunningProcesses = false;
            }
        }

        private void HandleCanSendCommandChanged(object sender, EventArgs e)
        {
            this.RunProcess();
        }

        private void HandleInputStateChanged(object sender, InputStateChangedEventArgs e)
        {
            bool checkCanSend = CanSendServerCommands();
            if (checkCanSend != this.lastCanSendValue)
            {
                this.lastCanSendValue = checkCanSend;
                this.InvokeCanSendCommandChanged(new CanSendCommandChangedEventArgs(checkCanSend));
            }
        }

        private void IsServerWaitingChanged(object sender, EventArgs args)
        {
            this.RunProcess();
        }

        private void RunProcess()
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                JobManager.RunAsyncOnPooledThread(state => this.DoRunProcess());
            }
            else
            {
                this.DoRunProcess();
            }
        }

        private void SBPlusClientOnConnected(object sender, EventArgs eventArgs)
        {
            SBPlusRuntime.Current.CommandProcessor.IsServerWaitingChanged += this.IsServerWaitingChanged;
            SBPlus.Current.InputStateChanged += this.HandleInputStateChanged;
            this.CanSendCommandChanged += this.HandleCanSendCommandChanged;
        }

        private void SBPlusDisconnected(object sender, EventArgs args)
        {
            SBPlusRuntime.Current.CommandProcessor.IsServerWaitingChanged -= this.IsServerWaitingChanged;
            SBPlus.Current.InputStateChanged -= this.HandleInputStateChanged;
            this.CanSendCommandChanged -= this.HandleCanSendCommandChanged;

            SBPlusClient.Connected += this.SBPlusClientOnConnected;
        }

        #endregion

        /// <summary>
        ///     The action definition.
        /// </summary>
        public class ActionDefinition : IActionDefinition
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ActionDefinition"/> class.
            /// </summary>
            /// <param name="canCauseUnexpectedResponses">if set to <c>true</c> [can cause unexpected responses].</param>
            /// <param name="actionToRun">The action to run.</param>
            /// <param name="name">The name.</param>
            /// <exception cref="System.ArgumentNullException">actionToRun</exception>
            public ActionDefinition(bool canCauseUnexpectedResponses, Action actionToRun, string name)
            {
                this.Name = name;
                if (actionToRun == null)
                {
                    throw new ArgumentNullException("actionToRun");
                }

                if (canCauseUnexpectedResponses)
                {
                    this.CanSendCommandToServer = () => CanSendServerCommands();
                }
                else
                {
                    this.CanSendCommandToServer = () => CanSendServerCommands(false);
                }

                this.Action = actionToRun;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ActionDefinition"/> class.
            /// </summary>
            /// <param name="canCauseUnexpectedResponsesToServer">
            /// The can cause unexpected responses to server.
            /// </param>
            /// <param name="actionToRun">
            /// The action to run.
            /// </param>
            public ActionDefinition(bool canCauseUnexpectedResponsesToServer, Action actionToRun)
                : this(canCauseUnexpectedResponsesToServer, actionToRun, string.Empty)
            {
            }

            #endregion

            #region Public Properties

            /// <summary>
            ///     Gets the action.
            /// </summary>
            public Action Action { get; private set; }

            /// <summary>
            ///     Gets the can send command to server.
            /// </summary>
            public Func<bool> CanSendCommandToServer { get; private set; }

            /// <summary>
            ///     Gets the name.
            /// </summary>
            public string Name { get; private set; }

            #endregion
        }

        /// <summary>
        ///     The sb process definition.
        /// </summary>
        public class SBProcessDefinition
        {
            #region Public Properties

            /// <summary>
            ///     Gets or sets the current form sb handle.
            /// </summary>
            public string CurrentFormSbHandle { get; set; }

            /// <summary>
            ///     Gets or sets a value indicating whether is in context.
            /// </summary>
            public bool IsInContext { get; set; }

            /// <summary>
            ///     Gets or sets the name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            ///     Gets or sets the parameter.
            /// </summary>
            public object Parameter { get; set; }

            /// <summary>
            ///     Gets or sets the sb process.
            /// </summary>
            public SBPlusProcess SbProcess { get; set; }

            /// <summary>
            ///     Gets or sets the server process failed callback.
            /// </summary>
            public ServerProcessFailed ServerProcessFailedCallback { get; set; }

            /// <summary>
            ///     Gets or sets the target.
            /// </summary>
            public IInputElement Target { get; set; }

            #endregion
        }

        private sealed class SBProcessCallAction : IActionDefinition, IDisposable
        {
            #region Fields

            private bool isDisposed;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="SBProcessCallAction"/> class.
            /// </summary>
            /// <param name="processName">
            /// The process name.
            /// </param>
            /// <param name="canCauseUnexpectedResponsesToServer">
            /// The can cause unexpected responses to server.
            /// </param>
            /// <param name="actionToRun">
            /// The action to run.
            /// </param>
            public SBProcessCallAction(string processName, bool canCauseUnexpectedResponsesToServer, Action<string> actionToRun)
                : this(processName, canCauseUnexpectedResponsesToServer, actionToRun, string.Empty)
            {
            }

            private SBProcessCallAction(string processName, bool canCauseUnexpectedResponses, Action<string> actionToRun, string name)
            {
                this.ProcessName = processName;

                this.Name = string.IsNullOrEmpty(name) ? this.ProcessName : name;
                if (actionToRun == null)
                {
                    throw new ArgumentNullException("actionToRun");
                }

                if (canCauseUnexpectedResponses)
                {
                    this.CanSendCommandToServer = () => CanSendServerCommands();
                }
                else
                {
                    this.CanSendCommandToServer = () => CanSendServerCommands(false);
                }

                this.Action = actionToRun;
            }

            #endregion

            #region Public Properties

            /// <summary>
            ///     Gets the action.
            /// </summary>
            public Action<string> Action { get; private set; }

            /// <summary>
            ///     Gets the can send command to server.
            /// </summary>
            public Func<bool> CanSendCommandToServer { get; private set; }

            /// <summary>
            ///     Gets the name.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            ///     Gets the process name.
            /// </summary>
            public string ProcessName { get; private set; }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     The dispose.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);
                // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Methods

            private void Dispose(bool disposing)
            {
                if (!this.isDisposed)
                {
                    if (disposing)
                    {
                        // Dispose managed resources.
                        this.CanSendCommandToServer = null;
                        this.Action = null;
                    }

                    // There are no unmanaged resources to release, but
                    // if we add them, they need to be released here.
                }

                this.isDisposed = true;

                // If it is available, make the call to the
                // base class's Dispose(Boolean) method
                // base.Dispose(disposing);
            }

            #endregion
        }

        private class SubroutineCallAction : IActionDefinition
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="SubroutineCallAction"/> class.
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
            /// <param name="subroutineCallCompleted">
            /// The subroutine call completed.
            /// </param>
            /// <param name="actionToRun">
            /// The action to run.
            /// </param>
            /// <param name="onlyServerSide">
            /// The only server side.
            /// </param>
            /// <exception cref="System.ArgumentNullException">actionToRun</exception>
            public SubroutineCallAction(
                string subroutineName, 
                SBString[] parameters, 
                object userState, 
                SubroutineCallCompleted subroutineCallCompleted, 
                Action<string, SBString[], object, SubroutineCallCompleted> actionToRun, 
                bool onlyServerSide = false)
            {
                this.SubroutineName = subroutineName;
                this.Name = string.Empty;
                this.Parameters = parameters;
                this.UserState = userState;
                this.SubroutineCallCompleted = subroutineCallCompleted;

                if (actionToRun == null)
                {
                    throw new ArgumentNullException("actionToRun");
                }

                if (!onlyServerSide)
                {
                    this.CanSendCommandToServer = () => CanSendServerCommands();
                }
                else
                {
                    this.CanSendCommandToServer = () => CanSendServerCommands(false);
                }

                this.Action = actionToRun;
            }

            #endregion

            #region Public Properties

            /// <summary>
            ///     Gets the action.
            /// </summary>
            public Action<string, SBString[], object, SubroutineCallCompleted> Action { get; private set; }

            /// <summary>
            ///     Gets the can send command to server.
            /// </summary>
            public Func<bool> CanSendCommandToServer { get; private set; }

            /// <summary>
            ///     Gets the name.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            ///     Gets the parameters.
            /// </summary>
            public SBString[] Parameters { get; private set; }

            /// <summary>
            ///     Gets the subroutine call completed.
            /// </summary>
            public SubroutineCallCompleted SubroutineCallCompleted { get; private set; }

            /// <summary>
            ///     Gets the subroutine name.
            /// </summary>
            public string SubroutineName { get; private set; }

            /// <summary>
            ///     Gets the user state.
            /// </summary>
            public object UserState { get; private set; }

            #endregion
        }
    }

    /// <summary>
    ///     Event arguments for the <see cref="SBProcessRunner.CanSendCommandChanged" /> event.
    /// </summary>
    public class CanSendCommandChangedEventArgs : EventArgs
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CanSendCommandChangedEventArgs"/> class.
        /// </summary>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public CanSendCommandChangedEventArgs(bool newValue)
        {
            this.NewValue = newValue;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether new value.
        /// </summary>
        public bool NewValue { get; set; }

        #endregion
    }
}