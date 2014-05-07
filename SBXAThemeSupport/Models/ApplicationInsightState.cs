// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationInsightState.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using System;
    using System.Collections.Specialized;

    using SBXA.Shared;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     The application insight state.
    /// </summary>
    [Serializable]
    public class ApplicationInsightState : SBEntityBase
    {
        #region Fields

        private bool isDebugWindowOpen;

        private StringCollection mruProcessList;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether always open.
        /// </summary>
        public bool AlwaysOpen { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is debug on.
        /// </summary>
        public bool IsDebugOn { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is debug window open.
        /// </summary>
        public bool IsDebugWindowOpen
        {
            get
            {
                return this.isDebugWindowOpen;
            }

            set
            {
                this.isDebugWindowOpen = value;
                if (!DebugViewModel.Initializing)
                {
                    DebugViewModel.Instance.SaveState();
                }
            }
        }

        public StringCollection MruProcessList
        {
            get
            {
                return this.mruProcessList;
            }
            set
            {
                var oldValue = this.mruProcessList;
                this.mruProcessList = value;
                this.OnPropertyChanged("MruProcessList", oldValue, this.mruProcessList);
            }
        }

        #endregion
    }
}