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
2. ⏳ Design system + public website foundation — **next**
3. ⏸️ Database + migrations + RLS + RBAC
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
- Frontend stack: **not yet decided** — first thing to resolve in Stage 2.
- Requirements persisted to `docs/REAK-requirements.md`.

## Stage 2 — Design System + Public Website Foundation (next)

Open decision before starting: frontend framework. No existing frontend or convention to
preserve, so this needs a decision (proposal: Next.js, for SSR/SEO on public pages and
`noindex` portals within one framework) — confirm with user before scaffolding.

Then: typography/spacing/color tokens, base components (buttons, inputs, cards, dialogs,
tables, badges, alerts, nav, tabs, dropdowns, pagination, loading/error/empty states), and the
public homepage shell with real CMS-driven sections (not fabricated content — empty/config
states where real content doesn't exist yet).

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
