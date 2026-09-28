# Deployment — ERP SaaS

## Prerequisites

- .NET SDK 9+
- Node.js 18+ / npm
- SQL Server (or use `docker-compose`)
- Redis optional (`Redis:Enabled=false` by default in Development)

## Configuration (environment / appsettings)

Secrets and connections are **no longer** compile-time constants. Bind via `appsettings*.json`, User Secrets, or environment variables:

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:DefaultConnection` | EF + Dapper + SQL deploy |
| `ConnectionStrings:Redis` | Redis multiplexer (when enabled) |
| `Redis:Enabled` | `true` to connect Redis at startup |
| `Jwt:SecretKey` | Signing key (min 32 characters) |
| `Jwt:Issuer` / `Jwt:Audience` / `Jwt:ExpiryMinutes` / `Jwt:RefreshTokenExpiryDays` | Auth |
| `Cors:AllowedOrigins` | Array of allowed SPA origins |
| `Database:FailOnMigrationError` | Fail startup if migrate fails (default true outside Development) |
| `Database:DeploySqlScripts` | Run schema/SP SQL files on startup |
| `Database:FailOnSqlDeployError` | Fail startup if SQL deploy fails |

Development defaults live in `ERP.Api/appsettings.Development.json`. Production `appsettings.json` leaves secrets empty — set via env:

```bash
ConnectionStrings__DefaultConnection=...
Jwt__SecretKey=...
Cors__AllowedOrigins__0=https://app.example.com
```

Frontend:

| Variable | Purpose |
|----------|---------|
| `VITE_API_URL` or `VITE_API_BASE_URL` | API base (default `https://localhost:7062/api`) |
| `VITE_GOOGLE_MAPS_API_KEY` | Optional Google Maps demo page |

## Backend

```bash
dotnet restore ERP.SaaS.sln
dotnet build ERP.SaaS.sln
dotnet run --project ERP.Api
```

Typical launch URLs: `https://localhost:7062`, `http://localhost:5252`.

On startup the API:

1. Applies `Database.Migrate()` (no `EnsureCreated` fallback — avoids schema drift)
2. Optionally runs `DatabaseDeploymentService` for schema/SP SQL files
3. Uses JWT fallback authorization (authenticated by default); anonymous only on auth/setup/login paths

## Docker Compose

```bash
docker compose up --build
```

Starts SQL Server, Redis, and the API (`ERP.Api/Dockerfile` targets **.NET 9**).

## Frontend

```bash
cd frontend
npm install
npm run dev
# optional: npm test (Vitest)
```

Default Vite URL: `http://localhost:5173`.

## First-run bootstrap

1. Start API (migrations + optional SQL deploy).
2. Start frontend.
3. Complete `/setup` to create first tenant + SuperAdmin.
4. Log in; seed permissions as needed (`POST /api/admin/permissions/seed`).

## CI

GitHub Actions workflow: `.github/workflows/ci.yml` — `dotnet build/test` + `npm ci && npm run build`.

## Security notes

- Rotate `Jwt:SecretKey` for any shared environment.
- Passwords use PBKDF2 with per-user salt (legacy unsalted SHA-256 verified and upgraded on login).
- GraphQL endpoint requires authentication; Admin mutations are `[Authorize]`.
- Tenant middleware runs **after** authentication; JWT `TenantId` wins over `X-Tenant-Id` except for SuperAdmin.
