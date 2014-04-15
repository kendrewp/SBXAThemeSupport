// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XuiDebug.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport
{
    using SBXA.Runtime;
    using SBXA.Shared;

    /// <summary>
    /// This class is a front end to the basic program XUI.DEBUG
    /// </summary>
    /// <remarks>
    ///     * 1 - Turn DEBUG ON/OFF. PARAM1 contins 0 - Off, 1 - On
    ///     * 2 - Process Called
    ///     * 3 - Returned from process
    ///     * 4 - Get common var
    ///     * 5 - Send message to the client.
    ///     * 6 - Get the current setting of ISDebug
    ///     * 7 - Write record.
    /// </remarks>
    internal class XuiDebug
    {
        #region Methods

        internal static void EnableDebug(SubroutineCallCompleted subroutineCallCompleted, bool enable)
        {
            SbProcessHandler.CallSubroutine(
                subroutineCallCompleted, 
                "XUI.DEBUG", 
                new[]
                    {
                        new SBString("1"), new SBString(enable ? "1" : "0"), new SBString(), new SBString(), new SBString("0"), new SBString()
                    }, 
                new object[0]);
        }

        internal static void GetCommonVariable(SubroutineCallCompleted subroutineCallCompleted, string whichVariable)
        {
            SbProcessHandler.CallSubroutine(
                subroutineCallCompleted, 
                "XUI.DEBUG", 
                new[] { new SBString("4"), new SBString(whichVariable), new SBString(), new SBString(), new SBString("0"), new SBString() }, 
                new object[0]);
        }

        internal static void IsDebugEnabled(SubroutineCallCompleted subroutineCallCompleted)
        {
            SbProcessHandler.CallSubroutine(
                subroutineCallCompleted, 
                "XUI.DEBUG", 
                new[] { new SBString("6"), new SBString(), new SBString(), new SBString(), new SBString("0"), new SBString() }, 
                new object[0]);
        }

        internal static void IsXuiDebugThere(SubroutineCallCompleted subroutineCallCompleted)
        {
            SBFile.Read("VOC", "XUI.DEBUG", subroutineCallCompleted);
        }

        /// <summary>
        ///     Writes the record.
        /// </summary>
        /// <remarks>
        ///     FILE.NAME = PARAM1&lt;1&gt;
        ///     ID = PARAM1&lt;2&gt;
        ///     ATTR = PARAM1&lt;3&gt;
        ///     MODE = PARAM1&lt;4&gt;
        ///     REC = PARAM2
        /// </remarks>
        /// <param name="writeCompleted">The write completed.</param>
        /// <param name="itemInfo">The item information.</param>
        /// <param name="record">The record.</param>
        internal static void WriteRecord(SubroutineCallCompleted writeCompleted, SBString itemInfo, SBString record)
        {
            SbProcessHandler.CallSubroutine(
                writeCompleted, 
                "XUI.DEBUG", 
                new[] { new SBString("7"), itemInfo, record, new SBString(), new SBString("0"), new SBString() }, 
                new object[0]);
        }

        #endregion
    }
}