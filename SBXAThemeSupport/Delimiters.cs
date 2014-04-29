// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Delimiters.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport
{
    /// <summary>
    ///     The delimiters.
    /// </summary>
    public static class Delimiters
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="Delimiters" /> class.
        /// </summary>
        static Delimiters()
        {
            AttributeMark = (char)26;
            ValueMark = (char)25;
            SubValueMark = (char)11;
            FileDelimitter = (char)24;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the attribute mark.
        /// </summary>
        public static char AttributeMark { get; private set; }

        /// <summary>
        ///     Gets the file delimitter.
        /// </summary>
        public static char FileDelimitter { get; private set; }

        /// <summary>
        ///     Gets the sub value mark.
        /// </summary>
        public static char SubValueMark { get; private set; }

        /// <summary>
        ///     Gets the value mark.
        /// </summary>
        public static char ValueMark { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The set delimiters.
        /// </summary>
        /// <param name="attributeMark">
        ///     The attribute mark.
        /// </param>
        /// <param name="valueMark">
        ///     The value mark.
        /// </param>
        /// <param name="subValueMark">
        ///     The sub value mark.
        /// </param>
        /// <param name="fileDelimitter">
        ///     The file delimitter.
        /// </param>
        public static void SetDelimiters(char attributeMark, char valueMark, char subValueMark, char fileDelimitter)
        {
            AttributeMark = attributeMark;
            ValueMark = valueMark;
            SubValueMark = subValueMark;
            FileDelimitter = fileDelimitter;
        }

        #endregion
    }
}