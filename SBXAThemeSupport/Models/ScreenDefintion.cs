// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScreenDefintion.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.Models
{
    using System;
    using System.Collections.ObjectModel;
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
            : base(fileName, name, expression)
        {
            this.FieldDescriptions = new ObservableCollection<FieldDefinition>();
            this.ParseScreenDefinition(definition);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the field descriptions.
        /// </summary>
        public ObservableCollection<FieldDefinition> FieldDescriptions { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add children to collection.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
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

            base.AddChildrenToCollection(collection);
        }

        #endregion

        #region Methods

        private static void ReadDictItemCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {
                var state = userState as object[];
                if (state == null)
                {
                    return;
                }

                if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0"))
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
                    this.FieldDescriptions.Add(
                        new FieldDefinition(this.FileName, definition.Extract(Fields, fno).Value)
                            {
                                ProcessBefore =
                                    definition.Extract(
                                        ProcessesBefore, 
                                        fno).Value, 
                                ProcessAfter =
                                    definition.Extract(
                                        ProcessesAfter, 
                                        fno).Value, 
                                IntuitiveHelp =
                                    definition.Extract(IntHelp, fno)
                                    .Value, 
                                ConversionCode =
                                    definition.Extract(
                                        ConversionCode, 
                                        fno).Value, 
                                FieldDefault =
                                    (!string.IsNullOrEmpty(
                                        definition.Extract(
                                            FieldPos, 
                                            fno).Value)
                                     && definition.Extract(
                                         FieldPos, 
                                         fno)
                                            .Value.Substring(0, 1)
                                            .Equals("0"))
                                        ? string.Empty
                                        : definition.Extract(
                                            DefaultDerived, 
                                            fno).Value, 
                                Derived =
                                    (!string.IsNullOrEmpty(
                                        definition.Extract(
                                            FieldPos, 
                                            fno).Value)
                                     && !definition.Extract(
                                         FieldPos, 
                                         fno)
                                             .Value.Substring(0, 1)
                                             .Equals("0"))
                                        ? string.Empty
                                        : definition.Extract(
                                            DefaultDerived, 
                                            fno).Value, 
                                Validation =
                                    definition.Extract(
                                        Validation, 
                                        fno).Value, 
                                StyleName =
                                    (definition.Dcount() >= StyleName
                                     && definition.Dcount(StyleName)
                                     >= fno)
                                        ? definition.Extract(
                                            StyleName, 
                                            fno).Value
                                        : string.Empty
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
                MessageBox.Show("Exception parsing screen definition (" + exception.Message + ")");
            }
        }

        #endregion
    }
}