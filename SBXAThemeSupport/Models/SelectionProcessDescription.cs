namespace SBXAThemeSupport.Models
{
    using SBXA.Shared;

    public class SelectionProcessDescription : DefinitionDescription
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputDefinitionDescription" /> class.
        /// </summary>
        /// <param name="fileName">
        ///     The file name.
        /// </param>
        /// <param name="name">
        ///     The name.
        /// </param>
        /// <param name="expression">
        ///     The expression.
        /// </param>
        /// <param name="definition">
        ///     The definition.
        /// </param>
        public SelectionProcessDescription(string fileName, string name, SBString definition)
            : base(fileName, name)
        {
        }

        #endregion
    }
}