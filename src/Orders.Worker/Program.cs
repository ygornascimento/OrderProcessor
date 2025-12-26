using Orders.Worker;
using Orders.Worker.Messaging;
using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Mongo;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

var cs = builder.Configuration.GetConnectionString("OrdersDb");
if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("ConnectionStrings:OrdersDb NÃO carregou no Orders.Worker.");

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));
builder.Services.AddHostedService<RabbitMqOrderConsumer>();

builder.Services.Configure<MongoOptions>(
    builder.Configuration.GetSection(MongoOptions.SectionName));
builder.Services.AddScoped<OrderReadModelWriter>();

builder.Services.AddSingleton<MongoDb>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

host.Run();
