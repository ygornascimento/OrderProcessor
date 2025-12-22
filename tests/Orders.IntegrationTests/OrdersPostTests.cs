using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Orders.IntegrationTests;

public class OrdersPostTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;

    public OrdersPostTests(CustomWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Post_api_orders_deve_retornar_202_e_publicar_evento()
    {
        // arrange
        var client = _factory.CreateClient();

        var payload = new
        {
            customerName = "Ygor",
            amount = 123.45m,
            orderDate = "2025-12-20T10:30:00Z"
        };

        // act
        var response = await client.PostAsJsonAsync("/api/Orders", payload);

        // assert (HTTP)
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        var id = json.RootElement.GetProperty("id").GetString();
        Assert.False(string.IsNullOrWhiteSpace(id));

        // assert (publish)
        Assert.True(_factory.Publisher.Messages.Count >= 1);

        // Confere que o evento publicado contém os campos esperados
        using var msg = JsonDocument.Parse(_factory.Publisher.Messages[^1]);
        Assert.Equal(id, msg.RootElement.GetProperty("Id").GetString());
        Assert.Equal("Ygor", msg.RootElement.GetProperty("CustomerName").GetString());
        Assert.Equal(123.45m, msg.RootElement.GetProperty("Amount").GetDecimal());
    }
}
