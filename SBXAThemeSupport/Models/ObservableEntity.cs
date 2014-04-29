// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObservableEntity.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="ObservableEntity.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.Models
{
    using System.ComponentModel;

    /// <summary>
    ///     The observable entity.
    /// </summary>
    public class ObservableEntity : INotifyPropertyChanged
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