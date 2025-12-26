using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orders.Application.Messaging;
using Orders.Domain.Entities;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Mongo;
using Orders.Infrastructure.ReadModel;

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
            IServiceScopeFactory scopeFactory
            )
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
                    var readModelWriter = scope.ServiceProvider.GetRequiredService<OrderReadModelWriter>();


                    // 1.1) Idempotência explícita: se já existe, ACK e sai
                    var exists = await db.Orders.AnyAsync(o => o.Id == message.Id, stoppingToken);
                    if (exists)
                    {
                        _logger.LogInformation("Order {Id} já existe (duplicada). ACK e segue.", message.Id);
                        channel.BasicAck(ea.DeliveryTag, multiple: false);
                        return;
                    }

                    // 2) Monta entidade com o MESMO Id da mensagem (idempotência via PK)
                    var order = new Order(
                        message.Id,
                        message.CustomerName,
                        message.Amount,
                        message.OrderDate);

                    db.Orders.Add(order);

                    // 3) Persiste no SQL
                    await db.SaveChangesAsync(stoppingToken);

                    // 3.1) Atualiza read model no Mongo (cache de leitura)
                    var doc = new OrderReadModel
                    {
                        Id = message.Id,
                        CustomerName = message.CustomerName,
                        Amount = message.Amount,
                        OrderDate = message.OrderDate
                    };

                    await readModelWriter.UpsertAsync(doc, stoppingToken);


                    // 4) Só ACK depois de SQL + Mongo
                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Falha ao persistir no SQL. NACK com requeue=true.");
                    channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Cancelamento solicitado. NACK com requeue=true.");
                    channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro processando mensagem. NACK sem requeue.");
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
