// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RevisionDefinitionViewModel.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
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
    ///     The revision definition view model.
    /// </summary>
    public class RevisionDefinitionViewModel : ViewModel
    {
        #region Constants

        /// <summary>
        ///     The data.
        /// </summary>
        public const string Data = "3";

        /// <summary>
        ///     The dict.
        /// </summary>
        public const string Dict = "1";

        /// <summary>
        ///     The dict and data.
        /// </summary>
        public const string DictAndData = "2";

        /// <summary>
        ///     The source and object
        /// </summary>
        public const string SourceAndObject = "5";

        #endregion

        #region Fields

        /// <summary>
        /// The revision definition item collection.
        /// </summary>
        private readonly RevisionDefinitionItemCollection revisionDefinitionItemCollection = new RevisionDefinitionItemCollection();

        /// <summary>
        /// The current action.
        /// </summary>
        private string currentAction;

        /// <summary>
        /// The definition name.
        /// </summary>
        private string definitionName;

        /// <summary>
        /// The is all selected.
        /// </summary>
        private bool isAllSelected;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RevisionDefinitionViewModel" /> class.
        /// </summary>
        public RevisionDefinitionViewModel()
        {
            SaveDefinitionCommand = new RelayCommand(this.SaveDefinitionCommandExecuted, this.CanExecuteSaveDefinitionCommand);
            CreateFileCommand = new RelayCommand(this.CreateFileCommandExecuted, this.CanExecuteCreateFileCommandCommand);
            MaintainDefinitionCommand = new RelayCommand(
                this.MaintainDefinitionCommandExecuted, 
                this.CanExecuteMaintainDefinitionCommandCommand);
            CreateDefinitionCommand = new RelayCommand(this.CreateDefinitionCommandExecuted, this.CanExecuteCreateDefinitionCommand);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the save definition command.
        /// </summary>
        /// <value>
        ///     The create definition command.
        /// </value>
        public static ICommand CreateDefinitionCommand { get; private set; }

        /// <summary>
        ///     Gets the create file command.
        /// </summary>
        /// <value>
        ///     The create file command.
        /// </value>
        public static ICommand CreateFileCommand { get; private set; }

        /// <summary>
        ///     Gets the maintain definition command.
        /// </summary>
        /// <value>
        ///     The maintain definition command.
        /// </value>
        public static ICommand MaintainDefinitionCommand { get; private set; }

        /// <summary>
        ///     Gets the save definition command.
        /// </summary>
        public static ICommand SaveDefinitionCommand { get; private set; }

        /// <summary>
        ///     Gets or sets the current action.
        /// </summary>
        /// <value>
        ///     The current action.
        /// </value>
        public string CurrentAction
        {
            get
            {
                return this.currentAction;
            }

            set
            {
                if (this.currentAction != value)
                {
                    this.currentAction = value;
                    this.RaisePropertyChanged("CurrentAction");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the name of the definition.
        /// </summary>
        /// <value>
        ///     The name of the definition.
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

        /// <summary>
        ///     Gets or sets a value indicating whether [is all selected].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [is all selected]; otherwise, <c>false</c>.
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
                    this.SelectAll(this.isAllSelected);
                    this.RaisePropertyChanged("IsAllSelected");
                }
            }
        }

        /// <summary>
        ///     Gets the revision definition item collection.
        /// </summary>
        public RevisionDefinitionItemCollection RevisionDefinitionItemCollection
        {
            get
            {
                return this.revisionDefinitionItemCollection;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create revision definition.
        /// </summary>
        /// <param name="startItem">
        /// The start item.
        /// </param>
        public void CreateRevisionDefinition(TreeItem startItem)
        {
            startItem.AddChildrenToCollection(this.RevisionDefinitionItemCollection);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add item to definition.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
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

            if (ProcessAnalysisViewModel.IsExcludeFile(item.FileName))
            {
                // do not include SB/XA program files.
                return;
            }

            if (!collection.ContainsItem(item.Action, item.FileName, item.Item))
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// The definition write completed.
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
        private static void DefinitionWriteCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            MessageBox.Show("Definition Saved.");
        }

        /// <summary>
        /// The can execute create definition command.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CanExecuteCreateDefinitionCommand(object parameter)
        {
            return !string.IsNullOrEmpty(this.DefinitionName) && !string.IsNullOrWhiteSpace(this.DefinitionName);
        }

        /// <summary>
        /// The can execute create file command command.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CanExecuteCreateFileCommandCommand(object parameter)
        {
            return !string.IsNullOrEmpty(this.DefinitionName) && !string.IsNullOrWhiteSpace(this.DefinitionName);
        }

        /// <summary>
        /// The can execute maintain definition command command.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CanExecuteMaintainDefinitionCommandCommand(object parameter)
        {
            return !string.IsNullOrEmpty(this.DefinitionName) && !string.IsNullOrWhiteSpace(this.DefinitionName)
                   && ApplicationHelper.CanSendServerCommands();
        }

        /// <summary>
        /// The can execute save definition command.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CanExecuteSaveDefinitionCommand(object parameter)
        {
            return !string.IsNullOrEmpty(this.DefinitionName) && !string.IsNullOrWhiteSpace(this.DefinitionName);
        }

        /// <summary>
        /// The create definition.
        /// </summary>
        private void CreateDefinition()
        {
            var defn = new SBString();
            var actions = new SBString();
            var fileNames = new SBString();
            var items = new SBString();
            var parameters = new SBString();

            foreach (var item in this.revisionDefinitionItemCollection)
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

            JobManager.RunInUIThread(
                DispatcherPriority.Normal, 
                delegate
                    {
                        var defnName = "REV.DEFN*" + this.DefinitionName + "*1";
                        SBFile.Write(SBPlusClient.Current.SystemId + "DEFN", defnName, defn, DefinitionWriteCompleted);
                    });
        }

        /// <summary>
        /// The create definition command executed.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        private void CreateDefinitionCommandExecuted(object parameter)
        {
            JobManager.RunInUIThread(DispatcherPriority.Normal, () => SbProcessHandler.CallProcess("REV.MAKE.MEDIA", false));
            /*
            var cmd = string.Format("P:(DATA '{0}';EXEC 'REV.DEFN')", this.DefinitionName);
            JobManager.RunInUIThread(DispatcherPriority.Normal, () => SbProcessHandler.CallProcess(cmd, false));
*/
        }

        /// <summary>
        /// The create file command executed.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        private void CreateFileCommandExecuted(object parameter)
        {
            this.CurrentAction = "Checking if " + this.DefinitionName + " exists.";
            SBFile.Read("VOC", "REV_" + this.DefinitionName, this.CreateRevFileFile, this.DefinitionName);
        }

        /// <summary>
        /// The create file completed.
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
        private void CreateFileCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            var fileName = ((object[])userState)[0] as string;
            this.CurrentAction = "Checking if " + fileName + " exists.";
            SBFile.Read("VOC", fileName, this.FinisedCreateRevFileCompleted, fileName);
        }

        /// <summary>
        /// The create rev file file.
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
        private void CreateRevFileFile(string subroutineName, SBString[] parameters, object userState)
        {
            var status = parameters[5];
            var fileName = userState as string;
            if (status.Count != 1 || !status.Value.Equals("0"))
            {
                SBString cmd = null;
                switch (ApplicationHelper.Platform)
                {
                    case Platform.UniData:
                        cmd = new SBString(string.Format(">:CREATE-FILE DIR REV_{0}", fileName));
                        break;
                    case Platform.UniVerse:
                        cmd = new SBString(string.Format(">:CREATE-FILE REV_{0} 1,1,18 1,1,19", fileName));
                        break;
                }

                if (cmd != null)
                {
                    this.CurrentAction = "Executing " + cmd.Value;
                    JobManager.RunInUIThread(
                        DispatcherPriority.Normal, 
                        () =>
                        SbProcessHandler.CallSubroutine(
                            this.CreateFileCompleted, 
                            "SB.PROCESS", 
                            new[] { cmd }, 
                            new object[] { "REV_" + this.DefinitionName }));
                }

                return;
            }

            this.CurrentAction = string.Format("{0} already exists, so we did not attempt to create it.", fileName ?? "NULL!");
            MessageBox.Show(this.CurrentAction);
        }

        /// <summary>
        /// The finised create rev file completed.
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
        private void FinisedCreateRevFileCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            var fileName = userState as string;
            var status = parameters[5];
            if (status.Count != 1 || !status.Value.Equals("0"))
            {
                this.CurrentAction = "Failed to created file " + (fileName ?? "NULL!");
                MessageBox.Show("Failed to created file " + (fileName ?? "NULL!"));
                return;
            }

            this.CurrentAction = "Created file " + (fileName ?? "NULL!");
            MessageBox.Show("Created file " + (fileName ?? "NULL!"));
        }

        /// <summary>
        /// The maintain definition command executed.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        private void MaintainDefinitionCommandExecuted(object parameter)
        {
            var cmd = string.Format("P:(DATA '{0}';EXEC 'REV.DEFN')", this.DefinitionName);
            JobManager.RunInUIThread(DispatcherPriority.Normal, () => SbProcessHandler.CallProcess(cmd, false));
        }

        /// <summary>
        /// The save definition command executed.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        private void SaveDefinitionCommandExecuted(object parameter)
        {
            this.CreateDefinition();
        }

        /// <summary>
        /// The select all.
        /// </summary>
        /// <param name="select">
        /// The select.
        /// </param>
        private void SelectAll(bool select)
        {
            foreach (var item in this.RevisionDefinitionItemCollection)
            {
                item.Include = select;
            }
        }

        #endregion
    }
}