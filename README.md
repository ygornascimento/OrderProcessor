# testeActenium
---

# Orders – Teste Técnico (API + Worker + RabbitMQ + SQL Server + Mongo + Observabilidade)

Este repositório implementa um fluxo assíncrono de criação de pedidos usando **RabbitMQ** (mensageria), **SQL Server** (persistência), **MongoDB** (read model/cache de leitura) e um **Worker** que consome mensagens e grava nos bancos. Inclui **Prometheus + Grafana** para métricas e **Serilog** para logs.

## Arquitetura (visão rápida)

**POST /api/orders** (API)
→ publica `OrderCreatedMessage` no RabbitMQ (fila `orders.created`)
→ **Worker** consome
→ grava **OrdersDb (SQL Server)**
→ atualiza **OrdersReadDb (Mongo)**
→ **GET /api/orders** lê do **Mongo** (read model)

Serviços:

* `orders-api` (ASP.NET Core Web API)
* `orders-worker` (BackgroundService consumer)
* `rabbitmq` (+ UI)
* `sqlserver`
* `mongodb`
* `prometheus`
* `grafana`
* `orders-front` (React/Vite build + Nginx)

## Docs

- Arquitetura (Mermaid): `docs/architecture.md`
[Diagrama — C4 Containers](docs/architecture.md#visão-c4--containers)


---

## Requisitos

* Docker Desktop (com Docker Compose)
* (Opcional) .NET 8 SDK (para rodar local sem Docker)
* (Opcional) Node.js (para rodar front local sem Docker)

---

## Como rodar tudo (modo recomendado – Docker Compose)

Na pasta `deploy/`:

```bash
docker compose up -d --build
```

### URLs

* **Front**: [http://localhost:3000](http://localhost:3000)
* **API**: [http://localhost:5000](http://localhost:5000)
* **Swagger**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
* **RabbitMQ UI**: [http://localhost:15672](http://localhost:15672)  (guest/guest)
* **Prometheus**: [http://localhost:9090](http://localhost:9090)
* **Grafana**: [http://localhost:3001](http://localhost:3000) (admin/admin)

## Teste rápido do fluxo (sem front)

### 1) Criar order (POST → RabbitMQ)

```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Ygor Teste",
    "amount": 123.45,
    "orderDate": "2025-12-26T10:30:00Z"
  }'
```

Resposta esperada: `202 Accepted` com `{ "id": "..." }`

### 2) Consultar orders (GET ← Mongo read model)

```bash
curl http://localhost:5000/api/orders
```

---

## Logs e troubleshooting

### Ver logs de um container

```bash
docker logs -f orders-api
docker logs -f orders-worker
docker logs -f orders-rabbitmq
docker logs -f orders-sqlserver
docker logs -f orders-mongodb
```

### Ver containers rodando

```bash
docker ps
```

### Parar tudo

```bash
docker compose down
```

### Reset total (apaga dados persistidos)

```bash
docker compose down -v
```

---

## Conferindo persistência (SQL Server e Mongo)

### SQL Server (tabela Orders)

Entrar no container:

```bash
docker exec -it orders-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Your_password123!" -C
```

Depois:

```sql
USE OrdersDb;
GO
SELECT TOP 20 * FROM dbo.Orders ORDER BY OrderDate DESC;
GO
```

### Mongo (read model)

```bash
docker exec -it orders-mongodb mongosh
```

```js
use OrdersReadDb
show collections
db.orders.find().sort({ orderDate: -1 }).limit(10).pretty()
```

---

## Observabilidade

### Métricas (Prometheus endpoint)

* `GET http://localhost:5000/metrics`

No Prometheus:

* Status → Targets (deve mostrar `orders-api` como **UP**)

No Grafana:

* Adicionar data source: **Prometheus**
* URL: `http://prometheus:9090`
* Query de teste: `up` ou `scrape_samples_scraped`

---

## Testes automatizados

### Integration tests (in-memory / WebApplicationFactory)

Rodar:

```bash
dotnet test tests/Orders.IntegrationTests/Orders.IntegrationTests.csproj
```

### E2E tests (precisa do ambiente Docker rodando)

Suba o compose e rode:

```bash
dotnet test tests/Orders.E2ETests/Orders.E2ETests.csproj
```

---

## Decisões de design (curtas e objetivas)

* **Mensageria:** RabbitMQ para desacoplar API e persistência
* **Write model:** SQL Server como fonte de verdade do pedido
* **Read model/cache:** MongoDB para consultas rápidas (GET)
* **Worker:** consumo com `BasicAck` apenas após persistir em SQL + Mongo
* **Idempotência mínima:** `AnyAsync` por `message.Id` antes de inserir

---

## Estrutura do projeto (resumo)

* `src/Orders.Api` – Web API
* `src/Orders.Worker` – consumidor RabbitMQ + persistência
* `src/Orders.Infrastructure` – SQL/Mongo/messaging
* `src/orders-front` – front React
* `deploy/docker-compose.yml` – stack local

---