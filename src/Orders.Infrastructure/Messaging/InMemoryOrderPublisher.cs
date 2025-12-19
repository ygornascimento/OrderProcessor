using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orders.Application.Contracts;

namespace Orders.Infrastructure.Messaging
{
    internal class InMemoryOrderPublisher : IOrderPublisher
    {
        public Task PublishAsync<T>(T message, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
