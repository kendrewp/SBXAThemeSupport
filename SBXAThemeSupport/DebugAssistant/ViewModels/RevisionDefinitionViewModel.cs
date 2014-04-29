namespace SBXAThemeSupport.DebugAssistant.ViewModels
{
    using System.Windows;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.UI.Client;

    using SBXAThemeSupport.Models;
    using SBXAThemeSupport.ViewModels;
    using ICommand = System.Windows.Input.ICommand;

    /// <summary>
    /// This class represents an item in a revision definition.
    /// </summary>
    public class RevisionDefinitionViewModel : ViewModel
    {
        /// <summary>
        /// The dictionary
        /// </summary>
        public const string Dict = "1";

        /// <summary>
        /// The data
        /// </summary>
        public const string Data = "3";

        /// <summary>
        /// The dictionary and data
        /// </summary>
        public const string DictAndData = "2";

        private readonly RevisionDefinitionItemCollection revisionDefinitionItemCollection = new RevisionDefinitionItemCollection();

        private bool isAllSelected;
        private string definitionName;

        public RevisionDefinitionItemCollection RevisionDefinitionItemCollection
        {
            get
            {
                return revisionDefinitionItemCollection;
            }
        }

        /// <summary>
        /// Creates the revision definition.
        /// </summary>
        /// <param name="startItem">The start item.</param>
        public void CreateRevisionDefinition(TreeItem startItem)
        {
            startItem.AddChildrenToCollection(RevisionDefinitionItemCollection);
        }

        internal static void AddItemToDefinition(RevisionDefinitionItemCollection collection, RevisionDefinitionItem item)
        {
            if (string.IsNullOrEmpty(item.FileName))
            {
                // do not add items that do not have a file name.
                return;
            }
            if (item.Action.Equals("IO") && (string.IsNullOrEmpty(item.FileName) || string.IsNullOrEmpty(item.Item)))
            {
                // do not add items that do not have a file name and item name if the action is IO.
                return;
            }
            if (item.FileName.Equals("DM") || item.FileName.Equals("DMUT") || item.FileName.Equals("DMSH") || item.FileName.Equals("DMGC") || item.FileName.Equals("DMGD") || item.FileName.Equals("DMGC"))
            {
                // do not include SB/XA program files.
                return;
            }
            if (!collection.ContainsItem(item.FileName, item.Item))
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is all selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is all selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsAllSelected
        {
            get
            {
                return this.isAllSelected;
            }
            set
            {
                if (this.isAllSelected != value)
                {
                    this.isAllSelected = value;
                    SelectAll(this.isAllSelected);
                    this.RaisePropertyChanged("IsAllSelected");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the definition.
        /// </summary>
        /// <value>
        /// The name of the definition.
        /// </value>
        public string DefinitionName
        {
            get
            {
                return this.definitionName;
            }
            set
            {
                if (this.definitionName != value)
                {
                    this.definitionName = value;
                    this.RaisePropertyChanged("DefinitionName");
                }
            }
        }

        private void SelectAll(bool select)
        {
            foreach (var item in RevisionDefinitionItemCollection)
            {
                item.Include = select;
            }
        }

        private void CreateDefinition()
        {
            SBString defn = new SBString();
            var actions = new SBString();
            var fileNames = new SBString();
            var items = new SBString();
            var parameters = new SBString();

            foreach (var item in revisionDefinitionItemCollection)
            {
                if (!item.Include)
                {
                    continue;
                }

                actions.Add(item.Action);
                fileNames.Add(item.FileName);
                items.Add(item.Item);
                parameters.Add(item.Parameters);
            }

            defn.Add(actions);
            defn.Add(fileNames);
            defn.Add(items);
            defn.Add(parameters);

            JobManager.RunInUIThread(DispatcherPriority.Normal,
                delegate
                    {
                        var defnName = "REV.DEFN*" + DefinitionName + "*1";
                        SBFile.Write(SBPlusClient.Current.SystemId + "DEFN", defnName, defn, DefinitionWriteCompleted);
                    });
        }

        private static void DefinitionWriteCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            MessageBox.Show("Definition Saved.");
        }

        public RevisionDefinitionViewModel()
        {
            SaveDefinitionCommand = new RelayCommand(SaveDefinitionCommandExecuted, CanExecuteSaveDefinitionCommand);
        }

        public static ICommand SaveDefinitionCommand { get; private set; }

        private bool CanExecuteSaveDefinitionCommand(object parameter)
        {
            return !string.IsNullOrEmpty(DefinitionName) && !string.IsNullOrWhiteSpace(DefinitionName);
        }

        private void SaveDefinitionCommandExecuted(object parameter)
        {
            CreateDefinition();
        }

    }
}
