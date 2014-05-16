// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FieldDefinition.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using System;

    using SBXA.Shared;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     The field definition.
    /// </summary>
    public class FieldDefinition : DefinitionDescription
    {
        #region Fields

        private string conversionCode;

        private string derived;

        private string dictionaryComboPopulationProcess;

        private string dictionaryConversionCode;

        private string dictionaryDefault;

        private string dictionaryDerived;

        private string dictionaryDoubleClickProcess;

        private string dictionaryHtmlProcess;

        private string dictionaryInputConversion;

        private string dictionaryIntuitiveHelp;

        private string dictionaryRightClickMenu;

        private string dictionaryValidation;

        private string fieldDefault;

        private bool hasDictionary;

        private string inputConversion;

        private string intuitiveHelp;

        private string processAfter;

        private string processBefore;

        private string styleName;

        private string validation;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDefinition"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public FieldDefinition(string fileName, string name)
            : base(fileName, name)
        {
            if (this.FileName.ToUpper().StartsWith("DICT "))
            {
                this.FileName = this.FileName.Substring(5);
            }

            SBFile.ReadDictionaryItem(this.FileName, "." + name, new object[] { this.FileName, name }, this.ReadFieldDefinitionCompleted);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the conversion code.
        /// </summary>
        public string ConversionCode
        {
            get
            {
                return this.conversionCode;
            }

            set
            {
                if (this.conversionCode != null && this.conversionCode.Equals(value))
                {
                    return;
                }

                this.conversionCode = value;
                if (!string.IsNullOrEmpty(this.conversionCode))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Expression, 
                        this.conversionCode, 
                        this, 
                        "Conversion Code");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the derived.
        /// </summary>
        public string Derived
        {
            get
            {
                return this.derived;
            }

            set
            {
                if (this.derived != null && this.derived.Equals(value))
                {
                    return;
                }

                this.derived = value;
                if (!string.IsNullOrEmpty(this.derived))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Expression, 
                        this.derived, 
                        this, 
                        "Derived Expression");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the dictionary combo population process.
        /// </summary>
        public string DictionaryComboPopulationProcess
        {
            get
            {
                return this.dictionaryComboPopulationProcess;
            }

            set
            {
                if (this.dictionaryComboPopulationProcess != null && this.dictionaryComboPopulationProcess.Equals(value))
                {
                    return;
                }

                this.dictionaryComboPopulationProcess = value;
                if (!string.IsNullOrEmpty(this.dictionaryComboPopulationProcess))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Field, 
                        SourceDefinition.Expression, 
                        this.dictionaryComboPopulationProcess, 
                        this, 
                        "Combo Population Expression");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the dictionary conversion code.
        /// </summary>
        public string DictionaryConversionCode
        {
            get
            {
                return this.dictionaryConversionCode;
            }

            set
            {
                if (this.dictionaryConversionCode != null && this.dictionaryConversionCode.Equals(value))
                {
                    return;
                }

                this.dictionaryConversionCode = value;
                if (!string.IsNullOrEmpty(this.dictionaryConversionCode))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Field, 
                        SourceDefinition.Expression, 
                        this.dictionaryConversionCode, 
                        this, 
                        "Conversion Code");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the dictionary fieldDefault.
        /// </summary>
        public string DictionaryDefault
        {
            get
            {
                return this.dictionaryDefault;
            }

            set
            {
                if (this.dictionaryDefault != null && this.dictionaryDefault.Equals(value))
                {
                    return;
                }

                this.dictionaryDefault = value;
                if (!string.IsNullOrEmpty(this.dictionaryDefault))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Field, 
                        SourceDefinition.Expression, 
                        this.dictionaryDefault, 
                        this, 
                        "fieldDefault");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the dictionary derived.
        /// </summary>
        public string DictionaryDerived
        {
            get
            {
                return this.dictionaryDerived;
            }

            set
            {
                if (this.dictionaryDerived != null && this.dictionaryDerived.Equals(value))
                {
                    return;
                }

                this.dictionaryDerived = value;
                if (!string.IsNullOrEmpty(this.dictionaryDerived))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Field, 
                        SourceDefinition.Expression, 
                        this.dictionaryDerived, 
                        this, 
                        "Derived Expression");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the dictionary double click process.
        /// </summary>
        public string DictionaryDoubleClickProcess
        {
            get
            {
                return this.dictionaryDoubleClickProcess;
            }

            set
            {
                if (this.dictionaryDoubleClickProcess != null && this.dictionaryDoubleClickProcess.Equals(value))
                {
                    return;
                }

                this.dictionaryDoubleClickProcess = value;
                if (!string.IsNullOrEmpty(this.dictionaryDoubleClickProcess))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Field, 
                        SourceDefinition.Process, 
                        this.dictionaryDoubleClickProcess, 
                        this, 
                        "Double Click Process");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the dictionary html process.
        /// </summary>
        public string DictionaryHtmlProcess
        {
            get
            {
                return this.dictionaryHtmlProcess;
            }

            set
            {
                if (this.dictionaryHtmlProcess != null && this.dictionaryHtmlProcess.Equals(value))
                {
                    return;
                }

                this.dictionaryHtmlProcess = value;
                if (!string.IsNullOrEmpty(this.dictionaryHtmlProcess))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Field, 
                        SourceDefinition.Process, 
                        this.dictionaryHtmlProcess, 
                        this, 
                        "HTML Process");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the dictionary input conversion.
        /// </summary>
        public string DictionaryInputConversion
        {
            get
            {
                return this.dictionaryInputConversion;
            }

            set
            {
                if (this.dictionaryInputConversion != null && this.dictionaryInputConversion.Equals(value))
                {
                    return;
                }

                this.dictionaryInputConversion = value;
                if (!string.IsNullOrEmpty(this.dictionaryInputConversion))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Field, 
                        SourceDefinition.Expression, 
                        this.dictionaryInputConversion, 
                        this, 
                        "Input Conversion");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the dictionary intuitive help.
        /// </summary>
        public string DictionaryIntuitiveHelp
        {
            get
            {
                return this.dictionaryIntuitiveHelp;
            }

            set
            {
                if (this.dictionaryIntuitiveHelp != null && this.dictionaryIntuitiveHelp.Equals(value))
                {
                    return;
                }

                this.dictionaryIntuitiveHelp = value;
                if (!string.IsNullOrEmpty(this.dictionaryIntuitiveHelp))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Field, 
                        SourceDefinition.Process, 
                        this.dictionaryIntuitiveHelp, 
                        this, 
                        "Intuitive Help");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the dictionary right click menu.
        /// </summary>
        public string DictionaryRightClickMenu
        {
            get
            {
                return this.dictionaryRightClickMenu;
            }

            set
            {
                if (this.dictionaryRightClickMenu != null && this.dictionaryRightClickMenu.Equals(value))
                {
                    return;
                }

                this.dictionaryRightClickMenu = value;
                if (!string.IsNullOrEmpty(this.dictionaryRightClickMenu))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Field, 
                        SourceDefinition.Menu, 
                        this.dictionaryDerived, 
                        this, 
                        "Right Click Menu");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the dictionary validation.
        /// </summary>
        public string DictionaryValidation
        {
            get
            {
                return this.dictionaryValidation;
            }

            set
            {
                if (this.dictionaryValidation != null && this.dictionaryValidation.Equals(value))
                {
                    return;
                }

                this.dictionaryValidation = value;
                if (!string.IsNullOrEmpty(this.dictionaryValidation))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Field, 
                        SourceDefinition.Expression, 
                        this.dictionaryValidation, 
                        this, 
                        "Validation");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the fieldDefault.
        /// </summary>
        public string FieldDefault
        {
            get
            {
                return this.fieldDefault;
            }

            set
            {
                if (this.fieldDefault != null && this.fieldDefault.Equals(value))
                {
                    return;
                }

                this.fieldDefault = value;
                if (!string.IsNullOrEmpty(this.fieldDefault))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Expression, 
                        this.fieldDefault, 
                        this, 
                        "fieldDefault");
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has dictionary.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has dictionary; otherwise, <c>false</c>.
        /// </value>
        public bool HasDictionary
        {
            get
            {
                return this.hasDictionary;
            }

            set
            {
                this.hasDictionary = value;
            }
        }

        /// <summary>
        ///     Gets or sets the input conversion.
        /// </summary>
        public string InputConversion
        {
            get
            {
                return this.inputConversion;
            }

            set
            {
                if (this.inputConversion != null && this.inputConversion.Equals(value))
                {
                    return;
                }

                this.inputConversion = value;
                if (!string.IsNullOrEmpty(this.inputConversion))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Expression, 
                        this.inputConversion, 
                        this, 
                        "Input Conversion");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the intuitive help.
        /// </summary>
        public string IntuitiveHelp
        {
            get
            {
                return this.intuitiveHelp;
            }

            set
            {
                if (this.intuitiveHelp != null && this.intuitiveHelp.Equals(value))
                {
                    return;
                }

                this.intuitiveHelp = value;
                if (!string.IsNullOrEmpty(this.intuitiveHelp))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Process, 
                        this.intuitiveHelp, 
                        this, 
                        "Intuitive Help");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the process after.
        /// </summary>
        public string ProcessAfter
        {
            get
            {
                return this.processAfter;
            }

            set
            {
                if (this.processAfter != null && this.processAfter.Equals(value))
                {
                    return;
                }

                this.processAfter = value;
                if (!string.IsNullOrEmpty(this.processAfter))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Process, 
                        this.processAfter, 
                        this, 
                        "After");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the process before.
        /// </summary>
        public string ProcessBefore
        {
            get
            {
                return this.processBefore;
            }

            set
            {
                if (this.processBefore != null && this.processBefore.Equals(value))
                {
                    return;
                }

                this.processBefore = value;
                if (!string.IsNullOrEmpty(this.processBefore))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Process, 
                        this.processBefore, 
                        this, 
                        "Before");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the style name.
        /// </summary>
        public string StyleName
        {
            get
            {
                return this.styleName;
            }

            set
            {
                if (this.styleName != null && this.styleName.Equals(value))
                {
                    return;
                }

                this.styleName = value;
                if (!string.IsNullOrEmpty(this.styleName))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Expression, 
                        this.inputConversion, 
                        this, 
                        "Style Name");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the validation.
        /// </summary>
        public string Validation
        {
            get
            {
                return this.validation;
            }

            set
            {
                if (this.validation != null && this.validation.Equals(value))
                {
                    return;
                }

                this.validation = value;
                if (!string.IsNullOrEmpty(this.validation))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Expression, 
                        this.validation, 
                        this, 
                        "Validation");
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add self.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        protected override void AddSelf(RevisionDefinitionItemCollection collection)
        {
            if (this.HasDictionary)
            {
                RevisionDefinitionViewModel.AddItemToDefinition(
                    collection, 
                    new RevisionDefinitionItem()
                        {
                            Action = "IO", 
                            FileName = this.FileName, 
                            Item = this.Name, 
                            Parameters = RevisionDefinitionViewModel.Dict
                        });
            }
        }

        private void ReadFieldDefinitionCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            const int DefaultExpression = 5;
            const int ValidationExpression = 6;
            const int ConversionExpression = 7;
            const int DerivedExpression = 8;
            const int IntuitiveHelpExpression = 13;
            const int RightClickMenu = 29; // .1;
            const int DoubleClickProcess = 29; //.2;
            const int HtmlProcess = 30;
            const int ComboPopulationProcess = 34;

            string fieldName = string.Empty;
            try
            {
                if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0"))
                {
                    // item not found.
                    this.HasDictionary = false;
                    return;
                }

                this.HasDictionary = true;

                var state = userState as object[];
                if (state == null)
                {
                    return;
                }

                var fileName = state[0] as string;
                fieldName = state[1] as string;

                var definition = parameters[3];

                if (!string.IsNullOrWhiteSpace(definition.Extract(DefaultExpression).Value))
                {
                    this.DictionaryDefault = definition.Extract(DefaultExpression).Value;
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(ValidationExpression).Value))
                {
                    this.DictionaryValidation = definition.Extract(ValidationExpression).Value;
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(ConversionExpression).Value))
                {
                    this.DictionaryConversionCode = definition.Extract(ConversionExpression).Value;
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(DerivedExpression).Value))
                {
                    this.DictionaryDerived = definition.Extract(DerivedExpression).Value;
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(IntuitiveHelpExpression).Value))
                {
                    // Check to see if the tooltip is there as well.
                    var intHelp = definition.Extract(IntuitiveHelpExpression).Value;
                    if (intHelp.IndexOf("[") >= 0)
                    {
                        intHelp = intHelp.Split("[".ToCharArray())[1];
                        this.DictionaryIntuitiveHelp = intHelp;
                    }
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(RightClickMenu, 1).Value))
                {
                    this.DictionaryRightClickMenu = definition.Extract(RightClickMenu, 1).Value;
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(DoubleClickProcess, 2).Value))
                {
                    this.DictionaryDoubleClickProcess = definition.Extract(DoubleClickProcess, 2).Value;
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(HtmlProcess).Value))
                {
                    this.DictionaryHtmlProcess = definition.Extract(HtmlProcess).Value;
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(ComboPopulationProcess).Value))
                {
                    this.DictionaryComboPopulationProcess = definition.Extract(ComboPopulationProcess).Value;
                }
            }
            catch (Exception exception)
            {
                CustomLogger.LogException(exception, "A problem occurred parsing the field definition for " + fieldName);
            }
        }

        #endregion
    }
}