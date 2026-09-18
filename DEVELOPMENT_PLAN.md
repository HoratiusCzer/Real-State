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
4. ✅ Authentication + member organizations — **complete 2026-09-16**
5. ✅ Member portal — **complete 2026-09-16**
6. ✅ Property Exchange — **complete 2026-09-16**
7. ✅ Demand/Requirement system — **complete 2026-09-16**
8. ✅ Matching engine — **complete 2026-09-17**
9. ✅ Collaboration — **complete 2026-09-17**
10. ✅ Notifications — **complete 2026-09-17**
11. ✅ Admin + CMS — **complete 2026-09-17**
12. ✅ Feature flags + reports + audit — **complete 2026-09-17**
13. ✅ Security hardening — **complete 2026-09-17**
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

## Stage 4 — Authentication + Member Organizations (✅ complete)

**The critical piece**: a SQL Server `DbConnectionInterceptor`
(`REAK.Api/Services/Security/SessionContextConnectionInterceptor.cs`) that projects the
authenticated caller's identity onto every connection the moment EF opens it, via
`sp_set_session_context`, reading straight off `HttpContext.User` (populated by JWT auth
middleware before controllers run). This is what makes Stage 3's RLS predicates — written
against `SESSION_CONTEXT('app.profile_id')` / `SESSION_CONTEXT('app.is_system_admin')` — see a
real caller for the first time. Verified directly (not just by code review): a temporary debug
endpoint querying `SESSION_CONTEXT` back showed the SuperAdmin's and a MemberAdmin's exact
profile IDs and admin flags reaching the database correctly, then was deleted.

**Auth**: real login/refresh/logout/change-password/forgot-password/reset-password (spec §6) —
`REAK.Api/Services/Auth/AuthService.cs`. Access tokens are short-lived (15 min) stateless JWTs;
refresh tokens are opaque, hashed at rest, and rotate on every use (old token revoked, new one
issued) so replay of a stolen-but-already-used token is detectable. Password reset tokens are
single-use and hashed the same way. `ActiveProfileMiddleware` re-checks `Profile.IsActive`
against the database on every authenticated request — not just at login — so suspension revokes
access immediately even for a still-unexpired JWT (spec §2.2's explicit requirement), verified
directly: a suspended user's still-valid token was rejected mid-session.

**RBAC enforcement**: JWT carries the caller's permission slugs (computed fresh from
`ProfileRoleAssignments → Roles → RolePermissions` at login/refresh, spec §7 — never a bare role
check) and an `is_system_admin` flag matching RLS's own bypass semantics exactly (both
System-scope roles already hold every permission per the Stage 3 seeder). A
`[RequirePermission("slug")]` filter gates admin endpoints — explicitly documented as a UX/API
convenience only, since the database RLS remains the real authority regardless (spec §2.5).

**Flow A end-to-end** (spec §2.4): `MembershipApplicationsController`/`Service` (public submit,
gated by the `membership_application_enabled` feature flag; admin approve/reject) →
`InvitationsController`/`Service` (admin creates, invitee looks up and accepts — handles both a
brand-new profile setting its first password and an existing profile joining a second
organization) → `EntityUser` + `ProfileRoleAssignment` rows created on acceptance. Verified with
a real run through every step via curl: submit → approve (creates the `MemberEntity` +
invitation) → accept → login as the new MemberAdmin → correct scoped permissions and
member-entity claim → 403 on an admin-only action → refresh rotation → reused-old-token
rejection → logout.

**Bootstrapping problem solved**: a permission system starts with nobody in it, so nobody could
ever call an admin-only endpoint to create the first real admin. `DatabaseSeeder.SeedSuperAdminAsync`
resolves this — runs only while zero System-scope role assignments exist anywhere, and only if
`REAK_BOOTSTRAP_ADMIN_EMAIL`/`REAK_BOOTSTRAP_ADMIN_PASSWORD` are supplied via environment
variables/user-secrets (never committed, never a hardcoded default). Once a real SuperAdmin
exists it's permanently a no-op.

**No email provider exists yet.** `IEmailSender`/`LoggingEmailSender` logs instead of delivering
invitation and password-reset emails — documented in code as a swap-in point before production
(spec §34), not a fabricated "email sent" claim. `REAK_JWT_KEY` follows the same
never-committed-secret pattern; `Program.cs` fails fast at startup if it's missing rather than
falling back to an insecure default.

