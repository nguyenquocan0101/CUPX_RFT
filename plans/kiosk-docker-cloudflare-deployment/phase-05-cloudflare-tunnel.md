# Phase 05: Cloudflare Tunnel

**Stories:** P1 public authenticated API  
**Depends on:** Phase 04

## Objective

Publish only the Kiosk API at `https://kiosk-api.alpa.vn` through a healthy, remotely managed Cloudflare Tunnel.

## Affected Files and External State

- `AutomaticBrewingCoffeeKioskBE\docker-compose.yml` cloudflared service
- `AutomaticBrewingCoffeeKioskBE\.env.example`
- Protected local Cloudflare token file or `.env`
- Cloudflare dashboard: tunnel, connector, published application route, DNS, optional WAF/rate limiting
- `AutomaticBrewingCoffeeKioskBE\scripts\verify-tunnel.ps1` (new)

## Tasks

1. Create a dedicated remotely managed tunnel for the kiosk and issue a new token; do not reuse the currently invalid connector secret.
2. Pin a reviewed cloudflared version/digest and run it on the Compose network without `network_mode: host`.
3. Prefer `TUNNEL_TOKEN_FILE` mounted as a protected secret; if environment token is used, document Docker-inspect exposure.
4. Configure `kiosk-api.alpa.vn` to route to `http://api:8080`. Add no route for Adminer, Swagger, PostgreSQL, CouchDB, Redis, RabbitMQ, or Windows Arm.
5. Ensure outbound connectivity to Cloudflare, including port 7844, and add connector restart policy.
6. Apply rate limiting/WAF appropriate for the API. Add Cloudflare Access only if every API client can supply a service token.
7. Verify DNS and connector health before enabling production callers.

## Verification

```powershell
docker compose up -d cloudflared
docker compose logs --since 10m cloudflared
Resolve-DnsName kiosk-api.alpa.vn
curl.exe -i -H "X-API-Key: <test-key>" https://kiosk-api.alpa.vn/api/v1/ping
curl.exe -i https://kiosk-api.alpa.vn/api/v1/ping
```

Expected:

- Cloudflare reports at least one healthy connector for 10 continuous minutes.
- Logs contain no `Invalid tunnel secret`.
- Authenticated public ping returns 200; missing/wrong key returns 401.
- Public p95 is below 1,500 ms for 100 requests from a normal Vietnam Internet connection.
- Attempts to reach Adminer, Swagger, or management/database ports through the hostname fail.

## Rollback and Safety

1. Disable/remove the published route before stopping the connector.
2. Revoke the tunnel token if it was exposed.
3. Keep local API available on loopback for diagnosis.

## Non-Goals

- Public database or management access.
- Interactive Access login unless API clients explicitly support it.
- Quick Tunnel for production.
