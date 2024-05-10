using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsbReceiver
{

    public class ServiceBusListener
    {
        private readonly IServiceBus _serviceBus;
        private readonly ILogger _logger;

        public ServiceBusListener(IServiceBus serviceBus, ILogger logger)
        {
            _serviceBus = serviceBus;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            while (true)
            {
                IList<string?> messages = await _serviceBus.ReceiveMessages();
            }
        }
    }
}
