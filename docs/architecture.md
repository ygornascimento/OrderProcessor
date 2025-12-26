# Arquitetura — Orders (API + RabbitMQ + Worker)

## Visão (C4 — Containers)

```mermaid
flowchart LR
  Client["Browser / Postman"] -->|HTTP| Front["orders-front (React + Nginx)"]
  Front -->|HTTP| API["orders-api (ASP.NET Core Web API)"]

  API -->|Publish (AMQP)| MQ[(RabbitMQ)]
  Worker["orders-worker (BackgroundService)"] -->|Consume (AMQP)| MQ

  Worker -->|Persist| SQL[(SQL Server)]
  Worker -->|Upsert read-model| Mongo[(MongoDB)]

  API -->|GET (Read)| Mongo

  Prom["Prometheus"] -->|Scrape /metrics| API
  Graf["Grafana"] -->|Query| Prom


flowchart TB

  subgraph API["Orders.Api"]
    Controller["OrdersController"] --> Handler["CreateOrderHandler (Use Case)"]
    Handler --> Publisher["IOrderPublisher (Port)"]
    Reader["OrderReadModelReader"] --> MongoDb["MongoDb (Client)"]
    Controller --> Reader
  end

  subgraph Infra["Orders.Infrastructure"]
    RabbitPublisher["RabbitMqOrderPublisher"] --> RabbitClient["RabbitMQ.Client"]
    MongoWriter["OrderReadModelWriter"] --> MongoDb
    MongoDb --> Mongo[(MongoDB)]
    EfContext["OrdersDbContext"] --> SQL[(SQL Server)]
  end

  subgraph Worker["Orders.Worker"]
    Consumer["RabbitMqOrderConsumer"] --> EfContext
    Consumer --> MongoWriter
    Consumer --> RabbitClient
  end

  MQ[(RabbitMQ)] --> Consumer
  Publisher --> RabbitPublisher --> MQ




