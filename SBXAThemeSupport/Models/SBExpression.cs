namespace SBXAThemeSupport.Models
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.UI.Client;

    using SBXAThemeSupport.DebugAssistant;
    using SBXAThemeSupport.DebugAssistant.ViewModels;

    public class SBExpression : DefinitionDescription
    {
        static readonly StringCollection StandardExpressions = new StringCollection { "QUIT", "G:U", "G:DE", "INVOKE.TEXT.ED", "SELECT", "G:F2", "G:F3", "G:F4" };

        public SBExpression(string fileName, string expression, SourceDefinition hookType, string sysid)
            : this(fileName, expression, hookType, sysid, string.Empty)
        {
        }

        public SBExpression(string fileName, string expression, SourceDefinition hookType, string sysid, string name)
            : base(fileName, name)
        {
            SystemId = sysid;
            HookType = hookType;
            Expression = expression;
        }

        public string SystemId { get; set; }
        public SourceDefinition HookType { get; set; }

        private string expression;

        /// <summary>
        /// Gets or sets the definition description.
        /// </summary>
        /// <value>
        /// The definition description.
        /// </value>
        public string Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                if (this.expression != null && this.expression.Equals(value))
                {
                    return;
                }
                this.expression = value;
                if (!string.IsNullOrEmpty(this.Expression) && !IsStandardSBExpression(expression))
                {
                    // now I have to parse it out in order to find process calls. I will do this by calling SB.EVAL.EXP (I think)
                    try
                    {
                        Debug.WriteLine("[SBExpression.Expression(53)] " + HookType + ", " + expression);
                        if (HookType == SourceDefinition.Expression)
                        {
                            DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
                            XuiDebug.StackExpression(ExpressionStackCompleted, this.expression, this.FileName);
                        }
                        else
                        {
                            string callType;
                            var processName = ProcessAnalysisViewModel.GetProcessName(expression, out callType);
                            switch (callType)
                            {
                                case "C":
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcess(processName, this, expression, string.Empty, SystemId);
                                    break;
                                case "B":
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadBasicProgramFromExpression(expression, this, string.Empty);
                                    break;
                                case "S":
                                    // The Expression is already added so don't do anything.
                                    break;
                                case "M":
                                    // TODO
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadMenu(processName, this, expression, string.Empty, SystemId);
                                    break;
                                case "P":
                                    // TODO
                                    // It is possible that there are calls and executes inside the P:() so I need to stack it and parse it out.
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
                                    XuiDebug.StackExpression(ExpressionStackCompleted, processName, this.FileName);
                                    break;
                                case (""):
                                    if (processName.Equals("SELECT"))
                                    {
                                        //TODO: Add the message to the rev defn.
                                        string kp;
                                    }
                                    else
                                    {
                                        DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcess(processName, this, expression, string.Empty, SystemId);
                                    }
                                    break;
                                default:
                                    // It is not a call or executed so do not add a sub-node.
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcess(processName, this, expression, string.Empty, SystemId);
                                    break;
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        CustomLogger.LogException(exception, "A problem occurred parsing the expression.");
                    }
                }
            }
        }

        private void ExpressionStackCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(-1);
            // It is possible that the expression is not evaluated. e.g. C:GROUP.CODE.VAL or V:Y,N
            Debug.WriteLine("[SBExpression.ExpressionStackCompleted(56)] " + parameters[4].GetStandardString() + " " + parameters[5].GetStandardString() + " " + parameters[1].GetStandardString() + ", " + parameters[3].GetStandardString());
            switch (parameters[4].Value)
            {
                case "2":
                        if (parameters[1].Value.StartsWith("C:"))
                        {
                            var processName = expression.Substring(2);
                            var commaPos = processName.IndexOf(",", StringComparison.Ordinal);
                            // Process call.
                            if (commaPos > 0)
                            {
                                // strip parameters
                                processName = processName.Substring(0, commaPos);
                            }
                            if (!string.IsNullOrEmpty(processName))
                            {
                                // now load the definition.
                                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcess(processName, this, expression, string.Empty, SystemId);
                            }

                        }
                        else
                        {
                            DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
                            XuiDebug.StackExpression(ExpressionStackCompleted, parameters[4].GetStandardString(), this.FileName);
                        }
                    break;
                case "0":
                    // look for a code table
                    var stack = parameters[3].Value.Split(GenericConstants.CHAR_ARRAY_SEMI_COLON);
                    for (int pos = 0; pos < stack.Length; pos++)
                    {
                        if (stack[pos].Equals("ZT"))
                        {
                            CodeTable = stack[pos - 1].Substring(1);
                        }
                    }
                    break;
                    
            }

        }

        public string CodeTable { get; set; }

        public static bool IsStandardSBExpression(string expression)
        {
            var exp = expression.Split(GenericConstants.CHAR_ARRAY_COMMA)[0];
            return StandardExpressions.Contains(exp);
        }

        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            base.AddChildrenToCollection(collection);
        }

        protected override void AddSelf(RevisionDefinitionItemCollection collection)
        {
            if (!IsError)
            {
                if (!string.IsNullOrEmpty(CodeTable))
                {
                    JobManager.RunInUIThread(DispatcherPriority.Normal,
                        delegate
                            {
                                var fileName = SystemId;
                                if (string.IsNullOrEmpty(fileName))
                                {
                                    fileName = SBPlusClient.Current.SystemId;
                                }
                                fileName = fileName + "DEFN";
                                JobManager.RunInDispatcherThread(DebugWindowManager.DebugConsoleWindow.Dispatcher, DispatcherPriority.Normal,
                                    () => RevisionDefinitionViewModel.AddItemToDefinition(collection, new RevisionDefinitionItem() { Action = "IO", FileName = fileName, Item = this.CodeTable, Parameters = RevisionDefinitionViewModel.Data }));

                            });
                }
            }
        }


    }

}