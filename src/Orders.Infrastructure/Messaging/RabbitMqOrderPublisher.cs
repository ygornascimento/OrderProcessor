using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Orders.Application.Contracts;
using RabbitMQ.Client;

namespace Orders.Infrastructure.Messaging
{
    public class RabbitMqOrderPublisher : IOrderPublisher, IDisposable
    {
        private readonly RabbitMqOptions _options;
        private readonly IConnection _connection;
        public RabbitMqOrderPublisher(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
            var factory = new ConnectionFactory { 
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
            };

            _connection = factory.CreateConnection();
        }

        public Task PublishAsync<T>(T message, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            using var channel = _connection.CreateModel();

            // garante que a fila existe
            channel.QueueDeclare(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = channel.CreateBasicProperties();
            props.Persistent = true; // tenta persistir no disco (se a fila for durable)

            // Default exchange: exchange = "" e routingKey = nome da fila
            channel.BasicPublish(
                exchange: "",
                routingKey: _options.QueueName,
                basicProperties: props,
                body: body
            );

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

    }
}
