using Arcus.Testing;
using GuardNet;

namespace Arcus.Observability.Tests.Integration.Configuration
{
    /// <summary>
    /// Represents the service principal to authenticate against Azure services.
    /// </summary>
    public class ServicePrincipal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServicePrincipal" /> class.
        /// </summary>
        public ServicePrincipal(string tenantId, string clientId, string clientSecret)
        {
            Guard.NotNullOrWhitespace(tenantId, nameof(tenantId));
            Guard.NotNullOrWhitespace(clientId, nameof(clientId));
            Guard.NotNullOrWhitespace(clientSecret, nameof(clientSecret));

            TenantId = tenantId;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        /// <summary>
        /// Gets the tenant ID of the Azure Active Directory tenant.
        /// </summary>
        public string TenantId { get; }

        /// <summary>
        /// Gets the client ID of the service principal.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Gets the client secret of the service principal.
        /// </summary>
        public string ClientSecret { get; }
    }

    /// <summary>
    /// Extensions on the <see cref="TestConfig"/> to load the service principal from the test configuration.
    /// </summary>
    public static class TestConfigExtensions
    {
        /// <summary>
        /// Loads the service principal from the test configuration.
        /// </summary>
        public static ServicePrincipal GetServicePrincipal(this TestConfig config)
        {
            return new ServicePrincipal(
                config["Arcus:TenantId"],
                config["Arcus:ServicePrincipal:ClientId"],
                config["Arcus:ServicePrincipal:ClientSecret"]);
        }
    }
}