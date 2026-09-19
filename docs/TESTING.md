# Testing

There is no automated test project in this repository (`*.Tests.csproj`, Jest/Vitest config) —
every stage's verification in this build was done by exercising the real running app (API via
curl/direct HTTP calls, frontend via a real browser once one became available in Stage 14) against
the real dev database, documented in `DEVELOPMENT_PLAN.md` and `Progress-Tracking/daily-logs/`.
This is a real gap relative to spec §27's "Unit... Integration..." requirement — see
`docs/PRODUCTION-BUILD-REPORT.md` for how this factors into the readiness verdict.

## How to reproduce the RLS test pass

`REAK.Api/Data/Security/rls_test.sql` is self-contained and self-cleaning: it creates two
throwaway member orgs and users with fresh random GUIDs, exercises 11 assertions (owner
visibility, outsider denial, network-share visibility, contact-privacy at every stage, hostile
delete/update attempts against a listing the caller doesn't own, anonymous visibility before/after
a listing goes public), then deletes everything it created. Safe to run against a real database at
any time:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d real-estate -i REAK.Api/Data/Security/rls_test.sql
# or, from PowerShell:
Invoke-Sqlcmd -ServerInstance "(localdb)\MSSQLLocalDB" -Database "real-estate" `
  -InputFile "REAK.Api/Data/Security/rls_test.sql"
```

All 11 rows should read `1` for the "should be visible" assertions and `0` for the "should be
hidden" ones (see the column names in the output — they're self-describing, e.g.
`ListingVisibleToOutsider` should be `0`). Rerun after any change touching `NetworkVisibility`,
contact privacy, or collaboration participation — it has been rerun after every stage that touched
that surface, with no regression, through Stage 15.

## How to reproduce the E2E + security pass

Stage 15's full write-up (`DEVELOPMENT_PLAN.md`, "Stage 15 — Full Regression Testing") is the
closest thing this project has to a test plan: the exact sequence of API calls for all 22 of spec
§27's numbered end-to-end scenarios and all 12 of its adversarial security tests, run live. To
reproduce any of it: log in via `POST /api/auth/login` for at least two different member-org
accounts plus one system-admin account (`docs/API.md` has the full route list), then follow the
scenario sequence documented there. Test accounts used (dev database only, never real data):
`assoc-admin@example.test` (SuperAdmin), `kathmandu-prime@example.test`,
`pokhara-lakeside@example.test` — all seeded/created with password `TestPass123!` in this dev
environment.

## Frontend checks

- `npm run lint` (ESLint) and `npm run build` (type-checks + production build) in `web/` — run
  clean as of every stage's completion through Stage 15.
- No automated component/E2E test suite (Playwright, Cypress, etc.) exists. Visual/interaction
  verification from Stage 14 onward was done with a live Chrome browser session, not scripted —
  see `DEVELOPMENT_PLAN.md`'s Stage 14 section for what was and wasn't verifiable (a real,
  disclosed gap: this environment's `resize_window` tool did not change the actual rendered
  viewport, so true mobile-breakpoint *pixel* verification was never done — only the underlying
  responsive CSS classes were confirmed structurally correct).

## Known testing gaps

- No unit tests for validation logic, matching-rule scoring, or permission logic in isolation
  (spec §27 explicitly asks for this).
- No integration test project — every "integration test" in this build's history was a live call
  against a real running API/DB, not an isolated, repeatable automated suite.
- No load/performance benchmarking beyond qualitative checks that key pages/queries respond
  promptly in dev.
