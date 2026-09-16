# REAK Platform — Development Plan

**Full requirements**: see `docs/REAK-requirements.md` (persisted from `Requirement/*.pdf`) —
read that file at the start of every session before continuing this plan.

**Project**: REAK — membership-based real estate association platform for Nepal
**Stack**: ASP.NET Core (.NET) Web API backend + SQL Server (native RLS via security
policies); frontend TBD at Stage 2 (leaning Next.js, not yet confirmed with user)
**Note**: this repo previously contained one day of misdirected work (a single-company CRM
that didn't match these requirements) — archived to `archive/`, see `archive/README.md`.
No `.claude` agent/workflow/context framework exists in this repo; ignore references to one
in the Production Development Prompt.

---

## Implementation Stage Order (from the spec, §2.6)

1. ✅ Repository audit + architecture confirmation — **complete 2026-09-16**
2. ✅ Design system + public website foundation — **complete 2026-09-16**
3. ✅ Database + migrations + RLS + RBAC — **complete 2026-09-16**
4. ⏳ Authentication + member organizations — **next**
5. ⏸️ Member portal
6. ⏸️ Property Exchange
7. ⏸️ Demand/Requirement system
8. ⏸️ Matching engine
9. ⏸️ Collaboration
10. ⏸️ Notifications
11. ⏸️ Admin + CMS
12. ⏸️ Feature flags + reports + audit
13. ⏸️ Security hardening
14. ⏸️ Performance + accessibility + responsive QA
15. ⏸️ Full regression testing
16. ⏸️ Documentation + production readiness

Each stage depends on the one before it. Do not accept a stage as done until its own slice of
testing and RLS checks pass (spec §27, §17) — don't defer all testing to the end. Each session
should re-inspect actual repo state rather than trust a previous session's summary.

## Stage 1 — Repository Audit + Architecture Confirmation (✅ complete)

- Confirmed no existing frontend, no Supabase, no `.claude` agent framework.
- Confirmed prior backend work (REAK.API CRM) did not match requirements; archived it.
- Backend stack decision: keep ASP.NET Core + SQL Server (user-approved), implement RLS via
  SQL Server native security policies rather than migrating to Postgres/Supabase.
- Frontend stack decision (confirmed in Stage 2): Next.js.
- Requirements persisted to `docs/REAK-requirements.md`.

## Stage 2 — Design System + Public Website Foundation (✅ complete)

**Frontend stack**: Next.js 16 (App Router, Turbopack) + React 19 + TypeScript + Tailwind v4,
scaffolded at `web/`. Confirmed with user.

**Design system**: generated via the `ui-ux-pro-max` skill, then curated by hand — the
auto-match leaned into a "luxury real estate" look (Cinzel serif, oversized display type,
teal marketplace colors) that directly contradicted the spec's own anti-patterns (§5: avoid
fake luxury imagery, avoid generic AI-SaaS look; Nepal-relevant/association-focused, not
marketplace/editorial). Replaced with a "Government/Public Service" navy + blue-accent
palette and "Corporate Trust" typography (Lexend headings / Source Sans 3 body). Full
rationale and tokens: `design-system/reak/MASTER.md`. Implemented as CSS custom properties +
Tailwind v4 `@theme inline` in `web/src/app/globals.css` (includes a dark-mode variant and
`prefers-reduced-motion` handling).

**Base components** (`web/src/components/ui/`): Button (link/button polymorphic, 4 variants),
Badge, Card, Alert, Input/Label, Skeleton, EmptyState. Tabs/Dialog/Dropdown deliberately not
built yet — nothing on the public site needs them; will add via Radix primitives when the
Member Portal (Stage 5) first needs them, rather than building unused components speculatively.

