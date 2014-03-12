// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalMachineCleanup.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="LocalMachineCleanup.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="LocalMachineCleanup.cs" company="American Auto Shield, LLC.">
//   Copyright © American Auto Shield, LLC. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
    using System;
    using System.IO;
    using System.Linq;

    using SBXA.Shared;
    using SBXA.UI.Client;

    /// <summary>
    ///     The local machine cleanup.
    /// </summary>
    public class LocalMachineCleanup
    {
        #region Public Methods and Operators

        /// <summary>
        /// This will remove the logs from before the date specified in before.
        /// </summary>
        /// <param name="before">
        /// The date which should be used to figure out which logs are cleaned.
        /// </param>
        public static void CleanLogs(DateTime before)
        {
            try
            {
                // get list of files  in "%APPDATA%/Rocket Software/SBXA/Logs"
                var files = Directory.GetFiles(Path.Combine(Log.LOG_DIRECTORY, "Client"));

                foreach (var fileInfo in from fileInfo in files
                                         let creationTime = File.GetLastWriteTime(fileInfo)
                                         where
                                             creationTime.Year < before.Year || creationTime.Month < before.Month
                                             || creationTime.Day < before.Day
                                         select fileInfo)
                {
                    File.Delete(fileInfo);
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Failed to clean the logs.", exception);
            }
        }

        /// <summary>
        ///     Calling this routine will remove all the SB/XA folders from previous versions.
        /// </summary>
        public static void CleanVersionFolders(bool deleteAll = false)
        {
            try
            {
                var versionDeleted = false;

                // get version
                string version = typeof(SBString).Assembly.GetName().Version.ToString();
                string rootPath = SBXA.Shared.Utilities.GetEnvironmentVariable(GenericConstants.SBOPATH);
                DirectoryInfo rootDirectory = Directory.GetParent(rootPath);
                // get list of folders in "%APPDATA%/Rocket Software/SBXA"
                DirectoryInfo[] directories = rootDirectory.GetDirectories();
                foreach (var dirInfo in directories)
                {
                    // delete all except current version.
                    if ((!dirInfo.Name.Equals(version) && !dirInfo.FullName.Equals(Log.LOG_DIRECTORY)) || deleteAll)
                    {
                        Directory.Delete(dirInfo.FullName, true);
                        versionDeleted = true;
                    }
                }

                // delete log directory if an older version was also deleted
                if (versionDeleted)
                {
                    Directory.Delete(Log.LOG_DIRECTORY, true);
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Failed to clean the previous version data.", exception);
            }
        }

        #endregion
    }
}
