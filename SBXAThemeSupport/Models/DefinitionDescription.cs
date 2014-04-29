
namespace SBXAThemeSupport.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    /// This class is the basis for SB/XA definitions, definition, field, etc.
    /// </summary>
    public class DefinitionDescription : TreeItem
    {
        private bool isCurrent;
        public string SourceExpression { get; private set; }
        public string FileName { get; set; }
        public bool IsError { get; set; }

        public DefinitionDescription HistoryProcessDescription { get; set; }

        public ObservableCollection<ProcessCall> ProcessCollection { get; private set; }

        public ProcessStack ChildProcesses { get; private set; }

        public ObservableCollection<SBExpression> DictionaryExpressions { get; set; }
        public ObservableCollection<SBExpression> ScreenExpressions { get; set; }
        public ObservableCollection<SBExpression> Expressions { get; set; } 


        public bool IsCurrent
        {
            get { return this.isCurrent; }
            set
            {
                this.isCurrent = value;
                RaisePropertyChanged("IsCurrent");
            }
        }

        public DefinitionDescription(string fileName, string name)
            : base(name)
        {
            this.ChildProcesses = new ProcessStack();
            this.ProcessCollection = new ObservableCollection<ProcessCall>();
            this.DictionaryExpressions = new ObservableCollection<SBExpression>();
            this.Expressions = new ObservableCollection<SBExpression>();
            this.ScreenExpressions = new ObservableCollection<SBExpression>();

            this.FileName = fileName;
        }

        public DefinitionDescription(string fileName, string name, string expression)
            : this(fileName, name)
        {
            if (!expression.Equals(name))
            {
                SourceExpression = expression;
            }
        }

        public void Clear()
        {
            foreach (var item in ChildProcesses)
            {
                item.Clear();
            }
            ChildProcesses.Clear();
        }

        public void ClearHistoryReferences()
        {
            HistoryProcessDescription = null;
            foreach (var item in ChildProcesses)
            {
                item.ClearHistoryReferences();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                foreach (var item in ChildProcesses)
                {
                    if (item != null)
                    {
                        ((IDisposable)item).Dispose();
                    }
                }
                foreach (var item in ProcessCollection)
                {
                    if (item != null)
                    {
                        ((IDisposable)item).Dispose();
                    }
                }
            }
            base.Dispose(disposing);

        }


        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            foreach (var item in ProcessCollection)
            {
                item.ProcessDescription.AddChildrenToCollection(collection);
            }

            foreach (var sbExpression in DictionaryExpressions)
            {
                sbExpression.AddChildrenToCollection(collection);
            }

            foreach (var sbExpression in ScreenExpressions)
            {
                sbExpression.AddChildrenToCollection(collection);
            }

            foreach (var sbExpression in Expressions)
            {
                sbExpression.AddChildrenToCollection(collection);
            }

            base.AddChildrenToCollection(collection);

            this.AddSelf(collection);
        }

        protected virtual void AddSelf(RevisionDefinitionItemCollection collection)
        {
            if (!IsError)
            {
                // do this last so it will only happen if it has not already happened.
                var parameters = RevisionDefinitionViewModel.Data;
                var fname = FileName;
                if (FileName.StartsWith("DICT "))
                {
                    parameters = RevisionDefinitionViewModel.Dict;
                    fname = FileName.Substring(5);
                }
                RevisionDefinitionViewModel.AddItemToDefinition(collection, new RevisionDefinitionItem { Action = "IO", FileName = fname, Item = Name, Parameters = parameters });
            }
        }
    }

    public class ProcessCollection : Collection<DefinitionDescription>, IDisposable
    {
        public bool ContainsKey(string fileName, string processName)
        {
            return this.Any(processDescription => (processDescription.FileName.Equals(fileName) && processDescription.Name.Equals(processName)));
        }

        public bool ContainsKey(string fileName, string processName, Type type)
        {
            if (type == typeof(BasicProgramDescription))
            {
                // do not check against the file name as 1) it may not be there yet and 2) it does not matter as there can only be one program with the name cataloged (at least locally).
                return this.Any(processDescription => (processDescription.Name.Equals(processName) && processDescription.GetType() == type));
            }
            return this.Any(processDescription => (processDescription.Name.Equals(processName) && processDescription.FileName.Equals(fileName) && processDescription.GetType() == type));
        }

        public DefinitionDescription this[string key]
        {
            get
            {
                return this.FirstOrDefault(processDescription => processDescription.Name.Equals(key));
            }
        }

                /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~ProcessCollection()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                foreach (var item in this)
                {
                    if (item != null)
                    {
                        ((IDisposable)item).Dispose();
                    }
                }
                this.Clear();
            }
        }

    }
}