using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Linq;
using SBXA.Runtime;
using SBXA.Shared;
using SBXA.Shared.Definitions;
using SBXA.UI.Client;
using SBXA.UI.WPFControls;

namespace SBXAThemeSupport
{
    public class SBProcessRunner
    {
        #region Delegates

        public delegate void SBPlusProcess(object parameter, string currentFormHandle, IInputElement target, bool isInContext, ServerProcessFailed serverProcessFailed = null);

        #endregion

        private static readonly SBProcessRunner _ProcessRunner = new SBProcessRunner();

        private bool _IsRunningProcesses;
        private readonly ConcurrentQueue<IActionDefinition> _Processes = new ConcurrentQueue<IActionDefinition>();

        private bool _LastCanSendValue;

        private SBProcessRunner()
        {
            SBPlusRuntime.Current.DisConnected += SBPlusDisconnected;
            SBPlusClientOnConnected(null, null);
        }

        #region CanSendCommandChanged

        void HandleInputStateChanged(object sender, InputStateChangedEventArgs e)
        {
            bool checkCanSend = CanSendServerCommands();
            if (checkCanSend != _LastCanSendValue)
            {
                _LastCanSendValue = checkCanSend;
                InvokeCanSendCommandChanged(new CanSendCommandChangedEventArgs(checkCanSend));
            }
        }

        /// <summary>
        /// Register a listener for the ExecuteSBProcess Event.
        /// </summary>
        public event CanSendCommandChangedEventHandler CanSendCommandChanged;

        public void InvokeCanSendCommandChanged(CanSendCommandChangedEventArgs e)
        {
            CanSendCommandChangedEventHandler handler = CanSendCommandChanged;
            if (handler != null) handler(this, e);
        }

        #endregion CanSendCommandChanged

        private void SBPlusDisconnected(object sender, EventArgs args)
        {
            SBPlusRuntime.Current.CommandProcessor.IsServerWaitingChanged -= IsServerWaitingChanged;
            SBPlus.Current.InputStateChanged -= HandleInputStateChanged;
            CanSendCommandChanged -= HandleCanSendCommandChanged;

            SBPlusClient.Connected += SBPlusClientOnConnected;
        }

        private void SBPlusClientOnConnected(object sender, EventArgs eventArgs)
        {
            SBPlusRuntime.Current.CommandProcessor.IsServerWaitingChanged += IsServerWaitingChanged;
            SBPlus.Current.InputStateChanged += HandleInputStateChanged;
            CanSendCommandChanged += HandleCanSendCommandChanged;
        }

        private void IsServerWaitingChanged(object sender, EventArgs args)
        {
            RunProcess();
        }

        void HandleCanSendCommandChanged(object sender, EventArgs e)
        {
            RunProcess();
        }

        public static SBProcessRunner Instance
        {
            get { return _ProcessRunner; }
        }
        /// <summary>
        /// Returns true if the are jobs in the queue waiting to be processed.
        /// </summary>
        public bool HasJobsToProcess
        {
            get { return _Processes.Count > 0; }
        }
        /// <summary>
        /// Returns the number of job currently in the queue.
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return _Processes.Count;
        }

        public int Count(string name)
        {
            return _Processes.ToArray().Cast<ActionDefinition>().Count(a => a.Name == name);
        }

        public int Count(Func<SBProcessDefinition, bool> predicate)
        {
            return
                // ReSharper disable SuspiciousTypeConversion.Global
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                _Processes.ToArray().Where(o => o is SBProcessDefinition).Cast<SBProcessDefinition>().Where(predicate).Count();
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            // ReSharper restore SuspiciousTypeConversion.Global
        }

