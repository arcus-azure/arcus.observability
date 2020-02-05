using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Arcus.Template.Tests.Integration
{
    [Trait(name: "Category", value: "Integration")]
    public class DemoIntegrationTest : IntegrationTest
    {
        // The same tests should be tested with different KeyVaultClientFactories 
        // What's the best approach for this ?

        public DemoIntegrationTest(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        [Fact]
        public async Task ThisJustWorks()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
