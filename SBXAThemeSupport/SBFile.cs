// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SBFile.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="SBFile.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    using Ionic.Zip;

    using SBXA.Runtime;
    using SBXA.Shared;

    /// <summary>
    ///     Generic interface to a file on the server.
    /// </summary>
    public class SBFile
    {
        #region Public Methods and Operators

        /// <summary>
        ///     The read.
        /// </summary>
        /// <param name="fileName">
        ///     The file name.
        /// </param>
        /// <param name="itemName">
        ///     The item name.
        /// </param>
        /// <param name="readCompleted">
        ///     The read completed.
        /// </param>
        /// <param name="userState">
        ///     The user state.
        /// </param>
        public static void Read(string fileName, string itemName, SubroutineCallCompleted readCompleted, object userState = null)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
            {
                JobManager.RunInUIThread(
                    DispatcherPriority.Normal,
                    () =>
                    SbProcessHandler.CallSubroutine(
                        readCompleted,
                        "UT.XUI.READ",
                        new[]
                            {
                                new SBString(fileName), new SBString(itemName), new SBString(), new SBString(), new SBString("0"),
                                new SBString()
                            },
                        userState ?? new object()));
            }
            else
            {
                SbProcessHandler.CallSubroutine(
                    readCompleted,
                    "UT.XUI.READ",
                    new[]
                        {
                            new SBString(fileName), new SBString(itemName), new SBString(), new SBString(), new SBString("0"), new SBString()
                        },
                    userState ?? new object());
            }
        }

        /// <summary>
        ///     The read dictionary item.
        /// </summary>
        /// <param name="fileName">
        ///     The file name.
        /// </param>
        /// <param name="itemName">
        ///     The item name.
        /// </param>
        /// <param name="readState">
        ///     The read state.
        /// </param>
        /// <param name="subroutineCallCompleted">
        ///     The subroutine call completed.
        /// </param>
        public static void ReadDictionaryItem(
            string fileName,
            string itemName,
            object readState,
            SubroutineCallCompleted subroutineCallCompleted)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(itemName))
            {
                return;
            }

            var fName = fileName.StartsWith("DICT ") ? fileName : "DICT " + fileName;
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
            {
                JobManager.RunInUIThread(DispatcherPriority.Input, () => Read(fName, itemName, subroutineCallCompleted, readState));
            }
            else
            {
                Read(fName, itemName, subroutineCallCompleted, readState);
            }
        }

        /// <summary>
        ///     Writes the specified file name.
        /// </summary>
        /// <param name="fileName">
        ///     Name of the file.
        /// </param>
        /// <param name="id">
        ///     The identifier.
        /// </param>
        /// <param name="record">
        ///     The record.
        /// </param>
        /// <param name="subroutineCallCompleted">
        ///     The subroutine call completed.
        /// </param>
        public static void Write(string fileName, string id, SBString record, SubroutineCallCompleted subroutineCallCompleted)
        {
            Write(fileName, id, string.Empty, "0", record, subroutineCallCompleted);
        }

        /// <summary>
        ///     Writes the specified file name.
        /// </summary>
        /// <param name="fileName">
        ///     Name of the file.
        /// </param>
        /// <param name="id">
        ///     The identifier.
        /// </param>
        /// <param name="attribute">
        ///     The attribute.
        /// </param>
        /// <param name="mode">
        ///     The mode.
        /// </param>
        /// <param name="record">
        ///     The record.
        /// </param>
        /// <param name="subroutineCallCompleted">
        ///     The subroutine call completed.
        /// </param>
        public static void Write(
            string fileName,
            string id,
            string attribute,
            string mode,
            SBString record,
            SubroutineCallCompleted subroutineCallCompleted)
        {
            /*
            FILE.NAME = PARAM1<1>
            ID = PARAM1<2>
            ATTR = PARAM1<3>
            MODE = PARAM1<4>
            REC = PARAM2
             * 
             * SUBROUTINE XUI.DEBUG(MODE, PARAM1, PARAM2, PARAM3, STATUS, STATUSDESC)
             */
            var param1 = new SBString();
            param1.SBInsert(1, fileName);
            param1.SBInsert(2, id);
            param1.SBInsert(3, attribute);
            param1.SBInsert(4, mode);

            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
            {
                JobManager.RunInUIThread(DispatcherPriority.Input, () => XuiDebug.WriteRecord(subroutineCallCompleted, param1, record));
            }
            else
            {
                XuiDebug.WriteRecord(subroutineCallCompleted, param1, record);
            }
        }

        /// <summary>
        ///     Zips the folder.
        /// </summary>
        /// <param name="directoryToZip">
        ///     The directory to zip.
        /// </param>
        /// <param name="zipFileToCreate">
        ///     The zip file to create.
        /// </param>
        public static void ZipFolder(string directoryToZip, string zipFileToCreate)
        {
            try
            {
                using (var zip = new ZipFile())
                {
                    zip.StatusMessageTextWriter = System.Console.Out;
                    zip.AddDirectory(directoryToZip); // recurses subdirectories
                    zip.Save(zipFileToCreate);
                }
            }
            catch (Exception ex1)
            {
                System.Console.Error.WriteLine("exception: " + ex1);
            }
        }

        #endregion
    }
}