// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RevisionDefinitionItemCollection.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using System.Collections.ObjectModel;

    using SBXA.Shared;

    /// <summary>
    ///     The revision definition item.
    /// </summary>
    public class RevisionDefinitionItem : SBEntityBase
    {
        #region Fields

        /// <summary>
        /// The include.
        /// </summary>
        private bool include;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the action.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        ///     Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [include].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [include]; otherwise, <c>false</c>.
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
        public string Item { get; set; }

        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        public string Parameters { get; set; }

        #endregion
    }

    /// <summary>
    ///     This is a collection of <see cref="RevisionDefinitionItem" />
    /// </summary>
    public class RevisionDefinitionItemCollection : ObservableCollection<RevisionDefinitionItem>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The contains item.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="itemName">
        /// The item name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ContainsItem(string action, string fileName, string itemName)
        {
            foreach (var item in this)
            {
                if (item.Action.Equals(action) && item.FileName.Equals(fileName) && item.Item.Equals(itemName))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}