        private void RunProcess()
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                JobManager.RunAsyncOnPooledThread(state => DoRunProcess());
            }
            else
            {
                DoRunProcess();
            }
        }

        private void DoRunProcess()
        {
            if (_IsRunningProcesses) return;
            _IsRunningProcesses = true;
            try
            {

                while (_Processes.Count > 0 && CanSendServerCommands(false))
                {
                    SBPlusClient.LogInformation(string.Format("Jobs to process {0}", _Processes.Count));

                    IActionDefinition targetAction;
                    if (!_Processes.TryPeek(out targetAction)) break; // if there is nothing in the queue break out.

                    // In theory, the value on CanSendCommandToServer can change between the check and the call.
                    if (!targetAction.CanSendCommandToServer()) break; // Nothing can be sent to the server so break out.

                    try
                    {
                        if (targetAction is ActionDefinition)
                        {
                            SBPlusClient.LogInformation("Executing action. Is on UI thread " + Application.Current.Dispatcher.CheckAccess());

                            ((ActionDefinition)targetAction).Action.Invoke();
                            _Processes.TryDequeue(out targetAction); // Only remove the process from the queue if it runs sucessfully.

                            SBPlusClient.LogInformation("Dequeued "+targetAction.Name+". Number of processes left = " + _Processes.Count);
                        }
                        else if (targetAction is SubroutineCallAction)
                        {

                            SBPlusClient.LogInformation("Executing action. Is on UI thread " + Application.Current.Dispatcher.CheckAccess());

                            // If the action is only to run a process on the server, then inclose the check and the call in a single block so as to prevent any action on the UI from getting in between and preventing 
                            // the server call.
                            JobManager.RunSyncInUIThread(DispatcherPriority.Normal,
                                                         () =>
                                                         {
                                                             SubroutineCallAction subroutineCallAction = targetAction as SubroutineCallAction;
                                                             if (subroutineCallAction == null) return;
                                                             if (!subroutineCallAction.CanSendCommandToServer()) return;
                                                             try
                                                             {
                                                                 SBPlusClient.LogInformation("Executing action. Is on UI thread " + Application.Current.Dispatcher.CheckAccess());
                                                                 subroutineCallAction.Action.Invoke(subroutineCallAction.SubroutineName, subroutineCallAction.Parameters, subroutineCallAction.UserState, subroutineCallAction.SubroutineCallCompleted);
                                                                 _Processes.TryDequeue(out targetAction); // Only remove the process from the queue if it runs sucessfully.

                                                                 SBPlusClient.LogInformation("Executed action. Number of processes left = " + _Processes.Count);
                                                             }
                                                             catch (Exception exception)
                                                             {
                                                                 SBPlusClient.LogError("Exception while executing SB+ process.", exception);
                                                             }
                                                         });
                            // Should have already been dequeued in the try/catch bloxk, _Processes.TryDequeue(out targetAction); // Only remove the process from the queue if it runs sucessfully.

                            SBPlusClient.LogInformation("Executed action. Number of processes left = " + _Processes.Count);
                        }
                        else
                        {
                            // If the action is only to run a process on the server, then inclose the check and the call in a single block so as to prevent any action on the UI from getting in between and preventing 
                            // the server call.
                            IActionDefinition action = targetAction;
                            JobManager.RunSyncInUIThread(DispatcherPriority.Normal,
                                                         () =>
                                                         {
                                                             if (!CanSendServerCommands(true)) return;
                                                             try
                                                             {
                                                                 SBProcessCallAction sbProcessCallAction = action as SBProcessCallAction;
                                                                 if (sbProcessCallAction == null) return;

                                                                 SBPlusClient.LogInformation("Executing action. Is on UI thread " + Application.Current.Dispatcher.CheckAccess());

                                                                 sbProcessCallAction.Action.Invoke(sbProcessCallAction.ProcessName);
                                                                 _Processes.TryDequeue(out action); // Only remove the process from the queue if it runs sucessfully.
                                                                 if (action is IDisposable) ((IDisposable)action).Dispose();

                                                                 SBPlusClient.LogInformation("Executed action. Number of processes left = " + _Processes.Count);
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
                        if (!CanSendServerCommands(false) ||
                            exception.Message.Contains("The server is currently busy and cannot accept requests."))
                        {
                            if (CanSendServerCommands(false))
                            {
                                // this can cause executing of the action in wrong order, but should never be executed
                                // do not use above peek because the Invoke of the action could cause the process runner to run again
                                SBPlusClient.LogError(
                                    string.Format("Exception is caught. Server is waiting: {0}",
                                                  SBPlusRuntime.Current.CommandProcessor.IsServerWaiting), exception);
                                Thread.Sleep(300);
                                Extensions.DoEvents();
                                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(RunProcess));
                                return;
                            }
                        }
                        else
                        {
                            SBPlusClient.LogError(
                                string.Format("Exception is caught. Server is waiting: {0}",
                                              SBPlusRuntime.Current.CommandProcessor.IsServerWaiting), exception);
                            throw;
                        }
                    }
                    catch (Exception exception)
                    {
                        SBPlusClient.LogError(
                            string.Format("Exception is caught. Server is waiting: {0}",
                                          SBPlusRuntime.Current.CommandProcessor.IsServerWaiting), exception);
                        throw;
                    }


                }
            }
            finally
            {

                _IsRunningProcesses = false;
            }
        }

        public void ExecuteMethod(Action myAction, string name)
        {
            ExecuteMethod(myAction, false, name);
        }

        public void ExecuteMethod(Action myAction, bool canCauseUnexpectedResponsesToServer, string name = null)
        {
            var actionDefinition = new ActionDefinition(canCauseUnexpectedResponsesToServer, myAction);
            _Processes.Enqueue(actionDefinition);
            RunProcess();

        }

        /// <summary>
        /// Executes a process via an Action.
        /// </summary>
        /// <param name="myAction"></param>
        /// <example>
        /// <code lang="c#"> 
        /// ...
        /// ...
        /// ...
        /// SbProcessRunner.Instance.ExecuteMethod(() => InitializeStartguiInternal(options));
        /// ...
        /// ...
        /// ...
        /// </code>
        /// </example>
        public void ExecuteMethod(Action myAction)
        {
            ExecuteMethod(myAction, false);
        }
        /// <summary>
        /// This method will read a record from the server when it gets the opportunity. After the record has been read the <see cref="SubroutineCallCompleted"/> is called.
        /// </summary>
        /// <param name="fileName">The name of the file to read the record from.</param>
        /// <param name="itemName">The item name to read</param>
        /// <param name="userState">And user defined state that should be passed to the <see cref="SubroutineCallCompleted"/>.</param>
        /// <param name="subroutineCallCompleted">The <see cref="SubroutineCallCompleted"/> to call when the read has occurred.</param>
        public static void ReadRecord(string fileName, string itemName, object userState, SubroutineCallCompleted subroutineCallCompleted)
        {
            Instance.CallSubroutine("UT.XUI.READ", 6, new[] { new SBString(fileName),
                                                                     new SBString(itemName),
                                                                     new SBString(),
                                                                     new SBString(),
                                                                     new SBString("0"),
                                                                     new SBString()
                                                                   },
                                                                   userState, subroutineCallCompleted, true);
        }

        public void ExecuteSBPlusProcess(SBPlusProcess sbPlusProcess, bool isInContext, object parameter, IInputElement target, ServerProcessFailed serverProcessFailed, string name = null)
        {
            RunProcess();
        }

        /// <summary>
        /// Executes an SB/XA process when it gets a chance.
        /// </summary>
        /// <param name="processName">The name of the process.</param>
        /// <param name="inContext">True if the command should be executed in context.</param>
        public static void ExecuteSBPlusProcess(string processName, bool inContext = false)
        {
            SBProcessCallAction actionDefinition = inContext ? new SBProcessCallAction(processName, true, ExecuteSBProcessInContext) : new SBProcessCallAction(processName, true, ExecuteSBProcess);
            Instance._Processes.Enqueue(actionDefinition);
            Instance.RunProcess();
        }
        /// <summary>
        /// Executes an SB/XA process with parameters, i.e. ProcessName,P1,P2. The process call will wait until the server is available.
        /// </summary>
        /// <param name="processName">The name of the process</param>
        /// <param name="parameters">The array with parameters</param>
        public static void ExecuteSBPlusProcess(string processName, string[] parameters = null)
        {
            string processCall = processName;
            if (parameters != null)
            {
                processCall = parameters.Aggregate(processCall, (current, param) => current + ("," + param));
            }
            ExecuteSBPlusProcess(processCall, false);
        }

        private static void ExecuteSBProcess(string processName)
        {
            SBPlusRuntime.Current.ExecuteServerProcess(processName, ServerProcessFailed);
        }

        private static void ExecuteSBProcessInContext(string processName)
        {
            SBPlusRuntime.Current.ExecuteInContextServerProcess(processName, InContextServerProcessFailed);
        }

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
            return (canSend);
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
                        if (sbPlusServerMessage != null && sbPlusServerMessage.Command != null && sbPlusServerMessage.Command is GuiInputCommand)
                        {
                            canSend = true;
                        }
                    }
                }
            }
            return canSend;
        }

        public static bool HasServerPendingJobsToProcess
        {
            get { return Instance.HasJobsToProcess || !CanSendServerCommands(false); }
        }

        public static bool HasServerPendingJobsToProcessOnlyWithName(string name)
        {
            int countWithName = Instance.Count(j => j.Name == name);
            int totalCount = Instance.Count();
            return countWithName == totalCount;
        }

        public static string CurrentFormSBHandle
        {
            get
            {
                if (SBPlus.Current != null && SBPlus.Current.CurrentForm != null)
                {
                    return SBFocusManager.FormWithFocus.SBObjectHandle;
                }
                return String.Empty;
            }
        }

        public static void CallProcess(string processName, bool isInContext, SBString parameter, string name = null)
        {
            string param = parameter != null ? parameter.GetRawString() : string.Empty;


            string procIncludingParam = processName;
            if (!string.IsNullOrEmpty(param)) procIncludingParam = string.Format("{0},{1}", processName, param);

            SBPlusClient.LogInformation(string.Format("CheckIsServerReady: {0}, process call: {1}", CanSendServerCommands(false), procIncludingParam));

            Instance.ExecuteSBPlusProcess(CallProcessInternal, isInContext, procIncludingParam, SBPlus.Current, null, name);
        }

        public static void CallProcess(string processName, bool isInContext, SBString parameter, ServerProcessFailed serverProcessFailed, string name = null)
        {
            string param = parameter.GetRawString();

            SBPlusClient.LogInformation(string.Format("Check Application.IsServerReady: {0}", CanSendServerCommands(false)));

            string procIncludingParam = processName;
            if (!string.IsNullOrEmpty(param)) procIncludingParam = string.Format("{0},{1}", processName, param);

            Instance.ExecuteSBPlusProcess(CallProcessInternal, isInContext,
                                                          procIncludingParam,
                                                          SBPlus.Current,
                                                          serverProcessFailed,
                                                          name);
        }

        private static void CallProcessInternal(object parameter, string currentFormHandle, IInputElement target, bool isInContext, ServerProcessFailed serverProcessFailed = null)
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
                SBPlusClient.LogWarning(string.Format("Ignore the call before the form is no longer the current form. Parameter: {0}", myLogParameter));
                SBPlusClient.LogWarning("Ignore the call before the form is no longer the current form.");
                return;
            }

            // change the incontext);
            if (isInContext && SBPlus.Current != null && SBPlus.Current.CurrentForm != null)
            {
                var form = SBFocusManager.FormWithFocus as SBMultiForm;
                GuiObjectDefinition guiObjectDefinition = form != null ? ((SBForm)form.CurrentForm).GuiObjectDefinition : SBFocusManager.FormWithFocus.GuiObjectDefinition;

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

        private static void ServerProcessFailed(string processName, object stateObject, Exception exception)
        {
            Debug.WriteLine("[SBProcessRunner.ServerProcessFailed(37)] Failed to execute " + processName);
        }

        private static void InContextServerProcessFailed(string processName, object stateObject, Exception exception)
        {
            Debug.WriteLine("[SBProcessRunner.ServerProcessFailed(37)] Failed to execute " + processName);
        }

        /// <summary>
        /// This method will execute a subroutine on the server synchronously. If the server is busy, then based on the ignore if busy flag will either ignore the call or throw an exception.
        /// </summary>
        /// <param name="subroutineName">The name of the subroutine</param>
        /// <param name="parCount">The numbr of parameters</param>
        /// <param name="parameter">The actual parameters being passed to the subroutine</param>
        /// <param name="ignoreIfBusy">If this is true, then the subroutine call will be ignored if the server is busy. If not then an exception will be thrown if the server is busy.</param>
        /// <param name="onlyServerSide">If this is true it means that the basic subroutine will not do anything to cause the server to make a call to the client and therefore we do not have to worry about checking if the UI is busy.</param>
        /// <returns>The values that were passed back from the subroutine.</returns>
        public static SBString[] CallSubroutine(string subroutineName, int parCount, SBString[] parameter, bool ignoreIfBusy = true, bool onlyServerSide = false)
        {
            if (subroutineName == null) throw new ArgumentNullException("subroutineName");

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
                retunSBStrings = onlyServerSide ? ExecuteSubroutineIfNoUi(subroutineName, parameter) : ExecuteSubroutine(subroutineName, parameter);
            }
            else
            {
                if (onlyServerSide)
                {
                    JobManager.RunSyncInUIThread(DispatcherPriority.Normal, () => retunSBStrings = ExecuteSubroutineIfNoUi(subroutineName, parameter));
                }
                else
                {
                    JobManager.RunSyncInUIThread(DispatcherPriority.Normal, () => retunSBStrings = ExecuteSubroutine(subroutineName, parameter));
                }
            }
            return retunSBStrings;
        }

        /// <summary>
        /// This method will wait for the server to be in a state to call the subroutine, then call it. It is an AsyncCall, otherwise the UI thread could be hung.
        /// </summary>
        /// <param name="subroutineName"></param>
        /// <param name="parCount"></param>
        /// <param name="parameters"></param>
        /// <param name="userState"></param>
        /// <param name="subroutineCallCompleted"></param>
        /// <param name="onlyServerSide"></param>
        /// <returns></returns>
        public void CallSubroutine(string subroutineName, int parCount, SBString[] parameters, object userState, SubroutineCallCompleted subroutineCallCompleted, bool onlyServerSide = false)
        {
            if (subroutineName == null) throw new ArgumentNullException("subroutineName");

            if (parameters.Length < parCount)
            {
                Array.Resize(ref parameters, parCount);
            }

            SubroutineCallAction subroutineCallAction = new SubroutineCallAction(subroutineName, parameters, userState, subroutineCallCompleted, ExecuteSubroutine, onlyServerSide);
            _Processes.Enqueue(subroutineCallAction);
            RunProcess();

        }

        private static void ExecuteSubroutine(string subroutineName, SBString[] parameters, object userState, SubroutineCallCompleted subroutineCallCompleted)
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


        #region Nested type: SBProcessDefinition

        public class SBProcessDefinition
        {
            public SBPlusProcess SbProcess { get; set; }
            public object Parameter { get; set; }
            public IInputElement Target { get; set; }
            public bool IsInContext { get; set; }
            public string CurrentFormSbHandle { get; set; }
            public string Name { get; set; }
            public ServerProcessFailed ServerProcessFailedCallback { get; set; }
        }

        #endregion

        #region Nested class ActionDefinition

        public interface IActionDefinition
        {
            string Name { get; }
            Func<bool> CanSendCommandToServer { get; }
        }

        public class ActionDefinition : IActionDefinition
        {
            public ActionDefinition(bool canCauseUnexpectedResponses, Action actionToRun, string name)
            {
                Name = name;
                if (actionToRun == null) throw new ArgumentNullException("actionToRun");
                if (canCauseUnexpectedResponses)
                {
                    CanSendCommandToServer = () => CanSendServerCommands();
                }
                else
                {
                    CanSendCommandToServer = () => CanSendServerCommands(false);
                }
                Action = actionToRun;

            }

            public ActionDefinition(bool canCauseUnexpectedResponsesToServer, Action actionToRun)
                : this(canCauseUnexpectedResponsesToServer, actionToRun, string.Empty)
            {
            }

            public string Name { get; private set; }
            public Func<bool> CanSendCommandToServer { get; private set; }
            public Action Action { get; private set; }
        }

        private sealed class SBProcessCallAction : IActionDefinition, IDisposable
        {
            private bool _IsDisposed;

            private SBProcessCallAction(string processName, bool canCauseUnexpectedResponses, Action<string> actionToRun, string name)
            {
                ProcessName = processName;

                Name = string.IsNullOrEmpty(name) ? ProcessName : name;
                if (actionToRun == null) throw new ArgumentNullException("actionToRun");
                if (canCauseUnexpectedResponses)
                {
                    CanSendCommandToServer = () => CanSendServerCommands();
                }
                else
                {
                    CanSendCommandToServer = () => CanSendServerCommands(false);
                }
                Action = actionToRun;

            }

            public SBProcessCallAction(string processName, bool canCauseUnexpectedResponsesToServer, Action<string> actionToRun)
                : this(processName, canCauseUnexpectedResponsesToServer, actionToRun, string.Empty)
            {
            }

            public string ProcessName { get; private set; }
            public string Name { get; private set; }
            public Func<bool> CanSendCommandToServer { get; private set; }
            public Action<string> Action { get; private set; }

            public void Dispose()
            {
                Dispose(true);
// ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!_IsDisposed)
                {
                    if (disposing)
                    {
                        // Dispose managed resources.
                        CanSendCommandToServer = null;
                        Action = null;
                    }

                    // There are no unmanaged resources to release, but
                    // if we add them, they need to be released here.
                }
                _IsDisposed = true;

                // If it is available, make the call to the
                // base class's Dispose(Boolean) method
                // base.Dispose(disposing);
            }

        }

        private class SubroutineCallAction : IActionDefinition
        {
            public SubroutineCallAction(string subroutineName, SBString[] parameters, object userState, SubroutineCallCompleted subroutineCallCompleted, Action<string, SBString[], object, SubroutineCallCompleted> actionToRun, bool onlyServerSide = false)
            {
                SubroutineName = subroutineName;
                Name = string.Empty;
                Parameters = parameters;
                UserState = userState;
                SubroutineCallCompleted = subroutineCallCompleted;

                if (actionToRun == null) throw new ArgumentNullException("actionToRun");

                if (!onlyServerSide)
                {
                    CanSendCommandToServer = () => CanSendServerCommands();
                }
                else
                {
                    CanSendCommandToServer = () => CanSendServerCommands(false);
                }
                Action = actionToRun;

            }

            public string SubroutineName { get; private set; }
            public string Name { get; private set; }

            public SBString[] Parameters { get; private set; }
            public object UserState { get; private set; }
            public SubroutineCallCompleted SubroutineCallCompleted { get; private set; }

            public Func<bool> CanSendCommandToServer { get; private set; }
            public Action<string, SBString[], object, SubroutineCallCompleted> Action { get; private set; }
        }


        #endregion Nested class

    }

    public delegate void CanSendCommandChangedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Event arguments for the <see cref="SBProcessRunner.CanSendCommandChanged"/> event.
    /// </summary>
    public class CanSendCommandChangedEventArgs : EventArgs
    {
        public CanSendCommandChangedEventArgs(bool newValue)
        {
            NewValue = newValue;
        }

        public bool NewValue { get; set; }
    }

}