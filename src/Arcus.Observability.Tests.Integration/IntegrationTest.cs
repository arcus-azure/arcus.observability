using Arcus.Testing;
using Xunit;

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