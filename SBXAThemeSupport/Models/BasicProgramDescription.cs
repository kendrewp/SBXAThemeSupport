// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicProgramDescription.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     This class represents a basic program.
    /// </summary>
    public class BasicProgramDescription : DefinitionDescription
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicProgramDescription"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public BasicProgramDescription(string fileName, string name)
            : base(fileName, name)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the object file location.
        /// </summary>
        public string ObjectFileLocation { get; set; }

        /// <summary>
        ///     Gets or sets the object location.
        /// </summary>
        public string ObjectLocation { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether parsed.
        /// </summary>
        public bool Parsed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is include.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is include; otherwise, <c>false</c>.
        /// </value>
        public static bool IsInclude { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The add self.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        protected override void AddSelf(RevisionDefinitionItemCollection collection)
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

                var parameters = RevisionDefinitionViewModel.SourceAndObject;
                if (IsInclude)
                {
                    parameters += ",1,1";
                }


                RevisionDefinitionViewModel.AddItemToDefinition(
                    collection,
                    new RevisionDefinitionItem()
                        {
                            Action = "FC",
                            FileName = this.FileName,
                            Item = string.Empty,
                            Parameters = parameters
                        });

                if (!IsInclude)
                {
                    RevisionDefinitionViewModel.AddItemToDefinition(collection, 
                    new RevisionDefinitionItem() { Action = "FB", FileName = this.FileName, Item = string.Empty, Parameters = string.Empty });
            }
        }
        }

        #endregion
    }
}