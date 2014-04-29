// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RevisionDefinitionViewModel.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.DebugAssistant.ViewModels
{
    using SBXAThemeSupport.Models;

    /// <summary>
    /// The revision definition view model.
    /// </summary>
    public class RevisionDefinitionViewModel
    {
        #region Constants

        /// <summary>
        /// The data.
        /// </summary>
        public const string Data = "3";

        /// <summary>
        /// The dict.
        /// </summary>
        public const string Dict = "1";

        /// <summary>
        /// The dict and data.
        /// </summary>
        public const string DictAndData = "2";

        #endregion

        #region Fields

        private readonly RevisionDefinitionItemCollection revisionDefinitionItemCollection = new RevisionDefinitionItemCollection();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the revision definition item collection.
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

            if (item.FileName.Equals("DM") || item.FileName.Equals("DMUT") || item.FileName.Equals("DMSH") || item.FileName.Equals("DMGC")
                || item.FileName.Equals("DMGD") || item.FileName.Equals("DMGC"))
            {
                // do not include SB/XA program files.
                return;
            }

            if (!collection.ContainsItem(item.FileName, item.Item))
            {
                collection.Add(item);
            }
        }

        #endregion
    }
}