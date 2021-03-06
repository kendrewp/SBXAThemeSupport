﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugViewModel.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="DebugViewModel.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="DebugViewModel.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.DebugAssistant.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.UI.Client;
    using SBXA.UI.WPFControls;

    using SBXAThemeSupport.Models;
    using SBXAThemeSupport.ViewModels;

#pragma warning disable 1570

    /// <summary>
    ///     The debug view model.
    /// </summary>
    public class DebugViewModel : ViewModel
    {
        #region Constants

        /// <summary>
        ///     The state identifier
        /// </summary>
        private const string StateId = "ApplicationInsightState";

        #endregion

        #region Static Fields

        /// <summary>
        ///     The debug view model
        /// </summary>
        private static DebugViewModel debugViewModel;

        /// <summary>
        ///     The disposing
        /// </summary>
        private static bool disposing;

        /// <summary>
        ///     The initializing
        /// </summary>
        private static bool initializing;

        #endregion

        #region Fields

        /// <summary>
        ///     The process history stack
        /// </summary>
        private readonly ProcessStack processHistoryStack = new ProcessStack();

        /// <summary>
        ///     The process stack
        /// </summary>
        private readonly ProcessStack processStack = new ProcessStack();

        /// <summary>
        ///     The section1
        /// </summary>
        private readonly StringCollection section1 = new StringCollection
                                                         {
                                                             "RECORD", 
                                                             "ORIG.REC", 
                                                             "KEY", 
                                                             "WORK", 
                                                             "OTHER.REC", 
                                                             "CNT", 
                                                             "ACTION", 
                                                             "GUI", 
                                                             "LINE", 
                                                             "MAINFILE", 
                                                             "LOCK.KEY", 
                                                             "HEAD", 
                                                             "STATUS.LINE", 
                                                             "USERDATA(1)", 
                                                             "USERDATA(2)", 
                                                             "USERDATA(3)", 
                                                             "USERDATA(4)", 
                                                             "USERDATA(5)", 
                                                             "USERDATA(6)", 
                                                             "USERDATA(7)", 
                                                             "USERDATA(8)", 
                                                             "USERDATA(9)", 
                                                             "USERDATA(10)", 
                                                             "SCREENNO", 
                                                             "LF.INFO", 
                                                             "XUI"
                                                         };

        /// <summary>
        /// The section 2.
        /// </summary>
        private readonly StringCollection section2 = new StringCollection
                                                         {
                                                             "VALUE", 
                                                             "RTN.FLAG", 
                                                             "PARAM", 
                                                             "REFRESH", 
                                                             "PROC.NAME", 
                                                             "MENU.OPT", 
                                                             "LEVEL.NO", 
                                                             "FILES.OPENED", 
                                                             "OTHER(1)", 
                                                             "OTHER(2)", 
                                                             "OTHER(3)", 
                                                             "OTHER(4)", 
                                                             "OTHER(5)", 
                                                             "OTHER(6)", 
                                                             "OTHER(7)", 
                                                             "OTHER(8)", 
                                                             "OTHER(9)", 
                                                             "OTHER(10)", 
                                                             "OTHER(11)", 
                                                             "OTHER(12)", 
                                                             "OTHER(13)", 
                                                             "OTHER(14)", 
                                                             "OTHER(15)", 
                                                             "OTHER(16)", 
                                                             "OTHER(17)", 
                                                             "OTHER(18)", 
                                                             "OTHER(19)", 
                                                             "OTHER(20)", 
                                                             "SBPARM(1)", 
                                                             "SBPARM(2)", 
                                                             "SBPARM(3)", 
                                                             "SBPARM(4)", 
                                                             "SBPARM(5)", 
                                                             "SBPARM(6)", 
                                                             "SBPARM(7)", 
                                                             "SBPARM(8)", 
                                                             "SBPARM(9)", 
                                                             "SBPARM(10)", 
                                                             "SBPARM(11)", 
                                                             "SBPARM(12)", 
                                                             "SBPARM(13)", 
                                                             "SBPARM(14)", 
                                                             "SBPARM(15)", 
                                                             "SBPARM(16)", 
                                                             "SBPARM(17)", 
                                                             "SBPARM(18)", 
                                                             "SBPARM(19)", 
                                                             "SBPARM(20)"
                                                         };

        /// <summary>
        /// The section 3.
        /// </summary>
        private readonly StringCollection section3 = new StringCollection
                                                         {
                                                             "SB.CONT", 
                                                             "CONTROL", 
                                                             "PORT", 
                                                             "SYSID", 
                                                             "ACNT.NAME", 
                                                             "USER.ID", 
                                                             "GUIDATA", 
                                                             "SBCLIENT", 
                                                             "XUIDATA", 
                                                             "TERM.DEFN", 
                                                             "PRINT.DEFN", 
                                                             "PASS.DEFN", 
                                                             "USER.KEYS", 
                                                             "PCTERM", 
                                                             "BT.NODE(1)", 
                                                             "BT.NODE(2)", 
                                                             "BT.NODE(3)", 
                                                             "BT.NODE(4)", 
                                                             "BT.NODE(5)", 
                                                             "BT.NODE(6)", 
                                                             "BT.NODE(7)", 
                                                             "BT.LEV.NO", 
                                                             "BT.ID", 
                                                             "BT.POS"
                                                         };

        /// <summary>
        /// The current process.
        /// </summary>
        private DefinitionDescription currentProcess;

        /// <summary>
        /// The debug console window height.
        /// </summary>
        private double debugConsoleWindowHeight = 600d;

        /// <summary>
        /// The debug console window width.
        /// </summary>
        private double debugConsoleWindowWidth = 600d;

        /// <summary>
        /// The is connected.
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// The is debug enabled.
        /// </summary>
        private bool isDebugEnabled;

        /// <summary>
        /// The is sbxa debug window open.
        /// </summary>
        private bool isSbxaDebugWindowOpen;

        /// <summary>
        /// The orig record.
        /// </summary>
        private NestedAttributeCollection origRecord;

        /// <summary>
        /// The parms.
        /// </summary>
        private NestedAttributeCollection parms;

        /// <summary>
        /// The process analysis view model.
        /// </summary>
        private ProcessAnalysisViewModel processAnalysisViewModel;

        /// <summary>
        /// The record.
        /// </summary>
        private NestedAttributeCollection record;

        /// <summary>
        /// The section 1 collection.
        /// </summary>
        private NestedAttributeCollection section1Collection;

        /// <summary>
        /// The section 2 collection.
        /// </summary>
        private NestedAttributeCollection section2Collection;

        /// <summary>
        /// The section 3 collection.
        /// </summary>
        private NestedAttributeCollection section3Collection;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Prevents a default instance of the <see cref="DebugViewModel"/> class from being created.
        /// </summary>
        private DebugViewModel()
        {
            Initializing = true;
            this.CheckIsConnected();
            // SBPlus.Current.ConnectionStatusChanged += HandleConnectionStatusChanged;
            SBPlusClient.Connected += this.HandleConnected;
            this.CreateParmsCollection();
            this.Section1Collection = this.CreateSection(this.section1);
            this.Section2Collection = this.CreateSection(this.section2);
            this.Section3Collection = this.CreateSection(this.section3);

            this.ProcessAnalysisViewModel = new ProcessAnalysisViewModel();

            this.ReadState();
            Initializing = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether [disposing].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [disposing]; otherwise, <c>false</c>.
        /// </value>
        public static bool Disposing
        {
            get
            {
                return disposing;
            }

            set
            {
                disposing = value;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [initializing].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [initializing]; otherwise, <c>false</c>.
        /// </value>
        public static bool Initializing
        {
            get
            {
                return initializing;
            }

            set
            {
                initializing = value;
            }
        }

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        public static DebugViewModel Instance
        {
            get
            {
                return debugViewModel ?? (debugViewModel = new DebugViewModel());
            }
        }

        /// <summary>
        ///     Gets or sets the application insight state.
        /// </summary>
        public ApplicationInsightState ApplicationInsightState { get; set; }

        /// <summary>
        ///     Gets or sets the current definition.
        /// </summary>
        /// <value>
        ///     The current definition.
        /// </value>
        public DefinitionDescription CurrentProcess
        {
            get
            {
                return this.currentProcess;
            }

            set
            {
                if (this.currentProcess != null)
                {
                    this.currentProcess.IsCurrent = false;
                }

                this.currentProcess = value;
                if (this.currentProcess != null)
                {
                    this.currentProcess.IsCurrent = true;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the DebugConsoleWindowHeight property. This property will raise a
        ///     <see cref="ViewModel.PropertyChanged" /> event.
        /// </summary>
        public double DebugConsoleWindowHeight
        {
            get
            {
                return this.debugConsoleWindowHeight;
            }

            set
            {
                if (this.debugConsoleWindowHeight != value)
                {
                    this.debugConsoleWindowHeight = value;
                    this.RaisePropertyChanged("DebugConsoleWindowHeight");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the DebugConsoleWindowWidth property. This property will raise a
        ///     <see cref="ViewModel.PropertyChanged" /> event.
        /// </summary>
        public double DebugConsoleWindowWidth
        {
            get
            {
                return this.debugConsoleWindowWidth;
            }

            set
            {
                if (this.debugConsoleWindowWidth == value)
                {
                    return;
                }

                this.debugConsoleWindowWidth = value;
                this.RaisePropertyChanged("DebugConsoleWindowWidth");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [is connected].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [is connected]; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                return this.isConnected;
            }

            set
            {
                if (this.isConnected != value)
                {
                    this.isConnected = value;
                    this.RaisePropertyChanged("IsConnected");
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether debugging is enabled. This property will raise a
        ///     <see cref="ViewModel.PropertyChanged" />
        ///     event.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [is debug enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool IsDebugEnabled
        {
            get
            {
                return this.isDebugEnabled;
            }

            set
            {
                if (this.isDebugEnabled != value)
                {
                    this.isDebugEnabled = value;
                    this.RaisePropertyChanged("IsDebugEnabled");
                    this.SetIsDebugEnabled(this.IsDebugEnabled);
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the SB/XA Debug Window is OPen or not. This property will raise a
        ///     <see cref="ViewModel.PropertyChanged" /> event.
        /// </summary>
        public bool IsSbxaDebugWindowOpen
        {
            get
            {
                return this.isSbxaDebugWindowOpen;
            }

            set
            {
                if (this.isSbxaDebugWindowOpen != value)
                {
                    this.isSbxaDebugWindowOpen = value;
                    DebugWindowManager.ShowDebugWindow(value);
                    this.RaisePropertyChanged("IsSbxaDebugWindowOpen");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the orig record.
        /// </summary>
        public NestedAttributeCollection OrigRecord
        {
            get
            {
                return this.origRecord;
            }

            set
            {
                if (this.origRecord == value)
                {
                    return;
                }

                this.origRecord = value;
                this.RaisePropertyChanged("OrigRecord");
            }
        }

        /// <summary>
        ///     Gets or sets the parms.
        /// </summary>
        public NestedAttributeCollection Parms
        {
            get
            {
                return this.parms;
            }

            set
            {
                if (this.parms == value)
                {
                    return;
                }

                this.parms = value;
                this.RaisePropertyChanged("Parms");
            }
        }

        /// <summary>
        ///     Gets or sets the process analysis view model.
        /// </summary>
        public ProcessAnalysisViewModel ProcessAnalysisViewModel
        {
            get
            {
                return this.processAnalysisViewModel;
            }

            set
            {
                this.processAnalysisViewModel = value;
            }
        }

        /// <summary>
        ///     Gets the definition history stack.
        /// </summary>
        public ProcessStack ProcessHistoryStack
        {
            get
            {
                return this.processHistoryStack;
            }
        }

        /// <summary>
        ///     Gets the definition stack.
        /// </summary>
        public ProcessStack ProcessStack
        {
            get
            {
                return this.processStack;
            }
        }

        /// <summary>
        ///     Gets or sets the record.
        /// </summary>
        public NestedAttributeCollection Record
        {
            get
            {
                return this.record;
            }

            set
            {
                if (this.record == value)
                {
                    return;
                }

                this.record = value;
                this.RaisePropertyChanged("Record");
            }
        }

        /// <summary>
        ///     Gets or sets the section 1 collection.
        /// </summary>
        public NestedAttributeCollection Section1Collection
        {
            get
            {
                return this.section1Collection;
            }

            set
            {
                if (this.section1Collection == value)
                {
                    return;
                }

                this.section1Collection = value;
                this.RaisePropertyChanged("Section1Collection");
            }
        }

        /// <summary>
        ///     Gets or sets the Section2Collection property. This property will raise a <see cref="ViewModel.PropertyChanged" />
        ///     event.
        /// </summary>
        public NestedAttributeCollection Section2Collection
        {
            get
            {
                return this.section2Collection;
            }

            set
            {
                if (this.section2Collection != value)
                {
                    this.section2Collection = value;
                    this.RaisePropertyChanged("Section2Collection");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the Section3Collection property. This property will raise a <see cref="ViewModel.PropertyChanged" />
        ///     event.
        /// </summary>
        public NestedAttributeCollection Section3Collection
        {
            get
            {
                return this.section3Collection;
            }

            set
            {
                if (this.section3Collection != value)
                {
                    this.section3Collection = value;
                    this.RaisePropertyChanged("Section3Collection");
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The enable error log.
        /// </summary>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public static void EnableErrorLog(bool enable)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
            {
                JobManager.RunInUIThread(
                    DispatcherPriority.Normal, 
                    delegate
                        {
                            SBPlus.Current.ApplicationDefinition.LogCustomError = enable;
                            if (enable)
                            {
                                SessionManager.SessionsLog.EnableLogLevel(Log.MT_CUSTOM_ERROR);
                                SessionManager.SessionsLog.EnableLogLevel(Log.LL_CUSTOM);
                            }
                            else
                            {
                                SessionManager.SessionsLog.DisableLogLevel(Log.MT_CUSTOM_ERROR);
                            }
                        });
            }
            else
            {
                SBPlus.Current.ApplicationDefinition.LogCustomError = enable;
                if (enable)
                {
                    SessionManager.SessionsLog.EnableLogLevel(Log.MT_CUSTOM_ERROR);
                    SessionManager.SessionsLog.EnableLogLevel(Log.LL_CUSTOM);
                }
                else
                {
                    SessionManager.SessionsLog.DisableLogLevel(Log.MT_CUSTOM_ERROR);
                }
            }
        }

        /// <summary>
        /// The enable information log.
        /// </summary>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public static void EnableInformationLog(bool enable)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
            {
                JobManager.RunInUIThread(
                    DispatcherPriority.Normal, 
                    delegate
                        {
                            SBPlus.Current.ApplicationDefinition.LogCustomInfo = enable;
                            if (enable)
                            {
                                SessionManager.SessionsLog.EnableLogLevel(Log.MT_CUSTOM_INFO);
                                SessionManager.SessionsLog.EnableLogLevel(Log.LL_CUSTOM);
                            }
                            else
                            {
                                SessionManager.SessionsLog.DisableLogLevel(Log.MT_CUSTOM_INFO);
                            }
                        });
            }
            else
            {
                SBPlus.Current.ApplicationDefinition.LogCustomInfo = enable;
                if (enable)
                {
                    SessionManager.SessionsLog.EnableLogLevel(Log.MT_CUSTOM_INFO);
                    SessionManager.SessionsLog.EnableLogLevel(Log.LL_CUSTOM);
                }
                else
                {
                    SessionManager.SessionsLog.DisableLogLevel(Log.MT_CUSTOM_INFO);
                }
            }
        }

        /// <summary>
        /// The enable logging.
        /// </summary>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public static void EnableLogging(bool enable)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
            {
                JobManager.RunInUIThread(
                    DispatcherPriority.Normal, 
                    delegate
                        {
                            if (enable)
                            {
                                SBPlusClient.EnableCustomLogging();
                            }
                            else
                            {
                                SBPlusClient.DisableCustomLogging();
                            }
                        });
            }
            else
            {
                if (enable)
                {
                    SBPlusClient.EnableCustomLogging();
                }
                else
                {
                    SBPlusClient.DisableCustomLogging();
                }
            }
        }

        /// <summary>
        /// The enable warning log.
        /// </summary>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public static void EnableWarningLog(bool enable)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
            {
                JobManager.RunInUIThread(
                    DispatcherPriority.Normal, 
                    delegate
                        {
                            SBPlus.Current.ApplicationDefinition.LogCustomInfo = enable;
                            if (enable)
                            {
                                SessionManager.SessionsLog.EnableLogLevel(Log.MT_CUSTOM_WARNING);
                                SessionManager.SessionsLog.EnableLogLevel(Log.LL_CUSTOM);
                            }
                            else
                            {
                                SessionManager.SessionsLog.DisableLogLevel(Log.MT_CUSTOM_WARNING);
                            }
                        });
            }
            else
            {
                SBPlus.Current.ApplicationDefinition.LogCustomInfo = enable;
                if (enable)
                {
                    SessionManager.SessionsLog.EnableLogLevel(Log.MT_CUSTOM_WARNING);
                    SessionManager.SessionsLog.EnableLogLevel(Log.LL_CUSTOM);
                }
                else
                {
                    SessionManager.SessionsLog.DisableLogLevel(Log.MT_CUSTOM_WARNING);
                }
            }
        }

        /// <summary>
        /// The get common variable.
        /// </summary>
        /// <param name="whichVariable">
        /// The which variable.
        /// </param>
        public static void GetCommonVariable(string whichVariable)
        {
            JobManager.RunInUIThread(DispatcherPriority.Input, () => XuiDebug.GetCommonVariable(GetCommonVariableCompleted, whichVariable));
        }

        /// <summary>
        /// The log information.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void LogInformation(string message)
        {
            EnableInformationLog(true);
            SBPlusClient.LogInformation(message);
        }

        /// <summary>
        /// The update definition stack.
        /// </summary>
        /// <param name="add">
        /// The add.
        /// </param>
        /// <param name="processName">
        /// The definition name.
        /// </param>
        /// <param name="paramaters">
        /// The proc name.
        /// </param>
        public static void UpdateProcessStack(bool add, string processName, SBString paramaters)
        {
            var actionTime = DateTime.Now;
            try
            {
                int serverActionTime = 0;
                if (paramaters.Extract(1, 1).Dcount() > 1)
                {
                    int.TryParse(paramaters.Extract(1, 1, 1).Value, out serverActionTime);
                    CustomLogger.LogDebug(() => string.Format("Add {0} {1} at {2}.", add, processName, serverActionTime));
                }
                else
                {
                    CustomLogger.LogDebug(() => string.Format("Add {0} {1}.", add, processName));
                }

                if (DebugWindowManager.DebugConsoleWindow == null)
                {
                    // no debug windows.
                    return;
                }

                if (Thread.CurrentThread.ManagedThreadId != DebugWindowManager.DebugConsoleWindow.Dispatcher.Thread.ManagedThreadId)
                {
                    JobManager.RunInDispatcherThread(
                        DebugWindowManager.DebugConsoleWindow.Dispatcher, 
                        DispatcherPriority.Normal, 
                        () => DoUpdateProcessStack(add, processName, actionTime, serverActionTime));
                }
                else
                {
                    DoUpdateProcessStack(add, processName, actionTime, serverActionTime);
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Exception caught.", exception);
            }
        }

        /// <summary>
        ///     Clears the history stack.
        /// </summary>
        public void ClearHistoryStack()
        {
            this.ProcessHistoryStack.Clear();

            // remove all the references
            foreach (var processDescription in this.ProcessStack)
            {
                processDescription.ClearHistoryReferences();
            }

            this.CurrentProcess = null;
            // add back the current definition
            //            CurrentProcess.Clear();
            //            ProcessHistoryStack.Push(CurrentProcess);
        }

        /// <summary>
        /// The refresh collection.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        public void RefreshCollection(NestedAttributeCollection collection)
        {
        }

        /// <summary>
        ///     The save state.
        /// </summary>
        public void SaveState()
        {
            if (this.ApplicationInsightState != null && !Initializing && !Disposing)
            {
                string xml = this.ApplicationInsightState.SerializeToXml();
                SBPlus.Current.GlobalStateFile.SetItem(new SBhStateFileItem(StateId, xml), false);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The build title.
        /// </summary>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <param name="newIndex">
        /// The new index.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        internal static string BuildTitle(string parent, string newIndex)
        {
            string variable;
            if (string.IsNullOrEmpty(parent))
            {
                return newIndex;
            }

            if (parent.EndsWith(">"))
            {
                variable = parent.Substring(0, parent.Length - 1) + "," + newIndex + ">";
            }
            else
            {
                variable = parent + "<" + newIndex + ">";
            }

            return variable;
        }

        /// <summary>
        /// The check is connected.
        /// </summary>
        internal void CheckIsConnected()
        {
            JobManager.RunInUIThread(
                DispatcherPriority.Input, 
                delegate
                    {
                        if (SBPlusClient.Current == null || SBPlusClient.Current.ConnectionStatus != ConnectionStatuses.Connected)
                        {
                            this.SetIsConnected(false);
                            return;
                        }

                        this.SetIsConnected(true);
                    });
        }

        /// <summary>
        /// The do update process stack.
        /// </summary>
        /// <param name="add">
        /// The add.
        /// </param>
        /// <param name="processName">
        /// The process name.
        /// </param>
        /// <param name="actionTime">
        /// The action time.
        /// </param>
        /// <param name="serverActionTime">
        /// The server action time.
        /// </param>
        private static void DoUpdateProcessStack(bool add, string processName, DateTime actionTime, int serverActionTime)
        {
            try
            {
                Debug.WriteLine("[DebugViewModel.DoUpdateProcessStack(837)] " + add + ", " + processName);
                lock (Instance.ProcessStack)
                {
                    if (add)
                    {
                        var historyProcess = new DefinitionDescription(string.Empty, processName)
                                                 {
                                                     StartTime = actionTime, 
                                                     ServerStartMilliseconds =
                                                         serverActionTime
                                                 };
                        try
                        {
                            PushProcess(
                                Instance.ProcessStack, 
                                new DefinitionDescription(string.Empty, processName)
                                    {
                                        HistoryProcessDescription = historyProcess, 
                                        StartTime = actionTime, 
                                        ServerStartMilliseconds = serverActionTime
                                    });
                        }
                        catch (Exception exception)
                        {
                            SBPlusClient.LogError("Exception caught adding " + processName + " to ProcessStack.", exception);
                        }

                        lock (Instance.ProcessHistoryStack)
                        {
                            try
                            {
                                if (Instance.CurrentProcess == null)
                                {
                                    Instance.ProcessHistoryStack.Push(historyProcess);
                                }
                                else
                                {
                                    Instance.CurrentProcess.ChildProcesses.Push(historyProcess);
                                }

                                Instance.CurrentProcess = historyProcess;
                            }
                            catch (Exception exception)
                            {
                                SBPlusClient.LogError("Exception caught adding " + processName + " to ProcessHistoryStack.", exception);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (Instance.ProcessStack.Count == 0)
                            {
                                return;
                            }

                            PopProcess(Instance.ProcessStack, actionTime, serverActionTime);
                        }
                        catch (Exception exception)
                        {
                            CustomLogger.LogException(exception, "Exception caught adding " + processName + " to ProcessHistoryStack.");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Exception caught.", exception);
            }
        }

        /// <summary>
        /// The find lowest process.
        /// </summary>
        /// <param name="definition">
        /// The definition.
        /// </param>
        /// <returns>
        /// The <see cref="DefinitionDescription"/>.
        /// </returns>
        private static DefinitionDescription FindLowestProcess(DefinitionDescription definition)
        {
            if (definition.ChildProcesses.Count == 0)
            {
                return definition;
            }

            return FindLowestProcess(definition.ChildProcesses.Peek());
        }

        /// <summary>
        /// The find lowest process parent.
        /// </summary>
        /// <param name="definition">
        /// The definition.
        /// </param>
        /// <returns>
        /// The <see cref="DefinitionDescription"/>.
        /// </returns>
        private static DefinitionDescription FindLowestProcessParent(DefinitionDescription definition)
        {
            // The zero is to prevent an infinite loop - it should never happen.
            if (definition.ChildProcesses.Count == 0)
            {
                return definition;
            }

            if (definition.ChildProcesses.Count == 1)
            {
                // I need to look if the child has a definition in it's children.
                var childProcessDescription = definition.ChildProcesses.Peek();
                if (childProcessDescription.ChildProcesses.Count == 0)
                {
                    return definition;
                }
            }

            return FindLowestProcessParent(definition.ChildProcesses.Peek());
        }

        /// <summary>
        /// The get common variable completed.
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
        private static void GetCommonVariableCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {
                if (DebugWindowManager.DebugConsoleWindow != null)
                {
                    JobManager.RunInDispatcherThread(
                        DebugWindowManager.DebugConsoleWindow.Dispatcher, 
                        DispatcherPriority.Normal, 
                        () => UpdateProperties(parameters));
                }
                else
                {
                    UpdateProperties(parameters);
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Exception caught.", exception);
            }
        }

        /// <summary>
        /// The pop process.
        /// </summary>
        /// <param name="stack">
        /// The stack.
        /// </param>
        /// <param name="endTime">
        /// The end time.
        /// </param>
        /// <param name="serverEndTime">
        /// The server end time.
        /// </param>
        private static void PopProcess(ProcessStack stack, DateTime endTime, int serverEndTime)
        {
            // Check to see if the definition on top of the stack has children, if so pop from the child, otherwise pop the top of the stack - recursively.
            if (stack.Count == 0)
            {
                return;
            }

            // want to find the definition with no children and pop it from it's parent.
            var parentProcess = FindLowestProcessParent(stack.Peek());
            var processStack = parentProcess.ChildProcesses;
            if (processStack.Count > 0)
            {
                var processDescription = processStack.Pop();
                processDescription.EndTime = endTime;
                if (processDescription.HistoryProcessDescription != null)
                {
                    // set the end time on the process description in the history stack.
                    processDescription.HistoryProcessDescription.EndTime = endTime;
                    processDescription.HistoryProcessDescription.ServerEndMilliseconds = serverEndTime;
                }

                processDescription.HistoryProcessDescription = null;
                Instance.CurrentProcess = parentProcess.HistoryProcessDescription;
            }
            else
            {
                // all the nested children have been removed so remove the top of the stack now.
                var processDescription = stack.Pop();
                processDescription.EndTime = endTime;
                processDescription.ServerEndMilliseconds = serverEndTime;

                if (processDescription.HistoryProcessDescription != null)
                {
                    // set the end time on the process description in the history stack.
                    processDescription.HistoryProcessDescription.EndTime = endTime;
                    processDescription.HistoryProcessDescription.ServerEndMilliseconds = serverEndTime;
                }

                processDescription.HistoryProcessDescription = null;
                Instance.CurrentProcess = null;
            }
        }

        /// <summary>
        /// The push process.
        /// </summary>
        /// <param name="stack">
        /// The stack.
        /// </param>
        /// <param name="definition">
        /// The definition.
        /// </param>
        private static void PushProcess(ProcessStack stack, DefinitionDescription definition)
        {
            // check if there is a definition on the stack, if there is add it to the list of children, not just push it.
            if (stack.Count == 0)
            {
                stack.Push(definition);
            }
            else
            {
                var lowestParent = FindLowestProcess(stack.Peek());
                if (lowestParent != null)
                {
                    lowestParent.ChildProcesses.Push(definition);
                }
            }
        }

        /// <summary>
        /// The set item data.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="which">
        /// The which.
        /// </param>
        /// <param name="collection">
        /// The collection.
        /// </param>
        private static void SetItemData(SBString value, string which, NestedAttributeCollection collection)
        {
            if (collection.ContainsIndex(which))
            {
                var item = collection.GetItemWithIndex(which);
                if (item == null)
                {
                    collection.Add(new NestedAttribute(which, value, BuildTitle(collection.Variable, which)));
                }

                if (item != null)
                {
                    item.Source = value;
                }
            }
            else if (!string.IsNullOrEmpty(collection.Variable) && collection.Variable.Equals(which))
            {
                collection.Source = value;
            }
            else
            {
                collection.Add(new NestedAttribute(which, value, BuildTitle(collection.Variable, which)));
            }
        }

        /// <summary>
        /// The update properties.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        private static void UpdateProperties(IList<SBString> parameters)
        {
            switch (parameters[1].Value)
            {
                case "PARMS":
                    // Instance.SetParms("PARMS", parameters[2], parameters[3]);
                    break;
                default:
                    Instance.UpdateCollection(parameters[1].Value, parameters[2]);
                    break;
            }
        }

        /// <summary>
        /// The clear stacks.
        /// </summary>
        private void ClearStacks()
        {
            this.ProcessHistoryStack.Clear();
            this.ProcessStack.Clear();
        }

        /// <summary>
        /// The create parms collection.
        /// </summary>
        private void CreateParmsCollection()
        {
            if (this.Parms != null)
            {
                return;
            }

            this.Parms = new NestedAttributeCollection();
            for (var pno = 1; pno <= 40; pno++)
            {
                this.Parms.Add(new NestedAttribute(string.Format("PARMS({0})", pno), new SBString(), string.Format("PARMS({0})", pno)));
            }
        }

        /// <summary>
        /// The create section.
        /// </summary>
        /// <param name="varCollection">
        /// The var collection.
        /// </param>
        /// <returns>
        /// The <see cref="NestedAttributeCollection"/>.
        /// </returns>
        private NestedAttributeCollection CreateSection(StringCollection varCollection)
        {
            var collection = new NestedAttributeCollection();

            foreach (var cvar in varCollection)
            {
                collection.Add(new NestedAttribute(cvar, new SBString(), cvar));
            }

            return collection;
        }

        /// <summary>
        /// The get collection.
        /// </summary>
        /// <param name="which">
        /// The which.
        /// </param>
        /// <returns>
        /// The <see cref="NestedAttributeCollection"/>.
        /// </returns>
        private NestedAttributeCollection GetCollection(string which)
        {
            if (this.section1.Contains(which))
            {
                return this.Section1Collection;
            }

            if (this.section2.Contains(which))
            {
                return this.Section2Collection;
            }

            if (this.section3.Contains(which))
            {
                return this.Section3Collection;
            }

            if (which.StartsWith("PARMS"))
            {
                return this.Parms;
            }

            return null;
        }

        /// <summary>
        /// The get is debug enabled.
        /// </summary>
        private void GetIsDebugEnabled()
        {
            if (!SBPlusClient.Current.CanSendServerCommands)
            {
                SBPlusClient.Current.ReadyToSendCommands += this.HandleReadyToSendCommands;
                return;
            }

            SBPlusClient.Current.ReadyToSendCommands -= this.HandleReadyToSendCommands;

            // SUBROUTINE UT.XUI.READ (FILE.NAME, ID, ATTR, ITEM, MODE, READ.STATUS)
            // First check to see it XUI.DEBUG is there in the VOC, if not never set IsDebugEnabled.
            XuiDebug.IsXuiDebugThere(this.ReadXuiDebugCompleted);
        }

        /// <summary>
        /// The handle connected.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleConnected(object sender, ConnectedEventArgs e)
        {
            this.SetIsConnected(e.Connected);
            if (e.Connected)
            {
                this.GetIsDebugEnabled();
            }
            else
            {
                if (this.isDebugEnabled)
                {
                    this.IsDebugEnabled = false;
                }
            }
        }

        /// <summary>
        /// The handle ready to send commands.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleReadyToSendCommands(object sender, ReadyToSendCommandsEventArgs e)
        {
            this.GetIsDebugEnabled();
        }

        /// <summary>
        /// The read for set xui debug completed.
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
        private void ReadForSetXuiDebugCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            var status = parameters[5];
            if (status.Count != 1 || !status.Value.Equals("0"))
            {
                if (this.isDebugEnabled)
                {
                    this.IsDebugEnabled = false;
                }

                return;
            }

            JobManager.RunInUIThread(
                DispatcherPriority.Input, 
                () => XuiDebug.EnableDebug(this.SetIsDebugEnabledCompleted, this.IsDebugEnabled));
        }

        /// <summary>
        /// The read state.
        /// </summary>
        private void ReadState()
        {
            if (this.ApplicationInsightState == null)
            {
                if (SBPlus.Current.GlobalStateFile.Exists(StateId))
                {
                    var xml = SBPlus.Current.GlobalStateFile.GetItem(StateId).Object as string;
                    this.ApplicationInsightState = Extensions.DeserializeFromXml<ApplicationInsightState>(xml);
                }

                if (this.ApplicationInsightState == null)
                {
                    this.ApplicationInsightState = new ApplicationInsightState();
                }

                if (this.ApplicationInsightState.MruProcessList == null)
                {
                    this.ApplicationInsightState.MruProcessList = new StringCollection();
                }
            }
        }

        /// <summary>
        /// The read xui debug completed.
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
        private void ReadXuiDebugCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            var status = parameters[5];
            if (status.Count != 1 || !status.Value.Equals("0"))
            {
                if (this.isDebugEnabled)
                {
                    this.IsDebugEnabled = false;
                }

                return;
            }

            XuiDebug.IsDebugEnabled(this.SetIsDebugEnabledCompleted);
        }

        /// <summary>
        /// The set is connected.
        /// </summary>
        /// <param name="connected">
        /// The connected.
        /// </param>
        private void SetIsConnected(bool connected)
        {
            // Set the IsConnected property on the correct thread.
            if (DebugWindowManager.DebugConsoleWindow == null)
            {
                return;
            }

            JobManager.RunInDispatcherThread(
                DebugWindowManager.DebugConsoleWindow.Dispatcher, 
                DispatcherPriority.Normal, 
                delegate { this.IsConnected = connected; });
        }

        /// <summary>
        /// The set is debug enabled.
        /// </summary>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        private void SetIsDebugEnabled(bool newValue)
        {
            try
            {
                if (newValue)
                {
                    this.ClearStacks();
                    EnableLogging(true);
                    EnableErrorLog(true);
                    EnableInformationLog(true);
                }
                else
                {
                    EnableErrorLog(false);
                    EnableInformationLog(false);
                    EnableLogging(false);
                }

                // First check to see it XUI.DEBUG is there in the VOC, if not never set IsDebugEnabled.
                JobManager.RunInUIThread(DispatcherPriority.Input, () => XuiDebug.IsXuiDebugThere(this.ReadForSetXuiDebugCompleted));
            }
            catch (Exception exception)
            {
                CustomLogger.LogException(exception, "There was a problem while setting IsDebugEnabled.");
            }
        }

        /// <summary>
        /// The set is debug enabled completed.
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
        private void SetIsDebugEnabledCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {
                if (parameters[1].Dcount() == 1 && !string.IsNullOrEmpty(parameters[1].Value))
                {
                    this.isDebugEnabled = parameters[1].Value.Equals("1");
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Exception setting IsDebugEnabled.", exception);
            }
        }

        /// <summary>
        /// The update collection.
        /// </summary>
        /// <param name="which">
        /// The which.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        private void UpdateCollection(string which, SBString value)
        {
            try
            {
                var collection = this.GetCollection(which);
                if (collection != null)
                {
                    SetItemData(value, which, collection);
                }

                // Either did not find it and/or the it is already displayed in a sub window so have a look.
                var window = DebugWindowManager.GetWindow(which);
                if (window == null)
                {
                    // check to see if I have an specific attribute or value.
                    if (which.Contains("<"))
                    {
                        // extract the parent
                        var parent = which.Substring(0, which.IndexOf("<", StringComparison.Ordinal));
                        // look for a window first then a base colleciton
                        window = DebugWindowManager.GetWindow(parent);
                        if (window != null)
                        {
                            collection = window.DataContext as NestedAttributeCollection;
                        }
                        else
                        {
                            collection = this.GetCollection(parent);
                        }
                    }
                }
                else
                {
                    collection = window.DataContext as NestedAttributeCollection;
                }

                if (collection == null)
                {
                    return;
                }

                SetItemData(value, which, collection);
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Problem getting common variable infor for " + which, exception);
            }
        }

        #endregion
    }
}