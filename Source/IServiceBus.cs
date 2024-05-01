using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsbReceiver
{
    public interface IServiceBus
    {
        Task<IList<Message>> ReceiveMessages();
    }
}
