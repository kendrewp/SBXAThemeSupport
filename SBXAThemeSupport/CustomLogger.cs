// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomLogger.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
    using System;
    using System.Reflection;

    using SBXA.UI.Client;

    /// <summary>
    ///     This is the interface taht the logger that is actually doing to logging should provide.
    /// </summary>
    public interface ICustomLogger
    {
    }

    /// <summary>
    ///     This class provides a front end for logging of information, debug warning and error.
    /// </summary>
    public class CustomLogger : ICustomLogger
    {
        #region Static Fields

        /// <summary>
        /// The logger.
        /// </summary>
        private static ICustomLogger logger;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the logger.
        /// </summary>
        /// <value>
        ///     The logger.
        /// </value>
        public static ICustomLogger Logger
        {
            get
            {
                return logger;
            }

            set
            {
                logger = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Logs the debug.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="loggerName">
        /// Name of the logger.
        /// </param>
        public static void LogDebug(Func<string> message, string loggerName = null)
        {
            if (Logger == null)
            {
                SBPlusClient.LogInformation(message());
                return;
            }

            // use reflection to call the LogDebug method on the Logger.
            Logger.GetType()
                .InvokeMember(
                    "LogDebug", 
                    BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, 
                    null, 
                    null, 
                    new object[] { message, loggerName });
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="loggerName">
        /// Name of the logger.
        /// </param>
        public static void LogError(Func<string> message, string loggerName = null)
        {
            if (Logger == null)
            {
                SBPlusClient.LogError(message());
                return;
            }

            // use reflection to call the LogError method on the Logger.
            Logger.GetType()
                .InvokeMember(
                    "LogError", 
                    BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, 
                    null, 
                    null, 
                    new object[] { message, loggerName });
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void LogException(Exception exception, string message = null)
        {
            if (Logger == null)
            {
                SBPlusClient.LogError(message, exception);
                return;
            }

            // use reflection to call the LogException method on the Logger.
            Logger.GetType()
                .InvokeMember(
                    "LogException", 
                    BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, 
                    null, 
                    null, 
                    new object[] { exception, message });
        }

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="loggerName">
        /// Name of the logger.
        /// </param>
        public static void LogWarning(Func<string> message, string loggerName = null)
        {
            if (Logger == null)
            {
                SBPlusClient.LogWarning(message());
                return;
            }

            try
            {
                // use reflection to call the LogException method on the Logger.
                Logger.GetType()
                    .InvokeMember(
                        "LogWarning", 
                        BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, 
                        null, 
                        null, 
                        new object[] { message(), loggerName });
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("There was a problem logging and Warning.", exception);
                SBPlusClient.LogWarning(message());
            }
        }

        #endregion
    }
}