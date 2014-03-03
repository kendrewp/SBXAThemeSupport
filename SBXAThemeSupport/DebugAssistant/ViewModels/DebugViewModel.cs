using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Threading;
using SBXA.Runtime;
using SBXA.Shared;
using SBXA.UI.Client;
using SBXA.UI.WPFControls;
using SBXAThemeSupport.Models;
using SBXAThemeSupport.ViewModels;

namespace SBXAThemeSupport.DebugAssistant.ViewModels
{
#pragma warning disable 1570
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// *
    /// * This process will make life easier and more consistent to
    /// * debug process
    /// *
    /// * P.DEBUG,[message]
    /// *
    /// LOCAL PNAME, MESSAGE, ORTN.FLAG, OREFRESH
    /// LOCAL ASSEMBLY_NAME, TYPE, METHOD_NAME, PARAMETERS, RETVAL, STATUS, STATUSDESC
    /// *
    /// IF NOT(IS.DEV) THEN EXIT
    /// ORTN.FLAG = @RTN.FLAG
    /// OREFRESH=@REFRESH
    /// PNAME = @PROC.NAME<2>
    /// MESSAGE = @PARAM"0,1"
    /// PARAMETERS = "[":PNAME:"] ":MESSAGE
    /// *
    /// IF @XUI THEN
    /// ASSEMBLY_NAME = "CustomSBPlusTheme"
    ///   TYPE = "CustomSBPlusTheme.DebugAssistant.ViewModels.DebugViewModel"
    ///   METHOD_NAME = "LogInformation"
    ///   RETVAL = ""
    ///   STATUS = ""
    ///   STATUSDESC = ""
    ///   CALL SB.CALL.STATIC.METHOD("", ASSEMBLY_NAME, TYPE, METHOD_NAME, PARAMETERS, RETVAL, STATUS, STATUSDESC)
    /// *  DISP 4, PARAMETERS
    /// *
    /// END ELSE
    ///   DISP 4, PARAMETERS
    /// END
    /// @RTN.FLAG = ORTN.FLAG
    /// @REFRESH=OREFRESH
    /// </example>
#pragma warning restore 1570
    public class DebugViewModel : ViewModel
    {
        private static DebugViewModel _DebugViewModel;

        private readonly StringCollection _Section1 = new StringCollection
            {
                "RECORD", "ORIG.REC", "KEY", "WORK", "OTHER.REC", "CNT", "ACTION",
                "GUI", "LINE", "MAINFILE", "LOCK.KEY", "HEAD", "STATUS.LINE", "USERDATA(1)",
                "USERDATA(2)", "USERDATA(3)", "USERDATA(4)", "USERDATA(5)", "USERDATA(6)", "USERDATA(7)",
                "USERDATA(8)", "USERDATA(9)", "USERDATA(10)", "SCREENNO", "LF.INFO", "XUI"
            };

        private readonly StringCollection _Section2 = new StringCollection
            {
                "VALUE",
                "RTN.FLAG",
                "PARAM",
                "REFRESH",
                "PROC.NAME",
                "MENU.OPT",
                "LEVEL.NO",
                "FILES.OPENED",
                "OTHER(1)", "OTHER(2)","OTHER(3)","OTHER(4)","OTHER(5)","OTHER(6)","OTHER(7)",
                "OTHER(8)","OTHER(9)","OTHER(10)","OTHER(11)","OTHER(12)","OTHER(13)","OTHER(14)",
                "OTHER(15)","OTHER(16)","OTHER(17)","OTHER(18)","OTHER(19)","OTHER(20)",
                "SBPARM(1)", "SBPARM(2)","SBPARM(3)","SBPARM(4)","SBPARM(5)","SBPARM(6)",
                "SBPARM(7)","SBPARM(8)","SBPARM(9)","SBPARM(10)","SBPARM(11)","SBPARM(12)",
                "SBPARM(13)","SBPARM(14)","SBPARM(15)","SBPARM(16)","SBPARM(17)","SBPARM(18)",
                "SBPARM(19)","SBPARM(20)"
            };

        private readonly StringCollection _Section3 = new StringCollection
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
                "BT.NODE(1)","BT.NODE(2)","BT.NODE(3)","BT.NODE(4)","BT.NODE(5)","BT.NODE(6)","BT.NODE(7)",
                "BT.LEV.NO",
                "BT.ID",
                "BT.POS"
            };

