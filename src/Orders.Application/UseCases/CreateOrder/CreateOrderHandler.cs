using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orders.Domain.Entities;

namespace Orders.Application.UseCases.CreateOrder
{
    public class CreateOrderHandler
    {
        public Task<Guid> HandleAsync(CreateOrderRequest request, CancellationToken ct = default)
        {
            var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, request.OrderDate);
            return Task.FromResult(order.Id);
        }
    }
}
