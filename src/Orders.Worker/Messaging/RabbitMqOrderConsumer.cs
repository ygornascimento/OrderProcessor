using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orders.Application.Messaging;
using Orders.Domain.Entities;
using Orders.Infrastructure.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Orders.Worker.Messaging
{
    public class RabbitMqOrderConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMqOrderConsumer> _logger;
        private readonly RabbitMqOptions _options;
        private readonly IServiceScopeFactory _scopeFactory;

        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMqOrderConsumer(
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqOrderConsumer> logger,
            IServiceScopeFactory scopeFactory)
        {
            _options = options.Value;
            _logger = logger;
            _scopeFactory = scopeFactory;
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

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

            _logger.LogInformation("Conectado ao RabbitMQ. Consumindo fila {Queue}", _options.QueueName);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channel = _channel ?? throw new InvalidOperationException("Canal RabbitMQ não inicializado.");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (_, ea) =>
            {
                try
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var message = JsonSerializer.Deserialize<OrderCreatedMessage>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (message is null)
                    {
                        _logger.LogWarning("Mensagem inválida: {Json}", json);
                        channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                        return;
                    }

                    _logger.LogInformation(
                        "Order recebida: Id={Id} Customer={Customer} Amount={Amount} Date={Date}",
                        message.Id, message.CustomerName, message.Amount, message.OrderDate);

                    // 1) Scope por mensagem (DbContext scoped)
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                    // 2) Monta entidade com o MESMO Id da mensagem (idempotência via PK)
                    var order = new Order(
                        message.Id,
                        message.CustomerName,
                        message.Amount,
                        message.OrderDate);

                    db.Orders.Add(order);

                    // 3) Persiste
                    await db.SaveChangesAsync(stoppingToken);

                    // 4) Só ACK depois do commit no banco
                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (DbUpdateException dbEx)
                {
                    // Idempotência mínima:
                    // Se a mesma mensagem chegar de novo (reentrega), o PK (Id) já existe.
                    // Em vez de reprocessar eternamente, você dá ACK e segue.
                    _logger.LogWarning(dbEx, "Possível duplicidade (PK). ACK para evitar reprocessamento infinito.");
                    _channel?.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (OperationCanceledException)
                {
                    // Se o serviço está parando, não inventa moda
                    _logger.LogInformation("Cancelamento solicitado. Encerrando processamento.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro processando mensagem. NACK sem requeue (mensagem pode estar 'podre').");
                    channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            channel.BasicConsume(
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
