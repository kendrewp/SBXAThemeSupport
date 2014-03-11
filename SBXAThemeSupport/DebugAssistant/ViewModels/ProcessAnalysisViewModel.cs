using System.Windows.Threading;
using SBXA.Runtime;
using SBXA.Shared;
using SBXA.UI.Client;
using SBXAThemeSupport.DebugAssistant.Models;
using SBXAThemeSupport.ViewModels;

namespace SBXAThemeSupport.DebugAssistant.ViewModels
{
    /// <summary>
    /// The view model to handle analysis of a process.
    /// </summary>
    public class ProcessAnalysisViewModel : ViewModel
    {
        private ProcessDescription _Process;
        private string _ProcessName;

        /// <summary>
        ///     Gets the process stack.
        /// </summary>
        public ProcessDescription Process
        {
            get
            {
                return this._Process;
            }
            set
            {
                _Process = value;
                RaisePropertyChanged("Process");
            }
        }

        /// <summary>
        /// Gets or sets the name of the process.
        /// </summary>
        /// <value>
        /// The name of the process.
        /// </value>
        public string ProcessName
        {
            get { return _ProcessName; }
            set
            {
                if (_ProcessName != null && _ProcessName.Equals(value))
                {
                    return;
                }
                _ProcessName = value;
                LoadProcess(_ProcessName);
                RaisePropertyChanged("ProcessName");

            }
        }

        /// <summary>
        /// Loads the process from xxPROCESS.
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        /// <param name="sysid">The system id to use otherwise the current system id will be used.</param>
        private void LoadProcess(string processName, string sysid = "")
        {
            // read process record from current xxProcess
            var processFile = sysid;
            if (string.IsNullOrEmpty(processFile))
            {
                processFile = SBPlusClient.Current.SystemId;
            }

            processFile += "PROCESS";

            JobManager.RunInUIThread(
                    DispatcherPriority.Input,
                    () =>
                    SBProcessRunner.Instance.CallSubroutine(
                        "UT.XUI.READ",
                        6,
                        new[] { new SBString("VOC"), new SBString("XUI.DEBUG"), new SBString(), new SBString(), new SBString("0"), new SBString() },
                        new object(),
                        this.ReadProcessCompleted));
        }

        private void ReadProcessCompleted(string subroutineName, SBString[] parameters, object userState)
        {
            
        }

    }
}
