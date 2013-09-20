using System;
using System.IO;
using SBXA.Shared;

namespace SBXAThemeSupport
{
    public class LocalMachineCleanup
    {
        /// <summary>
        /// Calling this routine will remove all the SB/XA folders from previous versions.
        /// </summary>
        public static void CleanVersionFolders()
        {
            try
            {
                var versionDeleted = false;

                // get version
                string version = typeof(SBString).Assembly.GetName().Version.ToString();
                string rootPath = Utilities.GetEnvironmentVariable(GenericConstants.SBOPATH);
                DirectoryInfo rootDirectory = Directory.GetParent(rootPath);
                // get list of folders in "%APPDATA%/Rocket Software/SBXA"
                DirectoryInfo[] directories = rootDirectory.GetDirectories();
                foreach (DirectoryInfo dirInfo in directories)
                {
                    // delete all except current version.
                    if (!dirInfo.Name.Equals(version) && !dirInfo.FullName.Equals(Log.LOG_DIRECTORY))
                    {
                        Directory.Delete(dirInfo.FullName, true);
                        versionDeleted = true;
                    }
                }

                // delete log directory if an older version was also deleted
                if(versionDeleted)
                {
                    Directory.Delete(Log.LOG_DIRECTORY, true);
                }
            }
            catch (Exception exception)
            {

                ;
            }
        }

    }
}
