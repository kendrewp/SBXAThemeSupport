// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SbProcessRunner.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// #define SHOW_DEBUG
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
    using SBXA.UI.Client;

    /// <summary>
    ///     The can send command changed event handler.
    /// </summary>
    /// <param name="sender">
    ///     The sender.
    /// </param>
    /// <param name="e">
    ///     The e.
    /// </param>
    public delegate void CanSendCommandChangedEventHandler(object sender, CanSendCommandChangedEventArgs e);

    /// <summary>
    ///     The SB/XA process runner.
    /// </summary>
    public class SbProcessRunner
    {
        #region Static Fields

        private static readonly SbProcessRunner ProcessRunner = new SbProcessRunner();

        #endregion

        #region Fields

        private readonly BlockingCollection<ActionDefinition> failedProcesses = new BlockingCollection<ActionDefinition>();

        private readonly ConcurrentQueue<ActionDefinition> processes = new ConcurrentQueue<ActionDefinition>();

        private readonly object syncObject = new object();

        #endregion

        #region Constructors and Destructors

        private SbProcessRunner()
        {
            // We need to attach and un-attach the handlers in order to prevent a memory leak when the client disconnects from the server.
            SBPlusRuntime.Current.DisConnected += this.SBPlusDisconnected;
            this.SBPlusClientOnConnected(null, null);
        }

        #endregion

        #region Delegates

        /// <summary>
        ///     The sb plus process.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="currentFormHandle">The current form handle.</param>
        /// <param name="target">The target.</param>
        /// <param name="isInContext">The is in context.</param>
        public delegate void SbPlusProcess(object parameter, string currentFormHandle, IInputElement target, bool isInContext);

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        public static SbProcessRunner Instance
        {
            get
            {
                return ProcessRunner;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether has jobs to process.
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
        ///     The count.
        /// </summary>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public int Count()
        {
            return this.processes.Count;
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
        /// <param name="isRetryWhenServerNotAccept">
        /// The is retry when server not accept.
        /// </param>
        /// <param name="isRunOnUiThread">
        /// The is run on ui thread.
        /// </param>
        public void ExecuteMethod(
            Action myAction, 
            bool canCauseUnexpectedResponsesToServer = false, 
            string name = null, 
            bool isRetryWhenServerNotAccept = true, 
            bool isRunOnUiThread = false)
        {

#if SHOW_DEBUG
            CustomLogger.LogDebug(() => string.Format("Adding new Action to the Queue. Name: {0}", name));
#endif
            var actionDefinition = new ActionDefinition(
                canCauseUnexpectedResponsesToServer, 
                myAction, 
                name, 
                isRetryWhenServerNotAccept, 
                isRunOnUiThread);

            this.processes.Enqueue(actionDefinition);

            this.RunProcess();
        }

        /// <summary>
        /// The execute method when not already in queue.
        /// </summary>
        /// <param name="myAction">
        /// The my action.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="canCauseUnexpectedResponsesToServer">
        /// The can cause unexpected responses to server.
        /// </param>
        /// <param name="isRetryWhenServerNotAccept">
        /// The is retry when server not accept.
        /// </param>
        /// <param name="isRunOnUiThread">
        /// The is run on ui thread.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// name
        /// </exception>
        public void ExecuteMethodWhenNotAlreadyInQueue(
            Action myAction, 
            string name, 
            bool canCauseUnexpectedResponsesToServer = true, 
            bool isRetryWhenServerNotAccept = true, 
            bool isRunOnUiThread = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (this.Count(name) > 0)
            {
#if SHOW_DEBUG
                CustomLogger.LogDebug(() => string.Format("Job with the name {0} is already in queue. So ignore the job.", name));
#endif
                return;
            }
#if SHOW_DEBUG
            CustomLogger.LogDebug(() => "No jobs found using the name " + name + " so run the method.");
#endif

            this.ExecuteMethod(myAction, canCauseUnexpectedResponsesToServer, name, isRetryWhenServerNotAccept, isRunOnUiThread);
        }

        #endregion

        #region Methods

        internal int Count(string name)
        {
            return this.processes.ToArray().Count(a => a.Name == name);
        }

        private void HandleCanSendCommandChanged(object sender, CanSendCommandChangedEventArgs e)
        {
            if (e.NewValue)
            {
                this.RunProcess();
            }
        }

        private void IsServerWaitingChanged(object sender, EventArgs args)
        {
            this.RunProcess();
        }

        private void RunProcess()
        {
            if (this.processes.Count == 0 || !ApplicationHelper.CanSendServerCommands(false, true))
            {
                return;
            }

            if (Application.Current != null)
            {
#if SHOW_DEBUG
                CustomLogger.LogDebug(
                    () =>
                    string.Format(
                        "Ui Thread Id: {0} Actual Thread Id: {1}", 
                        Application.Current.Dispatcher.Thread.ManagedThreadId, 
                        Thread.CurrentThread.ManagedThreadId));
#endif
            }

            // get the next item
#if SHOW_DEBUG
            CustomLogger.LogDebug(() => string.Format("Jobs to process {0}", this.processes.Count));
#endif
            ActionDefinition targetAction = this.processes.Peek();

            // check if there are failed job because those should be executed as first
            if (this.failedProcesses.Count > 0)
            {
#if SHOW_DEBUG
                CustomLogger.LogDebug(() => string.Format("Failed jobs found {0}", this.failedProcesses.Count));
#endif
                var failedAction = this.failedProcesses.OrderByDescending(a => a.FailedCount).FirstOrDefault();
                if (failedAction != null)
                {
#if SHOW_DEBUG
                    CustomLogger.LogDebug(() => string.Format("Failed job will be executed. FailedCount {0}", failedAction.FailedCount));
#endif
                    targetAction = failedAction;
                }
            }

            // when the server is not ready, execute this action
            // which is here defined
            var retryActionServerNotReady = new Action(
                () =>
                    {
                        targetAction.FailedCount = targetAction.FailedCount++;
                        this.failedProcesses.Add(targetAction);
                        CustomLogger.LogError(
                            () =>
                            string.Format(
                                "Exception is caught. Server is waiting: {0}", 
                                SBPlusRuntime.Current.CommandProcessor.IsServerWaiting));
                        Thread.Sleep(300);
                        Extensions.DoEvents();
                        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(this.RunProcess));
                    });

            // In theory, the value on CanSendCommandToServer can change between the check and the call.
            if (targetAction != null && targetAction.Action != null && targetAction.CanSendCommandToServer())
            {
                try
                {
#if SHOW_DEBUG
                    CustomLogger.LogDebug(
                        () =>
                        string.Format(
                            "Server allows to send. So run now the job with name {0} Method {1}. FailedCount {2}", 
                            targetAction.Name, 
                            targetAction.Action.Method.Name, 
                            targetAction.FailedCount));
#endif
                    //dequeue the actiondefinition
                    if (targetAction.FailedCount == 0)
                    {
                        lock (this.syncObject)
                        {
                            // can happen, because another thread did the peak already
                            // and removed the targetAction
                            if (this.processes.IsEmpty)
                            {
#if SHOW_DEBUG
                                CustomLogger.LogDebug(() => "Processes list is empty, so go out.");
#endif
                                return;
                            }

                            // check concurrency issue
                            var toRemove = this.processes.Peek();
                            if (!object.Equals(toRemove, targetAction))
                            {
                                CustomLogger.LogWarning(
                                    () =>
                                    string.Format("the action is already handled {0} so go out here.", targetAction.Action.Method.Name));
                                return;
                            }

                            //when it failes it will be added to the failedProcesses
                            //so it will be executed again
                            toRemove = this.processes.Dequeue();

                            if (!object.Equals(toRemove, targetAction))
                            {
                                CustomLogger.LogError(() => "there is still a problem with peek and dequeue");
                                Debugger.Break();
                            }
                        }
                    }
                    else
                    {
                        // When action comes from the failedProcesses remove the item
                        // if it fails will be added to the failedProcesses with higher priority
                        this.failedProcesses.TakeWhile(a => a == targetAction);
                    }
                    
#if SHOW_DEBUG
                    CustomLogger.LogDebug(
                        () =>
                        string.Format(
                            "Executing action with method name {0} definition name {1} thread {2} isRunOnUiThread {3}", 
                            targetAction.Action.Method.Name, 
                            targetAction.Name, 
                            Thread.CurrentThread.ManagedThreadId, 
                            targetAction.IsRunOnUiThread));
#endif
                    if (targetAction.IsRunOnUiThread && Application.Current != null)
                    {
#if SHOW_DEBUG
                        CustomLogger.LogDebug(() => "Run on Ui thread");
#endif
                        Application.Current.Dispatcher.Invoke(targetAction.Action);
                    }
                    else
                    {
#if SHOW_DEBUG
                        CustomLogger.LogDebug(() => string.Format("run on the actual thread {0}", Thread.CurrentThread.ManagedThreadId));
#endif
                        targetAction.Action.Invoke();
                    }

#if SHOW_DEBUG
                    CustomLogger.LogDebug(() => "Executed action");
#endif
                }
                catch (SBPlusApplicationException exception)
                {
                    CustomLogger.LogException(exception);

                    if (!ApplicationHelper.CanSendServerCommands(false)
                        || exception.Message.Contains("The server is currently busy and cannot accept requests."))
                    {
                        retryActionServerNotReady();
                        return;
                    }

                    CustomLogger.LogException(exception);
                    throw;
                }
                catch (ServerNotReadyException serverNotReadyException)
                {
                    if (targetAction.IsRetryWhenServerNotAccept)
                    {
                        CustomLogger.LogWarning(() => "Server was not ready to accept the requests, retry will be done");
                        retryActionServerNotReady();
                        return;
                    }

                    CustomLogger.LogException(
                        serverNotReadyException, 
                        "this will eventually crash the client, because retry flag was not set.");
                    throw;
                }
                catch (Exception exception)
                {
                    CustomLogger.LogException(exception);
                    throw;
                }

                this.RunProcess();
            }
        }

        private void SBPlusClientOnConnected(object sender, EventArgs eventArgs)
        {
            SBPlusRuntime.Current.CommandProcessor.IsServerWaitingChanged += this.IsServerWaitingChanged;
            ApplicationHelper.CanSendCommandChanged += this.HandleCanSendCommandChanged;
        }

        private void SBPlusDisconnected(object sender, EventArgs args)
        {
            SBPlusRuntime.Current.CommandProcessor.IsServerWaitingChanged -= this.IsServerWaitingChanged;
            ApplicationHelper.CanSendCommandChanged -= this.HandleCanSendCommandChanged;

            SBPlusClient.Connected += this.SBPlusClientOnConnected;
        }

        #endregion

        private class ActionDefinition
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="SbProcessRunner.ActionDefinition"/> class.
            /// </summary>
            /// <param name="canCauseUnexpectedResponses">
            /// The can cause unexpected responses.
            /// </param>
            /// <param name="actionToRun">
            /// The action to run.
            /// </param>
            /// <param name="name">
            /// The name.
            /// </param>
            /// <param name="isRetryWhenServerNotAccept">
            /// The is retry when server not accept.
            /// </param>
            /// <param name="isRunOnUiThread">
            /// The is run on ui thread.
            /// </param>
            /// <exception cref="System.ArgumentNullException">
            /// actionToRun
            /// </exception>
            public ActionDefinition(
                bool canCauseUnexpectedResponses, 
                Action actionToRun, 
                string name, 
                bool isRetryWhenServerNotAccept, 
                bool isRunOnUiThread)
            {
                if (actionToRun == null)
                {
                    throw new ArgumentNullException("actionToRun");
                }

                if (canCauseUnexpectedResponses)
                {
                    this.CanSendCommandToServer = () => ApplicationHelper.CanSendServerCommands(doSendEventsBeforeCheck: true);
                }
                else
                {
                    this.CanSendCommandToServer = () => ApplicationHelper.CanSendServerCommands(false, doSendEventsBeforeCheck: true);
                }

                this.Action = actionToRun;
                this.Name = name;
                this.IsRetryWhenServerNotAccept = isRetryWhenServerNotAccept;
                this.IsRunOnUiThread = isRunOnUiThread;
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
            ///     Gets or sets the failed count.
            /// </summary>
            public uint FailedCount { get; set; }

            /// <summary>
            ///     Gets a value indicating whether is retry when server not accept.
            /// </summary>
            public bool IsRetryWhenServerNotAccept { get; private set; }

            /// <summary>
            ///     Gets a value indicating whether is run on ui thread.
            /// </summary>
            public bool IsRunOnUiThread { get; private set; }

            /// <summary>
            ///     Gets the name.
            /// </summary>
            public string Name { get; private set; }

            #endregion
        }
    }

    /// <summary>
    ///     Event arguments for the <see cref="ApplicationHelper.CanSendCommandChanged" /> event.
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