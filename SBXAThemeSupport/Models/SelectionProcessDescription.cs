// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectionProcessDescription.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using SBXA.Shared;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     The selection process description.
    /// </summary>
    public class SelectionProcessDescription : SBExpression
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionProcessDescription"/> class.
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
        public SelectionProcessDescription(string fileName, string name, SourceDefinition hookType, string sysId, SBString definition)
            : base(fileName, name, hookType, sysId)
        {
            this.Name = name;
        }

        #endregion

        public override string SourceExpression { protected set; get; }


        }
}