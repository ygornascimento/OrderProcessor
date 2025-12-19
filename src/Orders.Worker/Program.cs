using Orders.Worker;
using Orders.Worker.Messaging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddHostedService<RabbitMqOrderConsumer>();

var host = builder.Build();
host.Run();
