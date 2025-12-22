using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orders.Application.Contracts;

namespace Orders.IntegrationTests;

public sealed class CustomWebAppFactory : WebApplicationFactory<Program>
{
    public FakeOrderPublisher Publisher { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove o publisher real (RabbitMqOrderPublisher)
            services.RemoveAll<IOrderPublisher>();

            // Coloca o fake (para o teste "ver" o publish)
            services.AddSingleton<IOrderPublisher>(Publisher);
        });
    }
}

public sealed class FakeOrderPublisher : IOrderPublisher
{
    private readonly List<string> _messages = new();
    public IReadOnlyList<string> Messages => _messages;

    public Task PublishAsync<T>(T message, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _messages.Add(JsonSerializer.Serialize(message));
        return Task.CompletedTask;
    }
}
