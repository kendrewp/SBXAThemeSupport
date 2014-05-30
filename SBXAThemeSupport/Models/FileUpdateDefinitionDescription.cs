// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUpdateDefinitionDescription.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using SBXA.Shared;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     This class contains all the info required to display a File Update definition.
    /// </summary>
    public class FileUpdateDefinitionDescription : DefinitionDescription
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUpdateDefinitionDescription"/> class.
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
        public FileUpdateDefinitionDescription(string fileName, string name, string expression, SBString definition)
            : base(fileName, name, SourceDefinition.Process, expression)
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