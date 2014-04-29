// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessCall.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.Models
{
    /// <summary>
    ///     This class represents a definition call.
    /// </summary>
    public class ProcessCall : TreeItem
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the definition description.
        /// </summary>
        /// <value>
        ///     The definition description.
        /// </value>
        public DefinitionDescription ProcessDescription { get; set; }

        #endregion
    }
}