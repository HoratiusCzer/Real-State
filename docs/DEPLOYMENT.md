# Deployment

This build has been developed and verified against local dev infrastructure only (LocalDB,
`dotnet run`, `npm run dev`) — it has not been deployed to a production host. This document
describes the steps and configuration REAK's own infrastructure team must supply; see
`docs/PRODUCTION-BUILD-REPORT.md` for the overall readiness verdict.

## Required environment variables (names only — never commit values, spec §29)

**REAK.Api** (`REAK.Api/.env.example`):
- `REAK_JWT_KEY` — required, app fails fast at startup if unset. Long, random
  (`openssl rand -base64 48`), never a guessable default.
- `REAK_BOOTSTRAP_ADMIN_EMAIL` / `REAK_BOOTSTRAP_ADMIN_PASSWORD` — optional, bootstraps one
  SuperAdmin on first run if both are set and no SuperAdmin exists yet. Unset after the first real
  admin is created.
- `Jwt__Issuer`, `Jwt__Audience`, `Jwt__AccessTokenMinutes`, `Jwt__RefreshTokenDays` — optional,
  safe defaults in `appsettings.json`.
- `Storage__LocalRoot`, `Storage__PublicUrlBase` — optional, local-disk storage path/URL prefix.
  Production should evaluate whether local disk storage is acceptable for the target host
  (persistent disk / shared volume required if running more than one API instance) or whether to
  swap `IFileStorage` for a cloud-storage implementation — no such implementation exists in this
  build.
- `ConnectionStrings__DefaultConnection` — the SQL Server connection string.

**web** (`web/.env.example`):
- `REAK_API_URL` — server-only, REAK.Api's base URL. Never exposed to the browser.
- `NEXT_PUBLIC_SITE_URL` — public, the canonical site URL for sitemap/robots/OG tags.

## Database

1. Provision a SQL Server instance reachable from the API host.
2. `dotnet ef database update` — applies all 6 migrations (`docs/DATABASE.md`).
3. Apply RLS: run `REAK.Api/Data/Security/RowLevelSecurity.sql` against the target database.
4. Seed initial data via `REAK.Api/Data/DatabaseSeeder.cs`'s seeding path (reference data,
   roles/permissions, feature flags — all seeded off except the ones Stage 12 documented as
   deliberately on for demo purposes; production should review every flag's seeded state before
   go-live).
5. Verify with `REAK.Api/Data/Security/rls_test.sql` (self-contained, self-cleaning — safe to run
   against a real database, see `docs/TESTING.md`).

## Backend (REAK.Api)

- `dotnet publish -c Release`.
- Set every required environment variable above via the host's real secret-management mechanism
  (never a committed `.env`).
- `UseHsts()` is already conditional on non-Development environments — confirm `ASPNETCORE_ENVIRONMENT=Production`
  is actually set.
- No CORS policy exists (deliberate, see `docs/SECURITY.md`) — this assumes the frontend is
  always same-origin to itself and never calls REAK.Api directly from the browser. If that
  assumption ever changes, CORS needs to be revisited, not silently left off.

## Frontend (web)

- `npm run build` then run with a Node server (`npm start`) or the hosting platform's Next.js
  adapter of choice — not evaluated against a specific platform in this build.
- Set `REAK_API_URL` to the deployed API's internal/private address (server-to-server, never a
  public browser-facing one is required) and `NEXT_PUBLIC_SITE_URL` to the real public domain.

## Not yet configured — REAK must supply before go-live

Per spec §36, these are business/content decisions, not code:
- Real logo, legal association name, brand colors (design system currently uses placeholder
  REAK branding).
- Committee member info, contact info, membership policy text (CMS content is currently
  QA/placeholder text created during testing).
- Approved matching rule set(s) — a `QA Standard Rules` set exists as a working example; REAK's
  actual weighting/tolerance decisions need review before `matching_enabled` goes live for real
  members.
- Approved area-unit conversion rules (`auto_unit_conversion` is currently seeded off).
- A real decision on which feature flags should be on at launch — every flag's *seeded* default is
  off except where Stage 12 documented an explicit, reviewable exception; see
  `REAK.Api/Data/DatabaseSeeder.cs` and `docs/PRODUCTION-BUILD-REPORT.md`.
- An actual email-sending integration — invitations and password resets currently require
  out-of-band token delivery (see `docs/SECURITY.md`'s known limitations).
