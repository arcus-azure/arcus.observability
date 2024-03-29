﻿using Arcus.Testing.Logging;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace Arcus.Observability.Tests.Integration
{
    public class IntegrationTest
    {
        protected IConfiguration Configuration { get; }

        public IntegrationTest(ITestOutputHelper testOutput)
        {
            // The appsettings.local.json allows users to override (gitignored) settings locally for testing purposes
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(path: "appsettings.json")
                .AddJsonFile(path: "appsettings.local.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}