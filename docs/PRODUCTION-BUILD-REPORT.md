# REAK Production Build Report

Prepared 2026-09-18, at the completion of Stage 16 (Documentation + Production Readiness), the
final stage of the 16-stage plan in `docs/REAK-requirements.md` §2.6. This report follows the
format that spec's §36 requires on genuine completion.

## Final status

## **NOT READY FOR PRODUCTION**

The core application — every feature the spec describes — is built, working, and verified live
end-to-end, including a dedicated adversarial security pass (Stage 15) that found zero real
defects. What blocks production is not broken functionality; it's a combination of missing
automated test coverage, missing production infrastructure (email delivery, a real deployment),
and business configuration only REAK itself can supply. Each is listed explicitly below.

### Blockers

1. **No automated test suite.** Spec §27 requires unit tests (validation, utilities, matching
   rules, permission logic) and integration tests (auth, database, RLS, API/business logic,
   storage) as first-class deliverables, separate from E2E/security testing. None exist in this
   repository — every verification pass in this build's history (Stages 1-15) was a live,
   manually-run call against the real API/database, not automated, repeatable test code. This is
   the single largest gap. See `docs/TESTING.md`.
2. **No email delivery integration.** Invitation tokens and password-reset tokens are generated
   and stored correctly, but nothing sends them — `email_notifications` has no provider wired up.
   In this build's dev/test environment, tokens were read directly from the database to exercise
   the flows. Real members cannot receive an invitation or reset a password until this exists.
3. **Never deployed.** This build has only run against local dev infrastructure (LocalDB,
   `dotnet run`, `npm run dev`). No deployment target, process manager, TLS termination, or
   production database has been provisioned or exercised. See `docs/DEPLOYMENT.md`.
4. **Business configuration REAK must supply is still placeholder.** Real logo/branding,
   committee/contact info, membership policy text, and an approved (not QA-default) matching
   rule set. See "Remaining configuration" below.
5. **No load/performance benchmarking.** Query and page-load behavior was checked qualitatively
   in dev (fast, no visible slowness) but never benchmarked under realistic data volume or
   concurrent load.

### What is solid and does not block production

- Every one of spec §27's 22 end-to-end scenarios works, verified live, fresh, in Stage 15.
- Every one of spec §27's 12 adversarial security tests correctly blocks the attempted violation,
  verified live in Stage 15 — cross-org access, private-data reads, unauthorized admin routes,
  disabled-feature bypass, role/permission tampering (which is structurally impossible over HTTP,
  not merely permission-gated).
- RLS (`rls_test.sql`, 11 assertions) passes clean and has been rerun after every stage that
  touched its surface, with zero regressions across the whole build.
- Rate limiting, security headers, audit logging of security-relevant events, and BCrypt password
  hashing are all in place and verified (Stage 13).
- `npm run lint` / `npm run build` and `dotnet build` are clean as of Stage 15.

## Project structure

See `docs/ARCHITECTURE.md` for the full breakdown. Two applications — `REAK.Api` (ASP.NET Core 10)
and `web` (Next.js 16) — sharing nothing at runtime except HTTP calls the browser never makes
directly.

## Completed features

Public website (marketing pages, published CMS content, public property/member directories behind
feature flags, membership application); Member Portal (property listings, client requirements/
demands, matching, collaboration workspaces with contact disclosure, saved items, notifications,
org/profile management); Admin Portal (membership review, member orgs, invitations, roles
read-only view, full CMS, feature flags, match rule authoring, reports, audit log). All 16 stages
of `docs/REAK-requirements.md` §2.6 complete — full history in `DEVELOPMENT_PLAN.md`.

## Full route list

`docs/API.md` (backend, 21 controllers) and, for the frontend, the route tree under
`web/src/app/{(public),portal,admin}/` — 77 page routes total as of Stage 16, spanning public
marketing/content pages, the member portal, and the admin portal.

## Database

61 application tables, 6 migrations, 186 indexes, 9 RLS predicate functions across 13 RLS-enabled
tables. Full detail in `docs/DATABASE.md`.

## Authentication flow

Membership application → admin approval → invitation → activation → login (Flow A). JWT access +
refresh tokens, BCrypt hashing, per-request suspension re-check via `ActiveProfileMiddleware`. Full
detail in `docs/AUTHENTICATION.md`.

## RBAC

4 roles (`SuperAdmin`, `AssociationAdmin` — System-scoped; `MemberAdmin`, `MemberStaff` —
Organization-scoped), 18 permissions. The Roles/Permissions matrix has no API mutation endpoint at
all — read-only over HTTP by design, changed only via `DatabaseSeeder.cs`. Full detail in
`docs/DATABASE.md` and `docs/AUTHENTICATION.md`.

## Security

RLS, tenant isolation (application + database layer, independently), PII protection (contact data
in separate RLS-protected tables, disclosed only via explicit action), public projections
(feature-flag-gated, server-enforced), storage access control, audit logging. Full detail in
`docs/SECURITY.md`.

## Matching implementation

Score-based engine (`MatchingEngine`) against the currently *Published* `MatchRuleSet`; nine
possible criteria (Location, Price/budget, Area, PropertyType, Purpose, Bedrooms, Bathrooms,
Amenities, Furnishing), each independently weighted/toleranced/required per the active rule set,
never hardcoded. A criterion missing data on either side is excluded from scoring rather than
penalized. No published rule set → the engine and API both say "Matching is not yet configured by
REAK" verbatim, never a fabricated score. Recomputed on every listing/demand create/update. Every
match carries a per-criterion `detailText` explanation, never a bare number — verified live in
Stage 15 (100%-score match with three passing-criterion explanations).