**Public site**: all 20 routes from spec §4.1 exist under `web/src/app/(public)/` (route group
so `/portal` and `/admin` get their own layouts later without inheriting this chrome).
Homepage (`web/src/app/(public)/page.tsx`) implements all 15 sections from §4.1 in spec order.
Content-dependent sections (mission/vision, benefits copy is structural/product-mechanics
only, verified members, news/notices/events, legal pages, membership application, login/
password reset, contact) use `EmptyState`/`PagePlaceholder` rather than fabricated content —
per §22, no invented official REAK content, statistics, testimonials, or legal text. The
public-properties homepage section respects a hardcoded conservative
`publicPropertiesEnabled: false` flag (`web/src/lib/feature-flags.ts`) until Stage 3/12 wire
up the real `feature_flags` table — it renders nothing when off, matching spec default.
Auth-adjacent pages (login, forgot/reset password) and forms that would need a backend
(membership apply, contact) intentionally show "not yet available" states instead of
non-functional forms, per §6 ("never fake login") and the "no fake data flow" principle (§35).

**SEO**: `robots.ts` (disallows `/portal`, `/admin`), `sitemap.ts` (static public routes),
per-route `<title>` metadata via the title template in the root layout, `noindex` on
auth-adjacent pages.

**Verified**: `npm run build` succeeds (24 routes, no errors), `npm run lint` clean, dev
server smoke-tested via curl (all routes 200 except deliberately-missing paths → 404;
robots.txt/sitemap.xml 200), homepage HTML spot-checked for expected empty-state copy. **Not
verified**: actual rendered appearance in a browser — no browser tooling was connected this
session (user chose not to install the Claude-in-Chrome extension). Visual QA (spacing,
responsive behavior at the 375/768/1024/1440 breakpoints, contrast) is a known gap for the
next session per spec §59/§23/§24.

## Stage 3 — Database + Migrations + RLS + RBAC (✅ complete)

**Project**: new `REAK.Api` (.NET 10 Web API) at the repo root, added to `REAK.slnx`. EF Core
10 + SQL Server. Connection string points at `real-estate` on `(localdb)\MSSQLLocalDB`.

**Schema**: 57 entities across `Models/Entities/{Identity,Reference,Listings,Demands,Matching,
Collaboration,Notifications,FeatureFlags,Audit,Cms}/`, covering every table group in spec
§2.2–§2.3, §8.3, §9, §10, §11, §12, §13, §15, §19, §21 — member orgs/roles/permissions/
invitations, the full listing/demand mirrored structure with isolated contact tables, the
matching engine (rule sets/rules/matches/components/actions, no hardcoded weights — `Weight`
defaults to 0 until an admin sets it), the collaboration workspace + 8 child tables +
disclosure grants, notifications, feature flags, append-only audit log, and CMS content types.
All primary keys are `Guid` per §21. Applied via two migrations:
`InitialCreate` (schema) and `AddRowLevelSecurity` (see below) — both in
`REAK.Api/Data/Migrations/`.

