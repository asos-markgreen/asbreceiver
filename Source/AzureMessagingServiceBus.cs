using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;

namespace AsbReceiver
{
    public class AzureMessagingServiceBus : IServiceBus
    {
        private ILogger _logger;
        private ServiceBusReceiver _messageReceiver;
        private TelemetryClient _telemetryClient;

        public AzureMessagingServiceBus(ILogger logger, ServiceBusReceiver messageReceiver, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _messageReceiver = messageReceiver;
            _telemetryClient = telemetryClient;
        }

        public async Task<IList<string?>> ReceiveMessages()
        {

            TimeSpan timeout = TimeSpan.FromSeconds(60);
            int batchSize = 500;

            _logger.LogInformation("Waiting {ReceiveBatchServerWaitTimeTotalSeconds} seconds or until {OptionsBatchSize} max messages.", timeout, batchSize);

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var messages = await _messageReceiver.ReceiveMessagesAsync(batchSize, timeout).ConfigureAwait(false);
                stopwatch.Stop();

                _logger.LogInformation("Received {MessagesCount} messages from the source. Took {StopwatchElapsed}.", messages.Count, stopwatch.Elapsed);

                if (messages.Count > 0)
                {
                    var tasks = new List<Task>();

                    foreach (ServiceBusReceivedMessage message in messages)
                    {
                        tasks.Add(_messageReceiver.CompleteMessageAsync(message));
                    }

                    await Task.WhenAll(tasks);
                }

                _telemetryClient.GetMetric("Batch Processing Time (ms)").TrackValue(stopwatch.Elapsed.TotalMilliseconds);
                _telemetryClient.GetMetric("Batch Size").TrackValue(messages.Count);

                return messages.Select(message => message.Body.ToString()).ToList();
            }
            catch (ServiceBusException exception)
            {
                _logger.LogInformation(exception, "Timed out waiting for messages.");
                return new List<string?>();
            }
        }
    }
}
