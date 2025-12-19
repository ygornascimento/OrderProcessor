using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Application.Contracts
{
    public interface IOrderPublisher
    {
        Task PublishAsync<T>(T message, CancellationToken ct = default);
    }
}
