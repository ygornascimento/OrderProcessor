# Arquitetura — Orders (API + RabbitMQ + Worker)

## Visão (C4 — Containers)

```mermaid
flowchart LR
  U[Cliente HTTP] -->|POST /api/orders| API[Orders.Api (ASP.NET Core Web API)]
  API -->|Publish (AMQP)| MQ[(RabbitMQ)]
  W[Orders.Worker (BackgroundService)] -->|Consume (AMQP)| MQ
  W -->|Persist| DB[(SQL Server)]
  API -->|GET (Read)| DB

flowchart TB
  subgraph API[Orders.Api]
    C[OrdersController]
    UC[CreateOrderHandler (Application)]
    P[IOrderPublisher (Port)]
  end

  subgraph INFRA[Orders.Infrastructure]
    RAB[RabbitMqOrderPublisher (Adapter)]
  end

  C --> UC
  UC --> P
  P --> RAB

flowchart TB
  subgraph W[Orders.Worker]
    CON[RabbitMqOrderConsumer]
  end

  subgraph APP[Orders.Application]
    MSG[OrderCreatedMessage]
  end

  subgraph INFRA[Orders.Infrastructure]
    PORT[IOrderRepository (Port)]
    REPO[EfOrderRepository (Adapter)]
    DBCTX[OrdersDbContext (EF Core)]
  end

  CON -->|deserialize| MSG
  CON --> PORT
  PORT --> REPO
  REPO --> DBCTX


