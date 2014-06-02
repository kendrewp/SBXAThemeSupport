// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScreenProcessDescription.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using System;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;

    using SBXAThemeSupport.DebugAssistant;
    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     This class represents a screen definition description
    /// </summary>
    public class ScreenProcessDescription : DefinitionDescription
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenProcessDescription"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file that contains the screen.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public ScreenProcessDescription(string fileName, string name)
            : base(fileName, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenProcessDescription"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file that contains the screen.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        public ScreenProcessDescription(string fileName, string name, string expression)
            : base(fileName, name, SourceDefinition.Process, expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenProcessDescription"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file that contains the screen.
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
        public ScreenProcessDescription(string fileName, string name, string expression, SBString definition)
            : this(fileName, name, expression)
        {
            if (!string.IsNullOrWhiteSpace(definition.Extract(5, 1).Value))
            {
                this.Dictionary = definition.Extract(5, 1).Value;
                this.ProcessCollection.Add(
                    new ProcessCall { Description = "Dictionary", ProcessDescription = new DictionaryDescription(this.Dictionary) });
            }

            if (!string.IsNullOrWhiteSpace(definition.Extract(6).Value))
            {
                this.ScreenName = definition.Extract(6).Value;
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadScreen(this.ScreenName, this.Dictionary, this, "Screen");

                // Now check if there are linked screens. To do this see if the last character is numeric. If it is go back until you find the first non-numeric. Now
                // we have a starting point, loop adding 1 untill there is no hit.
                int startIndex = this.ScreenName.Length - 1;
                while (SBXA.Shared.Utilities.IsNumber(this.ScreenName.Substring(startIndex)))
                {
                    startIndex++;
                }

                startIndex--; // because it would have been incremented one beyond.
                if (!string.IsNullOrEmpty(this.ScreenName.Substring(startIndex))
                    && Utilities.IsNumber(this.ScreenName.Substring(startIndex)))
                {
                    var nextScreen = Convert.ToInt16(this.ScreenName.Substring(startIndex)) + 1;
                    var prefix = this.ScreenName.Substring(0, startIndex);
                    DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
                    // I have a starting point, now read the definition.
                    SBFile.ReadDictionaryItem(
                        this.Dictionary, 
                        prefix + nextScreen, 
                        new object[] { nextScreen, prefix, this.Dictionary }, 
                        this.ReadLinkedScreenCompleted);
                }
            }

            // load the list of processes
            if (!string.IsNullOrWhiteSpace(definition.Extract(8, 1).Value))
            {
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                    SourceDefinition.Screen, 
                    SourceDefinition.Process, 
                    definition.Extract(8, 1).Value, 
                    this, 
                    "Before");
            }

            if (!string.IsNullOrWhiteSpace(definition.Extract(8, 2).Value))
            {
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                    SourceDefinition.Screen, 
                    SourceDefinition.Process, 
                    definition.Extract(8, 2).Value, 
                    this, 
                    "After");
            }

            if (!string.IsNullOrWhiteSpace(definition.Extract(8, 7).Value))
            {
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                    SourceDefinition.Screen, 
                    SourceDefinition.Process, 
                    definition.Extract(8, 7).Value, 
                    this, 
                    "Before Display");
            }

            if (!string.IsNullOrWhiteSpace(definition.Extract(8, 8).Value))
            {
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                    SourceDefinition.Screen, 
                    SourceDefinition.Process, 
                    definition.Extract(8, 8).Value, 
                    this, 
                    "Before Action Bar Option");
            }

            if (!string.IsNullOrWhiteSpace(definition.Extract(8, 4).Value))
            {
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                    SourceDefinition.Screen, 
                    SourceDefinition.Process, 
                    definition.Extract(8, 4).Value, 
                    this, 
                    "After Action Bar Option");
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the dictionary.
        /// </summary>
        /// <value>
        ///     The dictionary.
        /// </value>
        public string Dictionary { get; set; }

        /// <summary>
        ///     Gets or sets the name of the screen.
        /// </summary>
        /// <value>
        ///     The name of the screen.
        /// </value>
        public string ScreenName { get; set; }

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
            if (!this.IsError)
            {
                // First add this screen definition definition
                JobManager.RunInDispatcherThread(
                    DebugWindowManager.DebugConsoleWindow.Dispatcher, 
                    DispatcherPriority.Normal, 
                    () =>
                    RevisionDefinitionViewModel.AddItemToDefinition(
                        collection, 
                        new RevisionDefinitionItem
                            {
                                Action = "IO", 
                                FileName = this.FileName, 
                                Item = this.Name, 
                                Parameters = RevisionDefinitionViewModel.Data
                            }));
            }

            // Now add any definition calls.
            base.AddChildrenToCollection(collection);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The read linked screen completed.
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
        private void ReadLinkedScreenCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(-1);

            if (parameters[5].Count != 1 || !parameters[5].Value.Equals("0"))
            {
                // item not found end of reading linked screens.
                return;
            }

            var state = userState as object[];
            if (state == null)
            {
                return;
            }

            var nextScreen = (int)state[0];
            var prefix = state[1] as string;
            var dict = state[2] as string;

            DebugViewModel.Instance.ProcessAnalysisViewModel.LoadScreen(prefix + nextScreen, dict, this, "Screen");

            // increment the next screen and do it again.
            nextScreen++;
            DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
            SBFile.ReadDictionaryItem(
                this.Dictionary, 
                prefix + nextScreen, 
                new object[] { nextScreen, prefix, this.Dictionary }, 
                this.ReadLinkedScreenCompleted);
        }

        #endregion
    }
}