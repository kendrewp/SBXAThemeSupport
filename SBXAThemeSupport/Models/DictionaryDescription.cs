// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryDescription.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.Models
{
    /// <summary>
    /// The dictionary description.
    /// </summary>
    public class DictionaryDescription : DefinitionDescription
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryDescription"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        public DictionaryDescription(string fileName)
            : base(fileName, fileName)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add children to collection.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            base.AddChildrenToCollection(collection);
            // There is not dictionary only the fields, etc. in the dict so do not add it the name of the dictionary.

            // now add all the fields.
        }

        #endregion

        protected override void AddSelf(RevisionDefinitionItemCollection collection)
        {
            // I do not want to add the name of the dictionary. base.AddSelf(collection);
        }
    }
}