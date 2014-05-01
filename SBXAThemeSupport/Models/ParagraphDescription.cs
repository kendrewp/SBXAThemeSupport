// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParagraphDescription.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using SBXA.Shared;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     This class represents a paragraph.
    /// </summary>
    public class ParagraphDescription : DefinitionDescription
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParagraphDescription"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="definition">
        /// The definition.
        /// </param>
        public ParagraphDescription(string fileName, string name, string expression, SBString definition)
            : base(fileName, name, expression)
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
            if (!this.IsError)
            {
                RevisionDefinitionViewModel.AddItemToDefinition(
                    collection, 
                    new RevisionDefinitionItem()
                        {
                            Action = "IO", 
                            FileName = this.FileName, 
                            Item = this.Name, 
                            Parameters = RevisionDefinitionViewModel.Data
                        });
            }

            base.AddChildrenToCollection(collection);
        }

        #endregion
    }
}