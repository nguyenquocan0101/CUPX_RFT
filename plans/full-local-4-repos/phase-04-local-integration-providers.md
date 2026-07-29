# Phase 04: Local integration providers

**Milestone:** M2 - Auth/storage/email/payment/webhook local
**P1 stories:** Local integrations; backend local endpoints
**Dependencies:** Phases 02-03
**Outcome:** Core local integrations use SQL/JWT, MinIO, Mailpit and durable localhost webhooks while payment remains a sandbox UI success action until real payment integration is added.

## Existing Files And Symbols

Main backend:

- `APIs/Program.cs`
- `APIs/Extensions/ServicesDependency.cs`
- `Services/Implements/AuthService.cs`
- `Services/Firebase/Auth/IFirebaseAuthService.cs`
- `Services/Supabase/ISupabaseStorageService.cs`
- `Services/Supabase/SupabaseStorageService.cs`
- `Services/Implements/ProductService.cs`
- `Services/Implements/ProductCategoryService.cs`
- `Services/Implements/OrganizationService.cs`
- `Services/Email/Base/SmtpSettings.cs`
- `Services/Email/EmailSender.cs`
- `Services/Implements/OrderService.cs`
- `Services/VNPay/VNPayClient.cs`
- `Services/MPOS/MPOSClient.cs`
- `Services/CapRabbitMQ/Subscribers/PaymentCapSubscriber.cs`
- `Services/Implements/WebhookService.cs`

Kiosk:

- `Kiosk.ApiService/Controllers/OrderController.cs`
- `Services/ExternalClients/MainBackendClient.cs`
- `Services/Implements/OrderCacheService.cs`

## Planned Interfaces And Implementations

Create in Main backend:

- `Services/Storage/IObjectStorageService.cs`
- `Services/Storage/MinioObjectStorageService.cs`
- `Services/Storage/MinioOptions.cs`
- `Services/Payments/IPaymentProvider.cs`
- `Services/Payments/MockPaymentProvider.cs`
- `Services/Payments/MockPaymentOptions.cs`
- `APIs/Controllers/LocalIntegrationController.cs`
- provider-specific tests under `Services.Tests/Local/`

Retire `ISupabaseStorageService` only after all four consumers use `IObjectStorageService`. Keep `SupabaseStorageService` registered only for non-local provider selection.

## Implementation Steps

### Auth

1. Use the seeded SQL account and existing `AuthService.Login`/refresh path.
2. Local DI must not instantiate Firebase Admin or call Google endpoints.
3. Verify inactive call sites resolve the `DisabledFirebaseAuthService` introduced in Phase 02, which returns a clear local-mode error; do not register another implementation or mint a second JWT format.
4. Test issuer, audience, role/name identifier claims, expiry and refresh-token response envelope.

The disabled Firebase implementation is created and registered in Phase 02. This phase only verifies that provider selection remains compatible while other local adapters are added.

### Storage

1. Move consumer dependency to `IObjectStorageService`.
2. MinIO adapter uses S3-compatible path-style addressing.
3. Ensure bucket creation is owned by Compose initialization, not every upload request.
4. Preserve object naming and returned public URL shape expected by product/category/organization code.
5. Test product image upload, public URL read and byte-for-byte round-trip against MinIO with `scripts/local/Test-MinioObjectStorage.ps1`.

### Email

1. Extend `SmtpSettings` with `Host`, `Port`, `UseSsl`, `RequiresAuthentication`.
2. Mailpit uses `localhost:1025`, no TLS and no authentication.
3. Preserve recipient, subject, HTML body and attachment behavior.
4. Verify captured mail through Mailpit HTTP API rather than adding a second email mock.

### Payment

Payment is intentionally deferred from the local backend path. The Flutter sandbox provides a deterministic success button; VNPay/MPOS adapters remain compiled for a later integration and are not registered or called in `Local` mode.

1. `OrderService` depends on `IPaymentProvider`, not directly on VNPay/MPOS for local mode.
2. Mock states are deterministic: `Pending`, `Success`, `Failed`, `Cancelled`, `Refunded`.
3. Local-only controller creates/transitions a mock payment by provider reference ID.
4. Success/failure enters the same CAP topic/subscriber and SignalR path as existing callbacks.
5. Enforce one payment row with a database unique key on `(Provider, ProviderReferenceId)`.
6. Store external callbacks in a separate payment-event inbox with a unique key on `(Provider, ProviderReferenceId, EventType, ExternalEventId)`. A callback transaction claims the inbox event, then compare-and-sets the payment/order row using a row version or equivalent concurrency token.
7. Treat refund as a separate event type that is valid after `Success` while retaining the same payment provider reference. Replaying the same refund event is idempotent.
8. Commit state and an outbox event atomically. CAP/SignalR publishing occurs from the committed outbox; only the winning state transition emits side effects. A unique-key or row-version race is handled as an idempotent replay/conflict, not a 500.

### Webhook

1. Do not add a webhook mock service.
2. Seed `HealthCheck` and `ExecuteProduct` webhook records to `http://localhost:5160`.
3. Main API calls Kiosk API with the local API key.
4. Add local-only replay/trigger endpoint protected by local mode and API key.
5. Persist webhook inbox and outbox entries with a unique `(Source, EventType, EventId)` key. The trigger claims one outbox delivery with a lease, records success/failure, and returns the stored result on replay after restart. Run `scripts/local/Test-LocalWebhookPersistence.ps1`, then restart Main API and rerun it with `-ReplayOnly -EventId <same-id>`.

## Verification

```powershell
dotnet test .\AutomaticBrewingCoffeeBE\AutomaticBrewingCoffee.Main\Services.Tests\Services.Tests.csproj --filter "FullyQualifiedName~Local"
Invoke-WebRequest http://localhost:9000/minio/health/live -UseBasicParsing
Invoke-WebRequest http://localhost:8025/api/v1/messages -UseBasicParsing
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-MinioObjectStorage.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\local\Test-LocalWebhookPersistence.ps1
```

Integration sequence:

1. Login seeded account.
2. Upload/read a product image from MinIO and compare the round-trip bytes.
3. Trigger an email and verify one Mailpit message.
4. Create an order and mock payment.
5. Complete payment using one reference ID.
6. Replay the same callback twice, race two callbacks concurrently and replay after an API restart.
7. Verify one payment state transition, one order transition and compatible SignalR event.
8. Transition a second mock payment through `Refunded` and verify the existing refund contract.
9. Trigger Main→Kiosk health webhook, verify API-key authentication, durable inbox/outbox state and replay after a Main API restart.

## Gate

- No outbound call to Firebase, Supabase, SMTP provider, VNPay, MPOS, Azure or public webhook host.
- Existing JWT/API/SignalR/status contracts remain compatible.
- MinIO object and Mailpit message are inspectable.
- Duplicate payment/webhook callbacks are side-effect idempotent.

## Rollback

- Set provider selectors back to legacy providers outside `Local`.
- Revert local controller and adapters.
- Preserve MinIO/Mailpit/RabbitMQ data for diagnosis.
- If a unique-index migration was applied, use its reviewed down migration only after checking for data created after the phase.

## Risks

- Payment replay currently may insert duplicate `Payment` rows.
- Public object URL construction differs between Supabase and path-style MinIO.
- Webhook records must match exact business event names expected by `OrderService`.
