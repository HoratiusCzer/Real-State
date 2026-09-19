# REAK — Real Estate Association of Kathmandu

A multi-tenant platform for a Nepal real estate association: a public marketing site, a
member-organization portal (property exchange, client requirements, matching, brokered
collaboration), and an admin portal (membership, CMS, feature flags, reports, audit).

Full functional/security specification: [`docs/REAK-requirements.md`](docs/REAK-requirements.md).
Build history and per-stage decisions: [`DEVELOPMENT_PLAN.md`](DEVELOPMENT_PLAN.md) and
[`Progress-Tracking/daily-logs/`](Progress-Tracking/daily-logs/). Production readiness verdict:
[`docs/PRODUCTION-BUILD-REPORT.md`](docs/PRODUCTION-BUILD-REPORT.md).

## Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core 10 (`REAK.Api/`), EF Core 10 |
| Database | SQL Server (LocalDB in dev), native row-level security |
| Frontend | Next.js 16 (App Router, Turbopack), React 19 (`web/`) |
| Auth | JWT access + refresh tokens, BCrypt password hashing |

Two independently-run processes: the API (`REAK.Api`, default `http://localhost:5080`) and the
Next.js app (`web`, default `http://localhost:3000`). The browser only ever talks to Next.js —
every API call happens server-side (Server Components / Route Handlers) or through a same-origin
proxy route Next.js exposes; `REAK_API_URL` is never sent to the browser. See
[`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

## Quick start

Prerequisites: .NET 10 SDK, Node.js 20+, SQL Server (LocalDB is enough for dev).

```bash
# 1. Database
cd REAK.Api
dotnet ef database update          # applies all migrations, see docs/DATABASE.md
sqlcmd -S "(localdb)\MSSQLLocalDB" -d real-estate -i Data/Security/RowLevelSecurity.sql

# 2. API — copy REAK.Api/.env.example, set REAK_JWT_KEY at minimum, then:
dotnet run --project REAK.Api      # http://localhost:5080

# 3. Frontend — copy web/.env.example to web/.env.local, then:
cd web
npm install
npm run dev                        # http://localhost:3000
```

Full environment variable reference (names only, per spec §29 — never commit real values):
[`REAK.Api/.env.example`](REAK.Api/.env.example), [`web/.env.example`](web/.env.example).

## Documentation

| Doc | Covers |
|---|---|
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | System shape, tenancy model, request flow, project structure |
| [`docs/DATABASE.md`](docs/DATABASE.md) | Schema, migrations, RLS policies/functions, indexes |
| [`docs/AUTHENTICATION.md`](docs/AUTHENTICATION.md) | Login, JWT/refresh, invitation/activation flow, RBAC |
| [`docs/SECURITY.md`](docs/SECURITY.md) | RLS, tenant isolation, PII handling, storage policy, audit logging, rate limiting |
| [`docs/API.md`](docs/API.md) | Full controller/route reference |
| [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md) | Production deployment steps and required config |
| [`docs/TESTING.md`](docs/TESTING.md) | How to run/reproduce this project's test passes |
| [`docs/PRODUCTION-BUILD-REPORT.md`](docs/PRODUCTION-BUILD-REPORT.md) | Spec §36 final report: features, testing results, readiness verdict |

## Development instructions

- Every database change goes through an EF Core migration (`dotnet ef migrations add <Name>`,
  then `dotnet ef database update`) — never a manual schema edit. See `docs/DATABASE.md`.
- Frontend: `npm run lint` and `npm run build` in `web/` before considering any change done.
- Backend: `dotnet build` from the repo root or `REAK.Api/`.
- This Next.js version has real breaking changes from its training-data era — read
  `web/AGENTS.md` before touching frontend code.
- RLS changes must be re-verified with `REAK.Api/Data/Security/rls_test.sql` (self-contained,
  self-cleaning; see `docs/TESTING.md`).
