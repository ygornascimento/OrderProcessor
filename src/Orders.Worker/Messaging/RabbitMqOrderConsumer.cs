using Microsoft.Extensions.Options;
using Orders.Application.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Orders.Worker.Messaging
{
    public class RabbitMqOrderConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMqOrderConsumer> _logger;
        private readonly RabbitMqOptions _options;

        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMqOrderConsumer(
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqOrderConsumer> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Importante: não pegar um lote gigante sem processar
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

            _logger.LogInformation("Conectado ao RabbitMQ. Consumindo fila {Queue}", _options.QueueName);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel is null)
                throw new InvalidOperationException("Canal RabbitMQ não inicializado.");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var message = JsonSerializer.Deserialize<OrderCreatedMessage>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (message is null)
                    {
                        _logger.LogWarning("Mensagem inválida: {Json}", json);
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                        return;
                    }

                    _logger.LogInformation(
                        "Order recebida: Id={Id} Customer={Customer} Amount={Amount} Date={Date}",
                        message.Id, message.CustomerName, message.Amount, message.OrderDate);

                    // Aqui no futuro entra: persistência SQL
                    await Task.CompletedTask;

                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro processando mensagem.");
                    // Requeue=true pode causar loop infinito se mensagem estiver “podre”
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            _channel.BasicConsume(
                queue: _options.QueueName,
                autoAck: false,
                consumer: consumer);

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.Close();
            _connection?.Close();
            return base.StopAsync(cancellationToken);
        }
    }
}
