using System;
using Arcus.Observability.Telemetry.Serilog.Enrichers;
using Xunit;

namespace Arcus.Observability.Tests.Unit.Serilog
{
    public class AssemblyAppVersionTests
    {
        [Fact]
        public void CreateAppVersion_WithoutConsumerType_Throws()
        {
            Assert.ThrowsAny<ArgumentException>(() => new AssemblyAppVersion(consumerType: null));
        }
    }
}