## Collaboration and contact disclosure

Match → collaboration request → accept → workspace (messages, notes, tasks, viewings, files,
activity log). Contact data stays private (`null` to the API caller) even to an active
collaborator until an explicit `POST .../contact-disclosures` call names the specific data type to
reveal (`ListingContact` / `DemandContact` / `Phone` / `Email`) — collaboration membership alone
never discloses it. Verified live end-to-end in Stage 15, including the disclosure unlocking the
correct field for the correct collaborator only.

## Testing results

| Category | Result |
|---|---|
| Unit | **Not implemented** — no test project exists. Real gap vs. spec §27. |
| Integration | **Not implemented** as automated/repeatable code — every "integration test" in this build was a live manual call against the real API/DB, documented per-stage in `DEVELOPMENT_PLAN.md`. |
| End-to-end | **22/22 scenarios pass**, live, Stage 15, fresh data (new org → invite → activate → login → listing → demand → match → collaboration → disclosure → suspension → feature flag → CMS publish). |
| Security | **12/12 adversarial tests pass**, live, Stage 15 (cross-org read/write, private contact/document access, unauthorized admin routes, disabled-feature bypass, role/permission tampering). |
| RLS | **11/11 assertions pass** (`rls_test.sql`), rerun after every RLS-relevant stage through Stage 15, zero regressions. |
| Accessibility | Semantic landmarks, disclosure-pattern ARIA, responsive nav collapse, WCAG 2.2 AA contrast hand-verified for the one borderline color token (~4.80:1, passes 4.5:1). See `DEVELOPMENT_PLAN.md` Stage 14. Mobile-breakpoint **pixel** verification was not possible in this environment (`resize_window` tool limitation, disclosed at the time) — only the underlying responsive classes were confirmed structurally correct. |

## Performance findings

`next/image` adopted for all listing/property photos with proper `sizes`/`fill`; opt-in
`revalidate: 3600` caching for admin-managed reference/taxonomy data on top of a codebase-wide
`no-store` default elsewhere; the `/media` origin bug found and fixed in Stage 14 (listing photos
had 404'd for every real visitor since Stage 6 — see `DEVELOPMENT_PLAN.md`). No formal load testing
or query-plan benchmarking was performed; all checks were qualitative (pages and API calls respond
promptly against the current dev dataset's volume).

## Accessibility (WCAG) findings

See "Testing results" above and `DEVELOPMENT_PLAN.md`'s Stage 14 section for full detail: semantic
landmarks and disclosure-pattern ARIA already correct from Stage 2, replicated into the newly
responsive portal/admin sidebars in Stage 14; two missing `aria-label`s on icon-only buttons found
and fixed; `prefers-reduced-motion` globally handled since Stage 2.

## CMS functionality

Pages, news, notices, events, resources, committee members, navigation, and site settings, all
with a `Draft → Review → Published → Archived` status workflow (a direct `Draft → Published`
transition is correctly rejected) enforced server-side, and published content correctly reaches
the public, unauthenticated endpoints while draft content correctly 404s. Verified live in
Stage 15.

## Required environment variables (names only)

See `docs/DEPLOYMENT.md` for the full annotated list. `REAK.Api`: `REAK_JWT_KEY` (required),
`REAK_BOOTSTRAP_ADMIN_EMAIL`/`REAK_BOOTSTRAP_ADMIN_PASSWORD` (optional), `Jwt__*` (optional,
defaulted), `Storage__*` (optional, defaulted), `ConnectionStrings__DefaultConnection`. `web`:
`REAK_API_URL` (server-only), `NEXT_PUBLIC_SITE_URL` (public).

## Production deployment steps

See `docs/DEPLOYMENT.md` for the full sequence: provision SQL Server → apply migrations → apply
RLS SQL → seed → verify with `rls_test.sql` → `dotnet publish` the API with real secrets → `npm
run build` the frontend pointed at the deployed API's internal address.

## Remaining configuration REAK itself must supply

Real logo, legal association name, brand colors; committee member info, contact info, membership
policy text (current CMS content is QA/placeholder); an approved matching rule set (a
`QA Standard Rules` set exists as a working example only); approved area-unit conversion rules
(`auto_unit_conversion` seeded off); a considered decision on which feature flags should be on at
launch (every flag defaults off except where a specific, documented exception was made for demo
purposes in Stage 12 — see `REAK.Api/Data/DatabaseSeeder.cs`); an actual email-sending provider.

## Known limitations (stated honestly)

- No automated unit or integration test suite (the primary blocker above).
- No email delivery — invitation/reset tokens need out-of-band delivery in this build.
- No account-lockout-after-N-failed-attempts mechanism — a deliberate trade-off (rate limiting
  covers the same threat without the added lockout-UX complexity), not an oversight.
- No frontend Content-Security-Policy header — a deliberate, documented trade-off (see
  `docs/SECURITY.md`), not an oversight.
- Mobile-breakpoint visual QA is structurally, not pixel-level, verified (environment tooling
  limitation, disclosed when it occurred).
- No load/performance benchmarking under realistic volume or concurrency.
- Never deployed outside local dev infrastructure.
