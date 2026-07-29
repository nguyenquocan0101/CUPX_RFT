# Phase 03: Infrastructure and Persistence

**Stories:** P1 persistent state, P2 local Adminer  
**Depends on:** Phase 02

## Objective

Replace the stale root Compose definition with collision-free, persistent infrastructure and prove data survives normal recreation.

## Affected Files and Symbols

- `AutomaticBrewingCoffeeKioskBE\docker-compose.yml`
- `AutomaticBrewingCoffeeKioskBE\ABC_Kiosk_BE\docker-compose.yml` (mark deprecated or remove in implementation)
- `AutomaticBrewingCoffeeKioskBE\.env.example`
- `AutomaticBrewingCoffeeKioskBE\ABC_Kiosk_BE\Kiosk.ApiService\Extensions\StartupInitializer.cs`
- `StartupInitializer.InitializeAsync`, `DeleteCouchDbDatabase`, and `DeleteRabbitMqQueue`

## Tasks

1. Define a unique Compose project and these 10 roles: API, coffee, cup, ice, PostgreSQL, Adminer, CouchDB, Redis, RabbitMQ, cloudflared.
2. Remove `container_name` declarations. Use Compose service DNS and avoid collisions with existing `redis`, `rabbitmq`, and `abc-system`.
3. Add named volumes for PostgreSQL, CouchDB, Redis, and RabbitMQ; enable Redis AOF and durable RabbitMQ queues/messages.
4. Add native health checks: `pg_isready`, CouchDB `/_up`, `redis-cli ping`, and `rabbitmq-diagnostics -q ping`.
5. Keep PostgreSQL internal; expose Adminer only at `127.0.0.1:18080`. Expose RabbitMQ AMQP for Windows Arm only at `127.0.0.1:25672`; optional UI uses `127.0.0.1:25673`.
6. Use a non-guest RabbitMQ user/password and make controllers depend on broker health.
7. Gate all app-level destructive cleanup behind `StartupCleanup__Enabled=false` by default. Move connection/channel creation inside protected error handling.
8. Do not wire PostgreSQL into API EF services; it remains user-requested auxiliary infrastructure.
9. Make cloudflared and hardware services startable later without publishing infrastructure ports publicly.

## Verification

```powershell
docker compose config --quiet
docker compose up -d --wait --wait-timeout 120 postgres adminer couchdb redis rabbitmq
docker compose ps
```

Insert one unique test value/document/key and one persistent message in each relevant service, then:

```powershell
docker compose down
docker compose up -d --wait --wait-timeout 120 postgres adminer couchdb redis rabbitmq
```

Expected:

- Compose config has no unresolved variables or port collisions.
- All five infrastructure/UI containers reach running/healthy state.
- PostgreSQL, CouchDB, Redis, and RabbitMQ test state remains.
- Adminer is reachable at `127.0.0.1:18080` and not from another LAN host.

## Rollback and Safety

- Never use `docker compose down -v` in normal deployment or rollback.
- Back up named volumes before changing image major versions.
- If persistence fails, stop before starting API or cloudflared.

## Non-Goals

- Public routing.
- API-to-PostgreSQL integration.
- Hardware execution.
