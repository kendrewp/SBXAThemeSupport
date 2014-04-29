// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModel.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="ViewModel.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.ViewModels
{
    using System.ComponentModel;

    /// <summary>
    ///     The view model.
    /// </summary>
    public abstract class ViewModel : INotifyPropertyChanged
    {
        #region Public Events

        /// <summary>
        ///     The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        /// <summary>
        ///     The raise property changed.
        /// </summary>
        /// <param name="propertyName">
        ///     The property name.
        /// </param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}