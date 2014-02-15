using System;
using System.Diagnostics;
using System.Timers;
using System.Windows.Input;
using SBXA.Runtime;
using SBXA.UI.Client;

namespace SBXAThemeSupport.Utilities
{
    public class KeepAliveTimer
    {
        private static KeepAliveTimer _Instance;
        private TimerHelper _TimerHelper;
        private TimerHelper _LogoutTimerHelper;
        private readonly int _Timeout;
        private DateTime _LastMessage;

        private KeepAliveTimer(double interval, string processName, int timeout)
        {
            Interval = interval;
            ProcessName = processName;
            _Timeout = timeout;

            InputManager.Current.PostProcessInput += HandleInputManagerPostProcessInput; 

        }

        void HandleInputManagerPostProcessInput(object sender, ProcessInputEventArgs e)
        {
            if (((e.StagingItem == null) || (e.StagingItem.Input == null)) || (!(e.StagingItem.Input.Device is KeyboardDevice))) return;

            _LastMessage = DateTime.Now;
        }

        public double Interval { get; set; }
        public string ProcessName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval">In milliseconds</param>
        /// <param name="processName"></param>
        /// <param name="timeout">In minutes</param>
        /// <returns></returns>
        public static KeepAliveTimer GetInstance(double interval, string processName, int timeout)
        {
            if (_Instance == null) _Instance = new KeepAliveTimer(interval, processName, timeout);
            return (_Instance);
        }

        public void Start()
        {
            if (_TimerHelper == null && Interval > 0)
            {
                _TimerHelper = new TimerHelper(Interval) {Data = ProcessName};
                _TimerHelper.Elapsed += OnExecuteSBProcessTimerTick;
                _TimerHelper.Start();
                SBPlusClient.LogInformation("Keep alive timer started. With an interval of " + Interval);
            }
            if (_LogoutTimerHelper == null && _Timeout <= 0)
            {
                _LogoutTimerHelper = new TimerHelper(60000) {Data = "XUI.KEEPALIVE,3"};
                _LogoutTimerHelper.Elapsed += OnExecuteLogoutTimerTick;
                _LogoutTimerHelper.Start();
                SBPlusClient.LogInformation("Logout alive timer started with logout timeout = " + _Instance._Timeout);
            }
        }

        public static void Stop()
        {
            if (_Instance._TimerHelper != null)
            {
                _Instance._TimerHelper.Stop();
                _Instance._TimerHelper.Close();
                _Instance._TimerHelper = null;
            }
            if (_Instance._LogoutTimerHelper != null)
            {
                _Instance._LogoutTimerHelper.Stop();
                _Instance._LogoutTimerHelper.Close();
                _Instance._LogoutTimerHelper = null;
            }
        }

        static void OnExecuteSBProcessTimerTick(object sender, ElapsedEventArgs e)
        {
            var timer = sender as TimerHelper;
            if (timer == null) return;

            var processName = timer.Data as string;
            if (string.IsNullOrEmpty(processName)) return;

            SBPlusClient.LogInformation("["+DateTime.Now.ToShortDateString()+"] Executing " + processName);

            SBProcessRunner.ExecuteSBPlusProcess(processName, false);


        }

        static void OnExecuteLogoutTimerTick(object sender, ElapsedEventArgs e)
        {
            var timer = sender as TimerHelper;
            if (timer == null) return;

            var processName = timer.Data as string;
            if (string.IsNullOrEmpty(processName)) return;

            // check to see if I need to log off the user
            var currentTime = DateTime.Now;

            var timeout = new TimeSpan(0, _Instance._Timeout, 0);

            if (currentTime.Subtract(_Instance._LastMessage).CompareTo(timeout) <= 0) return;

            Stop(); // stop the timers so they do not fire while I am logging out.
            SBPlusClient.LogInformation("Executing " + processName + ", current time " + currentTime.ToShortTimeString() +", last message "+_Instance._LastMessage.ToShortTimeString());

            SBProcessRunner.ExecuteSBPlusProcess(processName, false);
        }

    }
}
