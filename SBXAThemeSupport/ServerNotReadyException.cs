// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServerNotReadyException.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
    using System;

    /// <summary>
    ///     The server not ready exception.
    /// </summary>
    public class ServerNotReadyException : Exception
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServerNotReadyException" /> class.
        /// </summary>
        public ServerNotReadyException()
            : base("The server is not able to accept requests at this time")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerNotReadyException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public ServerNotReadyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerNotReadyException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public ServerNotReadyException(string message, Exception exception)
            : base(message, exception)
        {
        }

        #endregion
    }
}