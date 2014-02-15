using System.Timers;

namespace SBXAThemeSupport.Utilities
{
    public class TimerHelper : Timer
    {
        public TimerHelper(double interval) : base(interval) { }

        private volatile object _Tag;
        public object Tag
        {
            get { return _Tag; }
            set { _Tag = value; }
        }

        public object Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        private volatile object _Data;

    }
}
