
namespace SBXAThemeSupport.DebugAssistant.ViewModels
{
    using System.Windows.Threading;
    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.UI.Client;
    using SBXAThemeSupport.ViewModels;
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using SBXAThemeSupport.Models;


    using ICommand = System.Windows.Input.ICommand;

    /// <summary>
    /// The view model to handle analysis of a definition.
    /// </summary>
    public class ProcessAnalysisViewModel : ViewModel
    {
        private DefinitionDescription definition;
        private string processName;
        private string lastProcessReadError;

        private string globalProcessFile = string.Empty;

        private string globalMenuFile = string.Empty;

        private readonly ProcessStack processStack = new ProcessStack();

        private readonly ProcessCollection processCollection = new ProcessCollection();

        private int isLoading;


        public ProcessAnalysisViewModel()
        {
            AnalyseProcessCommand = new RelayCommand(AnalyseProcessCommandExecuted, CanExecuteAnalyseProcessCommand);

        }
        
        public static ICommand AnalyseProcessCommand {get; private set; }

        public int IsLoading
        {
            get { return this.isLoading; }
            set
            {
                this.isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }

        public void SetIsLoading(int val)
        {
            JobManager.RunInDispatcherThread(DebugWindowManager.DebugConsoleWindow.Dispatcher, DispatcherPriority.Normal, () => IsLoading += val);
        }

        /// <summary>
        ///     Gets the definition stack.
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
                RaisePropertyChanged("definition");
            }
        }

        /// <summary>
        /// Gets or sets the name of the definition.
        /// </summary>
        /// <value>
        /// The name of the definition.
        /// </value>
        public string ProcessName
        {
            get { return this.processName; }
            set
            {
                if (this.processName != null && this.processName.Equals(value))
                {
                    return;
                }
                this.processName = value;
                RaisePropertyChanged("ProcessName");

            }
        }

        /// <summary>
        /// Gets or sets the last definition read error.
        /// </summary>
        /// <value>
        /// The last definition read error.
        /// </value>
        public string LastProcessReadError
        {
            get { return this.lastProcessReadError; }
            set
            {
                this.lastProcessReadError = value;
                RaisePropertyChanged("LastProcessReadError");

            }
        }

        public ProcessStack ProcessStack
        {
            get { return processStack; }
        }

        /// <summary>
        /// Loads the definition from expression.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="hookType">If this is a process hook point or expression hook point.</param>
        /// <param name="expression">The original expression that the definition name was derrived from.</param>
        /// <param name="hookPoint">The name of the definition slot where the definition was found.</param>
        /// <param name="sysid">The system id to use otherwise the current system id will be used.</param>
        /// <param name="parent">Parent <see cref="DefinitionDescription"/>, if this is null then it will be the root of the tree.</param>
        internal void LoadProcessFromExpression(SourceDefinition source, SourceDefinition hookType, string expression, DefinitionDescription parent = null, string hookPoint = "", string sysid = "")
        {
            var colonPos = expression.IndexOf(":", StringComparison.Ordinal);
            // a field definition has no process hooks they are all expressions, etc. int help.
            if (source != SourceDefinition.Paragraph && (colonPos > 0 || source == SourceDefinition.Field || source == SourceDefinition.Expression))
            {
                AddExpressionToCollection(source, hookType, hookPoint, parent, expression);
                return;
            }
            string callType;
            var pName = GetProcessName(expression, out callType);
            if (!string.IsNullOrEmpty(pName))
            {
                switch (callType)
                {
                    case "C":
                        LoadProcess(pName, parent, expression, hookPoint, sysid);
                        break;
                    case "B":
                        LoadBasicProgramFromExpression(pName, parent, hookPoint);
                        break;
                    case "M":
                        break;
                    default:
                        LoadProcess(pName, parent, expression, hookPoint, sysid);
                        break;
                }
            }
        }

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

            return (pName);
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

