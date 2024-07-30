using Arcus.Testing;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration
{
    public class IntegrationTest
    {
        protected TestConfig Configuration { get; }

        public IntegrationTest(ITestOutputHelper testOutput)
        {
            Configuration = TestConfig.Create();
        }
    }
}