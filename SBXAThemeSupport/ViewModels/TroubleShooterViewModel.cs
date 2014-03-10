using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using SBXA.Runtime;
using SBXA.Shared;
using SBXA.UI.WPFControls;

namespace SBXAThemeSupport.ViewModels
{
    /// <summary>
    /// TroubleShooterViewModel contraols the actions and behavior of the trouble shooter.
    /// </summary>
    public class TroubleShooterViewModel : ViewModel
    {
        const string START_STOP_ITEM_ID = "ApplicationStartStopLog";
        private static string _FileName = string.Empty;
        private const bool ENABLE_TROUBLESHOOTING = true;

        /// <summary>
        /// Applications the start.
        /// </summary>
        public static void ApplicationStart()
        {
            // First check if I already have an log item and if the last exit was clean.
            if (SBPlus.Current.GlobalStateFile.Exists(START_STOP_ITEM_ID))
            {
                var existingLogItem = SBPlus.Current.GlobalStateFile.GetItem(START_STOP_ITEM_ID);
                if (!((ApplicationStartStopLog) existingLogItem.Object).CleanExit)
                {
                    LogBadClose();
                }
                else
                {
                    JobManager.RunAsyncOnPooledThread(delegate { LocalMachineCleanup.CleanLogs(DateTime.Now); });
                    JobManager.RunAsyncOnPooledThread(delegate { LocalMachineCleanup.CleanVersionFolders(); });
                }
            }
            var logItem = new ApplicationStartStopLog {Start = DateTime.Now, CleanExit = false};
            SBPlus.Current.GlobalStateFile.SetItem(new SBhStateFileItem(START_STOP_ITEM_ID, logItem), true);
        }

        /// <summary>
        /// Applications the stop.
        /// </summary>
        public static void ApplicationStop()
        {
            try
            {
                if (!SBPlus.Current.GlobalStateFile.Exists(START_STOP_ITEM_ID)) return;

                var logItem = SBPlus.Current.GlobalStateFile.GetItem(START_STOP_ITEM_ID);
                ((ApplicationStartStopLog) logItem.Object).Stop = DateTime.Now;

                // ((ApplicationStartStopLog)logItem.Object).CleanExit = SBPlus.Current.ConnectionStatus == ConnectionStatuses.Disconnected;
                ((ApplicationStartStopLog) logItem.Object).CleanExit = true;
                SBPlus.Current.GlobalStateFile.SetItem(logItem, false);
            }
            catch (Exception)
            {
                ;
            }
        }

        /// <summary>
        /// Logs the bad close.
        /// </summary>
        private static void LogBadClose()
        {
            if (ENABLE_TROUBLESHOOTING) SendAbnormalClose("Application was not closed correctly.");
        }

        /// <summary>
        /// Sends the abnormal close.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void SendAbnormalClose(string message)
        {
            try
            {
                var windowsIdentity = SBPlus.Current.SBPlusRuntime.WindowsIdentity;

                var userId = windowsIdentity.Split(@"\".ToCharArray()).Count() == 2 ? windowsIdentity.Split(@"\".ToCharArray())[1] : windowsIdentity.Split(@"\".ToCharArray())[0];
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
                logText.AppendLine(message);

                logText.AppendLine(string.Format("User Id : {0} ", userId));
                logText.AppendLine(string.Format("Machine Name {0}", SystemInformation.ComputerName));
                logText.AppendLine(string.Format("Session Id {0}", sessionId));

                File.WriteAllText(fileName, logText.ToString());

                ExecuteExceptionReporter(fileName.Replace(" ", "%20%"), (userId + "(" + DateTime.Now.ToString() + ") AC").Replace(" ", "%20%"), logFolder.Replace(" ", "%20%"));
            }
            catch (Exception exception)
            {
                ;
            }
        }

        /// <summary>
        /// Executes the exception reporter.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="uniqueId">The unique identifier.</param>
        /// <param name="logFolder">The log folder.</param>
        private static void ExecuteExceptionReporter(string fileName, string uniqueId, string logFolder)
        {
            // string processInfo = "ExceptionReporter.exe "+fileName;
            var processInfo = new ProcessStartInfo("SBXAExceptionReporter.exe") { UseShellExecute = false, Arguments = fileName + " " + uniqueId + " " + logFolder };
            Process.Start(processInfo);
        }

        /// <summary>
        /// Gets the log file path.
        /// </summary>
        /// <returns></returns>
        private static string GetLogFilePath()
        {
            return (Log.LOG_DIRECTORY + @"\Client\");
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

                var userId = windowsIdentity.Split(@"\".ToCharArray()).Count() == 2 ? windowsIdentity.Split(@"\".ToCharArray())[1] : windowsIdentity.Split(@"\".ToCharArray())[0];
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
            catch (Exception exception)
            {
                ;
            }
        }

        /// <summary>
        /// Sends the ctrol shift g.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void SendCtrolShiftG(string message)
        {
            try
            {
                var windowsIdentity = SBPlus.Current.SBPlusRuntime.WindowsIdentity;

                var userId = windowsIdentity.Split(@"\".ToCharArray()).Count() == 2 ? windowsIdentity.Split(@"\".ToCharArray())[1] : windowsIdentity.Split(@"\".ToCharArray())[0];
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
                                logText.AppendLine(string.Format(" {0} {1} IsActive = {2}", form.ObjectHandle.GetType().Name, gobj.ProcessName, (form.ObjectHandle.ParentSBWindow != null && ((Window)form.ObjectHandle.ParentSBWindow).IsActive)));
                            }
                        }
                        else
                        {
                            var menu = form.ObjectHandle as SBMenu;
                            if (menu != null && menu.MenuDefinition != null)
                            {
                                logText.AppendLine(string.Format(" {0} {1} IsActive = {2}", form.ObjectHandle.GetType().Name, menu.MenuDefinition.Name, (menu.ParentSBWindow != null && ((Window)menu.ParentSBWindow).IsActive)));
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

                ExecuteExceptionReporter(fileName.Replace(" ", "%20%"), (userId + "(" + DateTime.Now.ToString() + ") CSG").Replace(" ", "%20%"), logFolder.Replace(" ", "%20%"));
            }
            catch (Exception exception)
            {
                ;
            }
        }

        /// <summary>
        /// Sends the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static void SendException(Exception exception)
        {
            var windowsIdentity = SBPlus.Current.SBPlusRuntime.WindowsIdentity;

            var userId = windowsIdentity.Split(@"\".ToCharArray()).Count() == 2 ? windowsIdentity.Split(@"\".ToCharArray())[1] : windowsIdentity.Split(@"\".ToCharArray())[0];
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

            ExecuteExceptionReporter(fileName.Replace(" ", "%20%"), (userId + "(" + DateTime.Now.ToString() + ")").Replace(" ", "%20%"), logFolder.Replace(" ", "%20%"));
        }
    }

    [Serializable]
    public class ApplicationStartStopLog : SBEntityBase
    {
        public DateTime Start;
        public DateTime Stop;
        public bool CleanExit;
    }
}
