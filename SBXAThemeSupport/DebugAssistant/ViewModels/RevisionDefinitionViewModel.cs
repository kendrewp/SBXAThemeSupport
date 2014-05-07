﻿// --------------------------------------------------------------------------------------------------------------------
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

        #endregion

        #region Fields

        private readonly RevisionDefinitionItemCollection revisionDefinitionItemCollection = new RevisionDefinitionItemCollection();

        private string definitionName;

        private bool isAllSelected;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RevisionDefinitionViewModel"/> class.
        /// </summary>
        public RevisionDefinitionViewModel()
        {
            SaveDefinitionCommand = new RelayCommand(this.SaveDefinitionCommandExecuted, this.CanExecuteSaveDefinitionCommand);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the save definition command.
        /// </summary>
        public static ICommand SaveDefinitionCommand { get; private set; }

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
        /// Gets or sets a value indicating whether [is all selected].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is all selected]; otherwise, <c>false</c>.
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

        private static void DefinitionWriteCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            MessageBox.Show("Definition Saved.");
        }

        private bool CanExecuteSaveDefinitionCommand(object parameter)
        {
            return !string.IsNullOrEmpty(this.DefinitionName) && !string.IsNullOrWhiteSpace(this.DefinitionName);
        }

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

        private void SaveDefinitionCommandExecuted(object parameter)
        {
            this.CreateDefinition();
        }

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