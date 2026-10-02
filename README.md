# BillingService

A learning project that shows how to deliver events between microservices **reliably**, using two patterns that work together:

- **Transactional outbox** (producer side, `OrderService`): an event is never lost, even if the broker is down or the process crashes.
- **Idempotent consumer** (consumer side, `PaymentService`): a duplicate event never causes a duplicate side effect.

Together they give you **effectively-once processing** on top of a broker that only guarantees at-least-once delivery.

Both patterns are hand-written, without MassTransit's built-in outbox or inbox, so you can see every moving part.

## The problem

When an order is created, two things must happen: save the order to PostgreSQL and publish `OrderCreated` to RabbitMQ. No transaction spans both systems, so either order of operations can fail halfway:

| Steps | Failure between them | Result |
|---|---|---|
| Save, then publish | Crash, or the broker is down | Order exists, **event lost** |
| Publish, then save | Database write fails | **Event for an order that doesn't exist** |

On the other side, RabbitMQ (like any broker) can deliver the same message **more than once**: after a consumer crash, a lost ack, a redelivery, or a republish by the outbox itself. A consumer that isn't idempotent would create two payments for one order.

## How it works

```
 OrderService                                                     PaymentService
┌──────────────────────────────────────┐                  ┌──────────────────────────────────────────┐
│ POST /api/orders                     │                  │ InboxFilter<T>                           │
│   └─ OrderManager                    │                  │   ├─ MessageId seen? ──yes──▶ skip (ack) │
│        one transaction:              │                  │   └─ no: add InboxMessages row           │
│        ├─ INSERT Orders              │                  │        └─ OrderCreatedConsumer           │
│        └─ INSERT OutboxMessages      │                  │             └─ PaymentManager            │
│                                      │    RabbitMQ      │                  ├─ payment for order?   │
│ OutboxMessageProcessor (every 200ms) │   OrderCreated   │                  │    yes ─▶ return it   │
│   ├─ read Pending rows               │ ───────────────▶ │                  └─ INSERT Payments      │
│   ├─ publish (MessageId = row id)    │                  │        one transaction:                  │
│   └─ mark Processed / retry / park   │                  │        InboxMessages + Payments          │
└──────────────────────────────────────┘                  └──────────────────────────────────────────┘
```

### 1. Transactional outbox (`OrderService`)

**Write.** `OrderManager.CreateOrderAsync` saves the `Order` **and** an `OutboxMessages` row (type + JSON payload) in **one** `SaveChanges`. Either both exist or neither does, so an order without its event is impossible.

**Relay.** `OutboxMessageProcessor` is a `BackgroundService` that runs every 200 ms:

1. Loads up to 10 rows with `Status = Pending` and `NextAttemptAt` either empty or in the past, oldest first.
2. Resolves the CLR type from the stored type name via `MessageTypeRegistry`, which is built at startup from `[MessageType]` attributes in `Contracts`.
3. Publishes with **`MessageId` = the outbox row's id**, so every retry of the same row carries the same id.
4. On success: `Status = Processed`.
5. On failure: increments `AttemptCount` and schedules the next try with exponential backoff (`NextAttemptAt` = now + 2^attempt seconds). After 5 attempts: `Status = Parked` and the error is logged. A parked message needs a human to fix the cause and requeue it.

```
            publish ok
 Pending ─────────────────────▶ Processed
   │  ▲
   │  │ NextAttemptAt reached
   ▼  │
 failed, attempt < 5 ── backoff 2s, 4s, 8s, 16s
   │
   │ attempt == 5
   ▼
 Parked
```

**Guarantee: at least once.** If the process dies after publishing but before marking the row `Processed`, the message is published again. That's why the consumer must be idempotent.

### 2. Idempotent consumer (`PaymentService`)

Duplicates are stopped at two independent levels.

**Message level: inbox (infrastructure).** `InboxFilter<T>` is a MassTransit consume filter that wraps every consumer:

1. If `InboxMessages` already has `(MessageId, Consumer)`, the message is acked and skipped. The consumer never runs.
2. Otherwise it adds an inbox row to the **same scoped `AppDbContext`** the consumer uses, and calls the consumer.
3. The consumer's `SaveChanges` commits the **inbox row and the payment in one transaction**. A crash before the commit leaves neither, so the redelivery is processed normally.
4. If two copies race, the second one hits the inbox primary key. The filter confirms the row exists and drops the duplicate.

**Business level: one payment per order (domain).** `PaymentManager.CreatePayment` returns the existing payment if the order already has one, and a **unique index on `Payments.OrderId`** enforces this in the database. This catches duplicates the inbox can't recognize, such as the same order arriving as a *different* message.

