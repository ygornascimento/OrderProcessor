using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orders.Domain.Entities;
using Orders.Application.Contracts;
using Orders.Application.Messaging;

namespace Orders.Application.UseCases.CreateOrder
{
    public class CreateOrderHandler
    {
        private readonly IOrderPublisher _publisher;

        public CreateOrderHandler(IOrderPublisher publisher)
        {
            _publisher = publisher;
        }
        public async Task<Guid> HandleAsync(CreateOrderRequest request, CancellationToken ct = default)
        {
            var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, request.OrderDate);

            var message = new OrderCreatedMessage
            (
                order.Id,
                order.CustomerName,
                order.Amount,
                order.OrderDate
            );

            await _publisher.PublishAsync(message, ct);
            
            return order.Id;
        }
    }
}
