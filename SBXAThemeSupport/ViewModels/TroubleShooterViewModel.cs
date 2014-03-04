// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TroubleShooterViewModel.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.UI.WPFControls;

    /// <summary>
    ///     The trouble shooter view model.
    /// </summary>
    public class TroubleShooterViewModel : ViewModel
    {
        #region Constants

        private const bool EnableTroubleshooting = true;

        private const string StartStopItemId = "ApplicationStartStopLog";

        #endregion

        #region Static Fields

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The application start.
        /// </summary>
        public static void ApplicationStart()
        {
            // First check if I already have an log item and if the last exit was clean.
            if (SBPlus.Current.GlobalStateFile.Exists(StartStopItemId))
            {
                var existingLogItem = SBPlus.Current.GlobalStateFile.GetItem(StartStopItemId);
                if (!((ApplicationStartStopLog)existingLogItem.Object).CleanExit)
                {
                    LogBadClose();
                }
                else
                {
                    JobManager.RunAsyncOnPooledThread(delegate { LocalMachineCleanup.CleanLogs(DateTime.Now); });
                    JobManager.RunAsyncOnPooledThread(delegate { LocalMachineCleanup.CleanVersionFolders(); });
                }
            }

            var logItem = new ApplicationStartStopLog { Start = DateTime.Now, CleanExit = false };
            SBPlus.Current.GlobalStateFile.SetItem(new SBhStateFileItem(StartStopItemId, logItem), true);
        }

        /// <summary>
        ///     The application stop.
        /// </summary>
        public static void ApplicationStop()
        {
            if (SBPlus.Current.GlobalStateFile.Exists(StartStopItemId))
            {
                var logItem = SBPlus.Current.GlobalStateFile.GetItem(StartStopItemId);
                ((ApplicationStartStopLog)logItem.Object).Stop = DateTime.Now;

                // ((ApplicationStartStopLog)logItem.Object).CleanExit = SBPlus.Current.ConnectionStatus == ConnectionStatuses.Disconnected;
                ((ApplicationStartStopLog)logItem.Object).CleanExit = true;
                SBPlus.Current.GlobalStateFile.SetItem(logItem, false);
            }
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
                File.WriteAllText(fileName, logText.ToString());

                ExecuteExceptionReporter(
                    fileName.Replace(" ", "%20%"), 
                    (userId + "(" + DateTime.Now + ")").Replace(" ", "%20%"), 
                    logFolder.Replace(" ", "%20%"));
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
        /// The send freeze.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void SendFreeze(string message)
        {
            try
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
                logText.Append("Ctrl-Shit-G was hit by the user.");
                logText.AppendLine(message);

                logText.AppendLine(string.Format("User Id : {0} ", userId));
                logText.AppendLine(string.Format("Machine Name {0}", SystemInformation.ComputerName));

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

                ExecuteExceptionReporter(
                    fileName.Replace(" ", "%20%"), 
                    (userId + "(" + DateTime.Now + ")").Replace(" ", "%20%"), 
                    logFolder.Replace(" ", "%20%"));
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

        #endregion
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
    }
}