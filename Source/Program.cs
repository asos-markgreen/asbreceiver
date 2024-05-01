using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AsbReceiver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<MyWorker>();
                    services.AddApplicationInsightsTelemetryWorkerService();
                });
    }

    public class MyWorker : BackgroundService
    {
        private readonly ILogger<MyWorker> _logger;
        private readonly IMessageReceiver _messageReceiver;
        private readonly IServiceBus _serviceBus;
        private readonly IConfiguration _configuration;

        public MyWorker(ILogger<MyWorker> logger, IConfiguration configuration, IHostEnvironment environment, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _configuration = configuration;
            string connectionString = _configuration["ServiceBus:ConnectionString"];
            string entityPath = _configuration["ServiceBus:EntityPath"];

            _messageReceiver = new MessageReceiver(connectionString, entityPath);

            _serviceBus = new ServiceBus(logger, _messageReceiver, telemetryClient);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var listener = new ServiceBusListener(_serviceBus, _logger);
                await listener.StartAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}