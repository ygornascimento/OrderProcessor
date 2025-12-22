using System.Text.Json;
using Orders.Application.Contracts;
using Orders.Application.UseCases.CreateOrder;
using Xunit;

namespace Orders.UnitTests;

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task HandleAsync_Deve_publicar_mensagem_e_retornar_id()
    {
        // arrange
        var publisher = new FakeOrderPublisher();
        var handler = new CreateOrderHandler(publisher);

        var request = new CreateOrderRequest(
            CustomerName: "Ygor",
            Amount: 123.45m,
            OrderDate: new DateTime(2025, 12, 20, 10, 30, 00, DateTimeKind.Utc)
        );

        // act
        var id = await handler.HandleAsync(request);

        // assert
        Assert.NotEqual(Guid.Empty, id);
        Assert.Single(publisher.Published);

        // Validação "genérica" do payload (funciona mesmo se você publicar um record OU um anonymous object)
        var json = JsonSerializer.Serialize(publisher.Published[0]);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(id.ToString(), doc.RootElement.GetProperty("Id").GetString());
        Assert.Equal("Ygor", doc.RootElement.GetProperty("CustomerName").GetString());
        Assert.Equal(123.45m, doc.RootElement.GetProperty("Amount").GetDecimal());
    }

    [Fact]
    public async Task HandleAsync_Com_token_cancelado_deve_lancar_OperationCanceledException()
    {
        // arrange
        var publisher = new FakeOrderPublisher();
        var handler = new CreateOrderHandler(publisher);

        var request = new CreateOrderRequest("Ygor", 10m, DateTime.UtcNow);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // act/assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.HandleAsync(request, cts.Token)
        );
    }

    private sealed class FakeOrderPublisher : IOrderPublisher
    {
        public List<object> Published { get; } = new();

        public Task PublishAsync<T>(T message, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            Published.Add(message!);
            return Task.CompletedTask;
        }
    }
}
