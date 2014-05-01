// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectionProcessDescription.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.Models
{
    using SBXA.Shared;

    /// <summary>
    /// The selection process description.
    /// </summary>
    public class SelectionProcessDescription : DefinitionDescription
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionProcessDescription"/> class. 
        /// Initializes a new instance of the <see cref="InputDefinitionDescription"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="definition">
        /// The definition.
        /// </param>
        public SelectionProcessDescription(string fileName, string name, SBString definition)
            : base(fileName, name)
        {
        }

        #endregion
    }
}