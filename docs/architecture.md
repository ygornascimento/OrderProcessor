# Arquitetura — Orders (API + RabbitMQ + Worker)

## Visão (C4 — Containers)

```mermaid
flowchart LR
  Client["Browser / Postman"] -->|HTTP| Front["orders-front (React + Nginx)"]
  Front -->|HTTP| ApiSvc["orders-api (ASP.NET Core Web API)"]

  ApiSvc -->|Publish (AMQP)| MQ[(RabbitMQ)]
  WorkerSvc["orders-worker (BackgroundService)"] -->|Consume (AMQP)| MQ

  WorkerSvc -->|Persist| SQL[(SQL Server)]
  WorkerSvc -->|Upsert read-model| Mongo[(MongoDB)]

  ApiSvc -->|GET (Read)| Mongo

  Prom["Prometheus"] -->|Scrape /metrics| ApiSvc
  Graf["Grafana"] -->|Query| Prom
```

