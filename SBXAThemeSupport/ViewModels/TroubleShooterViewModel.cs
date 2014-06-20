// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TroubleShooterViewModel.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="TroubleShooterViewModel.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.UI.Client;
    using SBXA.UI.WPFControls;

    using SBXAThemeSupport.Utilities;

    /// <summary>
    ///     The report problems to.
    /// </summary>
    public enum ReportProblemsTo
    {
        /// <summary>
        ///     The server.
        /// </summary>
        Server, 

        /// <summary>
        ///     The email.
        /// </summary>
        Email, 

        /// <summary>
        ///     The local folder.
        /// </summary>
        LocalFolder
    }

    /// <summary>
    ///     TroubleShooterViewModel controls the actions and behavior of the trouble shooter.
    /// </summary>
    /// <remarks>
    ///     Uploading of log files.
    ///     <para>
    ///         1) Files are uploaded to ServerFileName which by default is YYTROUBLESHOOTER.DATA.ZE
    ///         2) Files are uploaded from the [SB/XA log folder]/ToUpload, i.e. C:\Users\%userName%\AppData\Roaming\Rocket
    ///         Software\SBXA\Logs\ToUpload
    ///         3) When the application starts and connects to the server, there is a check to see if the previous instance of
    ///         the application was closed cleanly. If
    ///         not then there is a record of it made in ServerFileName, the logs are moved to the ToUpload folder and zip
    ///         after which they are uploaded to the server.
    ///         4) After all the error checking is complete, there is a check to see if there is anything in the ToUpload
    ///         folder, if there is it is uploaded to the server. This
    ///         check is to make sure we get all the log files off the client.
    ///     </para>
    /// </remarks>
    public class TroubleShooterViewModel : ViewModel
    {
        #region Constants

        /// <summary>
        /// The active application list id.
        /// </summary>
        private const string ActiveApplicationListId = "ActiveApplicationList";

        /// <summary>
        /// The enable troubleshooting.
        /// </summary>
        private const bool EnableTroubleshooting = true;

        /// <summary>
        /// The log rec description.
        /// </summary>
        private const int LogRecDescription = 2;

        /// <summary>
        /// The log rec type.
        /// </summary>
        private const int LogRecType = 1;

        #endregion

        #region Static Fields

        /// <summary>
        /// The organization.
        /// </summary>
        private static string organization;

        private static readonly object UploadSyncObj = new object();

        private static int fileUploadingCount;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="TroubleShooterViewModel" /> class.
        /// </summary>
        static TroubleShooterViewModel()
        {
            ReportProblemsTo = ReportProblemsTo.Server;
            ServerFileName = "YYTROUBLESHOOTER.DATA.ZE";
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the report problems to.
        /// </summary>
        /// <value>
        ///     The report problems to.
        /// </value>
        public static ReportProblemsTo ReportProblemsTo { get; set; }

        /// <summary>
        ///     Gets or sets the report problems to folder.
        /// </summary>
        /// <value>
        ///     The report problems to folder.
        /// </value>
        public static string ReportProblemsToFolder { get; set; }

        /// <summary>
        ///     Gets or sets the name of the server file.
        /// </summary>
        /// <value>
        ///     The name of the server file.
        /// </value>
        public static string ServerFileName { get; set; }

        /// <summary>
        /// Gets or sets the organization.
        /// </summary>
        /// <value>
        /// The organization.
        /// </value>
        public static string Organization
        {
            get
            {
                return organization;
            }

            set
            {
                organization = value;
                // now I need to add it to the application start item so it can be used later if need be.
                ActiveApplicationList activeApplicationList;
                int currentProcessId;
                string currentProcessName;

                var activeApplicationLog = ReadActiveApplicationLog(out activeApplicationList, out currentProcessId, out currentProcessName);
                if (activeApplicationLog != null)
                {
                    activeApplicationLog.Organization = organization;
                }
                // commit the updated collection.
                SBPlus.Current.GlobalStateFile.SetItem(new SBhStateFileItem(ActiveApplicationListId, activeApplicationList), true);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the upload folder.
        /// </summary>
        /// <value>
        ///     The upload folder.
        /// </value>
        private static string UploadFolder
        {
            get
            {
                return Path.Combine(Log.LOG_DIRECTORY, "ToUpload");
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The application start.
        /// </summary>
        public static void ApplicationStart()
        {
            try
            {
                // remove old logs.
                JobManager.RunAsyncOnPooledThread(delegate { LocalMachineCleanup.CleanLogs(DateTime.Now); });
                JobManager.RunAsyncOnPooledThread(delegate { LocalMachineCleanup.CleanVersionFolders(); });

                // first get the colleciton or create it if it does not already exist
                if (!SBPlus.Current.GlobalStateFile.Exists(ActiveApplicationListId))
                {
                    SBPlus.Current.GlobalStateFile.SetItem(new SBhStateFileItem(ActiveApplicationListId, new ActiveApplicationList()), true);
                }

                ActiveApplicationList activeApplicationList;
                int currentProcessId;
                string currentProcessName;

                var activeApplicationLog = ReadActiveApplicationLog(out activeApplicationList, out currentProcessId, out currentProcessName);

                // create a new log entry with the process id
                if (activeApplicationLog != null && activeApplicationList.ContainsProcessId(currentProcessId))
                {
                    // this is an error condition - there should not be a process with the same id already.
                    activeApplicationList.RemoveProcessId(currentProcessId);
                }

                // write it out.
                activeApplicationList.Add(
                    new ApplicationStartStopLog
                        {
                            ProcessId = currentProcessId, 
                            Start = DateTime.Now, 
                            CleanExit = false, 
                            Organization = Organization,
                            SessionId = SBPlus.Current.SessionId
                        });
                SBPlus.Current.GlobalStateFile.SetItem(new SBhStateFileItem(ActiveApplicationListId, activeApplicationList), true);

                // check for left over entries, but getting the list of active process ids, then going through the list of entries in the log collection
                // if an entry in the collection is not active, and not marked as a clean exit, we have a problem. If it is a clean exit removed it.
                var processCollection = Process.GetProcessesByName(currentProcessName);
                var toRemove = new List<ApplicationStartStopLog>();

                foreach (var appLog in
                    activeApplicationList.Where(appLog => appLog.ProcessId != currentProcessId)
                        .Where(appLog => processCollection.All(process => process.Id != appLog.ProcessId)))
                {
                    // now we have an orphan
                    if (appLog.CleanExit)
                    {
                        // application exited normally, so just remove the entry and commit it
                        toRemove.Add(appLog); // cannot remove it while I am processing it.
                        continue;
                    }

                    // and finally a bad entry, so do the required notifications.
                    LogBadClose(appLog); // only log an exception if this is the only process running.

                    // log the error so remove the entry from the logs.
                    toRemove.Add(appLog); // cannot remove it while I am processing it.
                }

                foreach (var log in toRemove)
                {
                    activeApplicationList.Remove(log);
                }

                // commit the updated collection.
                SBPlus.Current.GlobalStateFile.SetItem(new SBhStateFileItem(ActiveApplicationListId, activeApplicationList), true);

                // Check to see if there are any files to upload. This is the final catch all to ensure that all crash files are uploaded to the server.
                UploadFiles();
            }
            catch (Exception exception)
            {
                Debug.WriteLine("[TroubleShooterViewModel.ApplicationStart(73)] " + exception.Message);
            }
        }

        /// <summary>
        ///     The application stop.
        /// </summary>
        public static void ApplicationStop()
        {
            // first get the colleciton or create it if it does not already exist
            if (!SBPlus.Current.GlobalStateFile.Exists(ActiveApplicationListId))
            {
                // As we are closing down this case should never be hit, but in order to prevent problem we keep it here.
                SBPlus.Current.GlobalStateFile.SetItem(new SBhStateFileItem(ActiveApplicationListId, new ActiveApplicationList()), true);
            }

            // first get the collection, if it does not exist create a new one.
            var activeApplicationList = SBPlus.Current.GlobalStateFile.GetItem(ActiveApplicationListId).Object as ActiveApplicationList
                                        ?? new ActiveApplicationList();
            // get the process id and name
            var currentProcess = Process.GetCurrentProcess();
            var currentProcessId = currentProcess.Id;

            // create a new log entry with the process id
            if (!activeApplicationList.ContainsProcessId(currentProcessId))
            {
                return;
            }

            activeApplicationList[currentProcessId].CleanExit = true;
            activeApplicationList[currentProcessId].Stop = DateTime.Now;

            // commit the updated collection.
            SBPlus.Current.GlobalStateFile.SetItem(new SBhStateFileItem(ActiveApplicationListId, activeApplicationList), false);
        }

        /// <summary>
        /// Doeses the run once exist.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool DoesRunOnceExist(string name)
        {
            try
            {
                string logFolder = Path.Combine(Log.LOG_DIRECTORY, "Client");

                var fileName = Path.Combine(logFolder, "RunOnce_" + name);

                return File.Exists(fileName);
            }

                // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }

            return false;
        }

        /// <summary>
        /// Haves the run once.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public static void HaveRunOnce(string name)
        {
            try
            {
                string logFolder = Path.Combine(Log.LOG_DIRECTORY, "Client");

                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                var fileName = Path.Combine(logFolder, "RunOnce_" + name);

                File.WriteAllText(fileName, name);
            }

                // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception exception)
            {
                CustomLogger.LogException(exception, "There was a problem in the HaveRunOnce method.");
            }
        }

        /// <summary>
        /// The send abnormal close.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="deleteFiles">
        /// True is the files sent to the troubleshooter should be deleted after they have been packaged
        ///     up.
        /// </param>
        /// <param name="appLog">
        /// The Application Start Stop Log item
        /// </param>
        public static void SendAbnormalClose(string message, bool deleteFiles, ApplicationStartStopLog appLog = null)
        {
            try
            {
                SendAbnormalClose(message, string.Empty, null, deleteFiles, appLog);
            }

                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        /// <summary>
        /// Sends the abnormal close.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="abortDescriptionFileName">
        /// Name of the abort description file.
        /// </param>
        /// <param name="additionalFiles">
        /// The additional files.
        /// </param>
        /// <param name="delete">
        /// The delete.
        /// </param>
        /// <param name="orgId">
        /// The org identifier.
        /// </param>
        public static void SendAbnormalClose(
            string message, 
            string abortDescriptionFileName, 
            string[] additionalFiles, 
            bool delete = true, 
            ApplicationStartStopLog appLog = null)
        {
            var windowsIdentity = SBPlus.Current.SBPlusRuntime.WindowsIdentity;
            var sessionId = SBPlus.Current.SessionId;
            var sbxa = typeof(SBString).Assembly.GetName();
            var orgId = appLog == null || string.IsNullOrEmpty(appLog.Organization) ? string.Empty : appLog.Organization;
            var troubledSessionId = appLog == null ? "No troubled Session Id" : appLog.SessionId;
            var userId = windowsIdentity.Split(@"\".ToCharArray()).Count() == 2
                             ? windowsIdentity.Split(@"\".ToCharArray())[1]
                             : windowsIdentity.Split(@"\".ToCharArray())[0];

            var fileName = abortDescriptionFileName;
            if (string.IsNullOrEmpty(abortDescriptionFileName))
            {
                fileName = Path.GetRandomFileName();
            }

            var logFolder = Path.Combine(Log.LOG_DIRECTORY, "Client");

            fileName = Path.Combine(logFolder, fileName);

            var existingData = string.Empty;

            if (File.Exists(fileName))
            {
                existingData = File.ReadAllText(fileName);
            }

            var logText = new StringBuilder(existingData);
            logText.AppendLine(message);

            logText.AppendLine(string.Format("User Id : {0} ", userId));
            logText.AppendLine(string.Format("Machine Name {0}", SystemInformation.ComputerName));
            logText.AppendLine(string.Format("Session Id {0}", sessionId));
            logText.AppendLine(string.Format("Troubled Session Id {0}", troubledSessionId));
            logText.AppendLine(string.Format(
                "SB/XA Version {0}.{1}.{2}.{3}",
                sbxa.Version.Major,
                sbxa.Version.Minor,
                sbxa.Version.Build,
                sbxa.Version.Revision));

            logText.AppendLine(string.Format("Customer {0}", (string.IsNullOrEmpty(orgId) ? "Unknown" : orgId)));

            File.WriteAllText(fileName, logText.ToString());

            RegisterProblem("Abnormal Close", fileName, additionalFiles, userId, logFolder, delete, orgId);
        }

        /// <summary>
        /// The send exception.
        /// </summary>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <param name="deleteFiles">
        /// True if the files packaged up should be deleted.
        /// </param>
        /// <param name="orgId">
        /// The org Id.
        /// </param>
        public static void SendException(Exception exception, bool deleteFiles = true, string orgId = null)
        {
            var windowsIdentity = SBPlus.Current.SBPlusRuntime.WindowsIdentity;

            var userId = windowsIdentity.Split(@"\".ToCharArray()).Count() == 2
                             ? windowsIdentity.Split(@"\".ToCharArray())[1]
                             : windowsIdentity.Split(@"\".ToCharArray())[0];
            var fileName = Path.GetRandomFileName();
            var logFolder = Path.Combine(Log.LOG_DIRECTORY, "Client");

            fileName = Path.Combine(logFolder, fileName);

            var existingData = string.Empty;

            if (File.Exists(fileName))
            {
                existingData = File.ReadAllText(fileName);
            }

            var logText = new StringBuilder(existingData);

            logText.AppendLine("Exception : " + exception.Message);
            logText.AppendLine(exception.StackTrace);

            Exception innerException = exception.InnerException;
            while (innerException != null)
            {
                logText.AppendLine("Inner Exception : " + innerException.Message);
                logText.AppendLine(innerException.StackTrace);
                innerException = innerException.InnerException;
            }

            File.WriteAllText(fileName, logText.ToString());

            RegisterProblem("Exception", fileName, new string[0], userId, logFolder, deleteFiles, orgId);
        }

        /// <summary>
        /// Sends the freeze.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="deleteFiles">
        /// True if the files packaged up should be deleted.
        /// </param>
        /// <param name="orgId">
        /// The org Id.
        /// </param>
        public static void SendFreeze(string message, bool deleteFiles, string orgId = null)
        {
            try
            {
                var windowsIdentity = SBPlus.Current.SBPlusRuntime.WindowsIdentity;

                var userId = windowsIdentity.Split(@"\".ToCharArray()).Count() == 2
                                 ? windowsIdentity.Split(@"\".ToCharArray())[1]
                                 : windowsIdentity.Split(@"\".ToCharArray())[0];

                var sessionId = SBPlus.Current.SessionId;
                var fileName = Path.GetRandomFileName();
                var logFolder = Path.Combine(Log.LOG_DIRECTORY, "Client");

                fileName = Path.Combine(logFolder, fileName);

                var existingData = string.Empty;

                if (File.Exists(fileName))
                {
                    existingData = File.ReadAllText(fileName);
                }

                var logText = new StringBuilder(existingData);
                logText.Append("Ctrl-Shift-G was hit by the user.");
                logText.AppendLine(message);

                logText.AppendLine(string.Format("User Id : {0} ", userId));
                logText.AppendLine(string.Format("Machine Name {0}", SystemInformation.ComputerName));
                logText.AppendLine(string.Format("Session Id {0}", sessionId));

                // Now get form stack
                logText.AppendLine("Form Stack:");
                foreach (var form in SBPlus.Current.FormStack.RealStack)
                {
                    if (form.ObjectHandle != null)
                    {
                        if (form.ObjectHandle is SBForm)
                        {
                            var gobj = form.ObjectHandle.GuiObjectDefinition as SBXA.Shared.Definitions.FormObjectDefinition;

                            if (gobj != null && !string.IsNullOrEmpty(gobj.ProcessName))
                            {
                                logText.AppendLine(string.Format(" {0} {1}", form.ObjectHandle.GetType().Name, gobj.ProcessName));
                            }
                        }
                        else
                        {
                            var menu = form.ObjectHandle as SBMenu;
                            if (menu != null && menu.MenuDefinition != null)
                            {
                                logText.AppendLine(string.Format(" {0} {1}", form.ObjectHandle.GetType().Name, menu.MenuDefinition.Name));
                            }
                            else
                            {
                                logText.AppendLine(form.ObjectHandle.GetType().Name);
                            }
                        }
                    }
                    else
                    {
                        logText.AppendLine("Blank form handle");
                    }
                }

                File.WriteAllText(fileName, logText.ToString());

                RegisterProblem("Freeze", fileName, new string[0], userId, logFolder, deleteFiles, orgId);
            }

                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads the active application log.
        /// </summary>
        /// <param name="activeApplicationList">The active application list.</param>
        /// <param name="currentProcessId">The current process identifier.</param>
        /// <param name="currentProcessName">Name of the current process.</param>
        /// <returns>An intance of <see cref="ApplicationStartStopLog"/> or null.</returns>
        private static ApplicationStartStopLog ReadActiveApplicationLog(out ActiveApplicationList activeApplicationList, out int currentProcessId, out string currentProcessName)
        {
            // first get the collection, if it does not exist create a new one.
            activeApplicationList = SBPlus.Current.GlobalStateFile.GetItem(ActiveApplicationListId).Object as ActiveApplicationList
                                    ?? new ActiveApplicationList();
            // get the process id and name
            var currentProcess = Process.GetCurrentProcess();
            currentProcessId = currentProcess.Id;
            currentProcessName = currentProcess.ProcessName;
            if (currentProcessName.EndsWith("vshost"))
            {
                currentProcessName = currentProcessName.Substring(0, currentProcessName.Length - 7);
            }

            if (activeApplicationList.ContainsProcessId(currentProcessId))
            {
                return activeApplicationList[currentProcessId];
            }

            return null;
        }

        /// <summary>
        ///     This method will check to see if there are any files in the upload folder.
        /// </summary>
        /// <returns>True or false depending on if there are files that need to be uploaded.</returns>
        private static bool AreFilesToUpload()
        {
            if (!Directory.Exists(UploadFolder))
            {
                return false;
            }

            return Directory.GetFiles(UploadFolder).Count() != 0 || Directory.GetDirectories(UploadFolder).Count() != 0;
        }

        /// <summary>
        /// The check for trouble shooter file completed.
        /// </summary>
        /// <param name="subroutineName">
        /// The subroutine name.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="userState">
        /// The user state.
        /// </param>
        private static void CheckForTroubleShooterFileCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {
                var status = parameters[5];
                if (status.Count != 1 || !status.Value.Equals("0"))
                {
                    // Troubleshooter.data file does not exist in the current account so ignore the write.
                    return;
                }

                // write log record.
                var logRecord = new SBString();
                var stateObject = userState as object[];
                if (stateObject == null)
                {
                    return;
                }

                var id = stateObject[0] as string;
                var problemType = stateObject[1] as string;
                var fileName = stateObject[2] as string;
                var additionalFiles = stateObject[3] as string[];
                var logFolder = stateObject[5] as string;
                var additionalText = stateObject[6] as string;

                logRecord.SBInsert(LogRecType, problemType);

                if (!string.IsNullOrEmpty(additionalText))
                {
                    var tmp = additionalText.Replace(
                        "\r\n",
                        Convert.ToString(GenericConstants.DEFAULT_AM_CHAR));

                    var addText = new SBString(tmp, new char[] { GenericConstants.DEFAULT_AM_CHAR, GenericConstants.DEFAULT_VM_CHAR, GenericConstants.DEFAULT_SVM_CHAR });
                    logRecord.SBInsert(LogRecDescription, addText);
                }
                else
                {
                    if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                    {
                        var text = File.ReadAllText(fileName);
                        logRecord.SBInsert(LogRecDescription, text);
                    }
                }

                id = SBFile.MakeRecordIdLegal(id);

                JobManager.RunInUIThread(DispatcherPriority.Normal, () => SBFile.Write(ServerFileName, id, logRecord, WriteCompleted));
                // Now upload log files to server.
                UploadFiles();
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("There was a problem while creating the log record.", exception);
            }
        }

        /// <summary>
        /// This method will recursively clean all the files and folders from the folder specified
        /// </summary>
        /// <param name="folder">
        /// The folder to clean.
        /// </param>
        private static void CleanFolder(string folder)
        {
            foreach (var file in Directory.GetFiles(folder))
            {
                File.Delete(file);
            }

            foreach (var dir in Directory.GetDirectories(folder))
            {
                CleanFolder(dir);
                Directory.Delete(dir);
            }

            Directory.Delete(folder);
        }

        /// <summary>
        ///     This method will check to see if the upload folder exists, if not it will be created.
        /// </summary>
        private static void CreateToUploadFolder()
        {
            try
            {
                if (!Directory.Exists(UploadFolder))
                {
                    Directory.CreateDirectory(UploadFolder);
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("A problem occurred creating the upload folder '" + UploadFolder + "'", exception);
            }
        }

        /// <summary>
        /// The execute exception reporter.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="uniqueId">
        /// The unique id.
        /// </param>
        /// <param name="logFolder">
        /// The log folder.
        /// </param>
        private static void ExecuteExceptionReporter(string fileName, string uniqueId, string logFolder)
        {
            // string processInfo = "ExceptionReporter.exe "+fileName;
            var processInfo = new ProcessStartInfo("SBXAExceptionReporter.exe")
                                  {
                                      UseShellExecute = false, 
                                      Arguments = fileName + " " + uniqueId + " " + logFolder
                                  };
            Process.Start(processInfo);
        }

        /// <summary>
        /// The log bad close.
        /// </summary>
        /// <param name="appLog">
        /// The Application start stop log
        /// </param>
        private static void LogBadClose(ApplicationStartStopLog appLog)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (EnableTroubleshooting)
            {
                SendAbnormalClose("Application was not closed correctly.", true, appLog);
            }
        }

        /// <summary>
        /// The log problem in server.
        /// </summary>
        /// <param name="problemType">
        /// The problem type.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="additionalFiles">
        /// The additional files.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="logFolder">
        /// The log folder.
        /// </param>
        /// <param name="deleteFiles">
        /// The delete files.
        /// </param>
        /// <param name="org">
        /// The org.
        /// </param>
        private static void LogProblemInServer(
            string problemType, 
            string fileName, 
            string[] additionalFiles, 
            string userId, 
            string logFolder, 
            bool deleteFiles,
            string org = null)
        {
            // check if the file exists, if not ignore the call so we do not crash.
            // string id = "date" + "TIME" + "machine name";
            var now = DateTime.Now;

            var id = Convert.ToString(SBXA.Shared.SBConv.IConvDate(now.Date.Month + "/" + now.Date.Day + "/" + now.Date.Year, "D4/ M/D/Y"))
                     + "*";
            id += Convert.ToString(SBXA.Shared.SBConv.IConvTime(now.Hour + ":" + now.Minute + "/" + now.Second, "MTS")) + "*";
            id += SystemInformation.ComputerName;
            id += "*" + (string.IsNullOrEmpty(org) ? organization : org);

            var text = string.Empty;
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                text = File.ReadAllText(fileName);
            }

            // Create zip of log files.
            if (!string.IsNullOrEmpty(logFolder))
            {
                CreateToUploadFolder();
                string[] files;
                if (additionalFiles == null)
                {
                    files = new string[1];
                }
                else
                {
                    files = new string[additionalFiles.Length + 1];
                    additionalFiles.CopyTo(files, 0);
                }

                files[files.Length - 1] = fileName;
                MoveFilesToUpload(id, logFolder, files, true, deleteFiles);
            }

            SBFile.Read(
                "VOC", 
                ServerFileName, 
                CheckForTroubleShooterFileCompleted, 
                new object[] { id, problemType, fileName, additionalFiles, userId, logFolder, text });
        }

        /// <summary>
        /// The move files to upload.
        /// </summary>
        /// <param name="uniqueId">
        /// The unique id.
        /// </param>
        /// <param name="sourceFolder">
        /// The source folder.
        /// </param>
        /// <param name="additionalFiles">
        /// The additional files.
        /// </param>
        /// <param name="zipFolder">
        /// The zip folder.
        /// </param>
        /// <param name="deleteOriginal">
        /// The delete original.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool MoveFilesToUpload(
            string uniqueId, 
            string sourceFolder, 
            IEnumerable<string> additionalFiles, 
            bool zipFolder, 
            bool deleteOriginal)
        {
            var fileList = Directory.GetFiles(sourceFolder);
            var targetFolder = Path.Combine(UploadFolder, uniqueId.Replace("/", "-").Replace(":", "_").Replace("*", "_"));
            try
            {
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                foreach (var file in fileList)
                {
                    var parent = Directory.GetParent(file);
                    var fileName = file.Substring(parent.FullName.Length + 1, file.Length - parent.FullName.Length - 1);
                    if (file.Length > 230)
                    {
                        var lastPos = fileName.LastIndexOf("!;!", StringComparison.InvariantCulture);
                        if (lastPos > 0)
                        {
                            lastPos += 3;
                            fileName = fileName.Substring(lastPos, fileName.Length - lastPos);
                        }
                    }

                    File.Copy(file, Path.Combine(targetFolder, fileName));
                    // check if I need to delete the original.
                    if (deleteOriginal)
                    {
                        try
                        {
                            File.Delete(file);
                        }

                            // ReSharper disable once EmptyGeneralCatchClause
                        catch (Exception)
                        {
                            // in case the file is locked.
                        }
                    }
                }

                if (additionalFiles != null)
                {
                    foreach (var file in additionalFiles)
                    {
                        try
                        {
                            // if file not exists, take the next
                            if (!File.Exists(file))
                            {
                                continue;
                            }

                            var parent = Directory.GetParent(file);
                            var fileName = file.Substring(parent.FullName.Length + 1, file.Length - parent.FullName.Length - 1);
                            if (!File.Exists(file) || File.Exists(Path.Combine(targetFolder, fileName)))
                            {
                                continue;
                            }

                            File.Copy(file, Path.Combine(targetFolder, fileName));
                            // check if I need to delete the original.
                            if (deleteOriginal)
                            {
                                try
                                {
                                    File.Delete(file);
                                }

                                    // ReSharper disable once EmptyGeneralCatchClause
                                catch (Exception)
                                {
                                    // in case the file is locked.
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            // log the exception and keep trying to copy files.
                            CustomLogger.LogException(
                                exception, 
                                "An exception while trying to copy " + (string.IsNullOrEmpty(file) ? "empty file Name" : file));
                        }
                    }
                }

                // Check if I should zip the folder and then delete it.
                if (zipFolder)
                {
                    SBFile.ZipFolder(targetFolder, targetFolder + ".zip");
                    CleanFolder(targetFolder);
                }
            }
            catch (Exception exception)
            {
                CustomLogger.LogException(exception, "A problem occurred moving files to " + targetFolder);
                return false;
            }

            return true;
        }

        /// <summary>
        /// The register problem.
        /// </summary>
        /// <param name="problemType">
        /// The problem type.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="additionalFiles">
        /// The additional files.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="logFolder">
        /// The log folder.
        /// </param>
        /// <param name="deleteFiles">
        /// The delete files.
        /// </param>
        /// <param name="orgId">
        /// The org id.
        /// </param>
        private static void RegisterProblem(
            string problemType, 
            string fileName, 
            string[] additionalFiles, 
            string userId, 
            string logFolder, 
            bool deleteFiles, 
            string orgId = null)
        {
            // First move all the files to the upload location.
            var uniqueId = userId + "(" + DateTime.Now + ") " + problemType;
            if (!string.IsNullOrEmpty(orgId))
            {
                uniqueId = orgId + "_" + uniqueId;
            }

            switch (ReportProblemsTo)
            {
                case ReportProblemsTo.Email:
                    break;
                case ReportProblemsTo.Server:
                    LogProblemInServer(problemType, fileName, additionalFiles, userId, logFolder, deleteFiles, orgId);
                    break;
                case ReportProblemsTo.LocalFolder:
                    ExecuteExceptionReporter(fileName.Replace(" ", "%20%"), uniqueId.Replace(" ", "%20%"), logFolder.Replace(" ", "%20%"));
                    break;
            }
        }

        /// <summary>
        /// The upload completed.
        /// </summary>
        /// <param name="subroutineName">
        /// The subroutine name.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="userState">
        /// The user state.
        /// </param>
        private static void UploadCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            try
            {
                fileUploadingCount--;
                // if the upload was sucessful, delete the files.
                if (parameters[3].Dcount() == 1 && parameters[1].Dcount() > 1)
                {
                    if (string.IsNullOrEmpty(parameters[3].Value) || parameters[3].Value.Equals("0"))
                    {
                        // delete file
                        var fileName = parameters[1].Extract(1).Value;
                        File.Delete(fileName);
                    }
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("There was a problem deleting a file after it was uploaded.", exception);
            }
        }

        /// <summary>
        /// The upload files.
        /// </summary>
        private static void UploadFiles()
        {
            lock (UploadSyncObj)
            {
                if (!AreFilesToUpload() || fileUploadingCount > 0)
                {
                    return;
                }

                // first check if there are any folders. These did not get zipped before the client closed down.
                var folderList = Directory.GetDirectories(UploadFolder);
                // zip each of the folders
                foreach (var folder in folderList)
                {
                    SBFile.ZipFolder(folder, folder + ".zip");
                    CleanFolder(folder);
                }

                var fileList = Directory.GetFiles(UploadFolder);
                foreach (var fileName in fileList)
                {
                    var itemId = Path.GetFileName(fileName);
                    if (string.IsNullOrEmpty(itemId))
                    {
                        continue;
                    }
                    fileUploadingCount++;
                    itemId = itemId.Replace("(", "*").Replace(")", "*").Replace(" ", "*");
                    SBFileTransfer.Upload(fileName, ServerFileName, itemId, "B", UploadCompleted);
                }
            }
        }

        /// <summary>
        /// The write completed.
        /// </summary>
        /// <param name="subroutineName">
        /// The subroutine name.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="userState">
        /// The user state.
        /// </param>
        private static void WriteCompleted(string subroutineName, SBString[] parameters, object userState)
        {
        }

        #endregion
    }

    /// <summary>
    ///     The application start stop log.
    /// </summary>
    [Serializable]
    public class ApplicationStartStopLog : SBEntityBase
    {
        #region Fields

        /// <summary>
        /// The clean exit.
        /// </summary>
        private bool cleanExit;

        /// <summary>
        /// The process id.
        /// </summary>
        private int processId;

        /// <summary>
        /// The start.
        /// </summary>
        private DateTime start;

        private DateTime stop;

        /// <summary>
        /// Gets or sets the organization.
        /// </summary>
        /// <value>
        /// The organization.
        /// </value>
        public string Organization { get; set; }

        public string SessionId { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether [clean exit].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [clean exit]; otherwise, <c>false</c>.
        /// </value>
        public bool CleanExit
        {
            get
            {
                return this.cleanExit;
            }

            set
            {
                this.cleanExit = value;
            }
        }

        /// <summary>
        ///     Gets or sets the process identifier.
        /// </summary>
        /// <value>
        ///     The process identifier.
        /// </value>
        public int ProcessId
        {
            get
            {
                return this.processId;
            }

            set
            {
                this.processId = value;
            }
        }

        /// <summary>
        ///     Gets or sets the start.
        /// </summary>
        /// <value>
        ///     The start.
        /// </value>
        public DateTime Start
        {
            get
            {
                return this.start;
            }

            set
            {
                this.start = value;
            }
        }

        /// <summary>
        ///     Gets or sets the stop.
        /// </summary>
        /// <value>
        ///     The stop.
        /// </value>
        public DateTime Stop
        {
            get
            {
                return this.stop;
            }

            set
            {
                this.stop = value;
            }
        }

        #endregion
    }

    /// <summary>
    ///     This is a collection of application start stop logs.
    /// </summary>
    [Serializable]
    public class ActiveApplicationList : List<ApplicationStartStopLog>
    {
        #region Public Indexers

        /// <summary>
        /// Gets or sets the <see cref="ApplicationStartStopLog"/> with the specified pid.
        /// </summary>
        /// <value>
        /// The <see cref="ApplicationStartStopLog"/>.
        /// </value>
        /// <param name="pid">
        /// The pid.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ApplicationStartStopLog"/> or null.
        /// </returns>
        public new ApplicationStartStopLog this[int pid]
        {
            get
            {
                return this.FirstOrDefault(item => item.ProcessId == pid);
            }

            set
            {
                var logItem = this.FirstOrDefault(item => item.ProcessId == pid);
                if (logItem != null)
                {
                    this.Remove(logItem);
                }

                this.Add(value);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Determines whether [contains] [the specified pid].
        /// </summary>
        /// <param name="pid">
        /// The pid.
        /// </param>
        /// <returns>
        /// True if the item exists in the colleciton.
        /// </returns>
        public bool ContainsProcessId(int pid)
        {
            return this.Any(item => item.ProcessId == pid);
        }

        /// <summary>
        /// Removes the process identifier.
        /// </summary>
        /// <param name="pid">
        /// The pid.
        /// </param>
        public void RemoveProcessId(int pid)
        {
            var logItem = this.FirstOrDefault(item => item.ProcessId == pid);
            if (logItem != null)
            {
                this.Remove(logItem);
            }
        }

        #endregion
    }
}