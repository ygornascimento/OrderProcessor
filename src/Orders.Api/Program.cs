using Orders.Application.Contracts;
using Orders.Application.UseCases.CreateOrder;
using Orders.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Mongo;
using Serilog;
using Prometheus;

// Logger bootstrap
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Logger host
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RabbitMQ
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<IOrderPublisher, RabbitMqOrderPublisher>();
builder.Services.AddScoped<CreateOrderHandler>();

// SQL
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));

// Mongo
builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection(MongoOptions.SectionName));
builder.Services.AddSingleton<MongoDb>();
builder.Services.AddScoped<OrderReadModelWriter>();
builder.Services.AddScoped<OrderReadModelReader>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Swagger primeiro (opcional)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Logging cedo
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (ctx, http) =>
    {
        ctx.Set("TraceId", http.TraceIdentifier);
        ctx.Set("RequestPath", http.Request.Path);
    };
});

// Pipeline “clássico”
app.UseRouting();
app.UseCors("frontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Metrics como middleware (mede requests)
app.UseHttpMetrics();

// Endpoints
app.MapControllers();
app.MapMetrics();

app.Run();

public partial class Program { }