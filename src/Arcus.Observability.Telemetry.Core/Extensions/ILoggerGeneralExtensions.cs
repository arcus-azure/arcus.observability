using System;
using System.Collections.Generic;
using GuardNet;

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an informational message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an informational message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged informational message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an informational message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an informational message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged informational message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an error message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an error message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged error message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an error message with a telemetry context");
            Guard.NotNull(exception, nameof(exception), "Requires an exception to include with the logged error message");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an error message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged error message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an error message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an error message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged error message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an error message with a telemetry context");
            Guard.NotNull(exception, nameof(exception), "Requires an exception to include with the logged error message");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an error message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged error message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an critical message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an critical message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged critical message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an critical message with a telemetry context");
            Guard.NotNull(exception, nameof(exception), "Requires an exception to include with the logged critical message");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an critical message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged critical message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an critical message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an critical message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged critical message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an critical message with a telemetry context");
            Guard.NotNull(exception, nameof(exception), "Requires an exception to include with the logged critical message");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an critical message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged critical message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an warning message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an warning message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged warning message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an warning message with a telemetry context");
            Guard.NotNull(exception, nameof(exception), "Requires an exception to include with the logged warning message");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an warning message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged warning message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an warning message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an warning message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged warning message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an warning message with a telemetry context");
            Guard.NotNull(exception, nameof(exception), "Requires an exception to include with the logged warning message");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an warning message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged warning message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an trace message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an trace message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged trace message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an trace message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an trace message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged trace message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an debug message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an debug message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged debug message");

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
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write an debug message with a telemetry context");
            Guard.NotNullOrWhitespace(message, nameof(message), "Requires an debug message to write to the logger with a telemetry context");
            Guard.NotNull(context, nameof(context), "Requires a telemetry context to include with the logged debug message");

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
