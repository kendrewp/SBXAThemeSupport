using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBXAThemeSupport.Models
{
    using SBXAThemeSupport.DebugAssistant.ViewModels;

    [Serializable]
    public class ApplicationInsightState
    {
        private bool isDebugWindowOpen;

        public bool AlwaysOpen { get; set; }
        public bool IsDebugOn { get; set; }

        public bool IsDebugWindowOpen
        {
            get
            {
                return this.isDebugWindowOpen;
            }
            set
            {
                this.isDebugWindowOpen = value;
                DebugViewModel.Instance.SaveState();
            }
        }
    }
}
