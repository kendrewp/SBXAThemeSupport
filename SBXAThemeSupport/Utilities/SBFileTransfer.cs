// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SBFileTransfer.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Utilities
{
    using System.IO;

    using SBXA.Runtime;
    using SBXA.Shared;

    /// <summary>
    ///     The sb file transfer.
    /// </summary>
    public class SBFileTransfer
    {
        #region Public Methods and Operators

        /// <summary>
        /// The upload.
        /// </summary>
        /// <param name="sourceFileName">
        /// The source file name.
        /// </param>
        /// <param name="targetFileName">
        /// The target file name.
        /// </param>
        /// <param name="itemId">
        /// The item id.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="uploadCompleted">
        /// The upload completed.
        /// </param>
        public static void Upload(
            string sourceFileName, 
            string targetFileName, 
            string itemId, 
            string options, 
            SubroutineCallCompleted uploadCompleted)
        {
            // SUBROUTINE TU.UPLOAD(DOS.FILE.NAME, F.RXFILE, ITEMID, OPTIONS, DESCRIPTION, STATUS.REC)
            // MESSAGE = DOS.FILE.NAME : @AM : @AM : FILE.NAME : @AM : ITEMID
            // FT.OPTIONS = OPTIONS : "O"
            // DATA = ""
            // CALL SB.FT.MASTER(3, MESSAGE, DATA, FT.OPTIONS, STATUS.REC)
            if (!File.Exists(sourceFileName))
            {
                return;
            }
            var message = new SBString();
            message.SBInsert(1, sourceFileName);
            message.SBInsert(3, targetFileName);
            message.SBInsert(4, itemId);

            SbProcessHandler.CallSubroutine(
                uploadCompleted, 
                "SB.FT.MASTER", 
                new[] { new SBString("3"), message, new SBString(), new SBString(options + "O"), new SBString() }, 
                new object[] { sourceFileName, targetFileName, itemId, options });

            /*
            SbProcessHandler.CallSubroutine(
                "SB.FT.MASTER", 
                5, 
                new[] { new SBString("3"), message, new SBString(), new SBString(options + "O"), new SBString() }, 
                new object[] { sourceFileName, targetFileName, itemId, options }, 
                uploadCompleted);
*/
        }

        #endregion
    }
}