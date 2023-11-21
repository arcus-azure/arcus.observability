using System;
using Arcus.Observability.Tests.Core;
using GuardNet;
using Xunit;

namespace Arcus.Observability.Tests.Integration.Fixture
{
    /// <summary>
    /// Represents a temporary authenticated connection as a managed identity to an Azure resource.
    /// </summary>
    public class TemporaryManagedIdentityConnection : IDisposable
    {
        private const string TenantIdName = "AZURE_TENANT_ID",
                             ClientIdName = "AZURE_CLIENT_ID",
                             ClientSecretName = "AZURE_CLIENT_SECRET";

        private readonly TemporaryEnvironmentVariable[] _variables;

        private TemporaryManagedIdentityConnection(
            string clientId,
            params TemporaryEnvironmentVariable[] variables)
        {
            Guard.NotNullOrWhitespace(clientId, nameof(clientId));
            Guard.NotNull(variables, nameof(variables));
            Guard.NotAny(variables, nameof(variables));

            _variables = variables;
            ClientId = clientId;
        }

        /// <summary>
        /// Gets the ID of the service principal within this temporary managed identity connection.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Creates a temporary authenticated connection as a managed identity to an Azure resource.
        /// </summary>
        /// <param name="tenantId">The Azure tenant ID where the interaction takes place.</param>
        /// <param name="clientId">The ID of the service principal in this managed identity authentication.</param>
        /// <param name="clientSecret">The secret of the service principal in this managed identity authentication.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="tenantId"/>, <paramref name="clientId"/>, or the <paramref name="clientSecret"/> is blank.</exception>
        public static TemporaryManagedIdentityConnection Create(string tenantId, string clientId, string clientSecret)
        {
            Guard.NotNullOrWhitespace(tenantId, nameof(tenantId));
            Guard.NotNullOrWhitespace(clientId, nameof(clientId));
            Guard.NotNullOrWhitespace(clientSecret, nameof(clientSecret));

            return new TemporaryManagedIdentityConnection(
                clientId,
                TemporaryEnvironmentVariable.Create(TenantIdName, tenantId),
                TemporaryEnvironmentVariable.Create(ClientIdName, clientId),
                TemporaryEnvironmentVariable.Create(ClientSecretName, clientSecret));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Assert.All(_variables, v => v.Dispose());
        }
    }
}
