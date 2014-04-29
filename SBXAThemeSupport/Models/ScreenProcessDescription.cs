using SBXA.Shared;
using SBXAThemeSupport.DebugAssistant.ViewModels;

namespace SBXAThemeSupport.Models
{
    using System;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.UI.Client;

    using SBXAThemeSupport.DebugAssistant;

    using Utilities = SBXA.Shared.Utilities;

    /// <summary>
    /// This class represents a screen definition description
    /// </summary>
    public class ScreenProcessDescription : DefinitionDescription
    {
        /// <summary>
        /// Gets or sets the name of the screen.
        /// </summary>
        /// <value>
        /// The name of the screen.
        /// </value>
        public string ScreenName { get; set; }

        /// <summary>
        /// Gets or sets the dictionary.
        /// </summary>
        /// <value>
        /// The dictionary.
        /// </value>
        public string Dictionary { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenProcessDescription"/> class.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="name">The name.</param>
        public ScreenProcessDescription(string fileName, string name)
            : base(fileName, name)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenProcessDescription"/> class.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="name">The name.</param>
        /// <param name="expression">The expression.</param>
        public ScreenProcessDescription(string fileName, string name, string expression)
            : base(fileName, name, expression)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenProcessDescription"/> class.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="name">The name.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="definition">The definition.</param>
        public ScreenProcessDescription(string fileName, string name, string expression, SBString definition)
            : this(fileName, name, expression)
        {
            if (!string.IsNullOrWhiteSpace(definition.Extract(5, 1).Value))
            {
                Dictionary = definition.Extract(5, 1).Value;
                ProcessCollection.Add(new ProcessCall { Description = "Dictionary", ProcessDescription = new DictionaryDescription(Dictionary) });
            }
            if (!string.IsNullOrWhiteSpace(definition.Extract(6).Value))
            {
                ScreenName = definition.Extract(6).Value;
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadScreen(ScreenName, Dictionary, this, "Screen");

                // Now check if there are linked screens. To do this see if the last character is numeric. If it is go back until you find the first non-numeric. Now
                // we have a starting point, loop adding 1 untill there is no hit.

                int startIndex = ScreenName.Length - 1;
                while (SBXA.Shared.Utilities.IsNumber(ScreenName.Substring(startIndex)))
                {
                    startIndex++;
                }
                startIndex--; // because it would have been incremented one beyond.
                if (!string.IsNullOrEmpty(ScreenName.Substring(startIndex)) && Utilities.IsNumber(ScreenName.Substring(startIndex)))
                {
                    var nextScreen = Convert.ToInt16(ScreenName.Substring(startIndex)) + 1;
                    var prefix = ScreenName.Substring(0, startIndex);
                    DebugViewModel.Instance.ProcessAnalysisViewModel.SetIsLoading(1);
                    // I have a starting point, now read the definition.
                    SBFile.ReadDictionaryItem(Dictionary, prefix + nextScreen, new object[] { nextScreen, prefix, Dictionary }, ReadLinkedScreenCompleted);


                }

            }
            // load the list of processes
            if (!string.IsNullOrWhiteSpace(definition.Extract(8, 1).Value))
            {
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(SourceDefinition.Screen, SourceDefinition.Process, definition.Extract(8, 1).Value, this, "Before");
            }
            if (!string.IsNullOrWhiteSpace(definition.Extract(8, 2).Value))
            {
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(SourceDefinition.Screen, SourceDefinition.Process, definition.Extract(8, 2).Value, this, "After");
            }
            if (!string.IsNullOrWhiteSpace(definition.Extract(8, 7).Value))
            {
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(SourceDefinition.Screen, SourceDefinition.Process, definition.Extract(8, 7).Value, this, "Before Display");
            }
            if (!string.IsNullOrWhiteSpace(definition.Extract(8, 8).Value))
            {
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(SourceDefinition.Screen, SourceDefinition.Process, definition.Extract(8, 8).Value, this, "Before Action Bar Option");
            }
            if (!string.IsNullOrWhiteSpace(definition.Extract(8, 4).Value))
            {
                DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(SourceDefinition.Screen, SourceDefinition.Process, definition.Extract(8, 4).Value, this, "After Action Bar Option");
            }
        }

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
            SBFile.ReadDictionaryItem(Dictionary, prefix + nextScreen, new object[] { nextScreen, prefix, Dictionary }, ReadLinkedScreenCompleted);

        }

        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            // First add this screen definition definition
            JobManager.RunInDispatcherThread(DebugWindowManager.DebugConsoleWindow.Dispatcher, DispatcherPriority.Normal, 
                () => RevisionDefinitionViewModel.AddItemToDefinition(collection, new RevisionDefinitionItem { Action = "IO", FileName = FileName, Item = Name, Parameters = RevisionDefinitionViewModel.Data }));


            // Now add any definition calls.
            base.AddChildrenToCollection(collection);

        }


    }
}