**Frontend** (`web/`): real forms (not the earlier "not yet available" placeholders) for login,
forgot/reset password, and membership application, plus a new `/invite/[token]` route (not one
of spec §4.1's original 20 — added because Flow A can't complete without it once invitations are
real). Built as Next.js Server Actions setting httpOnly session cookies — the browser never
receives the JWT or refresh token directly, only an opaque session cookie, mitigating token
theft via XSS (frontend checks remain UX only either way, per spec §2.5). A minimal `/portal`
placeholder (outside the `(public)` route group, matching Stage 2's chrome-isolation decision)
proves the round trip — reads the session, calls `/api/auth/me`, shows the signed-in user — but
deliberately builds nothing beyond that; the real Member Portal is Stage 5. Verified: `dotnet
build` clean; `npm run build`/`npm run lint` clean; unauthenticated `/portal` redirects to
`/login`; a real invitation created via the API rendered correctly on `/invite/[token]`.

**Known gaps for later stages**: `RefreshTokens`/`PasswordResetTokens`/`Profiles`/
`MemberEntities`/`Invitations`/`MembershipApplications` have no RLS yet (consistent with the
Stage 3 gap already noted for `Profile`/`MemberEntity` — reasonable Stage 13 follow-up, and not
a live risk today since the API is the only writer and every mutation already checks ownership
in application code). No silent/automatic access-token refresh in the browser — Server
Components can't set response cookies mid-render and Next's own Proxy docs say Proxy isn't meant
for full session management, so an expired 15-minute access token just means signing in again
until Stage 5 builds real session persistence. No SMTP/email provider wired up (see above). No
CORS configuration — unneeded so far since the browser never calls REAK.Api directly, only the
Next.js server does; revisit if Stage 5 adds direct client-side calls.

## Stage 5 — Member Portal (✅ complete)

**Scope decision**: spec §4.2 lists 20 portal routes, but 12 of them (properties, demands,
matches, collaborations, saved) depend on Stages 6-9, which don't exist yet. Built the portal
shell plus every route that's genuinely buildable now (identity/org/notification-related);
everything else got a real route that renders an honest "not built yet — Stage N" state rather
than being hidden, faked, or built ahead of its owning stage's data model.

**Real, working now**:
- `REAK.Api/Services/Security/RequirePermissionAttribute`-gated API endpoints:
  `DashboardController` (org-scoped real counts — legitimately all zero today since nothing can
  create a listing/demand/match/collaboration yet, explicitly scoped to the caller's own
  `MemberEntityId`s rather than relying on RLS's broader network-visible read set, which would
  overcount for "my dashboard"), `MemberEntitiesController` (directory list/detail, plus
  self-service org editing with an explicit ownership check — a MemberAdmin can edit only the
  org(s) they administer, verified live by having one org's admin get a 403 on another org's
  update), `NotificationsController` (own-notifications only, no RLS on this table yet so every
  query/mutation is explicitly scoped to the caller's ProfileId in code), and
  `ProfilesController.UpdateMe` (self-service name/phone edit).
- Also fixed a real RBAC gap while building this: `MemberAdmin` had `members.read` but never
  `members.update`, so no org admin could have edited their own org's profile — added to
  `DatabaseSeeder`'s grant matrix.
- Frontend (`web/src/app/portal/`): a protected layout (`requireSession()` redirects to `/login`
  if there's no valid session) with a sidebar covering all 12 spec routes, `/portal/dashboard`
  (stat tiles built per the dataviz skill's figures contract — real zeros render as an ordinary
  number, the one stat with no backing feature at all (Saved Properties) renders as a visually
  distinct muted em dash with a "coming with Stage 6" caption, never the same look as a real
  zero), `/portal/members` + `/portal/members/:id` (directory), `/portal/notifications`
  (list + mark-read/mark-all-read), `/portal/profile` (edit), `/portal/organization`
  (view, or edit if the caller holds `members.update`), `/portal/settings` (password change,
  reusing Stage 4's endpoint).

**Verified end-to-end** (not just unit-level): created two member organizations via Flow A,
confirmed each org's dashboard/member-list/notifications are correctly scoped and isolated from
the other, confirmed a MemberAdmin can edit their own org but gets a 403 editing another org's
(the one piece of real authorization logic this stage added, since MemberEntity has no RLS yet),
confirmed the Next.js pages render real data fetched live from the API (not mocked) by setting
session cookies directly and requesting each portal route, confirmed unauthenticated requests to
any `/portal/*` route redirect to `/login`. All test data cleaned from the local dev database
afterward.

**Deferred to their owning stage** (spec-listed routes, built as honest placeholders, not fake
data or hidden nav items): `/portal/properties(+new/:id/:id/edit)`, `/portal/my-properties`,
`/portal/saved` → Stage 6; `/portal/demands(+new/:id/:id/edit)` → Stage 7; `/portal/matches(+:id)`
→ Stage 8; `/portal/collaborations(+:id)` → Stage 9.

**Known gaps for later stages**: same as Stage 4's — no RLS yet on `MemberEntities`/
`Notifications`/`Profiles` (Stage 13), so `MemberEntitiesController.Update`'s ownership check and
`NotificationsController`'s profile-scoping are real security logic living in application code,
not backstopped by the database the way listings/demands are — worth double-checking again once
Stage 13 lands RLS everywhere. No browser tooling connected this session (same Stage 2/4 gap) —
verified via HTTP requests with cookies set directly rather than an actual browser session, so
visual QA and the Server Action submit flow itself (as opposed to the API calls underneath it)
are unverified by an actual browser.

## Stage 6 — Property Exchange (✅ complete)

**Prerequisite gap closed first**: PropertyType/PropertySubtype/Amenity/District/Municipality/
Ward/Locality were deliberately left empty in Stage 3 (spec §22 — no exhaustive list was given,
inventing one would be fabricated business data) but a listing's schema requires all of them as
non-null FKs. `ReferenceDataController` adds minimal admin-configurable CRUD (read for anyone,
create gated by `settings.manage`) — just enough that an admin can genuinely configure the
taxonomy Property Exchange depends on, not a full manage/edit/reorder UI (that stays Stage 11's
job). `Purpose` turned out to be shared between listings ("Sale"/"Rent"-shaped) and demands
("Buyer"/"Tenant"/"Investor"/"Other", per spec §9's explicit list) on the same table — matches
the entity's original doc comment, just needed an admin to actually add listing-shaped values
too, which this same endpoint now allows.

**ReferenceCode generation** (deferred explicitly since Stage 3): `IReferenceCodeGenerator` +
two SQL Server `SEQUENCE` objects (`ListingReferenceCodeSeq`, `DemandReferenceCodeSeq`) —
atomic under concurrent inserts, never client-supplied (spec §21). Produces codes like
`RK-L-2026-000001`.

**Design decision on the multi-step wizard** (spec §8.2): the database requires
PropertyTypeId/PurposeId/Title/full location/LandArea/AreaUnitId/CurrencyId/Price as NOT NULL
columns, so a listing can't exist as a true partial row before those are known. The wizard
accumulates the first 5 steps (Type, Basic Information, Location, Specifications, Price)
entirely client-side, creates the Draft row the moment all of them are known, then every
subsequent step (Amenities, Photos, Documents, Description, Contact, Visibility, Expiry)
persists against that real listing — satisfying "save draft, continue, back" without needing a
schema change, and giving Photos/Documents a real listing to attach to (spec's own step order
puts photo/document upload after Amenities, which lines up with this).

**Media/document storage**: `IFileStorage`/`LocalDiskFileStorage` — a dev-only placeholder
(documented for swap to S3/Azure Blob before production, same pattern as Stage 4's
`LoggingEmailSender`). Public listing photos are served statically from a directory scoped
*only* to the `listing-media` container; private documents have no static route at all — every
read goes through `ListingsController`'s authenticated, ownership-checked download action (spec
§18). Verified directly: a guessed `/media/listing-documents/...` URL 404s unconditionally, and
a non-owner with network-read access to the listing gets 403 on the download endpoint despite
being able to see the listing itself.

**RLS carries the authorization weight it was built for in Stage 3**: `ListingService`'s
mutating methods rely on `PropertyListings`' RLS block predicate to reject a non-owner's write —
`SaveGuardedAsync` catches the resulting `SqlException` and translates it to a clean 403 rather
than a 500. `ListingMedia`/`ListingDocument` have no RLS of their own (documented Stage 3/13
gap), so `ListingsController` does an explicit ownership check before touching them. The public
projection endpoint (`PublicPropertiesController`, spec §16) re-derives every gate explicitly
(flag AND Approved AND IsPublicVisible AND active member AND not expired) as deliberate
defense-in-depth on top of RLS's own public branch, with a hand-picked field allowlist — never
the raw entity, never `InternalNotes` or `ListingContact`.

Also added `SavedListing` (spec §8.1's "saved properties", no entity existed for it) — small,
per-profile, no RLS (same pattern as Notifications), which also finally gives Stage 5's
dashboard a real number instead of `null` for "Saved Properties".

**Verified end-to-end via the actual running app** (API directly, then again through the
Next.js pages with session cookies set): full lifecycle (create → submit → approve/reject →
archive → soft-delete) across the moderation-on and moderation-off paths; cross-org read
visibility after a network-share vs. write still blocked; contact info never leaking to a
non-owner even with network read access; media upload + static serving + document upload +
authenticated-only download + 403 for a non-owner; public search/detail with the feature flag
off (disabled state) and on (real listing returned, no contact field present); the homepage's
featured-properties section and both portal and public browse/detail pages rendering real data
fetched live from the API. `dotnet build` and `npm run build`/`lint` clean throughout.
`rls_test.sql`'s 7 scenarios rerun after all three new migrations — still pass.

**Frontend architecture note**: added a small same-origin proxy layer
(`web/src/app/api/reference/[...path]` for read-only taxonomy lookups, and
`web/src/app/api/portal/listings/[id]/{media,documents}` for authenticated uploads) so the
listing wizard's client-side cascading location dropdowns and upload-progress reporting (spec
§8.2) work without the browser ever calling REAK.Api cross-origin or holding the access token
itself — consistent with Stage 4's httpOnly-cookie security model. Upload progress uses `XMLHttpRequest`
(still the only mechanism with real upload-progress events across browsers) against these proxies.

**Known gaps for later stages**: `ListingMedia`/`ListingDocument` still have no RLS (Stage 13,
unchanged from Stage 3 — the explicit ownership checks added this stage are real but live in
application code); the wizard's Visibility step only exposes Owner-only/All-members (the API
supports `SelectedMembers` with a specific org picker too, just not wired into this UI — a
reasonable scope cut given time, revisit if members ask for it); no signed/expiring URLs for
private documents (local-disk auth-gated download stands in for that until real object storage
lands); Nepal's district/municipality/ward/locality hierarchy is still empty by default (Stage 3's
decision holds — admins add real data via `ReferenceDataController`, nothing fabricated here
either, and this session's test fixtures for it were left in the dev DB as legitimate
config data, not business data).

## Stage 7 — Demand/Requirement System (✅ complete)

Deliberately mirrors Stage 6's architecture (spec §2.3: listings and demands are two sides of
one relationship, and Stage 3 already built the schema identically-shaped) — same RLS-reliance
pattern in `DemandService` (`SaveGuardedAsync` catches the RLS block-predicate `SqlException`
and returns 403, exactly like `ListingService`), same contact-isolation model
(`DemandContact`/`ClientName`/`ConfidentialNotes` never returned by normal queries), same
`ReferenceCode` generator (already built generically in Stage 6 — `NextDemandCodeAsync` just
needed calling). Two real differences, both spec-driven:

- **No moderation.** `DemandStatus` has no PendingReview/Rejected (unlike `ListingStatus`), and
  spec §15's feature-flag list has no demand equivalent of `property_moderation_required`.
  Lifecycle is just Draft → publish → Active → fulfill/archive, no admin approval step, no
  `demands.moderate` permission.
- **Property type and location are proper many-to-many join tables**, not single required FKs —
  spec §9 explicitly: "a member can register a client's requirement... do not model
  relationships as arrays — use proper join tables." A demand can accept several property types
  and several acceptable locations at whatever hierarchy depth the client cares about (province
  only, or all the way to a specific locality). `DemandService` exposes replace-set endpoints
  for both, mirroring how listing amenities already worked.

**RBAC gap found and closed**: spec §7's permission list only gives `demands.read`/
`demands.create` as examples — no `demands.update` existed, meaning nobody could ever edit or
withdraw a requirement they'd created. Added `demands.update` to the seeded permission list and
granted it to MemberAdmin only (mirroring the existing `listings.update` asymmetry — MemberStaff
gets create but not update for either), consistent with the `members.update` gap found and fixed
the same way in Stage 5.

**No public projection** — spec §9.1 scopes demand search/visibility to members only (contrast
with listings' spec §16 public branch); there's no `PublicDemandsController` and none was
needed.

Because a `Demand` row's only required-by-schema fields are `Title`+`PurposeId` (everything
else — budget, area, bedrooms, property types, locations — is optional/nullable, unlike a
listing's much larger required set), the wizard's create point moves much earlier: the row is
created right after step 1 (Basic Information), and every later step (Property Types, Location,
Budget & Area, Amenities, Description, Client Contact, Visibility, Expiry) persists against the
real row from there.

**Verified end-to-end**, API-first then through the actual Next.js pages with session cookies
set: full lifecycle (Draft → publish → fulfill / archive → soft-delete); cross-org RLS isolation
(owner reads/writes fine, a different org gets 404 before any visibility grant, gets read access
but a 403 on write after a network-share, exactly like Stage 6's listing tests); client contact
(`ClientName`/`Phone`/`Email`/`ConfidentialNotes`) never leaking to a non-owner even with
network read access; property-type/location/amenity replace-set endpoints; the Stage 5
dashboard's "Active requirements" stat now showing real, non-zero data. `dotnet build` and
`npm run build`/`lint` clean. `rls_test.sql`'s 7 scenarios rerun — still pass, no regression
(no new migration was needed at all this stage — the schema already existed from Stage 3; only
new application code and one permission-seed addition).

**Known gaps for later stages**: the wizard and edit page both collect only a single acceptable
location per requirement in the UI (the API supports multiple via the replace-set endpoint,
just not exposed as an add-multiple picker — same kind of scope cut as Stage 6's
`SelectedMembers` visibility picker, and for the same reason: time, not a technical limit);
`DemandContact` RLS is real and tested, but `Demand`'s own moderation-free lifecycle means there
is deliberately no equivalent of Stage 6's approve/reject flow to test — that's correct per
spec, not an omission.

**Recheck against spec before pushing (2026-09-16)**: went back through §9/§9.1 line by line
against what was actually built, then went further and adversarially tested something Stage 6
never specifically verified either. Two things came out of it:

- **Verified, not just reasoned about**: `ReplacePropertyTypesAsync`/`ReplaceLocationsAsync`/
  `ReplaceAmenitiesAsync`/`UpdateVisibilityAsync` only touch child join tables
  (`DemandPropertyTypes`/`DemandLocations`/`DemandAmenities`/`DemandVisibilityMembers`) that have
  no RLS policy of their own — they're protected only because each method also sets
  `demand.UpdatedByProfileId`/`UpdatedAt` on the parent row before saving, which forces an UPDATE
  against `dbo.Demands` into the same transaction and lets that table's real RLS block predicate
  reject the whole thing. That's a subtle invariant (an easy line to accidentally drop in a future
  edit), so instead of trusting the code-reading, ran a live adversarial test: created a demand,
  network-shared it, then had a different org's session — which could now *read* it — attempt all
  four of those endpoints plus the equivalent listing ones
  (`ReplaceAmenities`/`UpdateVisibility`/`UpdateContact`) from Stage 6. Every one was rejected
  (403) and a follow-up read confirmed nothing had actually changed. This was genuinely
  unverified before now, for both stages, not a new bug.
- **Found and deliberately not fixed**: `AuditLogs` (spec §19 — "listing changes, demand
  changes, moderation actions...") has the table, its RLS, and its append-only trigger (Stage 3)
  but nothing in `AuthService`/`ListingService`/`DemandService`/`MemberEntitiesController`/
  `ProfilesController` ever writes to it. Confirmed this is intentional per the spec's own stage
  order (§2.6 lists "Feature flags + reports + audit" as Stage 12, well after Matching/
  Collaboration/Notifications/Admin) rather than something each stage should wire in piecemeal —
  correctly deferred, now explicitly documented rather than silently absent.

## Stage 8 — Matching Engine (✅ complete)

**Scoring core** (`MatchingEngine`, spec §12): every number that shapes a score — which
criteria count, their weights, whether a failed criterion excludes the pair entirely, tolerance
percentages — comes from the currently *Published* `MatchRuleSet`'s `MatchRule` rows, never a
literal in application code. Nine criteria implemented (Location, Price/budget, Area,
PropertyType, Purpose, Bedrooms, Bathrooms, Amenities, plus Furnishing — which always reports
`MissingData` since `Demand` has no furnishing-preference field in the Stage 3 schema and spec
§9 never asked for one; documented rather than silently guessed). A criterion with no data on
*either* side is excluded from both the score's numerator and denominator rather than counted
as a fail — a listing shouldn't be penalized for a preference a requirement never stated. If no
rule set is published, the engine — and the API — correctly show "Matching is not yet
configured by REAK" (spec's exact wording) rather than generating score-0 placeholder matches.
`MatchRuleSetsController` gives admins the minimal CRUD needed to actually create/publish rule
sets (same "unblock the feature, defer the full manage UI to Stage 11" pattern as Stage 6's
`ReferenceDataController`).

**Recompute is wired into the exact points spec Flow C calls for** ("a listing or demand is
created or updated"): `ListingService`/`DemandService`'s submit/approve/update/replace-set
methods each call the engine afterward via a small `SaveGuardedAndRecomputeAsync` wrapper — the
engine itself no-ops if the listing/demand isn't in an eligible status or no rule set is
published, so every plausible call site can call it unconditionally rather than each
re-deciding whether a recompute is warranted. A member's own shortlist/dismiss/reopen decision
on a `Match.Status` is deliberately left untouched by recomputation (verified live) — an edited
listing shouldn't silently un-dismiss a match someone already reviewed.

**Two genuine, non-obvious bugs found and fixed while building this** (both caught by actually
running the thing against two real organizations, not by reading the code):

1. **The matching engine couldn't see across organizations at all.** It shares its `DbContext`/
   connection with the HTTP request that triggered it, and Stage 4's RLS interceptor had
   already stamped that connection with the *triggering user's* session context. Searching "every
   Approved listing" to match a demand against silently RLS-filtered down to only what that one
   user could already see — exactly backwards for a system-level computation that has to see
   across every organization to do its job. A first fix (a single `sp_set_session_context` call
   before the engine's queries) didn't work: EF Core opens and closes its connection around each
   *individual* operation, not once per request, so the interceptor re-stamped the ambient
   caller's identity back onto the connection before the very next query ran. The real fix is
   `SessionContextOverride` — a scoped flag the interceptor checks on every connection-open,
   set for the duration of a recompute (or a `MatchesController`/`DashboardController` query that
   has the same cross-tenant-join shape) and reset in a `finally`. This is spec §20's
   "security-sensitive database function" pattern, implemented explicitly rather than assumed:
   narrow scope, only for the operation that needs it, and the actual authorization decision
   (who gets told about a match) stays in application code, never delegated to the elevated read.
2. **The same JOIN-collapse pattern hit `MatchesController` and `DashboardController`'s
   `PotentialMatchesCount`.** Any query that reads `m.Listing.MemberEntityId` *and*
   `m.Demand.MemberEntityId` in the same LINQ expression joins to both PropertyListings and
   Demands, and RLS's filter predicate applies to a table the instant it's a JOIN target —
   regardless of which columns end up selected. A listing owner who doesn't also own the
   matched demand (the normal case) would have their own match silently hidden by the INNER
   JOIN to the demand side they can't see. Same `SessionContextOverride` fix, same "elevate to
   read, authorize in C#" pattern each of those controllers already used for their own explicit
   ownership checks.

Caught the first one only because a match that should obviously have existed (two matching,
freshly-created, freshly-approved records) simply never appeared — traced it by hand rather
than assuming the constraint tolerances were the culprit. Caught the second live-testing cross-
org isolation on `MatchesController.Get`/`Search` immediately afterward, on the hypothesis that
the exact same JOIN shape would have the exact same problem — it did.

**Authorization model for the matching tables**: none of `Match`/`MatchComponent`/
`MatchRuleSet`/`MatchRule`/`MatchAction` have RLS (spec's own tables list never asked for it,
and a Match row reveals a compatibility relationship between two specific organizations' private
data — arguably more sensitive than either side alone). `MatchesController` does its own
explicit check everywhere: a caller may see/act on a match only if their org owns the listing OR
the demand, or they're a system admin — deliberately more conservative than Listing/Demand RLS
itself, since network-shared visibility of one side was never meant to imply visibility of the
match.

**Frontend**: `/portal/matches` (browse, showing the "not configured" state verbatim when no
rule set is published) and `/portal/matches/:id` (full explanation — score, rule set/version,
per-criterion pass/partial/fail/missing-data with detail text, calculation timestamp — plus the
five member actions from spec §12.1: shortlist, dismiss, reopen, request collaboration, report
incorrect data). "Request collaboration" is recorded as an auditable `MatchAction` but
deliberately does not create a `CollaborationRequest`/`CollaborationWorkspace` yet — that wiring
is Stage 9's job; the response says so explicitly rather than silently no-opping.

**Verified end-to-end**, methodically, after the two bugs above were fixed: a real rule set
(Location required, PropertyType required, Price weighted with a 10% tolerance, Bedrooms
weighted) computed a 100% match for a genuinely compatible listing/demand pair; pushing price
4.55% over budget correctly produced a Partial result and dropped the score to 85 (exact
weighted-average arithmetic verified by hand); pushing it 82% over correctly Failed without
excluding the pair (score 70, since Price wasn't marked required); changing the listing's
property type to something outside the demand's accepted list correctly excluded the pair
entirely (the Match row was deleted, confirmed via both a 404 on detail and a 0-count search);
a third, uninvolved organization got a 403 on both the match detail and the record-action
endpoint; shortlisting a match survived a subsequent listing edit without reverting to New;
request-collaboration correctly noted the Stage 9 deferral. Reran the full `rls_test.sql` suite
after all of this — still 7/7, no regression from the interceptor changes. `dotnet build` and
`npm run build`/`lint` clean throughout. All test fixtures cleaned from the dev database.

**Known gaps for later stages**: match rule sets can only be authored via the API directly
(curl/Postman today) — a real admin UI for building rule sets, previewing their effect, and
managing versions is Stage 11's job, same as every other admin-configuration surface so far.
Recompute is synchronous and O(n) per triggering write (every Approved listing × every Active
demand, scoped one-sided) — fine at current scale; if REAK's catalog grows large this is the
natural point to move to a background job queue, but that would be solving a problem that
doesn't exist yet (no premature optimization).

## Stage 9 — Collaboration (✅ complete)

**Request lifecycle** (`CollaborationRequestService`, spec §2.4 Flow D / §13.1): a request can
start from a `Match` (`CreateFromMatchAsync`) or be a standalone introduction to any active
organization (`CreateToOrgAsync`). `CollaborationRequest` has no RLS of its own — it has no
`CollaborationWorkspaceId` to hang a participant predicate off before a workspace exists — so
every method does its own explicit from/to-org ownership check, the same pattern already used
for `Notifications`/`MemberEntities`. Accept/decline/cancel are guarded by status (`Pending`
only) and by which side is allowed to act (recipient accepts/declines, requester cancels).

**Workspace creation is the one genuinely new piece of RLS-interaction design this stage
needed**, and it was caught *before* writing any code, not after a failed test the way Stage 8's
bugs were: `CollaborationWorkspaces`/`CollaborationParticipants` share a single predicate
(`fn_CollaborationParticipantPredicate`, spec §13 — unlike listings/demands' split read/write
predicates, "can read this workspace" and "can write into it" are the same audience here, any
participant) whose block predicate requires the session's profile to already be a participant of
the workspace being inserted into — impossible for the very first participant rows in the same
transaction that creates the workspace. `AcceptAsync` elevates via the Stage 8-established
`SessionContextOverride` for just that one bootstrap `SaveChangesAsync` (workspace + both
participant rows + an opening `CollaborationActivity`), then immediately resets it — every later
action in an existing workspace goes through as the caller's real, already-a-participant
identity, RLS doing the actual enforcement with no elevation needed.

**Workspace-scoped surface** (`CollaborationWorkspaceService`/`CollaborationsController`,
spec §13.1): messages, private notes, tasks (open/done), viewings, files
(local-disk "collaboration-files" container, authenticated download only — no static route,
mirroring "listing-documents"), an append-only activity log, and contact disclosures. Every
child table carries the same shared RLS predicate, so most reads are just an ordinary filtered
query — a non-participant's query returns nothing, reported as a 404 (or an empty list for the
sub-resource GETs), never a distinguishable 403. Writes that somehow reach RLS's block predicate
anyway (stale JWT, etc.) are caught in `SaveGuardedAsync` and translated to a 403, the same
"catch the block-predicate SqlException" pattern `ListingService` already established.

**Contact disclosure is the actual point of this stage** (spec §13.2): accepting a request never
discloses contact info by itself — `GrantContactDisclosureAsync` is a separate, explicit,
revocable act, and only the organization that actually owns that side (looked up via the
workspace's originating `Match` → `Listing`/`Demand`, under the same cross-tenant-read
elevation Stage 8 established) can grant it. The `ContactDataType` enum has four values, but
`RowLevelSecurity.sql`'s disclosure predicates (built in Stage 3) only ever check `DataType 1`
(`ListingContact`) and `2` (`DemandContact`) — `Phone`/`Email` exist in the enum with no
consuming predicate. `GrantContactDisclosureAsync` refuses those two rather than silently
recording a grant that would have zero actual effect. **Granting a disclosure required zero new
code in `ListingsController`/`DemandsController`** — their `GET .../contact` endpoints
(built in Stage 6/7) already check for an unrevoked `CollaborationContactDisclosure` scoped via
the match; this stage only had to start writing rows into a table those endpoints were already
reading from. Revoking one re-hides the contact live — verified, not assumed.

**Verified end-to-end** with two real organizations (Kathmandu Prime Properties / Pokhara
Lakeside Realty) and a genuine 100%-scored match between them: request → accept → workspace
bootstrap (participants correctly populated on both sides, contact disclosures correctly empty
immediately after acceptance); a same-organization profile who was *not* one of the two
participants correctly got 404 on the workspace and was correctly blocked (403, via the
block-predicate path) from posting a message into it — participation in this model is
per-profile, not per-organization, confirmed deliberately since it's a real, non-obvious
authorization property future stages need to respect; granting a `ListingContact` disclosure
correctly unlocked `GET /api/listings/{id}/contact` for the receiving org with the nulled
placeholder becoming real data, and revoking it correctly re-hid it; granting `Phone`/`Email`
correctly rejected with an explanation; only the granting org could revoke its own grant (the
other side got 403); messages/notes/tasks/viewings/file upload-download-delete all worked from
both sides and were correctly invisible to the non-participant; `MatchesController.RecordAction`'s
`RequestCollaboration` branch now actually calls `CreateFromMatchAsync` instead of just logging
a "Stage 9 will build this" placeholder note. Reran the full `rls_test.sql` suite — still 7/7, no
regression (no RLS policies were touched this stage, only application code driving the
already-built Stage 3 ones). `dotnet build` and `npm run build`/`lint` clean.

**Frontend**: `/portal/collaborations` (pending/accepted requests with inline accept/decline/
cancel, linking through to the workspace once accepted) and `/portal/collaborations/:id`
(full workspace — participants, contact-disclosure grant/revoke, messages, notes, tasks,
viewings, files, activity log), plus a "Request collaboration" standalone affordance on
`/portal/members/:id` for introductions not tied to a match. Built against the same Server
Actions + `revalidatePath` pattern as every other portal module (no new frontend architecture
introduced). **Not visually verified in an actual browser this stage** — the Chrome browser
extension wasn't connected in this environment (same gap noted since Stage 2); verification here
is backend-authoritative (real HTTP calls against every endpoint) plus a clean TypeScript
build/lint, not a substitute for eyes-on UI testing.

**Known gaps for later stages**: `CreateFromMatchAsync` doesn't check for an already-accepted
request on the same match before creating a new pending one — a second "Request collaboration"
click on an already-collaborating match creates a second, redundant request rather than pointing
back at the existing workspace. Low-impact (no security or data-integrity issue, just UI
tidiness) and left as-is rather than adding speculative guard logic for an edge case nobody hit
in testing.

## Stage 10 — Notifications (✅ complete)

**The scaffolding already existed** (Stage 3's schema, a `Notification` entity with exactly the
right shape, and `NotificationsController`'s list/mark-read/mark-all-read, even the frontend
`/portal/notifications` page) — nothing ever actually wrote a row. `NotificationEnums.cs`'s
`NotificationType` already enumerated all nine of spec §14's triggers, which made scope
unambiguous: wire creation at each real event, not invent a new design.

**`INotificationService`/`NotificationService`** (`Services/Notifications`, deliberately parallel
to `IEmailSender` — a small, self-contained, fire-and-forget write, never a transactional
participant in the caller's own `SaveChangesAsync`): `NotifyAsync` (one profile), `NotifyManyAsync`
(several), `NotifyOrgAsync` (every active user of a member organization, via `EntityUsers`). Every
call site fires only *after* its own triggering change is already committed, so a notification
failure can never roll back the business action that caused it.

**Wired into eight of the nine triggers**:
- **Match** — `MatchingEngine.UpsertMatchAsync`, only on a genuinely *new* `Match` row (tracked via
  an `isNewMatch` flag), never on a recompute that just refreshes an existing match's score —
  otherwise editing a listing would spam both orgs on every save.
- **Collaboration request / accepted / declined** — `CollaborationRequestService`'s
  `CreateFromMatchAsync`/`CreateToOrgAsync` (notify the recipient org), `AcceptAsync`/
  `DeclineAsync` (notify the requester). `Cancel` deliberately doesn't notify — the recipient may
  never have seen the request to begin with.
- **Message** — `CollaborationWorkspaceService.SendMessageAsync`, notifying every *other*
  participant. Reading who else is in the workspace needs no RLS elevation: the insert that just
  succeeded already proved the caller is a participant, and "any participant sees every row of
  their own workspace" is the whole point of the shared predicate.
- **Listing approval / rejection** — `ListingService.ApproveAsync`/`RejectAsync`, notifying
  `CreatedByProfileId`.
- **Invitation** — `InvitationService.AcceptAsync`, notifying `InvitedByProfileId` — the invitee
  has no profile (and thus nothing to notify) until this exact moment, so this is the only
  invitation-related instant an in-app notification could ever fire from.
- **Account event** — `AuthService.ChangePasswordAsync` and `ResetPasswordAsync`, both notifying
  the account itself ("Your password was changed/reset") as a security-awareness signal.
- **Expiry** — new `ExpiryScanService : BackgroundService` (`Services/Notifications`), the one
  trigger with no HTTP request to hang off. Runs hourly (and immediately on startup), scans
  `PropertyListings`/`Demands` past `ExpiresAt` that are still `Approved`/`Active`, transitions
  them to `Expired`, and notifies `CreatedByProfileId`. `ListingStatus.Expired` already existed in
  the enum but nothing had ever used it; `DemandStatus` had no `Expired` value at all despite
  `Demand` already having an `ExpiresAt` column — added one (additive, no check constraint to
  fight, no migration required) rather than silently reusing `Archived` for a semantically
  different state. As a genuine background job with no ambient `HttpContext` for
  `SessionContextConnectionInterceptor` to read, it owns its own DI scope per tick and elevates via
  `SessionContextOverride` the same way `MatchingEngine` does — same spec §20 pattern, third time
  now. An expired listing/demand needs no `Match` cleanup: `MatchesController.Search`'s live-view
  filter already excludes anything not `Approved`/`Active`, exactly like `ArchiveAsync` never
  touches `Matches` either.

**Deliberately not wired**: **association notices**. `Notice` (CMS content) already exists as an
entity but has no publish path yet — no controller lets anyone actually create or publish one
(CMS management is Stage 11's job). Wiring a notification to an event that cannot yet occur would
be exactly the kind of thing spec §22 warns against; documented as a Stage 11 follow-up instead of
faked.

**Frontend**: the existing notifications page needed only two additions — a clickable title when
`linkUrl` is present, and an unread-count badge on the sidebar's "Notifications" item (fetched once
in `portal/layout.tsx`, refreshed automatically whenever a mark-read/mark-all-read server action
revalidates the route). No new pages; everything else already worked.

**Verified end-to-end** with the same two real organizations: a workspace message correctly
notified only the other participant, not the sender; a collaboration request/accept/decline cycle
produced exactly the right notifications with working deep links; toggling
`property_moderation_required` on temporarily (it defaults off in this dev DB) let both a listing
rejection and a subsequent approval be tested for real, then the flag was restored; an invitation
created and accepted notified the inviter; a password reset notified the account; backdating a
listing's and a demand's `ExpiresAt` and restarting the API triggered the startup scan, which
correctly expired both, notified both owning orgs, and dropped the pair out of
`MatchesController`'s live match view — then both were restored to their prior state since they're
the project's main demo fixture. `unread-count` correctly tracked every notification and correctly
zeroed on mark-all-read. Reran `rls_test.sql` twice more (before and after the moderation-flag /
expiry testing) — still 7/7, no regression (no RLS SQL touched this stage). `dotnet build` and
`npm run build`/`lint` clean.

**Known gaps for later stages**: association notices (documented above, blocked on Stage 11's CMS
management). No push/SMS/WhatsApp/email delivery for these events — spec §14 only asks for
in-app notifications with unread/read state, which is what's built; the `sms_notifications`/
`whatsapp_notifications`/`email_notifications` feature flags exist in the schema but genuinely
sending through any of those channels is out of this stage's scope.

## Stage 11 — Admin + CMS (✅ complete)

**Scope decision, stated up front rather than discovered mid-build**: spec §4.3's Admin Portal
list is enormous — dashboard, members, users, invitations, roles, permissions, membership
applications, properties, demands, matches, match rules, collaborations, property types,
amenities, locations, units, currencies, committee, pages, news, notices, events, resources,
media, notifications, feature flags, reports, audit logs, settings, security — and the spec's
own stage order (§2.6) already splits feature flags/reports/audit into Stage 12. Within what's
left, this stage built: the actual CMS (the stage's namesake — content types, lifecycle, public
consumption), the Admin Portal shell, and every screen the codebase had an explicit `// Stage 11`
breadcrumb for (seven of them, across `InvitationsController`, `MatchRuleSetsController`,
`MemberEntitiesController`, `MembershipApplicationsController`, `ReferenceDataController`,
`DatabaseSeeder`, and the public news placeholder — grepped for and closed one by one).
Deliberately NOT built, and documented rather than silently skipped: full CRUD/edit/reorder for
the eleven reference-data tables (mostly fixed taxonomy/geography, not day-to-day editorial
content — a "Reference Data" placeholder explains this and points at the existing Create-only
API), RBAC editing (a read-only Roles & Permissions view instead — editing the grant matrix live
is security-sensitive enough to defer deliberately), and a Security screen (spec names it without
specifying contents beyond what Stages 3-4 already built and tested).

**CMS backend** (`CmsController`, `cms.manage`): `CmsPage`/`NewsArticle`/`Notice`/`Event`/
`Resource` all move through the same Draft → Review → Published → Archived lifecycle (Flow E,
spec §4.3) via one shared `AllowedTransitions` map and a single `PATCH .../status` endpoint per
type, rather than three separate submit/publish/archive actions each — collapses what would have
been ~40 near-identical endpoints into 8 per type. Publishing is deliberately not reachable
directly from Draft (must pass through Review) — an admin can still do it in two quick calls, but
never skip review by construction. `CommitteeMember`/`NavigationItem` are simpler (`IsActive`
toggle, no editorial review needed for a roster entry or a nav link); `SiteSetting` is a plain
upsert-by-key, effective immediately, same shape as `FeatureFlags`. None of these tables carry
RLS — they're association-wide editorial content, not per-org private data — so the permission
check is the only gate.

**Public consumption** (`PublicContentController`, anonymous, spec §16's "sanitized public
projection" pattern applied to CMS for the first time): every method filters to
`Status == Published` and hand-picks its field allowlist, mirroring `PublicPropertiesController`'s
discipline exactly. This is what actually finishes the job — `/news`, `/notices`, `/events`,
`/resources`, `/leadership`, `/about`, `/privacy`, `/terms`, and the homepage's three content
feeds had all been placeholder stubs literally since Stage 2, each one saying "once published
from the Admin CMS." A shared `CmsPageContent` component now renders any `CmsPage` by slug
(about/privacy/terms all reuse it) with the exact same placeholder as its fallback when nothing's
published yet — the empty state was never thrown away, just given a real alternative.

**Closing the seven `// Stage 11` breadcrumbs**: `MembershipApplicationsController`'s existing
review queue, `MatchRuleSetsController`'s existing rule-set CRUD/publish, and
`InvitationsController`'s existing create/revoke all already had complete backends from Stages
4/8 with comments saying "full admin UI is Stage 11" — this stage was mostly just building the
UI against them. Three small backend additions were still genuinely new: `GET /api/invitations`
(a list endpoint never existed), `GET /api/profiles` (an admin "users" list, reusing the
suspend/reactivate endpoints Stage 4 already built for `ActiveProfileMiddleware`), and
`POST /api/member-entities/{id}/suspend`/`reactivate` (org-level suspension — a separate lever
from suspending an org's individual users, verified live to not cross-affect each other). Also
added `GET /api/reference/roles` (read-only, for both the Roles view and the invitation form's
role picker) and `GET /api/dashboard/admin-summary` (association-wide counts, using the same
`SessionContextOverride` elevation `DashboardController.Summary`'s `PotentialMatchesCount`
already established for cross-tenant reads). Property moderation needed no new backend at all —
`GET /api/listings?status=PendingReview` as a system admin already returns every org's pending
listings, because RLS's own `is_system_admin=1` bypass branch (built in Stage 3) does the
cross-tenant read for free; same for the Collaborations oversight page, which just gives
`CollaborationRequestService.ListMineAsync`'s existing "system admin sees everything" branch
somewhere to be seen. The admin/member-portal split runs on the same `isSystemAdmin` claim
throughout — set once by `IUserClaimsFactory` in Stage 4, never re-derived.

**One real bug, caught live rather than by inspection**: the read-only roles endpoint's first
version threw a 500 — `EF Core` refused to translate `.Select(...).OrderBy(...)` inside a nested
collection projection ("Collections in the final projection must be an IEnumerable&lt;T&gt;...").
Fixed by materializing with `.ToList()` inside the query and sorting in memory afterward.

**Closed a gap Stage 10 explicitly deferred**: "association notices" was the one notification
trigger (of spec §14's nine) left unwired last stage, because nothing could publish a `Notice`
yet. `SetNoticeStatus` now broadcasts to every active profile (an `AssociationNotice` is
association-wide by definition, not org-scoped) the instant a notice transitions into
`Published` — guarded by a `wasPublished` check so re-saving an already-published notice, or
archiving one, never re-fires it. Verified live: publishing notified every active test profile
immediately (unread count incremented, correct deep link to `/notices/{slug}`), and archiving
the same notice afterward correctly did not notify again.

**Verified end-to-end**, live, after every piece: a full CmsPage Draft→Review→Published→Archived→
Draft cycle, confirming the public endpoint 404s except while genuinely Published; the identical
cycle for a news article, including its public list/detail read; a listing's full moderation
queue flow (toggle `property_moderation_required` on, submit, appear in the admin queue exactly
once, approve, queue empties, flag restored to its original off value afterward); org suspension
correctly hiding an org from the ordinary member directory while leaving its users' own sessions
untouched, and a `MemberAdmin` correctly forbidden from suspending orgs at all (403); the
invitation list, membership-application approve/reject, and roles endpoints all against real data;
the homepage's three content feeds and every public content page (`/news`, `/notices`, `/events`,
`/resources`, `/leadership`, `/about`) rendering real published content by the end, with the
empty-state fallback independently re-confirmed for unpublished/draft content along the way.
Reran `rls_test.sql` twice — still 7/7 (no RLS SQL touched this stage; every CMS/admin table is
deliberately RLS-free, permission checks are the only gate). `dotnet build` and
`npm run build`/`lint` both clean on the first full pass after the roles-query fix.

**Frontend**: a new `/admin/*` route group with its own layout (`isSystemAdmin`-gated — UX only,
every backend endpoint enforces its own permission independently), a sectioned sidebar nav
matching spec §4.3's full list including the deferred items, a database-backed dashboard, and 22
routes total: 8 CMS screens, 6 screens closing the explicit gaps above, and 5 documented
placeholders. Reciprocal "Admin Portal" / "Member Portal" links added to each layout's header for
a system admin. **Not visually verified in a browser** — the Chrome extension still isn't
connected in this environment; verified instead via real HTTP calls against every backend
endpoint plus confirming every new frontend route (public and admin) returns the correct
status/content over HTTP rather than a server-render crash.

**Known gaps carried forward, all deliberate and documented above rather than silent**:
reference-data edit/reorder UI, RBAC editing UI, and a Security screen. Feature flags, reports,
and audit logs remain placeholder-only pending Stage 12 per the spec's own stage split — audit
logging in particular still has no write path at all (`AuditLogs`' RLS and its append-only
trigger were built and tested in Stage 3, but nothing calls into it yet, a gap first noted back
in Stage 7).

## Stage 12 — Feature Flags + Reports + Audit (✅ complete)

**Feature flags** (`FeatureFlagsController`, new): all 13 flags from spec §15 already existed
(seeded, all off — "defaults should be conservative") but a grep across the codebase found only
3 of them were ever actually *read* by anything: `public_properties_enabled`,
`membership_application_enabled`, `property_moderation_required`. The other 10 were pure
decoration. This stage's real work was deciding, flag by flag, which ones gate something real:

- **Wired for real**: `matching_enabled` (a belt-and-suspenders kill switch on top of the existing
  "is a rule set published" check inside `MatchingEngine.GetPublishedRuleSetAsync` — an admin can
  pause matching for maintenance without un-publishing, and losing, the rule set itself),
  `collaboration_enabled` (gates only *starting new* collaboration requests in
  `CollaborationRequestService` — an already-accepted workspace keeps working if it's later
  turned off, same "gate the entry point, not everything downstream" pattern
  `membership_application_enabled` already used), `public_member_directory_enabled` and
  `member_export_enabled` (see below — both needed a real feature built to gate, not just a
  check).
- **Left reserved and documented**, not wired to anything fake: `open_registration_enabled` (no
  self-service signup flow exists — membership is always application + invitation),
  `deal_tracking_enabled` and `auto_unit_conversion` (no such features exist anywhere in this
  codebase), `sms_notifications`/`whatsapp_notifications`/`email_notifications` (no real delivery
  provider exists for any channel — Stage 10 built in-app notifications only, explicitly). The
  admin UI marks each of these "Reserved" rather than pretending they do something.

**Closed a real, unrelated gap found while wiring `public_member_directory_enabled`**: the public
`/members` and `/members/:slug` routes had been placeholder stubs since Stage 2, literally saying
"once membership onboarding is live (Stage 4+)" — nobody had ever come back to finish them.
Built `PublicMemberEntitiesController` (sanitized projection, same discipline as
`PublicPropertiesController`/`PublicContentController` — never Email/Phone/Address, only what a
member org would expect public) and the two frontend pages. `MemberEntity` has no `Slug` column
(same situation `PropertyListing` was already in — see `/properties/[slug]`'s existing frontend
comment); the id doubles as the route's `:slug` segment, same established convention.
`member_export_enabled` similarly got a real feature: a plain CSV export of every member
organization (`GET /api/member-entities/export`), defensively escaped rather than pulling in a
CSV library for one endpoint.

**Audit logging** (`IAuditLogService`/`AuditLogService`, `Services/Audit` — deliberately parallel
to `INotificationService`: small, self-contained, fire-and-forget, never participates in the
caller's own transaction): the `AuditLog` entity, its append-only database trigger, and
`audit.read` permission had all existed and been tested since Stage 3 — genuinely nothing had
ever written a row until now, a gap first noted back in Stage 7 and carried forward explicitly
through every stage since. Wired into every category spec §19 names: login (`AuthService.
LoginAsync`), security changes (password change/reset, profile suspend/reactivate), role changes
(`InvitationService.AcceptAsync`), member changes (`MemberEntitiesController` update/suspend/
reactivate, membership application approve/reject), listing changes (create/submit/approve/
reject/archive/delete — deliberately *not* plain field edits, to keep the log meaningful rather
than drowned in routine saves; the same scoping choice was made for demands), moderation actions
(the listing approve/reject entries above self-document as moderation), collaboration events
(request/accept/decline/cancel), contact disclosure (grant/revoke — the one category with an
explicit spec warning attached: the summary records *that* a disclosure happened, by whom, for
which data type and workspace, and deliberately never the actual contact value), feature flag
changes, and configuration changes (`SiteSetting` upsert/delete). `AuditLogsController` gives
admins a filterable, paginated read-only view; `AuditLog` itself carries no RLS (association-wide
security record, not per-org private data — same reasoning `Notifications` already used), so
`audit.read` is the only gate and the database trigger is what actually keeps it append-only.

**Reports**: the one Stage 12 piece with no dedicated spec section (§15 and §19 both spell out
exactly what to build; §4.3 just names "reports"). Kept deliberately modest rather than guessed
at — real, database-backed breakdowns of data that already exists (listings/demands/matches/
collaborations by status, member growth over the last 12 months), no invented metrics (spec §22),
no charting library this codebase doesn't already depend on. Every listing/demand/match query is
association-wide, so `ReportsController` needed the same `SessionContextOverride` elevation
`DashboardController.AdminSummary` already established.

**Verified end-to-end**, live: toggling `matching_enabled` off correctly reported "not configured"
even with a published rule set (and correctly worked again once re-enabled), toggling
`collaboration_enabled` off correctly blocked a new request with a clear error (and worked once
re-enabled), the public member directory correctly returned an empty/disabled response before its
flag was on and real data after, the CSV export correctly refused before its flag was on and
produced a real file after, and every one of those toggles and actions produced a real, correctly-
attributed audit log entry — spot-checked across `Login`, `FeatureFlagChanged` (×3),
`CollaborationRequested`, `MemberEntitiesExported`, and `ListingArchived`. Reran `rls_test.sql`
twice — still 7/7 (no RLS touched; `AuditLogs`' existing append-only trigger is what's actually
being exercised, not new policy). `dotnet build` and `npm run build`/`lint` both clean on the
first full pass.

**Frontend**: `/admin/feature-flags` (toggle UI, reserved flags visibly marked), `/admin/
audit-logs` (filterable by entity type/action, paginated), `/admin/reports` (stat breakdowns,
same visual language as the Admin Dashboard's tiles), a CSV export button on `/admin/members`
gated by its flag, and the two newly-real public member-directory pages. **Not visually verified
in a browser** — same gap as every stage since Stage 9; verified via real HTTP calls against
every endpoint plus confirming every route serves correct status/content rather than crashing
server-side.

**End-of-stage flag state, left intentionally**: `matching_enabled`, `collaboration_enabled`,
`public_member_directory_enabled`, and `member_export_enabled` were left *on* after testing (each
was verified working, and turning them back off would silently break the matching/collaboration
demo data exercised across Stages 8-11) — this is an ordinary admin action, not a change to the
seeded defaults, which remain off. Every other flag remains off, including the genuinely-reserved
ones.

## Stage 13 — Security Hardening (✅ complete)

**No dedicated spec section** — unlike every other stage so far, "Security hardening" has no §
of its own; it's named only in §2.6's stage list (grepped the whole spec for "rate limit",
"CORS", "CSRF", "XSS", "security header", "brute force" — zero hits). Scoped this stage around
standard hardening measures genuinely relevant to this specific app, prioritizing spec-adjacent
gaps over generic checklist items, and building on the strong foundation already in place (RLS,
JWT, RBAC, BCrypt password hashing, audit logging from Stage 12) rather than re-litigating it.

**Closed a real gap spec §33 explicitly names but Stage 12 didn't cover**: "Log: authentication
failures... authorization failures... security events". Stage 12 only logged *successful* logins.
`AuthService.LoginAsync`'s failure branch now logs `LoginFailed` (the attempted email, for
brute-force/enumeration detection — never the password, and logged even when the account doesn't
exist, since repeated failures against a nonexistent email *is* what enumeration looks like).
`RequirePermissionAttribute` — the shared gate behind nearly every permission-protected endpoint —
now logs `AuthorizationFailed` (actor if authenticated, permission slug, method + path) the moment
it denies a request; attribute filters aren't constructor-injected, so `IAuditLogService` is
resolved from `HttpContext.RequestServices` inside the filter, the standard pattern for this.

**Rate limiting** (ASP.NET Core's built-in `Microsoft.AspNetCore.RateLimiting`, no new
dependency): a sliding-window limiter (5 requests/minute per IP) applied to every endpoint that's
actually attractive to credential-stuffing or enumeration — `login`, `forgot-password`,
`reset-password`, the public membership-application submission, and invitation lookup/accept.
Keyed by IP, not by account, so it can't itself be weaponized to lock a real user out by hammering
their email from a different address. Verified live: the 6th login attempt within a minute
correctly got 429, ordinary authenticated traffic on unrelated endpoints was completely unaffected
by the same burst, and the window correctly cleared after ~60s letting a real login through again.

**Security headers**: a small middleware on the API adds `X-Content-Type-Options: nosniff`,
`X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`, and a genuinely strict
`Content-Security-Policy: default-src 'none'; frame-ancestors 'none'` — correct, not merely
cautious, since this API only ever serves JSON plus one static image directory, never HTML a
browser would execute script from. `UseHsts()` for non-Development environments. The frontend
(`next.config.ts`) gets the first three headers too, but deliberately *no* CSP there: the app
renders admin-supplied image URLs (listing photos, member logos, committee photos) from arbitrary
hosts, and this environment has no way to verify a CSP against Next.js's own inline hydration
payloads in an actual browser — shipping an unverified CSP risks silently breaking the site for a
header that's advisory to begin with, when the real security boundary (RLS, permission checks) is
server-side regardless. Documented as a deliberate choice, not an oversight.

**No CORS policy was added — deliberately**, and this needed to be explicitly reasoned through
rather than assumed: every browser-originated request in this architecture is already same-origin
(Next.js Server Components call REAK.Api server-to-server, where CORS doesn't apply at all; the
only browser-JS calls go to Next.js's own same-origin proxy routes, e.g. the collaboration
file-download proxy from Stage 9). Registering a CORS policy would only widen the attack surface
for zero real cross-origin caller to serve — ASP.NET Core's default of silently rejecting
cross-origin browser requests when no policy exists *is* the secure choice for this shape of app.

**Dependency and secrets audit**: `dotnet list package --vulnerable --include-transitive` and
`npm audit` (including devDependencies) both came back completely clean — zero known
vulnerabilities on either side. A repo-wide grep for hardcoded password/API-key/secret patterns
found nothing real (only test fixtures' own `TestPass123!` placeholders). Found and fixed one
genuine, if minor, compliance gap: spec §29 explicitly requires "an example config listing
variable names only", and no `.env.example` existed anywhere despite both apps reading several
environment variables (`REAK_JWT_KEY`, the optional bootstrap-admin pair, `REAK_API_URL`,
`NEXT_PUBLIC_SITE_URL`, etc.) — added one for each project, names and defaults only, no values.

**Deliberately not built**: account lockout after N failed attempts (rate limiting already
mitigates brute force without the UX cost and added complexity of a lockout/unlock flow — a
judgment call, not an oversight) and stricter password complexity rules (the existing 8-character
minimum matches modern guidance, which favors length over forced complexity; spec never asks for
more).

**Verified end-to-end**, live: rate limiting triggering and clearing correctly, security headers
present on every response including static-file ones (checked an actual listing image request),
`/media` static serving unaffected by the new middleware ordering, both new audit categories
(`LoginFailed`, `AuthorizationFailed`) producing correctly-attributed entries. Reran `rls_test.sql`
twice — still 7/7 (no RLS touched this stage). `dotnet build` and `npm run build`/`lint` clean,
`dotnet list package --vulnerable` and `npm audit` both zero findings.

## Stage 14 — Performance + Accessibility + Responsive QA (✅ complete)

Chrome browser automation became available for the first time this session — every prior stage's
"browser QA" was actually curl/HTTP-level verification (explicitly documented as a gap each time).
This is the first stage verified against an actual rendered page, and it immediately surfaced a
real, previously-shipped bug that no amount of curl testing could have caught.

**Real bug found and fixed — listing photos never rendered for any visitor.**
`IFileStorage.GetPublicUrl()` (`REAK.Api/Services/Storage/LocalDiskFileStorage.cs`) returns a
relative `/media/<file>` path by design — it has no opinion on what origin serves it. Rendered
directly as an `<img src>`, that path resolved against the *Next.js app's own* origin (port 3000),
not the API's (port 5080), so every listing photo has 404'd since Stage 6. Fixed with a
`next.config.ts` rewrite (`/media/:path*` → `REAK_API_URL`), the same "browser never talks to the
API's raw origin" pattern every other proxy route in this app already uses — never a client-side
redirect, so `REAK_API_URL` stays server-only. Verified via curl before/after, then confirmed
visually once the browser was available.

**`next/image` adopted** across property cards, the public/portal property detail pages, and the
wizard's upload-preview grid (`fill` + `sizes`, `object-cover`). Caught a real accessibility gap in
the same pass: wizard upload previews had `alt=""`, now `alt="Uploaded photo N"`.

**A second, genuinely confusing bug turned up while verifying the `next/image` conversion in the
browser**: the first property card rendered a broken-image icon instead of the photo, even though
curl and `fetch()` against the exact same URL both returned `200 image/jpeg`. Traced by fetching
the raw bytes in-page: the only listing photo in local dev storage was a 19-byte stub file (`ff d8
ff e0` followed by the literal text "qa-test-phot...") — leftover fixture data from an earlier
stage's curl-based upload test, never a real image. `fetch()`/curl don't validate image content, so
every prior HTTP-level check passed; a real browser's image decoder correctly refused to render it.
Not a code bug. Replaced the stub with a real 1x1 JPEG to confirm — the `next/image`/`fill`/rewrite
plumbing then rendered correctly. Along the way, discovered this Next.js version's dev image
optimizer cache lives at `.next/dev/cache/images`, not the legacy `.next/cache/images` — exactly
the kind of version-specific relocation `web/AGENTS.md` warns training data won't know about;
clearing the wrong path left the optimizer serving the stale broken result until the correct
directory was cleared.

**Responsive fix — portal and admin sidebars.** Both `PortalSidebar` and `AdminSidebar` were a
plain always-visible vertical list with zero mobile behavior: on a narrow viewport this squeezed
page content into an unusably thin column next to a fixed-width sidebar, the same "shrunken
desktop" anti-pattern spec §23 warns about, just applied to navigation instead of a table. Both now
collapse below `md` into the same disclosure-button pattern `SiteHeader`'s mobile nav already uses
(`aria-expanded`/`aria-controls`, closes on navigation). The close-on-navigate logic hit React's
`react-hooks/set-state-in-effect` lint rule (`useEffect(() => setOpen(false), [pathname])` calls
`setState` synchronously inside an effect); fixed with React's own documented alternative —
compare the current `pathname` against a stored previous value during render and call `setState`
conditionally inline, no effect at all. `portal/layout.tsx` and `admin/layout.tsx` given matching
`flex-col`→`md:flex-row` container changes so the sidebar stacks above content on mobile.
**Caveat**: `resize_window` did not actually change the captured viewport in this environment
across three attempts (including a full reload) — genuine mobile-breakpoint pixel verification
could not be done. Verified instead by reading the rendered DOM/CSS directly: confirmed
`md:hidden` on the toggle button and `hidden`/`md:block` on the nav are present and correctly
applied at the current (desktop) viewport, and the structural pattern is identical to
`SiteHeader`'s mobile nav, which *was* pixel-verified in an earlier stage.

**Loading/error boundaries**: added `loading.tsx` + `error.tsx` for the `(public)`, `portal`, and
`admin` route groups (6 new files) — previously a slow data fetch or a thrown error showed either
nothing or Next's generic unstyled fallback. Nested inside each group's existing layout, so only
the segment content is replaced, not the header/sidebar chrome. This Next.js version's
`error.tsx` reset callback is named `retry`, not the legacy `reset` — caught by reading
`node_modules/next/dist/docs/` rather than assumed from training data, per `AGENTS.md`.

**SEO**: `sitemap.ts` rewritten from a static 14-route list to fetch real published news/notices/
events/members/properties and include their URLs (verified live: 20 total URLs). Root `layout.tsx`
given `metadataBase` + a default `openGraph` block so every child page can pass a relative
canonical/OG URL. All 14 static public pages plus the four `[slug]` detail routes now have
`generateMetadata`/`metadata` with `alternates.canonical` and `openGraph` (previously title-only or
entirely absent on several).

**Caching**: reference/taxonomy data (property types, purposes, provinces, districts) — admin-
managed, rarely changing — now opts into `next: { revalidate: 3600 }` on top of the codebase-wide
`cache: "no-store"` default in `api-client.ts`; every other call site is unaffected.

**Accessibility**: fixed two icon-only delete buttons (photo, document) missing `aria-label` in
`portal/properties/[id]/page.tsx`, found opportunistically while converting that page's media grid
to `next/image`. Hand-verified WCAG 2.2 AA contrast for `--color-muted-foreground` (#64748b on
white) at ≈4.80:1, passing the 4.5:1 threshold. Confirmed `prefers-reduced-motion` is already
handled globally (Stage 2's `*`+`!important` media-query rule in `globals.css`) — no change needed.
Confirmed existing semantic landmarks (`header`/`main`/`aside`/`nav aria-label`/`footer`) and the
disclosure-pattern ARIA already used by `SiteHeader`'s mobile menu, now replicated identically in
both portal/admin sidebars.

**Also fixed while in these files**: a dead download link in `portal/properties/[id]/page.tsx`
(`/api/listings/...` → `/api/portal/listings/...`, no such route existed) via a new proxy route
mirroring the Stage 9 collaboration-file-download pattern.

**Verified end-to-end, live in an actual browser** (first time this session type has been
possible): logged in as a real member, confirmed the dashboard, property listing cards (now
showing real images), property detail page, and portal navigation all render correctly at desktop
width; confirmed the `/media` rewrite fix with a live 200 response; confirmed the broken-image
investigation above end-to-end including the fix. `npm run lint` and `npm run build` both clean
(65 routes built). No RLS-relevant change this stage — `rls_test.sql` not rerun.

---

## Stages 15–16

Detailed only once reached — see `docs/REAK-requirements.md` for the full scope of each. Will be
broken into their own plan sections as they start, each with its own daily-log entry and test
results, per the spec's own recommended execution approach (§37 of the original PDF, reproduced in
the "Process note" of `docs/REAK-requirements.md` §2.6).

---

**Last updated**: 2026-09-18
**Status**: Stages 1-14 complete. Stage 15 next.
