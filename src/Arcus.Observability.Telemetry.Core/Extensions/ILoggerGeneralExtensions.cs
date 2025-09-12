using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Extensions on the <see cref="ILogger"/> to provide telemetry contextual information alongside the log message.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static partial class ILoggerExtensions
    {
        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogInformation(this ILogger logger, string message, Dictionary<string, object> context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Information))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Information, (EventId) 0, (Exception) null, message);
                }
            }
        }

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogInformation(
            this ILogger logger,
            string message,
            Dictionary<string, object> context,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Information))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Information, (EventId) 0, (Exception) null, message, args);
                }
            }
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogError(this ILogger logger, string message, Dictionary<string, object> context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Error))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Error, (EventId) 0, (Exception) null, message);
                }
            }
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="exception"/>, or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogError(
            this ILogger logger,
            Exception exception,
            string message,
            Dictionary<string, object> context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Error))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Error, (EventId) 0, exception, message);
                }
            }
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogError(
            this ILogger logger,
            string message,
            Dictionary<string, object> context,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Error))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Error, (EventId) 0, (Exception) null, message, args);
                }
            }
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="exception"/>, or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogError(
            this ILogger logger,
            Exception exception,
            string message,
            Dictionary<string, object> context,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Error))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Error, (EventId) 0, exception, message, args);
                }
            }
        }

        /// <summary>
        /// Formats and writes an critical log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogCritical(this ILogger logger, string message, Dictionary<string, object> context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Critical))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Critical, (EventId) 0, (Exception) null, message);
                }
            }
        }

        /// <summary>
        /// Formats and writes an critical log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="exception"/>, or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogCritical(
            this ILogger logger,
            Exception exception,
            string message,
            Dictionary<string, object> context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Critical))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Critical, (EventId) 0, exception, message);
                }
            }
        }

        /// <summary>
        /// Formats and writes an critical log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogCritical(
            this ILogger logger,
            string message,
            Dictionary<string, object> context,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Critical))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Critical, (EventId) 0, (Exception) null, message, args);
                }
            }
        }

        /// <summary>
        /// Formats and writes an critical log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="exception"/>, or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogCritical(
            this ILogger logger,
            Exception exception,
            string message,
            Dictionary<string, object> context,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Critical))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Critical, (EventId) 0, exception, message, args);
                }
            }
        }

        /// <summary>
        /// Formats and writes an warning log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogWarning(this ILogger logger, string message, Dictionary<string, object> context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Warning))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Warning, (EventId) 0, (Exception) null, message);
                }
            }
        }

        /// <summary>
        /// Formats and writes an warning log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="exception"/>, or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogWarning(
            this ILogger logger,
            Exception exception,
            string message,
            Dictionary<string, object> context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Warning))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Warning, (EventId) 0, exception, message);
                }
            }
        }

        /// <summary>
        /// Formats and writes an warning log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogWarning(
            this ILogger logger,
            string message,
            Dictionary<string, object> context,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Warning))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Warning, (EventId) 0, (Exception) null, message, args);
                }
            }
        }

        /// <summary>
        /// Formats and writes an warning log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/>, <paramref name="exception"/>, or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogWarning(
            this ILogger logger,
            Exception exception,
            string message,
            Dictionary<string, object> context,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Warning))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Warning, (EventId) 0, exception, message, args);
                }
            }
        }

        /// <summary>
        /// Formats and writes an trace log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogTrace(this ILogger logger, string message, Dictionary<string, object> context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Trace))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Trace, (EventId) 0, (Exception) null, message);
                }
            }
        }

        /// <summary>
        /// Formats and writes an trace log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogTrace(
            this ILogger logger,
            string message,
            Dictionary<string, object> context,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Trace))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Trace, (EventId) 0, (Exception) null, message, args);
                }
            }
        }

        /// <summary>
        /// Formats and writes an debug log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogDebug(this ILogger logger, string message, Dictionary<string, object> context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Debug, (EventId) 0, (Exception) null, message);
                }
            }
        }

        /// <summary>
        /// Formats and writes an debug log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="context">The context that provides more insights on the message that was measured.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="message"/> is blank.</exception>
        public static void LogDebug(
            this ILogger logger,
            string message,
            Dictionary<string, object> context,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(context);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                using (logger.BeginScope(context))
                {
                    logger.Log(LogLevel.Debug, (EventId) 0, (Exception) null, message, args);
                }
            }
        }
    }
}
