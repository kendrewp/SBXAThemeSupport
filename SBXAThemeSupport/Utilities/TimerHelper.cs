// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerHelper.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="TimerHelper.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Utilities
{
    using System.Timers;

    /// <summary>
    ///     The timer helper.
    /// </summary>
    public class TimerHelper : Timer
    {
        #region Fields

        private volatile object data;

        private volatile object tag;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerHelper"/> class.
        /// </summary>
        /// <param name="interval">
        /// The interval.
        /// </param>
        public TimerHelper(double interval)
            : base(interval)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the data.
        /// </summary>
        public object Data
        {
            get
            {
                return this.data;
            }

            set
            {
                this.data = value;
            }
        }

        /// <summary>
        ///     Gets or sets the tag.
        /// </summary>
        public object Tag
        {
            get
            {
                return this.tag;
            }

            set
            {
                this.tag = value;
            }
        }

        #endregion
    }
}