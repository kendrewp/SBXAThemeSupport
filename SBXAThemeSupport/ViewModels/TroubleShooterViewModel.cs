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

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.UI.WPFControls;

    /// <summary>
    /// TroubleShooterViewModel controls the actions and behavior of the trouble shooter.
    /// </summary>
    public class TroubleShooterViewModel : ViewModel
    {
        #region Constants

        private const bool EnableTroubleshooting = false;

        private const string StartStopItemId = "ApplicationStartStopLog";

        private const string ActiveApplicationListId = "ActiveApplicationList";

        #endregion

        #region Static Fields

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
                // first get the collection, if it does not exist create a new one.
                var activeApplicationList = SBPlus.Current.GlobalStateFile.GetItem(ActiveApplicationListId).Object as ActiveApplicationList ?? new ActiveApplicationList();
                // get the process id and name
                var currentProcess = Process.GetCurrentProcess();
                var currentProcessId = currentProcess.Id;
                var currentProcessName = currentProcess.ProcessName;
                if (currentProcessName.EndsWith("vshost"))
                {
                    currentProcessName = currentProcessName.Substring(0, currentProcessName.Length - 7);
                }

                // create a new log entry with the process id
                if (activeApplicationList.ContainsProcessId(currentProcessId))
                {
                    // this is an error condition - there should not be a process with the same id already.
                    activeApplicationList.RemoveProcessId(currentProcessId);
                }
                // write it out.
                activeApplicationList.Add(new ApplicationStartStopLog { ProcessId = currentProcessId, Start = DateTime.Now, CleanExit = false });
                SBPlus.Current.GlobalStateFile.SetItem(new SBhStateFileItem(ActiveApplicationListId, activeApplicationList), true);

                // check for left over entries, but getting the list of active process ids, then going through the list of entries in the log collection
                // if an entry in the collection is not active, and not marked as a clean exit, we have a problem. If it is a clean exit removed it.
                var processCollection = Process.GetProcessesByName(currentProcessName);
                var toRemove = new List<ApplicationStartStopLog>();

                foreach (var appLog in activeApplicationList.Where(appLog => appLog.ProcessId != currentProcessId).Where(appLog => processCollection.All(process => process.Id != appLog.ProcessId)))
                {
                    // now we have an orphan
                    if (appLog.CleanExit)
                    {
                        // application exited normally, so just remove the entry and commit it
                        toRemove.Add(appLog); // cannot remove it while I am processing it.
                        continue;
                    }
                    // and finally a bad entry, so do the required notifications.
                    LogBadClose(); // only log an exception if this is the only process running.

                    // log the error so remove the entry from the logs.
                    toRemove.Add(appLog); // cannot remove it while I am processing it.
                }

                foreach (var log in toRemove)
                {
                    activeApplicationList.Remove(log);
                }
                // commit the updated collection.
                SBPlus.Current.GlobalStateFile.SetItem(new SBhStateFileItem(ActiveApplicationListId, activeApplicationList), true);
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
            var activeApplicationList = SBPlus.Current.GlobalStateFile.GetItem(ActiveApplicationListId).Object as ActiveApplicationList ?? new ActiveApplicationList();
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
        /// The send abnormal close.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void SendAbnormalClose(string message)
        {
            try
            {
                string windowsIdentity = SBPlus.Current.SBPlusRuntime.WindowsIdentity;
                var sessionId = SBPlus.Current.SessionId;

                string userId = windowsIdentity.Split(@"\".ToCharArray()).Count() == 2
                                    ? windowsIdentity.Split(@"\".ToCharArray())[1]
                                    : windowsIdentity.Split(@"\".ToCharArray())[0];

                var fileName = Path.GetRandomFileName();
                string logFolder = Path.Combine(Log.LOG_DIRECTORY, "Client");

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

                File.WriteAllText(fileName, logText.ToString());

                ExecuteExceptionReporter(fileName.Replace(" ", "%20%"), (userId + "(" + DateTime.Now.ToString() + ") AC").Replace(" ", "%20%"), logFolder.Replace(" ", "%20%"));
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        /// <summary>
        /// The send exception.
        /// </summary>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public static void SendException(Exception exception)
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

            ExecuteExceptionReporter(
                fileName.Replace(" ", "%20%"), 
                (userId + "(" + DateTime.Now + ")").Replace(" ", "%20%"), 
                logFolder.Replace(" ", "%20%"));
        }

        /// <summary>
        /// Sends the freeze.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void SendFreeze(string message)
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
                logText.Append("Ctrl-Shit-G was hit by the user.");
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

                ExecuteExceptionReporter(fileName.Replace(" ", "%20%"), (userId + "(" + DateTime.Now.ToString() + ") Freeze").Replace(" ", "%20%"), logFolder.Replace(" ", "%20%"));
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        #endregion

        #region Methods

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

/*
        private static string GetLogFilePath()
        {
            return Log.LOG_DIRECTORY + @"\Client\";
        }
*/

        private static void LogBadClose()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (EnableTroubleshooting)
            {
                SendAbnormalClose("Application was not closed correctly.");
            }
        }

        private bool ContainsProcess(IEnumerable<Process> activeProcesses, int processId)
        {
            return activeProcesses.Any(process => process.Id == processId);
        }

        #endregion

        /// <summary>
        /// Doeses the run once exist.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static bool DoesRunOnceExist(string name)
        {
            try
            {
                string logFolder = Path.Combine(Log.LOG_DIRECTORY, "Client");

                var fileName = Path.Combine(logFolder, "RunOnce_" + name);

                return (File.Exists(fileName));
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
        /// <param name="name">The name.</param>
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
            }

        }
    }

    /// <summary>
    ///     The application start stop log.
    /// </summary>
    [Serializable]
    public class ApplicationStartStopLog : SBEntityBase
    {
        #region Fields

        private bool cleanExit;

        private DateTime start;

        private DateTime stop;

        private int processId;

        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether [clean exit].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [clean exit]; otherwise, <c>false</c>.
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
        /// Gets or sets the start.
        /// </summary>
        /// <value>
        /// The start.
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
        /// Gets or sets the stop.
        /// </summary>
        /// <value>
        /// The stop.
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

        /// <summary>
        /// Gets or sets the process identifier.
        /// </summary>
        /// <value>
        /// The process identifier.
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
    }

    /// <summary>
    /// This is a collection of application start stop logs.
    /// </summary>
    [Serializable]
    public class ActiveApplicationList : List<ApplicationStartStopLog>
    {
        /// <summary>
        /// Gets or sets the <see cref="ApplicationStartStopLog"/> with the specified pid.
        /// </summary>
        /// <value>
        /// The <see cref="ApplicationStartStopLog"/>.
        /// </value>
        /// <param name="pid">The pid.</param>
        /// <returns>An instance of <see cref="ApplicationStartStopLog"/> or null.</returns>
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

        /// <summary>
        /// Determines whether [contains] [the specified pid].
        /// </summary>
        /// <param name="pid">The pid.</param>
        /// <returns>True if the item exists in the colleciton.</returns>
        public bool ContainsProcessId(int pid)
        {
            return this.Any(item => item.ProcessId == pid);
        }

        /// <summary>
        /// Removes the process identifier.
        /// </summary>
        /// <param name="pid">The pid.</param>
        public void RemoveProcessId(int pid)
        {
            var logItem = this.FirstOrDefault(item => item.ProcessId == pid);
            if (logItem != null)
            {
                this.Remove(logItem);
            }
        }
    }
}
