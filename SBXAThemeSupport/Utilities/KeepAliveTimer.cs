// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeepAliveTimer.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="KeepAliveTimer.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="KeepAliveTimer.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.Utilities
{
    using System;
    using System.Timers;
    using System.Windows.Input;

    using SBXA.UI.Client;

    /// <summary>
    ///     The keep alive timer.
    /// </summary>
    public class KeepAliveTimer
    {
        #region Static Fields

        private static KeepAliveTimer instance;

        #endregion

        #region Fields

        private readonly int timeout;

        private DateTime lastMessage;

        private TimerHelper logoutTimerHelper;

        private TimerHelper timerHelper;

        #endregion

        #region Constructors and Destructors

        private KeepAliveTimer(double interval, string processName, int timeout)
        {
            this.Interval = interval;
            this.ProcessName = processName;
            this.timeout = timeout;

            InputManager.Current.PostProcessInput += this.HandleInputManagerPostProcessInput;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the interval.
        /// </summary>
        public double Interval { get; set; }

        /// <summary>
        ///     Gets or sets the process name.
        /// </summary>
        public string ProcessName { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <param name="interval">
        ///     The interval in milliseconds.
        /// </param>
        /// <param name="processName">
        ///     Name of the process.
        /// </param>
        /// <param name="timeout">
        ///     The timeout in seconds.
        /// </param>
        /// <returns>
        ///     Returns an instance of <see cref="KeepAliveTimer" />.
        /// </returns>
        public static KeepAliveTimer GetInstance(double interval, string processName, int timeout)
        {
            return instance ?? (instance = new KeepAliveTimer(interval, processName, timeout));
        }

        /// <summary>
        ///     The stop.
        /// </summary>
        public static void Stop()
        {
            if (instance.timerHelper != null)
            {
                instance.timerHelper.Stop();
                instance.timerHelper.Close();
                instance.timerHelper = null;
            }

            if (instance.logoutTimerHelper != null)
            {
                instance.logoutTimerHelper.Stop();
                instance.logoutTimerHelper.Close();
                instance.logoutTimerHelper = null;
            }
        }

        /// <summary>
        ///     The start.
        /// </summary>
        public void Start()
        {
            if (this.timerHelper == null && this.Interval > 0)
            {
                this.timerHelper = new TimerHelper(this.Interval) { Data = this.ProcessName };
                this.timerHelper.Elapsed += OnExecuteSBProcessTimerTick;
                this.timerHelper.Start();
                SBPlusClient.LogInformation("Keep alive timer started. With an interval of " + this.Interval);
            }

            if (this.logoutTimerHelper == null && this.timeout <= 0)
            {
                this.logoutTimerHelper = new TimerHelper(60000) { Data = "XUI.KEEPALIVE,3" };
                this.logoutTimerHelper.Elapsed += OnExecuteLogoutTimerTick;
                this.logoutTimerHelper.Start();
                SBPlusClient.LogInformation("Logout alive timer started with logout timeout = " + instance.timeout);
            }
        }

        #endregion

        #region Methods

        private static void OnExecuteLogoutTimerTick(object sender, ElapsedEventArgs e)
        {
            var timer = sender as TimerHelper;
            if (timer == null)
            {
                return;
            }

            var processName = timer.Data as string;
            if (string.IsNullOrEmpty(processName))
            {
                return;
            }

            // check to see if I need to log off the user
            var currentTime = DateTime.Now;

            var timeout = new TimeSpan(0, instance.timeout, 0);

            if (currentTime.Subtract(instance.lastMessage).CompareTo(timeout) <= 0)
            {
                return;
            }

            Stop(); // stop the timers so they do not fire while I am logging out.
            SBPlusClient.LogInformation(
                "Executing " + processName + ", current time " + currentTime.ToShortTimeString() + ", last message "
                + instance.lastMessage.ToShortTimeString());

            SbProcessHandler.CallProcess(processName, false);
        }

        private static void OnExecuteSBProcessTimerTick(object sender, ElapsedEventArgs e)
        {
            var timer = sender as TimerHelper;
            if (timer == null)
            {
                return;
            }

            var processName = timer.Data as string;
            if (string.IsNullOrEmpty(processName))
            {
                return;
            }

            SBPlusClient.LogInformation("[" + DateTime.Now.ToShortDateString() + "] Executing " + processName);

            SbProcessHandler.CallProcess(processName, false);
        }

        private void HandleInputManagerPostProcessInput(object sender, ProcessInputEventArgs e)
        {
            if (((e.StagingItem == null) || (e.StagingItem.Input == null)) || (!(e.StagingItem.Input.Device is KeyboardDevice)))
            {
                return;
            }

            this.lastMessage = DateTime.Now;
        }

        #endregion
    }
}