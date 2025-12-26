using System.Net;
using System.Net.Http.Json;
using Xunit;

public class OrdersE2ETests
{
    private readonly HttpClient _client;

    public OrdersE2ETests()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000") // compose
        };
    }

    [Fact]
    public async Task Post_then_GetList_should_eventually_contain_order()
    {
        // Arrange
        var req = new CreateOrderRequestDto(
            CustomerName: "Ygor IT E2E",
            Amount: 123.45m,
            OrderDate: DateTime.UtcNow);

        // Act 1: POST
        var post = await _client.PostAsJsonAsync("/api/Orders", req);
        Assert.Equal(HttpStatusCode.Accepted, post.StatusCode);

        var postBody = await post.Content.ReadFromJsonAsync<CreateOrderAcceptedDto>();
        Assert.NotNull(postBody);
        Assert.NotEqual(Guid.Empty, postBody!.Id);

        var id = postBody.Id;

        // Act 2: Poll GET list until the worker writes to Mongo (and GET reads it)
        var found = await EventuallyAsync(
            condition: async () =>
            {
                var list = await _client.GetFromJsonAsync<List<GetOrderResponseDto>>("/api/Orders");
                return list?.Any(o => o.Id == id) == true;
            },
            timeout: TimeSpan.FromSeconds(10),
            interval: TimeSpan.FromMilliseconds(250));

        Assert.True(found, $"Order {id} não apareceu no GET dentro do timeout.");

        // Optional: valida campos (fazendo uma última leitura “real”)
        var finalList = await _client.GetFromJsonAsync<List<GetOrderResponseDto>>("/api/Orders");
        var order = finalList!.Single(o => o.Id == id);

        Assert.Equal(req.CustomerName, order.CustomerName);
        Assert.Equal(req.Amount, order.Amount);
        // cuidado: se você normaliza timezone/precisão, compare com tolerância
    }

    private static async Task<bool> EventuallyAsync(
        Func<Task<bool>> condition,
        TimeSpan timeout,
        TimeSpan interval)
    {
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            if (await condition()) return true;
            await Task.Delay(interval);
        }

        return false;
    }

    // DTOs do teste
    public record CreateOrderRequestDto(string CustomerName, decimal Amount, DateTime OrderDate);
    public record CreateOrderAcceptedDto(Guid Id);
    public record GetOrderResponseDto(Guid Id, string CustomerName, decimal Amount, DateTime OrderDate);
}
