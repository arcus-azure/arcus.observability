using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Arcus.Observability.Tests.Runtimes.AzureFunctions
{
    public class TimerTriggerFunction
    {
        [Function("timer")]
        public static void Run(
            [TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo timer,
            ILogger log)
        {
            log.LogInformation("C# Timer trigger function processed a request.");
        }
    }
}