`PaymentManager` knows nothing about messages: deduplicating by message is plumbing, and "an order is paid once" is a business rule.

| Duplicate | Stopped by |
|---|---|
| Same message redelivered | Inbox lookup |
| Same message, two copies at once | Inbox primary key |
| Different message for the same order | `PaymentManager` lookup by `OrderId` |
| …at the same moment | Unique index on `Payments.OrderId` |

## Tech stack

- .NET 10, ASP.NET Core Minimal API
- MassTransit + RabbitMQ
- EF Core + PostgreSQL (Npgsql)
- Docker Compose

## Project structure

| Path | Description |
|------|-------------|
| `Contracts/` | Shared message contracts (`OrderCreated`) and the `[MessageType]` attribute |
| `OrderService/OrderService.API` | HTTP endpoints, DI, MassTransit, `OutboxMessageProcessor`, `MessageTypeRegistry` |
| `OrderService/OrderService.BLL` | `OrderManager`: writes the order and its outbox message in one transaction |
| `OrderService/OrderService.DAL` | `AppDbContext`, `Order`, `CustomOutboxMessage`, repositories, unit of work, migrations |
| `PaymentService/PaymentService.API` | MassTransit setup, `OrderCreatedConsumer` |
| `PaymentService/PaymentService.Application` | `PaymentManager`, repository / unit-of-work interfaces, `DuplicateException` |
| `PaymentService/PaymentService.Domain` | `Payment` entity and its status transitions |
| `PaymentService/PaymentService.Infrastructure` | `AppDbContext`, `InboxFilter`, `InboxMessage`, repositories, unit of work, migrations |
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

Both services apply their EF Core migrations on startup.

Create an order:

```bash
curl -X POST http://localhost:8081/api/orders \
  -H "Content-Type: application/json" \
  -d '{"buyerId":"3fa85f64-5717-4562-b3fc-2c963f66afa6","totalPrice":100.50}'
```

## Try it

Open a SQL shell to each database:

```bash
docker compose exec orderDb   psql -U postgres -d database   # outbox
docker compose exec paymentDb psql -U postgres -d database   # payments + inbox
```

### The outbox survives a broker outage

```bash
docker compose stop broker
# create an order with the curl above
```

```sql
-- orderDb: the event is waiting, with AttemptCount and NextAttemptAt growing
SELECT "MessageId", "Status", "AttemptCount", "NextAttemptAt", "ErrorMessage" FROM "OutboxMessages";
```

```bash
docker compose start broker
```

Once the broker is back (within the retry window), the row becomes `Processed` and the payment appears in `paymentDb`. If the broker stays down for more than 5 attempts, the row is `Parked`.

### The consumer ignores a duplicate

Simulate the outbox publishing the same event twice by putting a processed row back to `Pending`:

```sql
-- orderDb
UPDATE "OutboxMessages"
SET "Status" = 'Pending', "NextAttemptAt" = NULL
WHERE "MessageId" = '<message id>';
```

The processor republishes it with the **same** `MessageId`. In `payment-service` logs you'll see `Duplicate <id> for OrderCreated, skipping`, and the database still has exactly one row each:

```sql
-- paymentDb
SELECT * FROM "InboxMessages";
SELECT "Id", "OrderId", "Amount", "Status" FROM "Payments";
```

## API

### OrderService

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/health` | Health check |
| `POST` | `/api/orders` | Create an order and enqueue `OrderCreated` in the outbox |

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

## State machines

**Order:** `Pending` → `Paid` or `Pending` → `Canceled`.

**Payment:** `Pending` → `Processing` → `Succeeded` / `Failed` / `Canceled`.

Any other transition throws `InvalidOperationException`.

## Known limitations

This is a teaching project. A production setup would also need:

- **Several outbox processors:** with more than one `OrderService` instance, two processors can pick up the same rows. Claiming rows with `FOR UPDATE SKIP LOCKED` or a lease would fix this. Duplicates are still harmless because the consumer is idempotent.
- **Cleanup:** `Processed` outbox rows and old inbox rows are never deleted.
- **Parked messages:** there's no endpoint or alert for parked messages. Requeue them manually with SQL.
- **Ordering:** best effort only. A message that is backing off is overtaken by newer ones.
- **External side effects:** the inbox only protects database writes. A call to a real payment gateway would also need an idempotency key, for example the order id.

In a real project you'd likely use MassTransit's built-in EF Core outbox and inbox (`AddEntityFrameworkOutbox`), which implement the same ideas.
