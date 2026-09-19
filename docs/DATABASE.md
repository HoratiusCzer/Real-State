# Database

SQL Server, EF Core 10 code-first migrations, native row-level security (RLS). 61 application
tables (plus `__EFMigrationsHistory`), verified live against the dev database on 2026-09-18.

## Migrations

All schema changes go through EF Core migrations — never a manual/undocumented change (spec §30).
The core schema (identity, RBAC, location hierarchy, listings, demands, matching, collaboration,
notifications, CMS, feature flags, audit log) was designed and migrated comprehensively in Stage 3
("Database + migrations + RLS + RBAC") rather than incrementally per feature stage — a deliberate
upfront-schema choice, not a gap; genuinely new schema needs that emerged in later stages got their
own dedicated migrations:

| Migration | Adds |
|---|---|
| `20260916120020_InitialCreate` | Full application schema (identity, RBAC, locations, listings, demands, matching, collaboration, notifications, CMS, feature flags, audit log) |
| `20260916120636_AddRowLevelSecurity` | RLS predicate/block functions and security policies (see below) |
| `20260916121450_AddDatabaseQualityCheckConstraints` | `CHECK` constraints (spec §21 — database quality standards) |
| `20260916140205_AddAuthenticationSessions` | `RefreshTokens`, `PasswordResetTokens` |
| `20260916151152_AddReferenceCodeSequences` | SQL `SEQUENCE` objects backing human-readable reference codes (`RK-L-2026-000056` etc.) |
| `20260916152410_AddSavedListings` | `SavedListings` (member bookmarks) |

Apply with `dotnet ef database update` from `REAK.Api/`. The RLS SQL
(`Data/Security/RowLevelSecurity.sql`) is applied separately — see below — because SQL Server
security policies/predicate functions aren't something EF Core's migration DSL expresses directly;
it's checked into the same `Data/Security/` folder and is exactly as version-controlled as any
`.cs` migration.

## Row-level security

Enabled on 13 tables: `PropertyListings`, `Demands`, `ListingContacts`, `DemandContacts`,
`CollaborationWorkspaces`, and 8 more `Collaboration*` child tables (Activities,
ContactDisclosures, Files, Messages, Notes, Participants, Tasks, Viewings). Enforced via **9**
inline table-valued predicate functions in the `Security` schema
(`Data/Security/RowLevelSecurity.sql`):

- `fn_ListingReadPredicate` / `fn_ListingWritePredicate` — owner org, `AllMembers`/
  `SelectedMembers` network visibility, or system admin.
- `fn_DemandReadPredicate` / `fn_DemandWritePredicate` — same shape, for demands.
- `fn_ListingContactReadPredicate` / `fn_ListingContactWritePredicate` — owner org or system admin
  only; collaboration disclosure is enforced in the application layer (contact rows never leak via
  RLS to a collaborator — see `docs/SECURITY.md`).
- `fn_DemandContactReadPredicate` / `fn_DemandContactWritePredicate` — same, for demand contacts.
- `fn_CollaborationParticipantPredicate` — used as both the read and write filter on the
  workspace and every one of its 8 child tables: only the two participant organizations (or a
  system admin) may see a workspace exists at all.

Every predicate reads two pieces of `SESSION_CONTEXT`: `app.profile_id` and
`app.is_system_admin`, both stamped onto the ambient connection by
`SessionContextConnectionInterceptor` from the authenticated caller's JWT claims on every
connection-open — RLS is enforced at the database layer regardless of what application code does
above it. `Data/Security/RowLevelSecurity.Down.sql` reverts it. Re-verify after any RLS-relevant
change with `Data/Security/rls_test.sql` — see `docs/TESTING.md`.

## Indexes & constraints

186 indexes across application tables (primary keys plus explicit indexes on foreign keys and the
columns actual query patterns filter/sort by — member entity, status, network visibility,
reference code, etc.). `CHECK` constraints enforce enum-range and business invariants at the
database layer, not just in C# validation attributes (spec §21).

## RBAC data

4 roles seeded: `SuperAdmin`, `AssociationAdmin` (System-scoped — grant `IsSystemAdmin`),
`MemberAdmin`, `MemberStaff` (Organization-scoped). 18 permissions, granted to roles via
`RolePermissions`; a profile's effective permission set is the union of its roles' permissions,
computed fresh per request — see `docs/AUTHENTICATION.md`. The Roles/Permissions matrix itself has
**no** API mutation endpoint — it's read-only over HTTP by design (the admin Roles & Permissions
screen says so on its own page); changed only by editing `REAK.Api/Data/DatabaseSeeder.cs` and
re-seeding.

## Nepal location hierarchy

`Provinces` → `Districts` → `Municipalities` → `Wards` → `Localities` (optional), all
admin-reference tables consumed by both listings and demands for location matching.