            LoadBasicProgram(programName, parent, expression, hookPoint);
        }

        internal void AddExpressionToCollection(DefinitionDescription parent, string hookPoint, string expression)
        {
            // standard SB expression elements and therefore no need to try read the process, just add the element
            var processDescription = new DefinitionDescription(string.Empty, expression, string.Empty) { Description = hookPoint };
            AddProcessToCollection(hookPoint, parent, processDescription);
        }

        internal void LoadMenu(string menuName, DefinitionDescription parent = null, string expression = "", string hookPoint = "", string sysid = "")
        {
            if (SBExpression.IsStandardSBExpression(expression))
            {
                // standard SB expression elements and therefore no need to try read the process, just add the element
                AddExpressionToCollection(parent, hookPoint, expression);
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
                            SetIsLoading(1);
                            AddProcessToCollection(menuName, parent, this.processCollection[menuName]);
                            SetIsLoading(-1);
                            return;
                        }
                    }


                    SetIsLoading(1);
                    SBFile.Read(menuFile, menuName, this.ReadProcessCompleted, new ProcessLoadState { HookPoint = hookPoint, ParentDefinitionDescription = parent, Expression = expression });

                });

        }
        /// <summary>
        /// Loads the definition from xxPROCESS.
        /// </summary>
        /// <param name="pName">Name of the definition.</param>
        /// <param name="expression">The original expression that the definition name was derrived from.</param>
        /// <param name="hookPoint">The name of the definition slot where the definition was found.</param>
        /// <param name="sysid">The system id to use otherwise the current system id will be used.</param>
        /// <param name="parent">Parent <see cref="DefinitionDescription"/>, if this is null then it will be the root of the tree.</param>
        internal void LoadProcess(string pName, DefinitionDescription parent = null, string expression = "", string hookPoint = "", string sysid = "")
        {
            if (SBExpression.IsStandardSBExpression(expression))
            {
                // standard SB expression elements and therefore no need to try read the process, just add the element
                AddExpressionToCollection(parent, hookPoint, expression);
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
                            SetIsLoading(1);
                            AddProcessToCollection(pName, parent, this.processCollection[pName]);
                            SetIsLoading(-1);
                            return;
                        }
                    }


                    SetIsLoading(1);
                    SBFile.Read(processFile, pName, this.ReadProcessCompleted, new ProcessLoadState { HookPoint = hookPoint, ParentDefinitionDescription = parent, Expression = expression });

                });
        }

        private void LoadBasicProgram(string programName, DefinitionDescription parent = null, string expression = "", string hookPoint = "")
        {

            JobManager.RunInUIThread(DispatcherPriority.Input,
                    delegate
                    {
                        // check if I already have the definition in the collection
                        lock (this.processCollection)
                        {
                            if (this.processCollection.ContainsKey(string.Empty, programName, typeof(BasicProgramDescription)))
                            {
                                SetIsLoading(1);
                                AddBasicProgramToCollection(this.processCollection[programName] as BasicProgramDescription, new ProcessLoadState { Expression = expression, HookPoint = hookPoint, ParentDefinitionDescription = parent });
                                SetIsLoading(-1);
                                return;
                            }
                        }

                        SetIsLoading(1);
                        SBFile.Read("VOC", programName, this.ReadBasicProgramVocPointerCompleted, new ProcessLoadState { HookPoint = hookPoint, ParentDefinitionDescription = parent, Expression = expression });
                    });

        }

        private void ReadBasicProgramVocPointerCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {
                SetIsLoading(-1);
                var processState = userState as ProcessLoadState;
                if (processState == null)
                {
                    return;
                }

                var basicProgramDescription = new BasicProgramDescription(string.Empty, parameters[1].Value);

                AddBasicProgramToCollection(basicProgramDescription, new ProcessLoadState { Expression = processState.Expression, HookPoint = processState.HookPoint, ParentDefinitionDescription = processState.ParentDefinitionDescription });
                if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0"))
                {
                    // Error
                    LastProcessReadError = string.Format("Failed to read {0} from {1}.", parameters[1].Value, parameters[0].Value);
                    return;
                }

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
                if (!basicProgramDescription.Parsed)
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
                        // if I have a file name try open it and see if I can read the code.
                        JobManager.RunInUIThread(DispatcherPriority.Input,
                            delegate
                            {
                                SetIsLoading(1);
                                SBFile.Read(basicProgramDescription.FileName, basicProgramDescription.Name, this.ReadBasicProgramCompleted, new object[] { basicProgramDescription });

                            });
                    }
                }

            }
            catch (Exception ex)
            {
                LastProcessReadError = "Exception caught after reading definition definition. " + ex.Message;
            }
        }

        private void ReadBasicProgramCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            SetIsLoading(-1);

            if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0"))
            {
                // Error
                LastProcessReadError = string.Format("Failed to read {0} from {1}.", parameters[1].Value, parameters[0].Value);
                return;
            }
            var basicProgramDescription = ((object[])userState)[0] as BasicProgramDescription;
            this.FindCallsInBasicProgram(parameters[3], basicProgramDescription);
        }

        private void FindCallsInBasicProgram(SBString basicProgram, BasicProgramDescription basicProgramDescription)
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

        internal void LoadScreen(string screenName, string fileName, DefinitionDescription parent = null, string hookPoint = "" )
        {
            JobManager.RunInUIThread(
                DispatcherPriority.Input,
                    delegate
                    {
                        if (string.IsNullOrEmpty(screenName) || string.IsNullOrEmpty(fileName))
                        {

                            return;
                        }

                        SetIsLoading(1);
                        SBFile.ReadDictionaryItem(fileName, screenName, new ScreenDefinitionLoadState { FileName = fileName, ParentDefinitionDescription = parent, HookPoint = hookPoint }, this.ReadProcessCompleted);
                    });

        }

        private void ReadProcessCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {
                SetIsLoading(-1);
                var processState = userState as ProcessLoadState;
                if (processState == null)
                {
                    return;
                }
                var processFileName = parameters[0].Value;
                var pName = parameters[1].Value;
                if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0"))
                {
                    if (parameters[0].Value.EndsWith("MENUS"))
                    {
                    // Failed, but now I need to check if there is a global process file and if so try read it from there.
                        if (!string.IsNullOrEmpty(globalMenuFile) && !parameters[0].Value.Equals(globalMenuFile))
                        {
                            SetIsLoading(1);
                            SBFile.Read(globalMenuFile, pName, this.ReadProcessCompleted, processState);
                        }
                        else
                        {
                            // Error
                            LastProcessReadError = string.Format("Failed to read {0} from {1}.", parameters[1].Value, parameters[0].Value);
                            AddProcessToCollection(processFileName, pName, null, processState);
                        }
                    }
                    else
                    {
                        // Failed, but now I need to check if there is a global process file and if so try read it from there.
                        if (!string.IsNullOrEmpty(globalProcessFile) && !parameters[0].Value.Equals(globalProcessFile))
                        {
                            SetIsLoading(1);
                            SBFile.Read(globalProcessFile, pName, this.ReadProcessCompleted, processState);
                        }
                        else
                        {
                            // Error
                            LastProcessReadError = string.Format("Failed to read {0} from {1}.", parameters[1].Value, parameters[0].Value);
                            AddProcessToCollection(processFileName, pName, null, processState, true);
                        }
                    }
                    return;
                }
                AddProcessToCollection(processFileName, pName, parameters[3], processState);
            }
            catch (Exception ex)
            {
                LastProcessReadError = "Exception caught after reading definition definition. " + ex.Message;
            }
        }

        private void AddProcessToCollection(string processFileName, string pName, SBString defn, ProcessLoadState processLoadState = null, bool isError = false)
        {
            if (DebugWindowManager.DebugConsoleWindow == null)
            {
                return;
            }

            JobManager.RunInDispatcherThread(DebugWindowManager.DebugConsoleWindow.Dispatcher, DispatcherPriority.Normal,
                delegate
                {
                    try
                    {

                        if (processLoadState == null)
                        {
                            return;
                        }

                        // top of tree so just assign it.
                        var processDescription = CreateProcessDescription(processFileName, pName, defn, processLoadState);
                        processDescription.IsError = isError;
                        if (processDescription == null)
                        {
                            return;
                        }
                        // Add the definition to the total collection if it is not already there. It should never be there already as
                        // we should have bypassed the load if it is.
                        lock (this.processCollection)
                        {
                            if (!this.processCollection.ContainsKey(processFileName, pName, processDescription.GetType()))
                            {
                                this.processCollection.Add(processDescription);
                            }
                        }
                        AddProcessToCollection(processLoadState.HookPoint, processLoadState.ParentDefinitionDescription, processDescription);
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.LogException(ex, string.Format("There was a problem when adding {0} {1} to the collection.", processFileName, pName));
                    }
                });
        }

        private void AddProcessToCollection(string description, DefinitionDescription parentDefinitionDescription, DefinitionDescription processDescription)
        {
            JobManager.RunInDispatcherThread(DebugWindowManager.DebugConsoleWindow.Dispatcher, DispatcherPriority.Normal,
                delegate
                {
                    if (parentDefinitionDescription == null)
                    {
                        this.Definition = processDescription;
                        processStack.Dispose();
                        ProcessStack.Clear();
                        this.processCollection.Dispose();
                        this.processCollection.Clear();
                        ProcessStack.Push(this.Definition);
                    }
                    else
                    {
                        AddItemToCollection(parentDefinitionDescription.ProcessCollection, new ProcessCall { Description = description, ProcessDescription = processDescription });
                    }
                });
        }

        private void AddExpressionToCollection(SourceDefinition source, SourceDefinition hookType, string description, DefinitionDescription processDescription, string expression, string sysid = "")
        {
            JobManager.RunInDispatcherThread(DebugWindowManager.DebugConsoleWindow.Dispatcher, DispatcherPriority.Normal,
                delegate
                    {
                        try
                        {
                            switch (source)
                            {
                                case (SourceDefinition.Field):
                                    AddItemToCollection(processDescription.DictionaryExpressions, new SBExpression(processDescription.FileName, expression, hookType, sysid) { Description = description });
                                    break;
                                case (SourceDefinition.Screen):
                                    if (hookType == SourceDefinition.Process)
                                    {
                                        //AddItemToCollection(processDescription.ProcessCollection, );
                                        AddProcessToCollection(description, processDescription, new SBExpression(processDescription.FileName, expression, hookType, sysid, expression) { Description = description});
                                    }
                                    else
                                    {
                                        AddItemToCollection(processDescription.ScreenExpressions, new SBExpression(processDescription.FileName, expression, hookType, sysid) { Description = description });
                                    }
                                    break;
                                case (SourceDefinition.Menu):
                                    AddProcessToCollection(description, processDescription, new SBExpression(processDescription.FileName, expression, hookType, sysid, expression) { Description = description });
                                    break;
                                case (SourceDefinition.Paragraph):
                                    AddItemToCollection(processDescription.Expressions, new SBExpression(processDescription.FileName, expression, hookType, sysid) { Description = description });
                                    break;
                                case (SourceDefinition.Unknown):
                                    AddItemToCollection(processDescription.Expressions, new SBExpression(processDescription.FileName, expression, hookType, sysid) { Description = description });
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
            lock (collection)
            {
                collection.Add(processCall);
            }
        }

        private void AddItemToCollection(ObservableCollection<SBExpression> collection, SBExpression sbExpression)
        {
            lock (collection)
            {
                collection.Add(sbExpression);
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
                CustomLogger.LogError(()=>"A null basic program description was passed to AddBasicProgramToCollection.");
                return;
            }

            JobManager.RunInDispatcherThread(DebugWindowManager.DebugConsoleWindow.Dispatcher, DispatcherPriority.Normal,
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
                            ProcessStack.Clear();
                            ProcessStack.Push(this.Definition);
                        }
                        else
                        {
                            AddItemToCollection(processLoadState.ParentDefinitionDescription.ProcessCollection, new ProcessCall {Description = processLoadState.HookPoint, ProcessDescription = basicProgramDescription});
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.LogException(ex, "There was a problem adding " + basicProgramDescription.Name+" to the collection.");
                    }
                });
        }

        private bool CanExecuteAnalyseProcessCommand(object parameter)
        {
            return !string.IsNullOrEmpty(ProcessName) && !string.IsNullOrWhiteSpace(ProcessName);
        }

        private void AnalyseProcessCommandExecuted(object parameter)
        {
            ReadControlRecord(); // Make sure I have the latest control record.
            IsLoading = 0;
            LoadProcessFromExpression(SourceDefinition.Unknown, SourceDefinition.Process, DebugViewModel.Instance.ProcessAnalysisViewModel.ProcessName);
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
                        LoadProcessFromExpression(SourceDefinition.Paragraph, SourceDefinition.Process, element.Substring(1), processDescription, "Execute");
                        break;
                    case "B":
                        LoadBasicProgramFromExpression(element.Substring(1), processDescription, "Call");
                        break;
                }
            }

            return (processDescription);
        }

        private void ReadControlRecord()
        {
            // 6.2
            JobManager.RunInUIThread(DispatcherPriority.Normal,
                delegate
                    {
                        var controlFileName = SBPlusClient.Current.SystemId + "CONTROL";
                        SetIsLoading(1);
                        SBFile.Read(controlFileName, "PARAMS", this.ReadControlRecordCompleted);
                    });
        }

        private void ReadControlRecordCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            SetIsLoading(-1);

            if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0"))
            {
                // Error
                LastProcessReadError = string.Format("Failed to read {0} from {1}.", parameters[1].Value, parameters[0].Value);
                return;
            }

            var record = parameters[3];
            this.globalProcessFile = record.Extract(6, 2).Value;
            this.globalMenuFile = record.Extract(6, 3).Value;
        }

        /// <summary>
        /// This method will parse out the definition and figure out the type of definition.
        /// </summary>
        /// <param name="processFileName"></param>
        /// <param name="pName">The name of the definition.</param>
        /// <param name="defn">The definition description.</param>
        /// <param name="processLoadState">The information about the definition definition passed from the calling routine.</param>
        /// <returns></returns>
        private DefinitionDescription CreateProcessDescription(string processFileName, string pName, SBString defn, ProcessLoadState processLoadState)
        {
            DefinitionDescription processDescription;
            if (defn == null)
            {
                processDescription = new DefinitionDescription(processFileName, pName, processLoadState.Expression) { IsError = true };
                return (processDescription);
            }
            switch (defn.Extract(1).Value)
            {
                case ("I"):
                    processDescription = new InputDefinitionDescription(processFileName, pName, processLoadState.Expression, defn);
                    break;
                case ("O"):
                    processDescription = new OutputDefinitionDescription(processFileName, pName, processLoadState.Expression, defn);
                    break;
                case ("P"):
                    processDescription = CreateParagraphDescription(processFileName, pName, processLoadState.Expression, defn);
                    break;
                case ("SCREEN"):
                    processDescription = new ScreenDefintion(processFileName, pName, string.Empty, defn);
                    break;
                case ("F"):
                    processDescription = new FileUpdateDefinitionDescription(processFileName, pName, string.Empty, defn);
                    break;
                case ("M"):
                    processDescription = new MenuDefinitionDescription(processFileName, pName, string.Empty, defn);
                    break;
                default:
                    processDescription = new DefinitionDescription(processFileName, pName, processLoadState.Expression);
                    break;
            }
            return processDescription;
        }

        internal class ProcessLoadState
        {
            internal DefinitionDescription ParentDefinitionDescription;
            internal string HookPoint;
            internal string Expression;
        }

        internal class ScreenDefinitionLoadState : ProcessLoadState
        {
            internal string FileName;
        }

    }

    public enum SourceDefinition
    {
        Field,
        Screen,
        Paragraph,
        Process,
        Expression,
        Menu,
        Unknown
    }
}
