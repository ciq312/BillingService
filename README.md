# BillingService

A learning project that explores how events travel between microservices. `OrderService` creates orders and publishes an `OrderCreated` event over RabbitMQ (via MassTransit). `PaymentService` consumes that event.

## Tech stack

- .NET 10, ASP.NET Core Minimal API
- MassTransit + RabbitMQ
- EF Core + PostgreSQL (Npgsql)
- Docker Compose

## Project structure

| Path | Description |
|------|-------------|
| `Contracts/` | Shared message contracts (`OrderCreated`) and `BusOptions` (RabbitMQ settings) |
| `OrderService/OrderService.API` | HTTP endpoints, DI, MassTransit setup, auto-migrations on startup |
| `OrderService/OrderService.BLL` | `OrderManager`: creates orders and publishes events |
| `OrderService/OrderService.DAL` | `AppDbContext`, repository, Unit of Work, `Order` entity, migrations |
| `PaymentService/PaymentService.API` | MassTransit consumer `OrderCreatedConsumer` |
| `PaymentService/PaymentService.{Application,Domain,Infrastructure}` | Layers reserved for payment logic (not implemented yet) |
| `docker-compose.yaml` | RabbitMQ, two PostgreSQL instances and both services |
| `docker-compose.infra.yaml` | Infrastructure only, for running the services locally with `dotnet run` |

## Getting started

Requires Docker.

```bash
cp .env.example .env
docker compose up --build
```

| Service | URL |
|---------|-----|
| OrderService | http://localhost:8081 |
| PaymentService | http://localhost:8080 |
| RabbitMQ Management UI | http://localhost:15672 |

Send a test order:

```bash
curl -X POST http://localhost:8081/api/orders   -H "Content-Type: application/json"   -d '{"buyerId":"3fa85f64-5717-4562-b3fc-2c963f66afa6","totalPrice":100.50}'
```

The `payment-service` logs show:

```
Received message: <orderId>, <buyerId>, <createdAt>, 100.50
```

## API

### OrderService

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/health` | Health check |
| `POST` | `/api/orders` | Create an order and publish `OrderCreated` |

### PaymentService

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/health` | Health check |

## Events

### `OrderCreated`

| Field | Type |
|-------|------|
| `Id` | `Guid` |
| `BuyerId` | `Guid` |
| `TotalPrice` | `decimal` |
| `CreatedAt` | `DateTime` |

## Order states

`Pending` → `Paid` or `Pending` → `Canceled`. Any other transition throws `InvalidOperationException`.
