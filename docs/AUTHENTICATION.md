# Authentication & Authorization

## Account creation (Flow A)

1. Public submits a membership application (`POST /api/membership-applications`, gated by the
   `membership_application_enabled` feature flag, rate-limited) — no account created yet.
2. A system admin reviews and approves it (`POST /api/membership-applications/{id}/approve`) —
   this creates the `MemberEntity`.
3. A system admin (or an org's existing `MemberAdmin`) invites a person by email
   (`POST /api/invitations`) with a target `MemberEntityId` (or none, for a system-scoped
   invitation) and a `RoleId`. The invitation stores a random token, plaintext in the database
   (delivered out-of-band by email in production — there is no email-sending integration in this
   build; `email_notifications` is seeded off), expiring after 7 days.
4. The invitee opens `/invite/{token}` (`GET /api/invitations/{token}` to preview, `POST
   .../accept` with a chosen password to activate). A new `Profile` is created if one doesn't
   already exist for that email; the `EntityUsers`/`ProfileRoleAssignments` rows are created from
   the invitation's org/role.
5. Login (`POST /api/auth/login`) issues a JWT access token (15 min default,
   `Jwt__AccessTokenMinutes`) and a refresh token (30 days default, `Jwt__RefreshTokenDays`,
   stored hashed in `RefreshTokens`).

## Login & session

- Passwords hashed with BCrypt, never logged, never returned in any response.
- `POST /api/auth/login`, `/forgot-password`, `/reset-password`, and the two invitation-facing
  endpoints are rate-limited (5 requests/minute/IP, ASP.NET Core's built-in sliding-window
  limiter) — brute-force/enumeration mitigation, keyed by IP so it can't itself be weaponized to
  lock out a real user.
- A failed login logs a `LoginFailed` audit entry (the attempted email, never the password) —
  including against a nonexistent email, since repeated failures against one *is* what enumeration
  looks like.
- `POST /api/auth/refresh` rotates the refresh token (old one invalidated on use).
- Suspension is checked on **every** authenticated request, not just at login —
  `ActiveProfileMiddleware` re-checks `Profile.IsActive` per-request, so an already-issued access
  token stops working immediately once a profile is suspended, not just after it expires.
  Verified live in Stage 15 (existing token and fresh login both correctly rejected, 401,
  immediately after suspension).

## Authorization

- **JWT claims**: `sub` (profile id), `email`, `is_system_admin`, `perm` (the flattened permission
  slug list), `member_entity_id`(s). Built fresh at login by `UserClaimsFactory` from the current
  `ProfileRoleAssignments` — not cached, so a role change takes effect on the caller's *next*
  login/refresh, not retroactively on an already-issued token.
- **`[RequirePermission("slug")]`** — the attribute behind nearly every permission-protected
  controller action. Denies with 403 and logs an `AuthorizationFailed` audit entry (actor if
  authenticated, permission slug, method + path) the moment it denies — resolved via
  `HttpContext.RequestServices` since attribute filters aren't constructor-injected.
- **`IsSystemAdmin`** bypasses the Admin Portal's frontend route gate but *not* individual
  endpoint permission checks — every admin API endpoint independently checks its own permission
  (spec §2.5: the frontend gate is UX only, never the real security boundary).
- **RLS** is the data-layer backstop underneath all of this — see `docs/DATABASE.md` and
  `docs/SECURITY.md`. Authorization failing at the API layer and RLS filtering at the database
  layer are two independent, redundant controls, not one standing in for the other.

## Password reset

`POST /api/auth/forgot-password` issues a token (rate-limited, doesn't reveal whether the email
exists) stored in `PasswordResetTokens`; `POST /api/auth/reset-password` consumes it.
