// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeItem.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     The TreeItem interface.
    /// </summary>
    public interface ITreeItem
    {
        #region Public Events

        /// <summary>
        ///     The property changed.
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the children.
        /// </summary>
        IEnumerable Children { get; }

        /// <summary>
        ///     Gets or sets the description which is the text that is displayed on some items that describes the item.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        string Description { get; set; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        string Name { get; }

        #endregion
    }

    /// <summary>
    ///     The tree item base.
    /// </summary>
    public abstract class TreeItemBase : ObservableEntity
    {
        #region Public Methods and Operators

        /// <summary>
        /// The add children to collection.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        public abstract void AddChildrenToCollection(RevisionDefinitionItemCollection collection);

        #endregion
    }

    /// <summary>
    ///     The tree item.
    /// </summary>
    public class TreeItem : TreeItemBase, ITreeItem, IDisposable
    {
        #region Fields

        private IEnumerable children;

        private bool isError;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TreeItem" /> class.
        /// </summary>
        public TreeItem()
        {
            this.Children = new ObservableCollection<ITreeItem>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItem"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public TreeItem(string name)
        {
            this.Name = name;
            this.Children = new ObservableCollection<ITreeItem>();
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="TreeItem" /> class.
        /// </summary>
        ~TreeItem()
        {
            // Finalizer calls Dispose(false)
            this.Dispose(false);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is error; otherwise, <c>false</c>.
        /// </value>
        public bool IsError
        {
            get
            {
                return this.isError;
            }
            set
            {
                this.isError = value;
                this.RaisePropertyChanged("IsError");
            }
        }

        /// <summary>
        ///     Gets the children.
        /// </summary>
        public IEnumerable Children
        {
            get
            {
                return this.children;
            }

            internal set
            {
                this.children = value;
                this.RaisePropertyChanged("Children");
            }
        }

        /// <summary>
        ///     Gets or sets the description which is the text that is displayed on some items that describes the item.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public string Name { get; private set; }

        #endregion

        // The bulk of the clean-up code is implemented in Dispose(bool)
        #region Public Methods and Operators

        /// <summary>
        /// The add children to collection.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            foreach (var item in this.children)
            {
                RevisionDefinitionViewModel.AddItemToDefinition(
                    collection, 
                    new RevisionDefinitionItem()
                        {
                            Action = "1", 
                            FileName = "fileName", 
                            Item = item.GetType().Name, 
                            Parameters = "parameters"
                        });
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                foreach (var item in this.Children)
                {
                    if (item is IDisposable)
                    {
                        ((IDisposable)item).Dispose();
                    }
                }

                var coll = this.Children as ObservableCollection<ITreeItem>;
                if (coll != null)
                {
                    coll.Clear();
                }
            }
        }

        #endregion
    }
}