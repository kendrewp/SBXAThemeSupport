// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputDefinitionDescription.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using SBXA.Shared;

    /// <summary>
    ///     This class represents a intput definition.
    /// </summary>
    public class InputDefinitionDescription : ScreenProcessDescription
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InputDefinitionDescription"/> class.
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
        public InputDefinitionDescription(string fileName, string name, string expression, SBString definition)
            : base(fileName, name, expression, definition)
        {
        }

        #endregion
    }
}