// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SBExpression.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.UI.Client;

    using SBXAThemeSupport.DebugAssistant;
    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     The sb expression.
    /// </summary>
    public class SBExpression : DefinitionDescription
    {
        #region Static Fields

        private static readonly StringCollection StandardExpressions = new StringCollection
                                                                           {
                                                                               "QUIT", 
                                                                               "G:U", 
                                                                               "G:DE", 
                                                                               "INVOKE.TEXT.ED", 
                                                                               "SELECT", 
                                                                               "G:F2", 
                                                                               "G:F3", 
                                                                               "G:F4", 
                                                                               "README", 
                                                                               "DIARY", 
                                                                               "CALC", 
                                                                               "DIALOG", 
                                                                               "LOCK.KEYBRD5", 
                                                                               "ACTION", 
                                                                               "TOGGLE"
                                                                           };

        private readonly ObservableCollection<FieldDefinition> fieldDescriptions = new ObservableCollection<FieldDefinition>();

        public ObservableCollection<FieldDefinition> FieldDescriptions
        {
            get
            {
                return this.fieldDescriptions;
            }
        }

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SBExpression"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="hookType">
        /// The hook type.
        /// </param>
        /// <param name="sysid">
        /// The sysid.
        /// </param>
        public SBExpression(string fileName, string expression, SourceDefinition hookType, string sysid)
            : this(fileName, expression, hookType, sysid, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SBExpression"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="hookType">
        /// The hook type.
        /// </param>
        /// <param name="sysid">
        /// The sysid.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public SBExpression(string fileName, string expression, SourceDefinition hookType, string sysid, string name)
            : base(fileName, name, hookType, expression)
        {
            this.SystemId = sysid;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the code table.
        /// </summary>
        public string CodeTable { get; set; }

        /// <summary>
        ///     Gets or sets the definition description.
        /// </summary>
        /// <value>
        ///     The definition description.
        /// </value>
        public override string SourceExpression
        {
            protected set
            {
                if (this.SourceExpression != null && this.SourceExpression.Equals(value))
                {
                    return;
                }

                base.SourceExpression = value;
                if (!string.IsNullOrEmpty(this.SourceExpression) && !IsStandardSBExpression(this.SourceExpression) && !IsConstantValueExpression(value))
                {
                    // now I have to parse it out in order to find process calls. I will do this by calling SB.EVAL.EXP (I think)
                    try
                    {
                        // Debug.WriteLine("[SBExpression.Expression(53)] " + this.HookType + ", " + this.expression);
                        if (this.HookType == SourceDefinition.Expression)
                        {
                            DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
                            XuiDebug.StackExpression(this.ExpressionStackCompleted, this.SourceExpression, this.FileName);
                        }
                        else
                        {
                            string callType;
                            var processName = ProcessAnalysisViewModel.GetProcessName(this.SourceExpression, out callType);
                            switch (callType)
                            {
                                case "C":
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcess(
                                        processName, 
                                        this,
                                        this.SourceExpression, 
                                        string.Empty, 
                                        this.SystemId);
                                    break;
                                case "B":
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadBasicProgramFromExpression(
                                        this.SourceExpression, 
                                        this, 
                                        string.Empty);
                                    break;
                                case "S":
                                    // The Expression is already added so don't do anything.
                                    break;
                                case "M":
                                    // TODO
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadMenu(
                                        processName, 
                                        this,
                                        this.SourceExpression, 
                                        string.Empty, 
                                        this.SystemId);
                                    break;
                                case "P":
                                    // TODO
                                    // It is possible that there are calls and executes inside the P:() so I need to stack it and parse it out.
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
                                    XuiDebug.StackExpression(this.ExpressionStackCompleted, processName, this.FileName);
                                    break;
                                case "D":
                                    // Default in a process slot, e.g. D:()
                                    // It is possible that there are calls and executes inside the P:() so I need to stack it and parse it out.
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
                                    XuiDebug.StackExpression(this.ExpressionStackCompleted, processName, this.FileName);
                                    break;
                                case "V":
                                    // Validation in a process slot e.g. V:()
                                    // It is possible that there are calls and executes inside the P:() so I need to stack it and parse it out.
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
                                    XuiDebug.StackExpression(this.ExpressionStackCompleted, processName, this.FileName);
                                    break;
                                case "":
                                    if (processName.Equals("SELECT"))
                                    {
                                        //TODO: Add the message to the rev defn.
                                    }
                                    else
                                    {
                                        DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcess(
                                            processName, 
                                            this,
                                            this.SourceExpression, 
                                            string.Empty, 
                                            this.SystemId);
                                    }

                                    break;
                                default:
                                    // It is not a call or executed so do not add a sub-node.
                                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcess(
                                        processName, 
                                        this,
                                        this.SourceExpression, 
                                        string.Empty, 
                                        this.SystemId);
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

        /// <summary>
        ///     Gets or sets the system id.
        /// </summary>
        public string SystemId { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The is standard sb expression.
        /// </summary>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsStandardSBExpression(string expression)
        {
            var exp = expression.Split(GenericConstants.CHAR_ARRAY_COMMA)[0];
            return StandardExpressions.Contains(exp);
        }

        /// <summary>
        /// The add children to collection.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            base.AddChildrenToCollection(collection);
            foreach (var fieldDescription in FieldDescriptions)
            {
                fieldDescription.AddChildrenToCollection(collection);
            }
        }

        #endregion

        #region Methods


        public static bool IsConstantValueExpression(string val)
        {
            return ((val.StartsWith("\"") && val.EndsWith("\""))
                    || (val.StartsWith("\"") && val.EndsWith("\"[M]"))
                    || Utilities.IsNumber(val));
        }

        /// <summary>
        /// The add self.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        protected override void AddSelf(RevisionDefinitionItemCollection collection)
        {
            if (!this.IsError)
            {
                if (!string.IsNullOrEmpty(this.CodeTable))
                {
                    JobManager.RunInUIThread(
                        DispatcherPriority.Normal, 
                        delegate
                            {
                                var fileName = this.SystemId;
                                if (string.IsNullOrEmpty(fileName))
                                {
                                    fileName = SBPlusClient.Current.SystemId;
                                }

                                fileName = fileName + "DEFN";
                                JobManager.RunInDispatcherThread(
                                    DebugWindowManager.DebugConsoleWindow.Dispatcher, 
                                    DispatcherPriority.Normal, 
                                    () =>
                                    RevisionDefinitionViewModel.AddItemToDefinition(
                                        collection, 
                                        new RevisionDefinitionItem()
                                            {
                                                Action = "IO", 
                                                FileName = fileName, 
                                                Item = this.CodeTable, 
                                                Parameters = RevisionDefinitionViewModel.Data
                                            }));
                            });
                }
            }
        }

        private void ExpressionStackCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(-1);
            // It is possible that the expression is not evaluated. e.g. C:GROUP.CODE.VAL or V:Y,N
            Debug.WriteLine(
                "[SBExpression.ExpressionStackCompleted(56)] " + parameters[4].GetStandardString() + " " + parameters[5].GetStandardString()
                + " " + parameters[1].GetStandardString() + ", " + parameters[3].GetStandardString());
            switch (parameters[4].Value)
            {
                case "2":
                    switch (parameters[1].Value.Substring(0, 2))
                    {
                        case "C:":
                            var processName = this.SourceExpression.Substring(2);
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
                                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcess(
                                    processName,
                                    this,
                                    this.SourceExpression,
                                    string.Empty,
                                    this.SystemId);
                            }
                            break;
                        case "F:":
                            var parts = parameters[1].Value.Substring(2).Split(GenericConstants.CHAR_ARRAY_COMMA);
                            var fieldDefinition = new FieldDefinition(parts[0], parts[1]);
                            this.FieldDescriptions.Add(fieldDefinition);
                            break;
                        default:
                            DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
                            XuiDebug.StackExpression(this.ExpressionStackCompleted, parameters[4].GetStandardString(), this.FileName);
                            break;
                    }
                    break;
                case "1":
                    DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
                    XuiDebug.StackExpression(this.ExpressionStackCompleted, parameters[4].GetStandardString(), this.FileName);
                    break;
                case "0":
                    // look for a code table
                    var stack = parameters[3].Value.Split(GenericConstants.CHAR_ARRAY_SEMI_COLON);
                    for (int pos = 0; pos < stack.Length; pos++)
                    {
                        if (stack[pos].Equals("ZT"))
                        {
                            this.CodeTable = stack[pos - 1].Substring(1);
                        }
                    }

                    break;
            }
        }

        #endregion
    }
}