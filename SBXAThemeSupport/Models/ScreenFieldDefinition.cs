// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScreenFieldDefinition.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="ScreenFieldDefinition.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    /// <summary>
    /// The screen field definition.
    /// </summary>
    public class ScreenFieldDefinition : FieldDefinition
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenFieldDefinition"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public ScreenFieldDefinition(string fileName, string name)
            : base(fileName, name)
        {
        }

        #endregion
    }
}