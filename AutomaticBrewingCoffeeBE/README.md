## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- Database Server (SQLServer)
- Redis
- CapRabbitMQ

### Setup Instructions
1. Clone the repository:
   ```bash
   git clone https://github.com/FPT-Team-Execution/AutomaticBrewingCoffee_BE.git
   ```
2. Navigate to the project directory:
   ```bash
   cd AutomaticBrewingCoffee_BE
   ```
3. Install dependencies:
   ```bash
   dotnet restore
   ```
4. Configure database and environment variables in `appsettings.json` or `.env`.
5. Apply database migrations:
   ```bash
   dotnet ef database update
   ```
6. Run the backend.

## Publish the API through Cloudflare Tunnel

The Docker Compose stack includes `cloudflared`. The API origin is bound to
`127.0.0.1:30475`, so Internet traffic should enter through Cloudflare instead
of reaching the origin port directly.

1. In Cloudflare Zero Trust, open **Networking > Tunnels** and create a remotely
   managed tunnel.
2. Add a public hostname such as `api.example.com` with service
   `http://api:8080`. `api` is the Docker Compose service name, not `localhost`.
3. Copy `AutomaticBrewingCoffee.Main/.env.example` to
   `AutomaticBrewingCoffee.Main/.env` and set:
   - `TUNNEL_TOKEN` to the tunnel connector token.
   - `PUBLIC_API_URL` to the public API URL, for example
     `https://api.example.com`.
   - `WEBAPP_DOMAIN` to the exact frontend origin allowed by CORS.
   - `MSSQL_SA_PASSWORD` to the SQL Server password.
   - `SQLSERVER_HOST_PORT` can stay at `11433` when Windows already uses port `1433`.
   - `JWT_KEY`, `SUPABASE_URL`, `SUPABASE_KEY`, and RabbitMQ credentials.
   - `VNPAY_TMN_CODE` and `VNPAY_HASH_SECRET` from your VNPay sandbox merchant;
     `VNPAY_BASE_URL` defaults to
     `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`.
   Keep other service credentials in `APIs/appsettings.json` or inject them as
   .NET environment variables using the `Section__Key` convention.
4. Start the stack from `AutomaticBrewingCoffee.Main`:

   ```powershell
   docker compose up -d --build
   ```

5. Verify the tunnel and API:

   ```powershell
   docker compose ps
   docker compose logs cloudflared
   curl.exe --fail --silent --show-error https://api.example.com/health
   ```

Swagger is intentionally enabled only in the Development environment.

`TUNNEL_TOKEN` is a connector credential. It is separate from
`CLOUDFLARE_API_TOKEN`, which this application only needs when it manages other
Cloudflare tunnels through the Cloudflare API.
