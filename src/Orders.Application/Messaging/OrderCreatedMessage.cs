using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Application.Messaging
{
    public record OrderCreatedMessage(Guid Id, string CustomerName, decimal Amount, DateTime OrderDate);
}
