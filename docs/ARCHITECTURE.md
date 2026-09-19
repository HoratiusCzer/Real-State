# Architecture

## High-level shape

Three logical surfaces, two runtime processes:

```
Browser
  │  (same-origin only — never talks to REAK.Api directly)
  ▼
Next.js (web/, port 3000)
  │  Server Components / Route Handlers call REAK.Api server-to-server.
  │  A small set of Route Handlers (e.g. file downloads) act as same-origin
  │  proxies so the browser never needs REAK.Api's own origin or a CORS policy.
  ▼
REAK.Api (ASP.NET Core, port 5080)
  │  JWT-authenticated, RBAC-checked, RLS-enforced on every request.
  ▼
SQL Server (native row-level security)
```

No CORS policy exists on REAK.Api, deliberately — every browser-originated request in this
architecture is already same-origin to Next.js. Registering one would only widen the attack
surface for zero real cross-origin caller (see `docs/SECURITY.md`).

## Applications

- **Public website** (`web/src/app/(public)/`) — marketing pages, published CMS content
  (news/notices/events/pages), public member directory and property exchange (each gated by its
  own feature flag), membership application form.
- **Member Portal** (`web/src/app/portal/`) — authenticated, org-scoped: property listings,
  client requirements (demands), matches, collaboration workspaces, saved items, org/profile
  settings. Every route requires a session; RLS enforces tenant isolation regardless of what the
  frontend does.
- **Admin Portal** (`web/src/app/admin/`) — system-admin only: membership review, member
  organizations, invitations, roles (read-only), CMS, feature flags, match rules, reports, audit
  log, security overview. Gated by `user.isSystemAdmin` at the layout level (UX only — every
  underlying API endpoint independently enforces its own permission check; the frontend gate is
  never the real authority, spec §2.5).

## Identity & tenancy model

- `Profile` — one human, one login, org-independent.
- `MemberEntity` — one member organization (a brokerage).
- `EntityUsers` — join table: which profiles belong to which member entities, `IsActive` per
  membership.
- `Role` — has a `Scope`: `System` (association staff — `SuperAdmin`, `AssociationAdmin`) or
  `Organization` (member-org staff — `MemberAdmin`, `MemberStaff`).
- `ProfileRoleAssignments` — a profile's role grants. `IsSystemAdmin` (the claim gating the Admin
  Portal) is computed as "does this profile hold any `System`-scoped role" — not a flat column.
- `Permissions` / `RolePermissions` — the actual authorization unit checked by
  `[RequirePermission("slug")]` on controller actions; a profile's effective permission set is the
  union of its roles' permissions, computed fresh per request in `UserClaimsFactory`.

A profile can belong to more than one member entity. `CallerContext` (built once per request by
`CallerContextFactory`) carries `ProfileId`, every `MemberEntityId` the caller belongs to, and
`IsSystemAdmin` — this is what every service method authorizes against, independent of RLS.

## Listings & demands — mirrored structures

`PropertyListing` (supply) and `Demand` (client requirement) are structurally parallel: both have
a `NetworkVisibility` (OwnerOnly / SelectedMembers / AllMembers), an `IsPublicVisible` flag gated
by a feature flag, a separate private-contact sub-record only ever readable by the owner or an
explicitly-disclosed collaborator, and a moderation status (Draft → PendingReview/Approved →
Archived, auto-approved when `property_moderation_required` is off).

## Core flows

- **Flow A — membership**: public application → admin approval (creates the `MemberEntity`) →
  admin invitation → invitee accepts (sets password, activates) → login.
- **Flow B — exchange**: member creates a listing/demand → submits → approved (auto or manual) →
  visible per its `NetworkVisibility` → optionally public per its feature flag.
- **Flow C — matching**: a listing or demand is created/updated → `MatchingEngine` recomputes
  scores against the currently *Published* `MatchRuleSet` → results carry a per-criterion
  explanation, never a bare number.
- **Flow D — collaboration**: a match → a collaboration request → accept → a workspace opens
  (messages/notes/tasks/viewings/files) → contact stays private until an explicit
  `POST .../contact-disclosures` call — collaboration membership alone never discloses it.

## Security-sensitive database functions

Two places compute matches/counts across organizations that RLS would otherwise filter down to
"what the triggering user can see" — the matching engine's cross-org score computation and
`MatchesController`'s own-match visibility (a JOIN across a listing and a demand only needs RLS
permission on *one* side, but RLS's predicate applies to the JOIN target regardless of which
columns are projected). Both use `SessionContextOverride`, a scoped flag the RLS connection
interceptor checks on every connection-open: elevate for the duration of the system-level
computation, authorize the result set explicitly in C#, reset in a `finally`. This is the
project's answer to spec §20's "security-sensitive database function" requirement — implemented
explicitly rather than left implicit in ordinary RLS.

## Repository layout

```
REAK.Api/              ASP.NET Core backend
  Controllers/          21 controllers — see docs/API.md
  Services/              business logic, one folder per domain area
  Models/                Entities, Dto, Enums
  Data/                  DbContext, Migrations, Security/ (RLS SQL + rls_test.sql)
web/                    Next.js frontend
  src/app/(public)/      public site
  src/app/portal/        member portal
  src/app/admin/         admin portal
  src/app/api/           same-origin proxy routes (downloads, reference-data cache)
  src/lib/                api-client, auth/session, per-area API wrappers
docs/                   this documentation set
Progress-Tracking/      daily logs, one per working day
DEVELOPMENT_PLAN.md     the 16-stage build log — the authoritative build history
```
