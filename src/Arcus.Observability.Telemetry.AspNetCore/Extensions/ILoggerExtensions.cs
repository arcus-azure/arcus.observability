using System;
using System.Collections.Generic;
using Arcus.Observability.Telemetry.Core;
using GuardNet;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Telemetry extensions on the <see cref="ILogger"/> instance to write Application Insights compatible log messages.
    /// </summary>
    public static class ILoggerExtensions
    {
        /// <summary>
        ///     Logs an HTTP request
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that was done</param>
        /// <param name="response">Response that will be sent out</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        public static void LogRequest(this ILogger logger, HttpRequest request, HttpResponse response, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(request, nameof(request));
            Guard.NotNull(response, nameof(response));
            Guard.For<ArgumentException>(() => request.Scheme != null && request.Scheme.Contains(" "), "HTTP request scheme cannot contain whitespace");
            Guard.For<ArgumentException>(() => !request.Host.HasValue, "HTTP request host requires a value");
            Guard.For<ArgumentException>(() => request.Host.ToString()?.Contains(" ") == true, "HTTP request host name cannot contain whitespace");

            LogRequest(logger, request, response.StatusCode, duration, context);
        }

        /// <summary>
        ///     Logs an HTTP request
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="request">Request that was done</param>
        /// <param name="responseStatusCode">HTTP status code returned by the service</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="context">Context that provides more insights on the HTTP request that was tracked</param>
        public static void LogRequest(this ILogger logger, HttpRequest request, int responseStatusCode, TimeSpan duration, Dictionary<string, object> context = null)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(request, nameof(request));
            Guard.For<ArgumentException>(() => request.Scheme != null && request.Scheme.Contains(" "), "HTTP request scheme cannot contain whitespace");
            Guard.For<ArgumentException>(() => !request.Host.HasValue, "HTTP request host requires a value");
            Guard.For<ArgumentException>(() => request.Host.ToString()?.Contains(" ") == true, "HTTP request host name cannot contain whitespace");

            context = context ?? new Dictionary<string, object>();

            PathString resourcePath = request.Path;
            var host = $"{request.Scheme}://{request.Host}";

            logger.LogWarning(MessageFormats.RequestFormat, request.Method, host, resourcePath, responseStatusCode, duration, DateTimeOffset.UtcNow.ToString(FormatSpecifiers.InvariantTimestampFormat), context);
        }
    }
}
