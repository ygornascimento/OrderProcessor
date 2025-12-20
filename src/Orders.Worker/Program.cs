using Orders.Worker;
using Orders.Worker.Messaging;
using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

var cs = builder.Configuration.GetConnectionString("OrdersDb");
if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("ConnectionStrings:OrdersDb NÃO carregou no Orders.Worker.");

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));
builder.Services.AddHostedService<RabbitMqOrderConsumer>();

var host = builder.Build();
host.Run();
