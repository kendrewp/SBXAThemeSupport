// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessAnalysisViewModel.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.DebugAssistant.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.UI.Client;

    using SBXAThemeSupport.Models;
    using SBXAThemeSupport.ViewModels;

    using ICommand = System.Windows.Input.ICommand;

    /// <summary>
    ///     The source definition.
    /// </summary>
    public enum SourceDefinition
    {
        /// <summary>
        ///     The field.
        /// </summary>
        Field, 

        /// <summary>
        ///     The screen.
        /// </summary>
        Screen, 

        /// <summary>
        ///     The paragraph.
        /// </summary>
        Paragraph, 

        /// <summary>
        ///     The process.
        /// </summary>
        Process, 

        /// <summary>
        ///     The expression.
        /// </summary>
        Expression, 

        /// <summary>
        ///     The menu.
        /// </summary>
        Menu, 

        /// <summary>
        ///     The unknown.
        /// </summary>
        Unknown
    }

    /// <summary>
    ///     The view model to handle analysis of a definition.
    /// </summary>
    public class ProcessAnalysisViewModel : ViewModel
    {
        #region Static Fields

        private static readonly StringCollection ExcludeFileList = new StringCollection
                                                                       {
                                                                           "DM", 
                                                                           "DMUT", 
                                                                           "DMSH", 
                                                                           "DMGC", 
                                                                           "DMGD", 
                                                                           "TUBP", 
                                                                           "ASCPROGS"
                                                                       };

        #endregion

        #region Fields

        private readonly ProcessCollection processCollection = new ProcessCollection();

        private readonly ProcessStack processStack = new ProcessStack();

        private DefinitionDescription definition;

        private string globalMenuFile = string.Empty;

        private string globalProcessFile = string.Empty;

        private int isLoading;

        private string lastProcessReadError;

        private string processName;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessAnalysisViewModel" /> class.
        /// </summary>
        public ProcessAnalysisViewModel()
        {
            AnalyseProcessCommand = new RelayCommand(this.ExecutedAnalyseProcessCommand, this.CanExecuteAnalyseProcessCommand);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the analyse process command.
        /// </summary>
        public static ICommand AnalyseProcessCommand { get; private set; }

        /// <summary>
        ///     Gets or sets the definition stack.
        /// </summary>
        public DefinitionDescription Definition
        {
            get
            {
                return this.definition;
            }

            set
            {
                this.definition = value;
                this.RaisePropertyChanged("definition");
            }
        }

        /// <summary>
        ///     Gets or sets the is loading.
        /// </summary>
        public int IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.isLoading = value;
                this.RaisePropertyChanged("IsLoading");
            }
        }

        /// <summary>
        ///     Gets or sets the last definition read error.
        /// </summary>
        /// <value>
        ///     The last definition read error.
        /// </value>
        public string LastProcessReadError
        {
            get
            {
                return this.lastProcessReadError;
            }

            set
            {
                this.lastProcessReadError = value;
                this.RaisePropertyChanged("LastProcessReadError");
            }
        }

        /// <summary>
        ///     Gets or sets the name of the definition.
        /// </summary>
        /// <value>
        ///     The name of the definition.
        /// </value>
        public string ProcessName
        {
            get
            {
                return this.processName;
            }

            set
            {
                if (this.processName != null && this.processName.Equals(value))
                {
                    return;
                }

                this.processName = value;
                this.RaisePropertyChanged("ProcessName");
            }
        }

        /// <summary>
        ///     Gets the process stack.
        /// </summary>
        public ProcessStack ProcessStack
        {
            get
            {
                return this.processStack;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get process name.
        /// </summary>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="callType">
        /// The call type.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetProcessName(string expression, out string callType)
        {
            var pName = expression;
            // Having a [ witha message before is possible, so first strip that piece.
            if (pName.IndexOf("[", StringComparison.Ordinal) >= 0)
            {
                pName = pName.Substring(pName.IndexOf("[", StringComparison.Ordinal) + 1);
            }

            var colonPos = pName.IndexOf(":", StringComparison.Ordinal);
            callType = string.Empty;
            if (colonPos > 0)
            {
                callType = pName.Substring(0, 1);
                pName = pName.Substring(colonPos + 1);
            }

            var commaPos = pName.IndexOf(",", StringComparison.Ordinal);

            if (commaPos > 0)
            {
                // strip parameters
                pName = pName.Substring(0, commaPos);
            }

            return pName;
        }

        /// <summary>
        /// The is exclude file.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsExcludeFile(string fileName)
        {
            return ExcludeFileList.Contains(fileName);
        }

        /// <summary>
        /// The set is loading.
        /// </summary>
        /// <param name="val">
        /// The val.
        /// </param>
        public void SetIsLoading(int val)
        {
            JobManager.RunInDispatcherThread(
                DebugWindowManager.DebugConsoleWindow.Dispatcher, 
                DispatcherPriority.Normal, 
                () => this.IsLoading += val);
        }

        #endregion

        #region Methods

        internal void AddExpressionToCollection(DefinitionDescription parent, string hookPoint, string expression)
        {
            // standard SB expression elements and therefore no need to try read the process, just add the element
            var processDescription = new DefinitionDescription(string.Empty, expression, string.Empty) { Description = hookPoint };
            this.AddProcessToCollection(hookPoint, parent, processDescription);
        }

        internal void LoadBasicProgramFromExpression(string expression, DefinitionDescription parent = null, string hookPoint = "")
        {
            string callType;
            var programName = ProcessAnalysisViewModel.GetProcessName(expression, out callType);

            if (programName.IndexOf("(") > 0)
            {
                programName = programName.Substring(0, expression.IndexOf("(") - 1);
            }

            if (string.IsNullOrEmpty(programName))
            {
                return;
            }

            this.LoadBasicProgram(programName, parent, expression, hookPoint);
        }

        internal void LoadMenu(
            string menuName, 
            DefinitionDescription parent = null, 
            string expression = "", 
            string hookPoint = "", 
            string sysid = "")
        {
            if (SBExpression.IsStandardSBExpression(expression))
            {
                // standard SB expression elements and therefore no need to try read the process, just add the element
                this.AddExpressionToCollection(parent, hookPoint, expression);
                return;
            }

            if (string.IsNullOrEmpty(expression))
            {
                expression = menuName;
            }

            JobManager.RunInUIThread(
                DispatcherPriority.Input, 
                delegate
                    {
                        var menuFile = sysid;
                        if (string.IsNullOrEmpty(menuFile))
                        {
                            menuFile = SBPlusClient.Current.SystemId;
                        }

                        // read definition record from current xxProcess
                        menuFile += "MENUS";
                        // check if I already have the definition in the collection
                        lock (this.processCollection)
                        {
                            if (this.processCollection.ContainsKey(menuFile, menuName))
                            {
                                this.SetIsLoading(1);
                                this.AddProcessToCollection(menuName, parent, this.processCollection[menuName]);
                                this.SetIsLoading(-1);
                                return;
                            }
                        }

                        this.SetIsLoading(1);
                        SBFile.Read(
                            menuFile, 
                            menuName, 
                            this.ReadProcessCompleted, 
                            new ProcessLoadState { HookPoint = hookPoint, ParentDefinitionDescription = parent, Expression = expression });
                    });
        }

        /// <summary>
        /// Loads the definition from xxPROCESS.
        /// </summary>
        /// <param name="pName">
        /// Name of the definition.
        /// </param>
        /// <param name="parent">
        /// Parent <see cref="DefinitionDescription"/>, if this is null then it will be the root of the tree.
        /// </param>
        /// <param name="expression">
        /// The original expression that the definition name was derrived from.
        /// </param>
        /// <param name="hookPoint">
        /// The name of the definition slot where the definition was found.
        /// </param>
        /// <param name="sysid">
        /// The system id to use otherwise the current system id will be used.
        /// </param>
        internal void LoadProcess(
            string pName, 
            DefinitionDescription parent = null, 
            string expression = "", 
            string hookPoint = "", 
            string sysid = "")
        {
            if (SBExpression.IsStandardSBExpression(expression))
            {
                // standard SB expression elements and therefore no need to try read the process, just add the element
                this.AddExpressionToCollection(parent, hookPoint, expression);
                return;
            }

            if (string.IsNullOrEmpty(expression))
            {
                expression = pName;
            }

            JobManager.RunInUIThread(
                DispatcherPriority.Input, 
                delegate
                    {
                        var processFile = sysid;
                        if (string.IsNullOrEmpty(processFile))
                        {
                            processFile = SBPlusClient.Current.SystemId;
                        }

                        // read definition record from current xxProcess
                        processFile += "PROCESS";
                        // check if I already have the definition in the collection
                        lock (this.processCollection)
                        {
                            if (this.processCollection.ContainsKey(processFile, pName))
                            {
                                this.SetIsLoading(1);
                                this.AddProcessToCollection(pName, parent, this.processCollection[pName]);
                                this.SetIsLoading(-1);
                                return;
                            }
                        }

                        this.SetIsLoading(1);
                        SBFile.Read(
                            processFile, 
                            pName, 
                            this.ReadProcessCompleted, 
                            new ProcessLoadState { HookPoint = hookPoint, ParentDefinitionDescription = parent, Expression = expression });
                    });
        }

        /// <summary>
        /// Loads the definition from expression.
        /// </summary>
        /// <param name="source">
        /// Where the expression is being called from.
        /// </param>
        /// <param name="hookType">
        /// If this is a process hook point or expression hook point.
        /// </param>
        /// <param name="expression">
        /// The original expression that the definition name was derrived from.
        /// </param>
        /// <param name="parent">
        /// Parent <see cref="DefinitionDescription"/>, if this is null then it will be the root of the tree.
        /// </param>
        /// <param name="hookPoint">
        /// The name of the definition slot where the definition was found.
        /// </param>
        /// <param name="sysid">
        /// The system id to use otherwise the current system id will be used.
        /// </param>
        internal void LoadProcessFromExpression(
            SourceDefinition source, 
            SourceDefinition hookType, 
            string expression, 
            DefinitionDescription parent = null, 
            string hookPoint = "", 
            string sysid = "")
        {
            var colonPos = expression.IndexOf(":", StringComparison.Ordinal);
            // a field definition has no process hooks they are all expressions, etc. int help.
            if (source != SourceDefinition.Paragraph
                && (colonPos > 0 || source == SourceDefinition.Field || source == SourceDefinition.Expression))
            {
                this.AddExpressionToCollection(source, hookType, hookPoint, parent, expression);
                return;
            }

            string callType;
            var pName = GetProcessName(expression, out callType);
            if (!string.IsNullOrEmpty(pName))
            {
                switch (callType)
                {
                    case "C":
                        this.LoadProcess(pName, parent, expression, hookPoint, sysid);
                        break;
                    case "B":
                        this.LoadBasicProgramFromExpression(pName, parent, hookPoint);
                        break;
                    case "M":
                        break;
                    case "D":
                        this.AddExpressionToCollection(source, hookType, hookPoint, parent, expression);
                        break;
                    case "V":
                        this.AddExpressionToCollection(source, hookType, hookPoint, parent, expression);
                        break;
                    default:
                        this.LoadProcess(pName, parent, expression, hookPoint, sysid);
                        break;
                }
            }
        }

        internal void LoadScreen(string screenName, string fileName, DefinitionDescription parent = null, string hookPoint = "")
        {
            JobManager.RunInUIThread(
                DispatcherPriority.Input, 
                delegate
                    {
                        if (string.IsNullOrEmpty(screenName) || string.IsNullOrEmpty(fileName))
                        {
                            return;
                        }

                        this.SetIsLoading(1);
                        SBFile.ReadDictionaryItem(
                            fileName, 
                            screenName, 
                            new ScreenDefinitionLoadState
                                {
                                    FileName = fileName, 
                                    ParentDefinitionDescription = parent, 
                                    HookPoint = hookPoint
                                }, 
                            this.ReadProcessCompleted);
                    });
        }

        private static void AddProcessToMru(string process)
        {
            var mru = DebugViewModel.Instance.ApplicationInsightState.MruProcessList;
            if (mru.Contains(process))
            {
                mru.Remove(process); // remove it so that we can insert it at the top again.
            }

            mru.Insert(0, process);
            // force the UI to update.
            DebugViewModel.Instance.ApplicationInsightState.MruProcessList = new StringCollection();
            // Now remove all those with an index > 9
            while (mru.Count > 10)
            {
                mru.RemoveAt(10);
            }

            DebugViewModel.Instance.ApplicationInsightState.MruProcessList = mru;
            DebugViewModel.Instance.SaveState();
        }

        private static void RemoveProcessFromMru(string process)
        {
            var mru = DebugViewModel.Instance.ApplicationInsightState.MruProcessList;
            if (mru.Contains(process))
            {
                mru.Remove(process); // remove it so that we can insert it at the top again.
                // force the UI to update.
                DebugViewModel.Instance.ApplicationInsightState.MruProcessList = new StringCollection();
                DebugViewModel.Instance.ApplicationInsightState.MruProcessList = mru;
                DebugViewModel.Instance.SaveState();
            }
        }

        private void AddBasicProgramToCollection(DefinitionDescription basicProgramDescription, ProcessLoadState processLoadState = null)
        {
            if (DebugWindowManager.DebugConsoleWindow == null)
            {
                return;
            }

            if (basicProgramDescription == null)
            {
                CustomLogger.LogError(() => "A null basic program description was passed to AddBasicProgramToCollection.");
                return;
            }

            JobManager.RunInDispatcherThread(
                DebugWindowManager.DebugConsoleWindow.Dispatcher, 
                DispatcherPriority.Normal, 
                delegate
                    {
                        try
                        {
                            if (processLoadState == null)
                            {
                                CustomLogger.LogError(() => "A nullprocessLoadState was passed to AddBasicProgramToCollection.");
                                return;
                            }

                            if (processLoadState.ParentDefinitionDescription == null)
                            {
                                this.Definition = basicProgramDescription;
                                this.ProcessStack.Clear();
                                this.ProcessStack.Push(this.Definition);
                            }
                            else
                            {
                                this.AddItemToCollection(
                                    processLoadState.ParentDefinitionDescription.ProcessCollection, 
                                    new ProcessCall
                                        {
                                            Description = processLoadState.HookPoint, 
                                            ProcessDescription = basicProgramDescription
                                        });
                            }
                        }
                        catch (Exception ex)
                        {
                            CustomLogger.LogException(
                                ex, 
                                "There was a problem adding " + basicProgramDescription.Name + " to the collection.");
                        }
                    });
        }

        private void AddExpressionToCollection(
            SourceDefinition source, 
            SourceDefinition hookType, 
            string description, 
            DefinitionDescription processDescription, 
            string expression, 
            string sysid = "")
        {
            JobManager.RunInDispatcherThread(
                DebugWindowManager.DebugConsoleWindow.Dispatcher, 
                DispatcherPriority.Normal, 
                delegate
                    {
                        try
                        {
                            switch (source)
                            {
                                case SourceDefinition.Field:
                                    this.AddItemToCollection(
                                        processDescription.DictionaryExpressions, 
                                        new SBExpression(processDescription.FileName, expression, hookType, sysid)
                                            {
                                                Description =
                                                    description
                                            });
                                    break;
                                case SourceDefinition.Screen:
                                    /*
                                    if (hookType == SourceDefinition.Process)
                                    {
                                        //AddItemToCollection(processDescription.ProcessCollection, );
                                        this.AddProcessToCollection(
                                            description, 
                                            processDescription, 
                                            new SBExpression(processDescription.FileName, expression, hookType, sysid, expression)
                                                {
                                                    Description
                                                        =
                                                        description
                                                });
                                    }
                                    else
                                    {
*/
                                    this.AddItemToCollection(
                                        processDescription.ScreenExpressions, 
                                        new SBExpression(processDescription.FileName, expression, hookType, sysid)
                                            {
                                                Description =
                                                    description
                                            });
                                    /*
                                    }
*/
                                    break;
                                case SourceDefinition.Menu:
                                    this.AddProcessToCollection(
                                        description, 
                                        processDescription, 
                                        new SBExpression(processDescription.FileName, expression, hookType, sysid, expression)
                                            {
                                                Description
                                                    =
                                                    description
                                            });
                                    break;
                                case SourceDefinition.Paragraph:
                                    this.AddItemToCollection(
                                        processDescription.Expressions, 
                                        new SBExpression(processDescription.FileName, expression, hookType, sysid)
                                            {
                                                Description =
                                                    description
                                            });
                                    break;
                                case SourceDefinition.Unknown:
                                    this.AddItemToCollection(
                                        processDescription.Expressions, 
                                        new SBExpression(processDescription.FileName, expression, hookType, sysid)
                                            {
                                                Description =
                                                    description
                                            });
                                    break;
                            }
                        }
                        catch (Exception exception)
                        {
                            CustomLogger.LogException(exception, "Problem adding expression to the collection.");
                        }
                    });
        }

        private void AddItemToCollection(ObservableCollection<ProcessCall> collection, ProcessCall processCall)
        {
            try
            {
                lock (collection)
                {
                    // first check to see if the collection contains the item
                    foreach (var item in collection)
                    {
                        if (string.IsNullOrEmpty(processCall.Name))
                        {
                            if (item.ProcessDescription != null && processCall.ProcessDescription != null
                                && item.ProcessDescription.FileName.Equals(processCall.ProcessDescription.FileName)
                                && item.ProcessDescription.Name.Equals(processCall.ProcessDescription.Name))
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (item.Name.Equals(processCall.Name))
                            {
                                // do not add the duplicate to the collection.
                                return;
                            }
                        }
                    }

                    collection.Add(processCall);
                }
            }
            catch (Exception exception)
            {
                CustomLogger.LogException(exception, "A problem while adding an item to the collection.");
            }
        }

        private void AddItemToCollection(ObservableCollection<SBExpression> collection, SBExpression sbExpression)
        {
            lock (collection)
            {
                collection.Add(sbExpression);
            }
        }

        private void AddProcessToCollection(
            string processFileName, 
            string pName, 
            SBString defn, 
            ProcessLoadState processLoadState = null, 
            bool isError = false)
        {
            if (DebugWindowManager.DebugConsoleWindow == null)
            {
                return;
            }

            JobManager.RunInDispatcherThread(
                DebugWindowManager.DebugConsoleWindow.Dispatcher, 
                DispatcherPriority.Normal, 
                delegate
                    {
                        try
                        {
                            if (processLoadState == null)
                            {
                                return;
                            }

                            // top of tree so just assign it.
                            var processDescription = this.CreateProcessDescription(processFileName, pName, defn, processLoadState);
                            processDescription.IsError = isError;

                            // Add the definition to the total collection if it is not already there. It should never be there already as
                            // we should have bypassed the load if it is.
                            lock (this.processCollection)
                            {
                                if (!this.processCollection.ContainsKey(processFileName, pName, processDescription.GetType()))
                                {
                                    this.processCollection.Add(processDescription);
                                }
                            }

                            this.AddProcessToCollection(
                                processLoadState.HookPoint, 
                                processLoadState.ParentDefinitionDescription, 
                                processDescription);
                        }
                        catch (Exception ex)
                        {
                            CustomLogger.LogException(
                                ex, 
                                string.Format("There was a problem when adding {0} {1} to the collection.", processFileName, pName));
                        }
                    });
        }

        private void AddProcessToCollection(
            string description, 
            DefinitionDescription parentDefinitionDescription, 
            DefinitionDescription processDescription)
        {
            JobManager.RunInDispatcherThread(
                DebugWindowManager.DebugConsoleWindow.Dispatcher, 
                DispatcherPriority.Normal, 
                delegate
                    {
                        if (parentDefinitionDescription == null)
                        {
                            this.Definition = processDescription;
                            this.processStack.Dispose();
                            this.ProcessStack.Clear();
                            this.processCollection.Dispose();
                            this.processCollection.Clear();
                            this.ProcessStack.Push(this.Definition);
                        }
                        else
                        {
                            this.AddItemToCollection(
                                parentDefinitionDescription.ProcessCollection, 
                                new ProcessCall { Description = description, ProcessDescription = processDescription });
                        }
                    });
        }

        private bool CanExecuteAnalyseProcessCommand(object parameter)
        {
            return !string.IsNullOrEmpty(this.ProcessName) && !string.IsNullOrWhiteSpace(this.ProcessName);
        }

        private ParagraphDescription CreateParagraphDescription(string processFileName, string pName, string expression, SBString defn)
        {
            var processDescription = new ParagraphDescription(processFileName, pName, expression, defn);

            // Now parse out the paragraph to find definition and basic program calls
            var paragraphExpression = defn.Extract(3).Value;

            var elements = paragraphExpression.Split(GenericConstants.CHAR_ARRAY_SEMI_COLON);
            // C0;AP4;P4;C1;=;I2;EP.BRANCH,INT.CALL;J5;P4;C2;=;I1;BBASIC.PROGRAM
            foreach (var element in elements)
            {
                if (string.IsNullOrEmpty(element.Trim()))
                {
                    continue;
                }

                switch (element.Substring(0, 1))
                {
                    case "E":
                        this.LoadProcessFromExpression(
                            SourceDefinition.Paragraph, 
                            SourceDefinition.Process, 
                            element.Substring(1), 
                            processDescription, 
                            "Execute");
                        break;
                    case "B":
                        this.LoadBasicProgramFromExpression(element.Substring(1), processDescription, "Call");
                        break;
                }
            }

            return processDescription;
        }

        /// <summary>
        /// This method will parse out the definition and figure out the type of definition.
        /// </summary>
        /// <param name="processFileName">
        /// The name of the file that contains the process.
        /// </param>
        /// <param name="pName">
        /// The name of the definition.
        /// </param>
        /// <param name="defn">
        /// The definition description.
        /// </param>
        /// <param name="processLoadState">
        /// The information about the definition definition passed from the calling routine.
        /// </param>
        /// <returns>
        /// The <see cref="DefinitionDescription"/>.
        /// </returns>
        private DefinitionDescription CreateProcessDescription(
            string processFileName, 
            string pName, 
            SBString defn, 
            ProcessLoadState processLoadState)
        {
            DefinitionDescription processDescription;
            if (defn == null)
            {
                processDescription = new DefinitionDescription(processFileName, pName, processLoadState.Expression) { IsError = true };
                return processDescription;
            }

            switch (defn.Extract(1).Value)
            {
                case "I":
                    processDescription = new InputDefinitionDescription(processFileName, pName, processLoadState.Expression, defn);
                    break;
                case "O":
                    processDescription = new OutputDefinitionDescription(processFileName, pName, processLoadState.Expression, defn);
                    break;
                case "P":
                    processDescription = this.CreateParagraphDescription(processFileName, pName, processLoadState.Expression, defn);
                    break;
                case "SCREEN":
                    processDescription = new ScreenDefintion(processFileName, pName, string.Empty, defn);
                    break;
                case "F":
                    processDescription = new FileUpdateDefinitionDescription(processFileName, pName, string.Empty, defn);
                    break;
                case "M":
                    processDescription = new MenuDefinitionDescription(processFileName, pName, string.Empty, defn);
                    break;
                case "S":
                    processDescription = new SelectionProcessDescription(processFileName, pName, defn);
                    break;
                default:
                    processDescription = new DefinitionDescription(processFileName, pName, processLoadState.Expression);
                    break;
            }

            return processDescription;
        }

        private void ExecutedAnalyseProcessCommand(object parameter)
        {
            this.ReadControlRecord(); // Make sure I have the latest control record.
            this.IsLoading = 0;
            var pName = DebugViewModel.Instance.ProcessAnalysisViewModel.ProcessName;
            // Add process to MRU list, but it it is not found it will be removed.
            AddProcessToMru(DebugViewModel.Instance.ProcessAnalysisViewModel.ProcessName);
            DebugViewModel.Instance.ProcessAnalysisViewModel.ProcessName = pName;
            this.LoadProcess(DebugViewModel.Instance.ProcessAnalysisViewModel.ProcessName);
        }

        private void FindCallsInBasicProgram(IEnumerable<SBString> basicProgram, BasicProgramDescription basicProgramDescription)
        {
            foreach (var line in basicProgram)
            {
                if (line.Dcount() == 1 && !string.IsNullOrEmpty(line.Value))
                {
                    if (line.Value.IndexOf("CALL") >= 0)
                    {
                        // have CALL statement
                        //CALL PROGRAM(
                        var code = line.Value.Trim();
                        var parts = code.Split("(".ToCharArray());
                        int start = parts[0].IndexOf("CALL") + 5;
                        var progName = parts[0].Substring(start);
                        this.LoadBasicProgram(progName, basicProgramDescription, line.Value, "CALL");
                    }
                }
            }

            basicProgramDescription.Parsed = true;
        }

        private void LoadBasicProgram(
            string programName, 
            DefinitionDescription parent = null, 
            string expression = "", 
            string hookPoint = "")
        {
            JobManager.RunInUIThread(
                DispatcherPriority.Input, 
                delegate
                    {
                        // check if I already have the definition in the collection
                        lock (this.processCollection)
                        {
                            if (this.processCollection.ContainsKey(string.Empty, programName, typeof(BasicProgramDescription)))
                            {
                                Debug.WriteLine("[ProcessAnalysisViewModel.LoadBasicProgram(320)] Loaded " + programName + " from cache");
                                this.AddBasicProgramToCollection(
                                    this.processCollection[programName] as BasicProgramDescription, 
                                    new ProcessLoadState
                                        {
                                            Expression = expression, 
                                            HookPoint = hookPoint, 
                                            ParentDefinitionDescription = parent
                                        });
                                return;
                            }

                            // not in cache so add it.
                            Debug.WriteLine("[ProcessAnalysisViewModel.LoadBasicProgram(328)] Added " + programName + " to cache.");
                            var basicProgramDescription = new BasicProgramDescription(string.Empty, programName);
                            this.processCollection.Add(basicProgramDescription);
                        }

                        this.SetIsLoading(1);
                        SBFile.Read(
                            "VOC", 
                            programName, 
                            this.ReadBasicProgramVocPointerCompleted, 
                            new ProcessLoadState { HookPoint = hookPoint, ParentDefinitionDescription = parent, Expression = expression });
                    });
        }

        private void ReadBasicProgramCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            this.SetIsLoading(-1);

            if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0"))
            {
                // Error
                this.LastProcessReadError = string.Format("Failed to read {0} from {1}.", parameters[1].Value, parameters[0].Value);
                return;
            }

            var basicProgramDescription = ((object[])userState)[0] as BasicProgramDescription;
            if (basicProgramDescription == null || IsExcludeFile(basicProgramDescription.FileName))
            {
                return;
            }

            this.FindCallsInBasicProgram(parameters[3], basicProgramDescription);
        }

        private void ReadBasicProgramVocPointerCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {
                this.SetIsLoading(-1);
                var processState = userState as ProcessLoadState;
                if (processState == null)
                {
                    return;
                }

                BasicProgramDescription basicProgramDescription = null;
                lock (this.processCollection)
                {
                    // it should be in the cache so retrieve it.
                    if (this.processCollection.ContainsKey(string.Empty, parameters[1].Value, typeof(BasicProgramDescription)))
                    {
                        basicProgramDescription = this.processCollection[parameters[1].Value] as BasicProgramDescription;
                        Debug.WriteLine(
                            "[ProcessAnalysisViewModel.ReadBasicProgramVocPointerCompleted(355)] Read " + parameters[1].Value
                            + " from cache.");
                    }

                    if (basicProgramDescription == null)
                    {
                        basicProgramDescription = new BasicProgramDescription(string.Empty, parameters[1].Value);
                        this.processCollection.Add(basicProgramDescription);
                        Debug.WriteLine(
                            "[ProcessAnalysisViewModel.ReadBasicProgramVocPointerCompleted(363)] Added " + basicProgramDescription.Name
                            + " to cache.");
                    }
                }

                if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0"))
                {
                    // Error
                    this.LastProcessReadError = string.Format("Failed to read {0} from {1}.", parameters[1].Value, parameters[0].Value);
                    basicProgramDescription.IsError = true;
                    return;
                }

                this.AddBasicProgramToCollection(
                    basicProgramDescription, 
                    new ProcessLoadState
                        {
                            Expression = processState.Expression, 
                            HookPoint = processState.HookPoint, 
                            ParentDefinitionDescription = processState.ParentDefinitionDescription
                        });

                /*
                 * UniVerse VOC pointer
                 *  V
                    C:\U2\SBXA3427\APPLICATIONSERVER\UNIVERSE\SBDEMO\CHPROGS.O/B.BRANCH
                    B
                    BNP

                    PICK.FORMAT
                    SýNýPýIýAýEýH
                    NO.WARNýNOPAGEýLPTRýKEEP.COMMONýýTRAPýHDR-SUPP
                    C:\U2\SBXA3427\APPLICATIONSERVER\UNIVERSE\SBDEMO\CHPROGS.O

                 * UniData VOC Pointer
                 * C
                 * E:\U2\SBXA\APPLICATIONSERVER\UNIDATA\SB.DEFN\DM\_SB.LOGIN
                 */
                if (!basicProgramDescription.Parsed && !basicProgramDescription.IsError)
                {
                    if (parameters[3].Extract(1).Value.Equals("V"))
                    {
                        // UniVerse
                        basicProgramDescription.ObjectFileLocation = parameters[3].Extract(9).Value;
                        basicProgramDescription.ObjectLocation = parameters[3].Extract(2).Value;
                        if (!string.IsNullOrEmpty(basicProgramDescription.ObjectFileLocation))
                        {
                            // figure out the name of the file
                            var fileName = Path.GetFileName(basicProgramDescription.ObjectFileLocation);
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                if (fileName.EndsWith(".O"))
                                {
                                    basicProgramDescription.FileName = fileName.Substring(0, fileName.Length - 2);
                                }
                            }
                        }
                    }
                    else
                    {
                        // UniData, on windows.
                        var parts = parameters[3].Extract(2).Value.Split(@"\".ToCharArray());
                        basicProgramDescription.FileName = parts[parts.Length - 2];
                    }

                    if (!string.IsNullOrEmpty(basicProgramDescription.FileName))
                    {
                        // no need to go any further if I am excluding this file.
                        if (!IsExcludeFile(basicProgramDescription.FileName))
                        {
                            // if I have a file name try open it and see if I can read the code.
                            JobManager.RunInUIThread(
                                DispatcherPriority.Input, 
                                delegate
                                    {
                                        this.SetIsLoading(1);
                                        SBFile.Read(
                                            basicProgramDescription.FileName, 
                                            basicProgramDescription.Name, 
                                            this.ReadBasicProgramCompleted, 
                                            new object[] { basicProgramDescription });
                                    });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.LastProcessReadError = "Exception caught after reading definition definition. " + ex.Message;
            }
        }

        private void ReadControlRecord()
        {
            // 6.2
            JobManager.RunInUIThread(
                DispatcherPriority.Normal, 
                delegate
                    {
                        var controlFileName = SBPlusClient.Current.SystemId + "CONTROL";
                        this.SetIsLoading(1);
                        SBFile.Read(controlFileName, "PARAMS", this.ReadControlRecordCompleted);
                    });
        }

        private void ReadControlRecordCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            this.SetIsLoading(-1);

            if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0"))
            {
                // Error
                this.LastProcessReadError = string.Format("Failed to read {0} from {1}.", parameters[1].Value, parameters[0].Value);
                return;
            }

            var record = parameters[3];
            this.globalProcessFile = record.Extract(6, 2).Value;
            this.globalMenuFile = record.Extract(6, 3).Value;
        }

        private void ReadProcessCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {
                this.SetIsLoading(-1);
                var processState = userState as ProcessLoadState;
                if (processState == null)
                {
                    return;
                }

                var processFileName = parameters[0].Value;
                var pName = parameters[1].Value;
                if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0"))
                {
                    // Failed to read the process.
                    if (parameters[0].Value.EndsWith("MENUS"))
                    {
                        // Failed, but now I need to check if there is a global process file and if so try read it from there.
                        if (!string.IsNullOrEmpty(this.globalMenuFile) && !parameters[0].Value.Equals(this.globalMenuFile))
                        {
                            this.SetIsLoading(1);
                            SBFile.Read(this.globalMenuFile, pName, this.ReadProcessCompleted, processState);
                        }
                        else
                        {
                            // Error
                            this.LastProcessReadError = string.Format(
                                "Failed to read {0} from {1}.", 
                                parameters[1].Value, 
                                parameters[0].Value);
                            this.AddProcessToCollection(processFileName, pName, null, processState, true);
                        }
                    }
                    else
                    {
                        // Failed, but now I need to check if there is a global process file and if so try read it from there.
                        if (!string.IsNullOrEmpty(this.globalProcessFile) && !parameters[0].Value.Equals(this.globalProcessFile))
                        {
                            this.SetIsLoading(1);
                            SBFile.Read(this.globalProcessFile, pName, this.ReadProcessCompleted, processState);
                        }
                        else
                        {
                            // Error
                            this.LastProcessReadError = string.Format(
                                "Failed to read {0} from {1}.", 
                                parameters[1].Value, 
                                parameters[0].Value);
                            this.AddProcessToCollection(processFileName, pName, null, processState, true);
                            // Now remove it from the MRU is it is there and display a message.
                            RemoveProcessFromMru(pName);
                            MessageBox.Show(
                                string.Format(
                                    "Failed to read process '{0}', referenced by '{1}' in the file '{2}'.", 
                                    pName, 
                                    !string.IsNullOrEmpty(processState.ParentDefinitionDescription.Name)
                                        ? processState.ParentDefinitionDescription.Name
                                        : string.Empty, 
                                    !string.IsNullOrEmpty(processState.ParentDefinitionDescription.FileName)
                                        ? processState.ParentDefinitionDescription.FileName
                                        : string.Empty));
                        }
                    }

                    return;
                }

                this.AddProcessToCollection(processFileName, pName, parameters[3], processState);
            }
            catch (Exception ex)
            {
                this.LastProcessReadError = "Exception caught after reading definition definition. " + ex.Message;
            }
        }

        #endregion

        internal class ProcessLoadState
        {
            #region Properties

            internal string Expression { get; set; }

            internal string HookPoint { get; set; }

            internal DefinitionDescription ParentDefinitionDescription { get; set; }

            #endregion
        }

        internal class ScreenDefinitionLoadState : ProcessLoadState
        {
            #region Properties

            internal string FileName { get; set; }

            #endregion
        }
    }
}