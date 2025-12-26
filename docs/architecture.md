# Arquitetura — Orders (API + RabbitMQ + Worker)

## Visão (C4 — Containers)

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




