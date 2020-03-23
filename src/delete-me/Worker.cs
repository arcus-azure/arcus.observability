using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace delete_me
{
    public class Worker : BackgroundService
    {
        private readonly Repository _repository = new Repository();
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await LogHttpDependency();

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task LogHttpDependency()
        {
            var httpClient = new HttpClient();
            var durationMeasurement = new Stopwatch();
            var context = new Dictionary<string, object>
            {
                { "Tenant", "Contoso"},
            };


            var request = new HttpRequestMessage(HttpMethod.Post, "http://requestbin.net/r/ujxglouj")
            {
                Content = new StringContent("{\"message\":\"Hello World!\"")
            };

            var startTime = DateTimeOffset.UtcNow;
            durationMeasurement.Start();
            var response = await httpClient.SendAsync(request);

            _logger.LogHttpDependency(request, response.StatusCode, startTime, durationMeasurement.Elapsed, context: context);
        }

        private async Task SQL()
        {
            var durationMeasurement = new Stopwatch();
            var context = new Dictionary<string, object>
            {
                { "Catalog", "Products"},
                { "Tenant", "Contoso"},
            };

            var startTime = DateTimeOffset.UtcNow;
            durationMeasurement.Start();
            var products = await _repository.GetProducts();

            _logger.LogSqlDependency("sample-server", "sample-database", "my-table", "get-products", isSuccessful: true, startTime: startTime, duration: durationMeasurement.Elapsed, context: context);

            //_logger.LogMetric("Invoice Received", 133.37, context);

            //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }
    }

    public class Repository
    {
        public async Task<List<string>> GetProducts()
        {
            await Task.Delay(1234);
            return new List<string>();
        }
    }
}
