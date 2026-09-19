# Security

Priority order this project follows when trade-offs arise (spec §35): **Security → Correctness →
Data integrity → Real functionality → Maintainability → Accessibility → Performance → Visual
quality.**

## Tenant isolation

Enforced twice, independently:
1. **Application layer** — every service method authorizes against `CallerContext`
   (`ProfileId` + every `MemberEntityId` the caller belongs to + `IsSystemAdmin`).
2. **Database layer** — RLS security policies on 13 tables (`docs/DATABASE.md`), enforced
   regardless of what application code above it does.

Verified in Stage 15's security-test pass: cross-org listing edit/delete → 403; cross-org demand
modification → 404; accessing a collaboration workspace without being a participant → 404.

## PII isolation

Listing/demand contact details (`ListingContacts`, `DemandContacts`) are separate tables from the
listing/demand itself, with their own RLS predicates restricting reads to the owner org or system
admin — never the wider `NetworkVisibility` audience a listing/demand itself is visible to. A
collaborator only gains access to a specific contact field through an explicit
`POST /api/collaborations/{id}/contact-disclosures` call (`ContactDataType`: `ListingContact` /
`DemandContact` / `Phone` / `Email`) — collaboration participation alone never discloses contact
data. Verified live end-to-end in Stage 15: contact fields return `null` to a non-owner,
non-disclosed collaborator; correct values only after an explicit disclosure call.

## Storage security

Uploaded files (`REAK.Api/App_Data/storage/`) are served through a controlled `/media` path with
no directory listing; download endpoints (`GET .../documents/{id}/download`,
`.../collaborations/{id}/files/{id}/download`) apply the same ownership/participant/RLS checks as
the record they belong to — a private document isn't reachable just because someone knows or
guesses its URL. Verified: a non-collaborator gets 403 on a private document download.

## Public data security

Public-facing endpoints (`/api/public/*`) never expose a private contact, an internal note, or a
non-approved/non-public-visible record — each is gated by both `IsPublicVisible` on the record
*and* its own feature flag (`public_properties_enabled`, `public_member_directory_enabled`),
checked server-side, not just hidden in the UI. Verified live: `POST` to a feature-gated endpoint
with its flag off returns a real 400, not a UI-only restriction.

## Audit logging

`AuditLogs`, written by `IAuditLogService`, covers: login success/failure, authorization
failures, membership application submit/approve/reject, listing/demand submit/approve/reject,
collaboration request/accept/decline, invitation send/accept/revoke, member entity
suspend/reactivate, profile suspend/reactivate, feature flag changes, CMS content status changes.
Never logs passwords, access tokens, or raw private contact data.

## Rate limiting & headers

- Sliding-window rate limiting (5 req/min/IP) on every credential-stuffing/enumeration-attractive
  endpoint: login, forgot-password, reset-password, public membership-application submission,
  invitation lookup/accept.
- Response headers (both apps): `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`,
  `Referrer-Policy: strict-origin-when-cross-origin`. REAK.Api additionally sends a strict
  `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'` (correct, since the API
  only ever serves JSON plus static media, never HTML a browser would execute) and `UseHsts()`
  outside Development. The frontend deliberately has **no** CSP — it renders admin-supplied image
  URLs from arbitrary hosts, and an unverified CSP risked silently breaking image rendering for a
  header that's advisory to begin with; the real security boundary (RLS, permission checks) is
  server-side regardless. See `DEVELOPMENT_PLAN.md`'s Stage 13 section for the full reasoning.
- No CORS policy on REAK.Api, deliberately — every browser call in this architecture is already
  same-origin to Next.js (see `docs/ARCHITECTURE.md`); ASP.NET Core's default of rejecting
  cross-origin browser requests when no policy exists *is* the secure choice here.

## What is deliberately not built

- **Account lockout after N failed attempts** — rate limiting already mitigates brute force
  without the added complexity/UX cost of a lockout-unlock flow.
- **Stricter password complexity rules** — the existing 8-character minimum matches modern
  guidance (length over forced complexity); the spec never asks for more.
- **A frontend CSP** — see above; a considered trade-off, not an oversight.

## Known limitations

- No account-lockout mechanism (see above — accepted trade-off, not a gap).
- No email-sending integration (`email_notifications` feature flag is seeded off) — invitation and
  password-reset tokens are generated and stored correctly but must be delivered out-of-band in
  this build; there's no SMTP/provider wiring yet.
- No production penetration test has been run against this build; the security-test pass in
  `docs/TESTING.md` covers the adversarial scenarios spec §27 explicitly lists, not an exhaustive
  external audit.
