namespace SBXAThemeSupport.Models
{
    using System.Collections.ObjectModel;

    using SBXA.Shared;

    /// <summary>
    ///     This class represents a single revision definition item.
    /// </summary>
    public class RevisionDefinitionItem : SBEntityBase
    {
        private bool include;

        /// <summary>
        ///     Gets or sets the action.
        /// </summary>
        /// <value>
        ///     The action.
        /// </value>
        public string Action { get; set; }

        /// <summary>
        ///     Gets or sets the name of the file.
        /// </summary>
        /// <value>
        ///     The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="RevisionDefinitionItem" /> is include.
        /// </summary>
        /// <value>
        ///     <c>true</c> if include; otherwise, <c>false</c>.
        /// </value>
        public bool Include
        {
            get
            {
                return this.include;
            }
            set
            {
                if (this.include != value)
                {
                    bool oldValue = this.include;
                    this.include = value;
                    this.OnPropertyChanged("Include", oldValue, this.include);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public string Item { get; set; }

        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        public string Parameters { get; set; }
    }

    /// <summary>
    ///     This is a collection of <see cref="RevisionDefinitionItem" />
    /// </summary>
    public class RevisionDefinitionItemCollection : ObservableCollection<RevisionDefinitionItem>
    {
        /// <summary>
        ///     Determines whether the specified file name contains item.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="itemName">Name of the item.</param>
        /// <returns></returns>
        public bool ContainsItem(string fileName, string itemName)
        {
            foreach (var item in this)
            {
                if (item.FileName.Equals(fileName) && item.Item.Equals(itemName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}