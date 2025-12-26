using Microsoft.AspNetCore.Mvc;
using Orders.Application.UseCases.CreateOrder;
using Orders.Infrastructure.Mongo;
using Orders.Api.Dto;

namespace Orders.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly CreateOrderHandler _handler;
        private readonly OrderReadModelReader _reader;

        public OrdersController(CreateOrderHandler handler, OrderReadModelReader reader)
        {
            _handler = handler;
            _reader = reader;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        {
            var id = await _handler.HandleAsync(request, ct);
            return Accepted(new { id });
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<GetOrderResponseDto>>> Get(CancellationToken ct)
        {
            var docs = await _reader.GetLatestAsync(limit: 100, ct);

            var response = docs.Select(d => new GetOrderResponseDto(
                d.Id,
                d.CustomerName,
                d.Amount,
                d.OrderDate)).ToList();

            return Ok(response);
        }

    }
}
