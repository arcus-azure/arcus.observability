using System;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Telemetry
{
    [Trait("Category", "Unit")]
    public class DependencyMeasurementTests
    {
        [Fact]
        public async Task DependencyMeasurement_StartMeasuringAction_HoldsStartAndElapsedTime()
        {
            // Act
            using (var measurement = DependencyMeasurement.Start()) 
            {
                // Assert
                await Task.Delay(TimeSpan.FromMilliseconds(100));

                Assert.NotNull(measurement);
                Assert.True(DateTimeOffset.UtcNow > measurement.StartTime, "Measurement should have lesser start time");
                Assert.True(TimeSpan.Zero < measurement.Elapsed, "Measurement should be running");
            }
        }

        [Fact]
        public async Task DependencyMeasurement_StopsMeasuringAction_WhenDisposed()
        {
            // Arrange
            var measurement = DependencyMeasurement.Start();
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            measurement.Dispose();
            var elapsed = measurement.Elapsed;

            // Act
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            // Assert
            Assert.NotNull(measurement);
            Assert.Equal(elapsed, measurement.Elapsed);
        }

        [Fact]
        public async Task DependencyMeasurementWithDependencyData_StartMeasuringAction_HoldsStartAndElapsedTime()
        {
            // Act
            const string dependencyData = "Operation";
            using (var measurement = DependencyMeasurement.Start(dependencyData)) 
            {
                // Assert
                await Task.Delay(TimeSpan.FromMilliseconds(100));

                Assert.NotNull(measurement);
                Assert.Equal(dependencyData, measurement.DependencyData);
                Assert.True(DateTimeOffset.UtcNow > measurement.StartTime, "Measurement should have lesser start time");
                Assert.True(TimeSpan.Zero < measurement.Elapsed, "Measurement should be running");
            }
        }

        [Fact]
        public async Task DependencyMeasurementWithDependencyData_StopsMeasuringAction_WhenDisposed()
        {
            // Arrange
            const string dependencyData = "Operation";
            var measurement = DependencyMeasurement.Start(dependencyData);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            measurement.Dispose();
            var elapsed = measurement.Elapsed;

            // Act
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            // Assert
            Assert.NotNull(measurement);
            Assert.Equal(dependencyData, measurement.DependencyData);
            Assert.Equal(elapsed, measurement.Elapsed);
        }
    }
}
