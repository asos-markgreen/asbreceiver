using Microsoft.ApplicationInsights;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsbReceiver
{
    public class ServiceBus : IServiceBus
    {
        private readonly ILogger _logger;
        private readonly IMessageReceiver _messageReceiver;
        private readonly TelemetryClient _telemetryClient;
        public ServiceBus(ILogger logger, IMessageReceiver messageReceiver, TelemetryClient telemetryClient) { 
            _logger = logger;
            _messageReceiver = messageReceiver;
            _telemetryClient = telemetryClient;
        }

        public async Task<IList<Message>> ReceiveMessages()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(60);
            int batchSize = 500;

            _logger.LogInformation("Waiting {ReceiveBatchServerWaitTimeTotalSeconds} seconds or until {OptionsBatchSize} max messages.", timeout, batchSize);

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var messages = await _messageReceiver.ReceiveAsync(batchSize, timeout).ConfigureAwait(false) ?? new List<Message>();
                stopwatch.Stop();

                _logger.LogInformation("Received {MessagesCount} messages from the source. Took {StopwatchElapsed}.", messages.Count, stopwatch.Elapsed);
                    
                if (messages.Count > 0) { 
                    await _messageReceiver.CompleteAsync(messages.Select(m => m.SystemProperties.LockToken)).ConfigureAwait(false);
                }

                _telemetryClient.GetMetric("Batch Processing Time (ms)").TrackValue(stopwatch.Elapsed.TotalMilliseconds);
                _telemetryClient.GetMetric("Batch Size").TrackValue(messages.Count);

                return messages;
            }
            catch (ServiceBusTimeoutException exception)
            {
                _logger.LogInformation(exception, "Timed out waiting for messages.");
                return new List<Message>();
            }
        }
    }
}
