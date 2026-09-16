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
3. ⏳ Database + migrations + RLS + RBAC — **next**
4. ⏸️ Authentication + member organizations
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

## Stage 3 — Database + Migrations + RLS + RBAC

Full schema per spec §2.2–§2.3, §8.3, §9, §10, §11, §12, §13, §15, §19, §21: profiles,
member_entities, entity_users, invitations, membership_applications, roles, permissions,
role_permissions; property_listings + listing_media/documents/amenities/contacts/
visibility_members; demands + demand_property_types/locations/amenities/contacts/
visibility_members; Nepal location hierarchy tables; land area unit reference table;
match_rule_sets/match_rules/matches/match_components/match_actions; collaboration_requests/
collaborations/collaboration_contact_disclosures; notifications; feature_flags; audit_logs;
CMS content tables. EF Core migrations + SQL Server security policies for RLS on every private
table, tested with direct query attempts (not just through the API).

## Stages 4–16

Detailed only once we reach them — see `docs/REAK-requirements.md` §4, §6–§14, §27, §34, §36
for the full scope of each. Will be broken into their own plan sections as they start, each
with its own daily-log entry and test results, per the spec's own recommended execution
approach (§37 of the original PDF, reproduced in the "Process note" of
`docs/REAK-requirements.md` §2.6).

---

**Last updated**: 2026-09-16
**Status**: Stage 1 complete, Stage 2 not yet started (needs frontend stack decision)
