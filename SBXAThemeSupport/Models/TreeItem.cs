using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SBXAThemeSupport.Models
{
    using SBXAThemeSupport.DebugAssistant.ViewModels;

    public interface ITreeItem
    {
        /// <summary>
        /// Gets or sets the description which is the text that is displayed on some items that describes the item.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string Description { get; set; }

        string Name { get; }
        IEnumerable Children { get; }

        /// <summary>
        ///     The property changed.
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;
    }

    public abstract class TreeItemBase : ObservableEntity
    {
        public abstract void AddChildrenToCollection(RevisionDefinitionItemCollection collection);
        
    }

    public class TreeItem : TreeItemBase, ITreeItem, IDisposable
    {
        private IEnumerable children;

        /// <summary>
        /// Gets or sets the description which is the text that is displayed on some items that describes the item.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        public string Name { get; private set; }

        public IEnumerable Children
        {
            get { return this.children; }
            internal set
            {
                this.children = value;
                RaisePropertyChanged("Children");
            }
        }

        public TreeItem()
        {
            Children = new ObservableCollection<ITreeItem>();
        }

        public TreeItem(string name)
        {
            Name = name;
            Children = new ObservableCollection<ITreeItem>();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~TreeItem()
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
                foreach (var item in Children)
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


        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            foreach (var item in children)
            {
                RevisionDefinitionViewModel.AddItemToDefinition(collection, new RevisionDefinitionItem() { Action = "1", FileName = "fileName", Item = item.GetType().Name, Parameters = "parameters" });
            }
        }
    }
}