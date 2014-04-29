// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputDefinitionDescription.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.Models
{
    using SBXA.Shared;

    /// <summary>
    ///     This class represents a output process.
    /// </summary>
    public class OutputDefinitionDescription : ScreenProcessDescription
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputDefinitionDescription"/> class.
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
        public OutputDefinitionDescription(string fileName, string name, string expression, SBString definition)
            : base(fileName, name, expression, definition)
        {
        }

        #endregion
    }
}