        private DebugViewModel()
        {
            JobManager.RunInUIThread(DispatcherPriority.Input,
                                     delegate
                                     {
                                         if (SBPlusClient.Current == null || SBPlusClient.Current.ConnectionStatus != ConnectionStatuses.Connected) return;

                                         if (DebugWindowManager.DebugConsoleWindow == null) return;

                                         SetIsDebugEnabled();

                                     });
            // SBPlus.Current.ConnectionStatusChanged += HandleConnectionStatusChanged;
            SBPlusClient.Connected += HandleConnected;
            CreateParmsCollection();
            Section1Collection = CreateSection(_Section1);
            Section2Collection = CreateSection(_Section2);
            Section3Collection = CreateSection(_Section3);
        }

        private NestedAttributeCollection CreateSection(StringCollection varCollection)
        {
            var collection = new NestedAttributeCollection();

            foreach (var cvar in varCollection)
            {
                collection.Add(new NestedAttribute(cvar, new SBString(), cvar));
            }
            return collection;
        }

        public void RefreshCollection(NestedAttributeCollection collection)
        {

        }

        void HandleConnected(object sender, ConnectedEventArgs e)
        {
            if (e.Connected)
            {
                SetIsDebugEnabled();
            }
            else
            {
                IsDebugEnabled = false;
            }
        }

        void HandleReadyToSendCommands(object sender, ReadyToSendCommandsEventArgs e)
        {
            SetIsDebugEnabled();
        }

        private void SetIsDebugEnabled(bool newValue)
        {
            if (newValue) ClearStacks();
            JobManager.RunInUIThread(DispatcherPriority.Input,
                         () =>
                         SBProcessRunner.Instance.CallSubroutine("XUI.DEBUG", 6, new[] { new SBString("1"), new SBString(IsDebugEnabled ? "1" : "0"), new SBString(), new SBString(), new SBString(), new SBString() }, new object(), SetIsDebugEnabledCompleted)
                         );
        }
        private void SetIsDebugEnabled()
        {
            if (!SBPlusClient.Current.CanSendServerCommands)
            {
                SBPlusClient.Current.ReadyToSendCommands += HandleReadyToSendCommands;
                return;
            }

            SBPlusClient.Current.ReadyToSendCommands -= HandleReadyToSendCommands;

            JobManager.RunInUIThread(DispatcherPriority.Input,
                         () =>
                         SBProcessRunner.Instance.CallSubroutine("XUI.DEBUG", 6, new[] { new SBString("6"), new SBString(), new SBString(), new SBString(), new SBString(), new SBString() }, new object(), SetIsDebugEnabledCompleted)
                         );

        }

        private void SetIsDebugEnabledCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {

                if (parameters[1].Dcount() == 1 && !String.IsNullOrEmpty(parameters[1].Value))
                {
                    IsDebugEnabled = parameters[1].Value.Equals("1");
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Exception setting IsDebugEnabled.", exception);
            }
        }
        private void ClearStacks()
        {
            ProcessHistoryStack.Clear();
            ProcessStack.Clear();
        }
        public static DebugViewModel Instance
        {
            get
            {
                if (_DebugViewModel == null) _DebugViewModel = new DebugViewModel();
                return (_DebugViewModel);
            }
        }

        public static void LogInformation(string message)
        {
            EnableInformationLog(true);
            SBPlusClient.LogInformation(message);
        }

        public static void EnableInformationLog(bool enable)
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

        public static void EnableWarningLog(bool enable)
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

        public static void EnableErrorLog(bool enable)
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

        public static void EnableLogging(bool enable)
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

        public static void UpdateProcessStack(bool add, string processName, SBString procName)
        {

            try
            {
                if (DebugWindowManager.DebugConsoleWindow != null)
                {
                    JobManager.RunInDispatcherThread(DebugWindowManager.DebugConsoleWindow.Dispatcher, DispatcherPriority.Send, () => DoUpdateProcessStack(add, processName));
                }
                else
                {
                    DoUpdateProcessStack(add, processName);
                }

            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Exception caught.", exception);
            }
        }

