using System;
using System.IO;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Arcus.Observability.Tests.Runtimes.AzureFunctions
{
    public class HttpOrderTrigger
    {
        [FunctionName("order")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            using (var measurement = DurationMeasurement.Start())
            {
                try
                {
                    return new OkResult();
                }
                finally
                {
                    log.LogRequest(req, responseStatusCode: 200, measurement);
                }
            }
        }
    }
}
