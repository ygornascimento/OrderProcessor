using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Application.UseCases.CreateOrder
{
    public record CreateOrderRequest(string CustomerName, decimal Amount, DateTime OrderDate);
}
