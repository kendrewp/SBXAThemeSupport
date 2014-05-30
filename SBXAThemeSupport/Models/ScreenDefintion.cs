// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScreenDefintion.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;

    using SBXAThemeSupport.DebugAssistant;
    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     This class represents a paragraph and is capable of parsing it out to extract definition calls.
    /// </summary>
    public class ScreenDefintion : DefinitionDescription
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenDefintion"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="definition">
        /// The definition.
        /// </param>
        public ScreenDefintion(string fileName, string name, string expression, SBString definition)
            : base(fileName, name, SourceDefinition.Definition, expression)
        {
            this.FieldDescriptions = new ObservableCollection<FieldDefinition>();
            this.ParseScreenDefinition(definition);
            // I need to read the .GUI definition to get the processes associated with buttons that do not have a FK assignment.
            SBFile.ReadDictionaryItem(fileName, name + ".GUI", new object(), GuiItemReadCompleted);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the field descriptions.
        /// </summary>
        public ObservableCollection<FieldDefinition> FieldDescriptions { get; private set; }

        #endregion

        #region Public Methods and Operators

        protected override void AddSelf(RevisionDefinitionItemCollection collection)
        {
            // Add all the screen items, if they exist.

            // Primary screen definition
            SBFile.ReadDictionaryItem(
                this.FileName,
                this.Name,
                new object[] { this.FileName, this.Name, collection },
                ReadDictItemCompleted);
            // .TXT
            SBFile.ReadDictionaryItem(
                this.FileName,
                this.Name + ".TXT",
                new object[] { this.FileName, this.Name + ".TXT", collection },
                ReadDictItemCompleted);
            // .GUI
            SBFile.ReadDictionaryItem(
                this.FileName,
                this.Name + ".GUI",
                new object[] { this.FileName, this.Name + ".GUI", collection },
                ReadDictItemCompleted);
            // .XUI
            SBFile.ReadDictionaryItem(
                this.FileName,
                this.Name + ".XUI",
                new object[] { this.FileName, this.Name + ".XUI", collection },
                ReadDictItemCompleted);

            foreach (var fieldDescription in this.FieldDescriptions)
            {
                fieldDescription.AddChildrenToCollection(collection);
            }

            // add the dictionary and data, if the process definition has the same data file, other wise add the dict and the different data file.
            if (DebugWindowManager.DebugConsoleWindow != null)
            {
                // Adding screen definition item from dictionary.
                JobManager.RunInDispatcherThread(
                    DebugWindowManager.DebugConsoleWindow.Dispatcher,
                    DispatcherPriority.Normal,
                    () =>
                    RevisionDefinitionViewModel.AddItemToDefinition(
                        collection,
                        new RevisionDefinitionItem
                        {
                            Action = "FC",
                            FileName = FileName.StartsWith("DICT ") ? FileName.Substring(5) : FileName,
                            Item = string.Empty,
                            Parameters = RevisionDefinitionViewModel.DictAndData
                        }));
            }
        }

        #endregion

        #region Methods

        private void GuiItemReadCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {

            if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0") || parameters[3].IsNullOrEmpty() || !parameters[3].Extract(1).Value.Equals("SCREEN.GUIDEFS"))
            {
                // item not found.
                return;
            }

            const int AttributeList = 10;
            const int ObjectIndex = 12; // off set by 15, 0 based

            // Loop through the objects and look for a button. I know it is a button because it has a pbclass in the third attribute of the object.
            var record = parameters[3];
            var attributeList = record.Extract(AttributeList).CopyToStringCollection();
            var indexOfPbClass = Convert.ToString(attributeList.IndexOf("pbclass") + 2);
            var indexOfUserData = Convert.ToString(attributeList.IndexOf("user_data") + 1);
            var indexOfString = Convert.ToString(attributeList.IndexOf("string") + 1);
            var noAttributes = record.Dcount();
            for (int aNo = 15; aNo < noAttributes - 4; aNo += 4)
            {
                var attributes = record.Extract(aNo + 2).CopyToStringCollection();
                if (attributes.Contains(indexOfPbClass))
                {
                    ProcessButton(indexOfUserData, indexOfString, attributes, record.Extract(aNo + 3).CopyToStringCollection());
                }
            }
            }
            catch (Exception exception)
            {
                CustomLogger.LogException(exception, "There was a problem processing the .GUI item for " + this.Name);
            }
        }

        private void ProcessButton(string indexOfUserData, string indexOfString, StringCollection attributes, StringCollection values)
        {
            if (attributes.Contains(indexOfUserData))
            {
                var processName = values[attributes.IndexOf(indexOfUserData)];
                var content = attributes.Contains(indexOfString) ? "Button " + values[attributes.IndexOf(indexOfString)] : "Button ??";
                if (!Utilities.IsNumber(processName))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen,
                        SourceDefinition.Process,
                        processName,
                        this,
                        content);
                }

            }
        }

        private static void ReadDictItemCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {
                var state = userState as object[];
                if (state == null)
                {
                    return;
                }

                if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0") || parameters[3].IsNullOrEmpty())
                {
                    // item not found.
                    return;
                }

                var revisionDefinitionItemCollection = state[2] as RevisionDefinitionItemCollection;
                if (revisionDefinitionItemCollection == null)
                {
                    return;
                }

                var fileName = state[0] as string;
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                var itemName = state[1] as string;
                if (string.IsNullOrEmpty(itemName))
                {
                    return;
                }

                if (DebugWindowManager.DebugConsoleWindow != null)
                {
                    // Adding screen definition item from dictionary.
                    JobManager.RunInDispatcherThread(
                        DebugWindowManager.DebugConsoleWindow.Dispatcher, 
                        DispatcherPriority.Normal, 
                        () =>
                        RevisionDefinitionViewModel.AddItemToDefinition(
                            revisionDefinitionItemCollection, 
                            new RevisionDefinitionItem
                                {
                                    Action = "IO", 
                                    FileName = fileName.StartsWith("DICT ") ? fileName.Substring(5) : fileName, 
                                    Item = itemName, 
                                    Parameters = RevisionDefinitionViewModel.Dict
                                }));
                }
            }
            catch (Exception ex)
            {
                CustomLogger.LogException(ex, "There was a problem processing a dictionary item.");
            }
        }

        private void ParseScreenDefinition(SBString definition)
        {
            const int ButtonDesc = 11;
            const int Buttons = 12;
            const int Fields = 15;
            const int ProcessesBefore = 25;
            const int ProcessesAfter = 26;
            const int IntHelp = 27;
            const int FieldPos = 31;
            const int ConversionCode = 34;
            const int DefaultDerived = 35;
            const int Validation = 36;
            // So far I have not found where or what this is, in the BRANCH S1 screen it looks like a hangove from rev control. const int InputConversion   = 39;
            const int StyleName = 51;

            try
            {
                // fields
                var noFields = definition.Dcount(Fields);
                for (int fno = 1; fno <= noFields; fno++)
                {
                    var processBefore = (definition.Dcount() >= ProcessesBefore && definition.Extract(ProcessesBefore).Dcount() >= fno) ? definition.Extract(ProcessesBefore, fno).Value : string.Empty; 
                    var processAfter = (definition.Dcount() >= ProcessesAfter && definition.Extract(ProcessesAfter).Dcount() >= fno) ? definition.Extract(ProcessesAfter, fno).Value : string.Empty; 
                    var intuitiveHelp = (definition.Dcount() >= IntHelp && definition.Extract(IntHelp).Dcount() >= fno) ? definition.Extract(IntHelp, fno).Value : string.Empty; 
                    var conversionCode = (definition.Dcount() >= ConversionCode && definition.Extract(ConversionCode).Dcount() >= fno) ? definition.Extract(ConversionCode, fno).Value : string.Empty;
                    var fieldDefault = string.Empty;
                    if (definition.Dcount() >= FieldPos && definition.Extract(FieldPos).Dcount() >= fno)
                    {
                        fieldDefault = (!string.IsNullOrEmpty(definition.Extract(FieldPos, fno).Value) && definition.Extract(FieldPos, fno).Value.Substring(0, 1).Equals("0")) ? string.Empty : definition.Extract(DefaultDerived, fno).Value;
                    }
                    var derived = string.Empty;
                    if (definition.Dcount() >= FieldPos && definition.Extract(FieldPos).Dcount() >= fno)
                    {
                        derived = (!string.IsNullOrEmpty(definition.Extract(FieldPos, fno).Value) && definition.Extract(FieldPos, fno).Value.Substring(0, 1).Equals("0")) ? string.Empty : definition.Extract(DefaultDerived, fno).Value;
                    }
                    var validation = (definition.Dcount() >= Validation && definition.Extract(Validation).Dcount() >= fno) ? definition.Extract(Validation, fno).Value : string.Empty;
                    var styleName = (definition.Dcount() >= StyleName && definition.Extract(StyleName).Dcount() >= fno) ? definition.Extract(StyleName, fno).Value : string.Empty;

                    this.FieldDescriptions.Add(
                        new ScreenFieldDefinition(this.FileName, definition.Extract(Fields, fno).Value)
                            {
                                ProcessBefore = processBefore,
                                ProcessAfter = processAfter,
                                IntuitiveHelp = intuitiveHelp,
                                ConversionCode = conversionCode,
                                FieldDefault = fieldDefault,
                                Derived = derived,
                                Validation = validation,
                                StyleName = styleName
                            });
                }

                // Buttons
                var noButtons = definition.Dcount(Buttons);
                for (int bNo = 1; bNo <= noButtons; bNo++)
                {
                    if (!definition.Extract(Buttons, bNo).IsNullOrEmpty())
                    {
                        DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                            SourceDefinition.Screen, 
                            SourceDefinition.Process, 
                            definition.Extract(Buttons, bNo).Value, 
                            this, 
                            "Button " + definition.Extract(ButtonDesc, bNo).Value);
                    }
                }

                // load the list of processes
                if (!string.IsNullOrWhiteSpace(definition.Extract(7, 1).Value))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Process, 
                        definition.Extract(7, 1).Value, 
                        this, 
                        "Before Screen Display");
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(7, 2).Value))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Process, 
                        definition.Extract(7, 2).Value, 
                        this, 
                        "After Screen Display");
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(7, 3).Value))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Process, 
                        definition.Extract(7, 3).Value, 
                        this, 
                        "After Read Record");
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(7, 4).Value))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Process, 
                        definition.Extract(7, 4).Value, 
                        this, 
                        "After Accept");
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(7, 5).Value))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Process, 
                        definition.Extract(7, 5).Value, 
                        this, 
                        "After Update");
                }

                if (!string.IsNullOrWhiteSpace(definition.Extract(7, 6).Value))
                {
                    DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                        SourceDefinition.Screen, 
                        SourceDefinition.Process, 
                        definition.Extract(7, 6).Value, 
                        this, 
                        "If Escape");
                }

                // Buttons
            }
            catch (Exception exception)
            {
                CustomLogger.LogException(exception, "Exception parsing screen definition");
                // MessageBox.Show("Exception parsing screen definition (" + exception.Message + ")");
            }
        }

        #endregion
    }
}