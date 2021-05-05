using System;
using System.Collections.Generic;
using System.Linq;
using GuardNet;

namespace Arcus.Observability.Telemetry.Core.Logging
{
    /// <summary>
    /// Represents a custom dependency as a logging entry.
    /// </summary>
    public class DependencyLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyLogEntry"/> class.
        /// </summary>
        /// <param name="dependencyType">The custom type of dependency.</param>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <param name="dependencyData">The custom data of dependency.</param>
        /// <param name="targetName">The name of dependency target.</param>
        /// <param name="resultCode">The code of the result of the interaction with the dependency.</param>
        /// <param name="isSuccessful">The indication whether or not the operation was successful.</param>
        /// <param name="startTime">The point in time when the interaction with the HTTP dependency was started.</param>
        /// <param name="duration">The duration of the operation.</param>
        /// <param name="context">The context that provides more insights on the dependency that was measured.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="dependencyData"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="dependencyData"/> is blank.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public DependencyLogEntry(
            string dependencyType,
            string dependencyName,
            object dependencyData,
            string targetName, 
            TimeSpan duration,
            DateTimeOffset startTime,
            int? resultCode,
            bool isSuccessful,
            IDictionary<string, object> context)
        {
            Guard.NotNullOrWhitespace(dependencyType, nameof(dependencyType), "Requires a non-blank custom dependency type when tracking the custom dependency");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time duration of the dependency operation");
            
            DependencyType = dependencyType;
            DependencyName = dependencyName;
            DependencyData = dependencyData;
            TargetName = targetName;
            ResultCode = resultCode;
            IsSuccessful = isSuccessful;

            StartTime = startTime.ToString(FormatSpecifiers.InvariantTimestampFormat);
            Duration = duration;
            Context = context;
        }

        /// <summary>
        /// Gets the custom type of the dependency.
        /// </summary>
        public string DependencyType { get; }
        
        /// <summary>
        /// Gets the name of the dependency.
        /// </summary>
        public string DependencyName { get; }
        
        /// <summary>
        /// Gets the custom data of the dependency.
        /// </summary>
        public object DependencyData { get; }
        
        /// <summary>
        /// Gets the name of the dependency target.
        /// </summary>
        public string TargetName { get; }
        
        /// <summary>
        /// Gets the code of the result of the interaction with the dependency.
        /// </summary>
        public int? ResultCode { get; }
        
        /// <summary>
        /// Gets the indication whether or not the operation was successful.
        /// </summary>
        public bool IsSuccessful { get; }
        
        /// <summary>
        /// Gets the point in time when the interaction with the HTTP dependency was started.
        /// </summary>
        public string StartTime { get; }
        
        /// <summary>
        /// Gets the duration of the operation.
        /// </summary>
        public TimeSpan Duration { get; }
        
        /// <summary>
        /// Gets the context that provides more insights on the dependency that was measured.
        /// </summary>
        public IDictionary<string, object> Context { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            string contextFormatted = $"{{{String.Join("; ", Context.Select(item => $"[{item.Key}, {item.Value}]"))}}}";
            return $"{DependencyType} {DependencyName} {DependencyData} named {TargetName} in {Duration} at {StartTime} " +
                   $"(Successful: {IsSuccessful} - ResultCode: {ResultCode} - Context: {contextFormatted})";
        }
    }
}
