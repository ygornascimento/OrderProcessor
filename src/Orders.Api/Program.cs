using Orders.Application.Contracts;
using Orders.Application.UseCases.CreateOrder;
using Orders.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Mongo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//RabbitMQ
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<IOrderPublisher, RabbitMqOrderPublisher>();
builder.Services.AddScoped<CreateOrderHandler>();

//SQL
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));

//Mongo
builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection(MongoOptions.SectionName));
builder.Services.AddSingleton<MongoDb>();
builder.Services.AddScoped<OrderReadModelWriter>();
builder.Services.AddScoped<OrderReadModelReader>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();

public partial class Program { }