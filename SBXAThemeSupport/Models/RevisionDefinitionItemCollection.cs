// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RevisionDefinitionItemCollection.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.Models
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// The revision definition item.
    /// </summary>
    public class RevisionDefinitionItem
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        public string Parameters { get; set; }

        #endregion
    }

    /// <summary>
    /// The revision definition item collection.
    /// </summary>
    public class RevisionDefinitionItemCollection : ObservableCollection<RevisionDefinitionItem>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The contains item.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="itemName">
        /// The item name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
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

        #endregion
    }
}