using Microsoft.AspNetCore.Mvc;
using Orders.Application.UseCases.CreateOrder;

namespace Orders.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly CreateOrderHandler _handler;

        public OrdersController(CreateOrderHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        {
            var id = await _handler.HandleAsync(request, ct);
            return Accepted(new { id });
        }
    }
}