        private static void DoUpdateProcessStack(bool add, string processName)
        {

            try
            {
                lock (Instance.ProcessStack)
                {
                    if (add)
                    {
                        try
                        {
                            Instance.ProcessStack.Push(processName);
                        }
                        catch (Exception exception)
                        {
                            SBPlusClient.LogError("Exception caught adding " + processName + " to ProcessStack.", exception);
                        }
                        lock (Instance.ProcessHistoryStack)
                        {
                            try
                            {
                                Instance.ProcessHistoryStack.Push(processName); // only push it to the history never pop it.
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
                            if (Instance.ProcessHistoryStack.Count == 0) return;
                            Instance.ProcessStack.Pop();
                        }
                        catch (Exception exception)
                        {
                            SBPlusClient.LogError("Exception caught adding " + processName + " to ProcessHistoryStack.", exception);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Exception caught.", exception);
            }
        }

        public static void GetCommonVariable(string whichVariable)
        {
            JobManager.RunInUIThread(DispatcherPriority.Input,
                                     () =>
                                     SBProcessRunner.Instance.CallSubroutine("XUI.DEBUG", 6, new[] { new SBString("4"), new SBString(whichVariable), new SBString(), new SBString(), new SBString(), new SBString() }, new object(), GetCommonVariableCompleted)
                                     );
        }

        static void GetCommonVariableCompleted(string subroutineName, SBString[] parameters, object userState)
        {

            try
            {
                if (DebugWindowManager.DebugConsoleWindow != null)
                {
                    JobManager.RunInDispatcherThread(DebugWindowManager.DebugConsoleWindow.Dispatcher, DispatcherPriority.Send, () => UpdateProperties(parameters));
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

        static void UpdateProperties(IList<SBString> parameters)
        {
            switch (parameters[1].Value)
            {
                case ("PARMS"):
                    // Instance.SetParms("PARMS", parameters[2], parameters[3]);
                    break;
                default:
                    Instance.UpdateCollection(parameters[1].Value, parameters[2]);
                    break;
            }
        }

        /*
                private void SetSingleVariables(CommonVariables which, SBString value)
                {
                    if (Section1Collection == null) Section1Collection = new NestedAttributeCollection();
                    SetItemData(value, which.ToString(), Section1Collection);
                }
        */

        private void CreateParmsCollection()
        {
            if (Parms != null) return;

            Parms = new NestedAttributeCollection();
            for (var pno = 1; pno <= 40; pno++)
            {
                Parms.Add(new NestedAttribute(String.Format("PARMS({0})", pno), new SBString(), String.Format("PARMS({0})", pno)));
            }
        }

        private void UpdateCollection(string which, SBString value)
        {
            try
            {

                var collection = GetCollection(which, value);
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
                        var parent = which.Substring(0, which.IndexOf("<"));
                        // look for a window first then a base colleciton
                        window = DebugWindowManager.GetWindow(parent);
                        if (window != null)
                        {
                            collection = window.DataContext as NestedAttributeCollection;
                        }
                        else
                        {
                            collection = GetCollection(parent, value);
                        }

                    }
                    
                }
                else
                {
                    collection = window.DataContext as NestedAttributeCollection;
                }
                if (collection == null) return;
                SetItemData(value, which, collection);

            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Problem getting common variable infor for " + which, exception);
            }

        }

        private NestedAttributeCollection GetCollection(string which, SBString value)
        {
            if (_Section1.Contains(which))
            {
                return Section1Collection;
            }
            if (_Section2.Contains(which))
            {
                return Section2Collection;
            }
            if (_Section3.Contains(which))
            {
                return Section3Collection;
            }
            if (which.StartsWith("PARMS"))
            {
                return Parms;
            }
            return null;
        }

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

        internal static string BuildTitle(string parent, string newIndex)
        {
            string variable;
            if (String.IsNullOrEmpty(parent)) return newIndex;
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


        private readonly ObservableStack<string> _ProcessStack = new ObservableStack<string>();
        public ObservableStack<string> ProcessStack
        {
            get { return _ProcessStack; }
        }

        private readonly ObservableStack<string> _ProcessHistoryStack = new ObservableStack<string>();
        public ObservableStack<string> ProcessHistoryStack
        {
            get { return _ProcessHistoryStack; }
        }

        private NestedAttributeCollection _Record;
        public NestedAttributeCollection Record
        {
            get { return _Record; }
            set
            {
                if (_Record == value) return;
                _Record = value;
                RaisePropertyChanged("Record");
            }
        }

        private NestedAttributeCollection _Parms;
        public NestedAttributeCollection Parms
        {
            get { return _Parms; }
            set
            {
                if (_Parms == value) return;
                _Parms = value;
                RaisePropertyChanged("Parms");
            }
        }

        private NestedAttributeCollection _OrigRecord;
        public NestedAttributeCollection OrigRecord
        {
            get { return _OrigRecord; }
            set
            {
                if (_OrigRecord == value) return;
                _OrigRecord = value;
                RaisePropertyChanged("OrigRecord");
            }
        }


        private NestedAttributeCollection _Section1Collection;
        public NestedAttributeCollection Section1Collection
        {
            get { return _Section1Collection; }
            set
            {
                if (_Section1Collection == value) return;
                _Section1Collection = value;
                RaisePropertyChanged("Section1Collection");
            }
        }

        #region Section2Collection Property

        private NestedAttributeCollection _Section2Collection;

        /// <summary>
        /// Gets or sets the Section2Collection property. This property will raise a <see cref="ViewModel.PropertyChanged"/> event.
        /// </summary>
        public NestedAttributeCollection Section2Collection
        {
            get { return _Section2Collection; }
            set
            {
                if (_Section2Collection != value)
                {
                    _Section2Collection = value;
                    RaisePropertyChanged("Section2Collection");
                }
            }
        }

        #endregion Section2Collection Property

        #region Section3Collection Property

        private NestedAttributeCollection _Section3Collection;

        /// <summary>
        /// Gets or sets the Section3Collection property. This property will raise a <see cref="ViewModel.PropertyChanged"/> event.
        /// </summary>
        public NestedAttributeCollection Section3Collection
        {
            get { return _Section3Collection; }
            set
            {
                if (_Section3Collection != value)
                {
                    _Section3Collection = value;
                    RaisePropertyChanged("Section3Collection");
                }
            }
        }

        #endregion Section3Collection Property


        #region DebugConsoleWindowWidth Property

        private double _DebugConsoleWindowWidth = 600d;

        /// <summary>
        /// Gets or sets the DebugConsoleWindowWidth property. This property will raise a <see cref="ViewModel.PropertyChanged"/> event.
        /// </summary>
        public double DebugConsoleWindowWidth
        {
            get { return _DebugConsoleWindowWidth; }
            set
            {
                if (_DebugConsoleWindowWidth == value) return;
                _DebugConsoleWindowWidth = value;
                RaisePropertyChanged("DebugConsoleWindowWidth");
            }
        }

        #endregion DebugConsoleWindowWidth Property

        #region DebugConsoleWindowHeight Property

        private double _DebugConsoleWindowHeight = 600d;

        /// <summary>
        /// Gets or sets the DebugConsoleWindowHeight property. This property will raise a <see cref="ViewModel.PropertyChanged"/> event.
        /// </summary>
        public double DebugConsoleWindowHeight
        {
            get { return _DebugConsoleWindowHeight; }
            set
            {
                if (_DebugConsoleWindowHeight != value)
                {
                    _DebugConsoleWindowHeight = value;
                    RaisePropertyChanged("DebugConsoleWindowHeight");
                }
            }
        }

        #endregion DebugConsoleWindowHeight Property

        #region IsDebugEnabled Property

        private bool _IsDebugEnabled;

        /// <summary>
        /// Gets or sets the IsDebugEnabled property. This property will raise a <see cref="ViewModel.PropertyChanged"/> event.
        /// </summary>
        public bool IsDebugEnabled
        {
            get { return _IsDebugEnabled; }
            set
            {
                if (_IsDebugEnabled != value)
                {
                    _IsDebugEnabled = value;
                    RaisePropertyChanged("IsDebugEnabled");
                    SetIsDebugEnabled(IsDebugEnabled);
                }
            }
        }

        #endregion IsDebugEnabled Property

        #region IsSBXADebugWindowOpen Property

        private bool _IsSbxaDebugWindowOpen;

        /// <summary>
        /// Gets or sets the IsSBXADebugWindowOpen property. This property will raise a <see cref="ViewModel.PropertyChanged"/> event.
        /// </summary>
        public bool IsSBXADebugWindowOpen
        {
            get { return _IsSbxaDebugWindowOpen; }
            set
            {
                if (_IsSbxaDebugWindowOpen != value)
                {
                    _IsSbxaDebugWindowOpen = value;
                    DebugWindowManager.ShowDebugWindow(value);
                    RaisePropertyChanged("IsSBXADebugWindowOpen");
                }
            }
        }

        #endregion IsSBXADebugWindowOpen Property

    }

    public enum CommonVariables
    {
        // ReSharper disable InconsistentNaming
        RECORD,
        PARAM,
        KEY,
        VALUE,
        PARMS,
        ORIGREC,
        PROCNAME,
        WORK
        // ReSharper restore InconsistentNaming
    }
}