**RLS**: implemented as native SQL Server row-level security (`CREATE SECURITY POLICY` +
`SCHEMABINDING` inline table-valued predicate functions in a `Security` schema), not just
application-level filtering — see `docs/REAK-requirements.md` §2.5's stack note for why this
satisfies the spec's Postgres-flavored RLS language on this stack. Source:
`REAK.Api/Data/Security/RowLevelSecurity.sql` (+ `.Down.sql`), embedded into the
`AddRowLevelSecurity` migration. Covers: `PropertyListings` and `Demands` (tenant + network +
public-projection visibility, spec §16 — the public branch is flag/approval/expiry gated and
doesn't depend on session context, so it also covers genuinely anonymous connections),
`ListingContacts`/`DemandContacts` (owning-org read, or an explicit unrevoked
`CollaborationContactDisclosure` scoped to the specific listing/demand via the collaboration's
originating match — never a blanket org-to-org grant), `CollaborationWorkspaces` + its 8 child
tables (participants only), and an `INSTEAD OF UPDATE, DELETE` trigger making `AuditLogs`
genuinely append-only. **Read and write are separate predicates** for listings/demands/
contacts — broader network/public read visibility never doubles as write authorization; a
first draft of this got that wrong (missing `BEFORE DELETE` block predicates meant anyone with
read access could delete a row they didn't own) — caught before shipping by writing an actual
adversarial test, not just eyeballing the SQL. See `REAK.Api/Data/Security/rls_test.sql` — a
self-cleaning script that seeds two orgs/users and empirically proves: owner access, outsider
denial, hostile UPDATE/DELETE blocked, read-without-write after a visibility grant, anonymous
denial then correct anonymous access once a listing is genuinely public, and the audit-log
trigger. Re-run it after any RLS-relevant schema change.

**RBAC**: seeded 4 roles (SuperAdmin/AssociationAdmin — system-level; MemberAdmin/MemberStaff —
org-level) and the 17 permissions from spec §7, with a documented default grant matrix (full
grant for both admin roles; a reasonable subset for Member roles) — not spec-mandated at this
granularity, adjustable later via the Admin Portal (Stage 11).

**Seeded reference data** — deliberately limited to what the spec itself enumerates, nothing
invented: the 9 land area units (§11, conversion coefficients left `null` — disabled until an
admin configures them), the 4 demand purposes (§9: buyer/tenant/investor/other), Nepal's 7
provinces (an administrative fact, not a REAK business fact), and NPR as a currency. Feature
flags: all 13 from §15 seeded `false` (conservative default). **Not seeded**: property types,
subtypes, amenities, districts, municipalities, wards, localities (spec marks these
admin-configurable without giving an exhaustive list — tables exist, ready for real admin
input or a verified data import later) — and no SuperAdmin user account (needs a real
password set through a proper flow; that's Stage 4, not a hardcoded seeded credential).

**Recheck against spec (2026-09-16, before pushing)**: went back through
`docs/REAK-requirements.md` §21 (Database Quality Standards) line by line against what was
actually built. Found two real gaps:
- No CHECK constraints anywhere (only app-level `[Range]` annotations, which don't reach the
  database). Added a third migration, `AddDatabaseQualityCheckConstraints`, with 15 constraints
  on the fields that have a genuine invariant (price/area/bedroom counts ≥ 0, demand
  min/max budget and area ranges internally consistent, match rule weight ≥ 0, match score ≥
  0). Verified with a direct negative-price insert attempt — correctly rejected by the DB.
- `ReferenceCode` columns exist on `PropertyListings`/`Demands` but nothing generates them yet
  (spec: "generated server-side/database-side, never client-side"). Deliberately **not**
  fixed in this stage — no service exists yet that creates listings/demands (that's Stage 6/7),
  so there's no live risk of client-side generation today. Flagged so Stage 6/7 build the
  generator (e.g. a SQL Server `SEQUENCE` + computed default) instead of accepting a
  client-supplied value.

Reran the full `rls_test.sql` suite after adding the check-constraints migration — all 7
scenarios still pass, no regression.

**Verified**: `dotnet build` clean (0 errors) after every change; all three migrations applied
cleanly to a real `real-estate` database; `rls_test.sql` passes all 7 scenarios (twice — before
and after the check-constraints migration); a negative-price insert is rejected by the new
constraint; seeded data spot-checked via direct SQL queries.

**Known gaps for later stages**: `ListingDocument`/`ListingMedia`, `Profile`, and
`MemberEntity` don't have RLS policies yet (documented, not forgotten) — general PII
protection on `Profile` and the public/private split on listing media is a reasonable Stage 13
(Security hardening) follow-up. The disclosure predicate only resolves a listing/demand when
the collaboration originated from a match (`CollaborationRequest.MatchId` set) — a
non-match-originated collaboration currently has no way to scope a contact disclosure to a
specific record; would need an explicit target on the disclosure or workspace itself. The
session context (`app.profile_id`, `app.is_system_admin`) that the predicates depend on isn't
wired into the ASP.NET request pipeline yet — that requires the JWT auth middleware, which is
Stage 4's job; until then, every predicate fails closed (no session context = no access),
which is the correct default, not a bypass.

## Stages 4–16

Detailed only once we reach them — see `docs/REAK-requirements.md` §4, §6–§14, §27, §34, §36
for the full scope of each. Will be broken into their own plan sections as they start, each
with its own daily-log entry and test results, per the spec's own recommended execution
approach (§37 of the original PDF, reproduced in the "Process note" of
`docs/REAK-requirements.md` §2.6).

---

**Last updated**: 2026-09-16
**Status**: Stage 1 complete, Stage 2 not yet started (needs frontend stack decision)
