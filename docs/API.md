# API Reference

REAK.Api, default base `http://localhost:5080`. All routes below are `/api/...`. Unless marked
`[AllowAnonymous]` below, every route requires a valid JWT (`Authorization: Bearer <token>`);
most additionally require a specific permission slug via `[RequirePermission("...")]` — see
`docs/AUTHENTICATION.md`. Enum-typed request fields (e.g. `NetworkVisibility`, `ContentStatus`)
must be sent as their **numeric** value — the API has no `JsonStringEnumConverter` configured;
response DTOs expose enums as pre-stringified `string` fields instead, which is why a GET response
and a PUT request body use different representations for the same concept.

## Auth — `api/auth`
`POST /login` (anon) · `POST /refresh` · `POST /logout` · `GET /me` · `POST /change-password` ·
`POST /forgot-password` (anon) · `POST /reset-password` (anon)

## Membership applications — `api/membership-applications`
`POST /` (anon, Flow A step 1) · `GET /` · `POST /{id}/approve` · `POST /{id}/reject`

## Invitations — `api/invitations`
`GET /` · `POST /` · `GET /{token}` (anon, preview) · `POST /{token}/accept` (anon) ·
`POST /{id}/revoke`

## Member entities — `api/member-entities`
`GET /` · `GET /{id}` · `PUT /{id}` · `POST /{id}/suspend` · `POST /{id}/reactivate` ·
`GET /export` (CSV, gated by `member_export_enabled`)

## Profiles — `api/profiles`
`GET /` · `PATCH /me` · `POST /{id}/suspend` · `POST /{id}/reactivate`

## Listings — `api/listings`
`GET /` · `GET /{id}` · `POST /` · `PUT /{id}` · `POST /{id}/submit` · `POST /{id}/approve` ·
`POST /{id}/reject` · `POST /{id}/archive` · `DELETE /{id}` · `PUT /{id}/amenities` ·
`PUT /{id}/visibility` · `GET /{id}/contact` · `PUT /{id}/contact` · `GET /saved` ·
`POST /{id}/save` · `DELETE /{id}/save` · `POST /{id}/media` ·
`DELETE /{id}/media/{mediaId}` · `POST /{id}/documents` ·
`GET /{id}/documents/{documentId}/download` · `DELETE /{id}/documents/{documentId}`

## Demands — `api/demands`
`GET /` · `GET /{id}` · `POST /` · `PUT /{id}` · `POST /{id}/publish` · `POST /{id}/fulfill` ·
`POST /{id}/archive` · `DELETE /{id}` · `PUT /{id}/property-types` · `PUT /{id}/locations` ·
`PUT /{id}/amenities` · `PUT /{id}/visibility` · `GET /{id}/contact` · `PUT /{id}/contact`

## Matching — `api/match-rule-sets`, `api/matches`
`GET /match-rule-sets` · `GET /{id}` · `POST /` · `POST /{id}/rules` ·
`DELETE /{id}/rules/{ruleId}` · `POST /{id}/publish` · `POST /{id}/archive`
`GET /matches` (supports `?listingId=` / `?demandId=`) · `GET /matches/{id}` (score + per-criterion
explanation) · `POST /matches/{id}/actions` (shortlist/dismiss/reopen)

## Collaboration — `api/collaboration-requests`, `api/collaborations`
`GET /collaboration-requests` · `POST /from-match` · `POST /to-org` · `POST /{id}/accept` ·
`POST /{id}/decline` · `POST /{id}/cancel`
`GET /collaborations/{id}` (workspace) · `GET|POST /{id}/messages` · `GET|POST /{id}/notes` ·
`GET|POST /{id}/tasks` · `PUT /{id}/tasks/{taskId}/status` · `GET|POST /{id}/viewings` ·
`GET|POST /{id}/files` · `GET /{id}/files/{fileId}/download` ·
`DELETE /{id}/files/{fileId}` · `GET /{id}/activities` ·
`POST /{id}/contact-disclosures` (`ContactDataType`: `ListingContact`=1/`DemandContact`=2/
`Phone`=3/`Email`=4) · `DELETE /{id}/contact-disclosures/{disclosureId}`

## Notifications — `api/notifications`
`GET /` · `GET /unread-count` · `POST /{id}/read` · `POST /read-all`

## Dashboard — `api/dashboard`
`GET /summary` (member) · `GET /admin-summary` (system admin only)

## Admin: CMS — `api/cms`
Full CRUD + `PATCH .../status` for `pages`, `news`, `notices`, `events`, `resources`; CRUD +
`toggle-active` for `committee` and `navigation`; `GET|PUT` and `DELETE {id}` for `settings`.
Status transitions are a workflow, not a free-form set: `Draft`(1) → `Review`(2) → `Published`(3) →
`Archived`(4); a direct `Draft` → `Published` PATCH is rejected — verified live in Stage 15.

## Admin: governance — `api/audit-logs`, `api/feature-flags`, `api/reports`
`GET /audit-logs` · `GET /audit-logs/entity-types`
`GET /feature-flags` · `PUT /feature-flags/{key}` (`{"isEnabled": bool}`)
`GET /reports/summary`

## Reference data — `api/reference`
Read for all: `roles`, `property-types`, `purposes`, `amenities`, `area-units`, `currencies`,
`provinces`, `districts`, `municipalities`, `wards`, `localities`. Admin-write (`POST`) for the
location hierarchy and taxonomy tables; `roles` has **no** write endpoint at all (see
`docs/DATABASE.md`).

## Public (anonymous, feature-flag gated) — `api/public/*`
`GET /public/properties`, `GET /public/properties/{id}` — gated by `public_properties_enabled`;
returns `{"enabled": false, "items": [], ...}` (not an error) when off.
`GET /public/members`, `GET /public/members/{id}` — gated by `public_member_directory_enabled`.
`GET /public/pages/{slug}`, `/public/news[/{slug}]`, `/public/notices[/{slug}]`,
`/public/events[/{slug}]`, `/public/resources`, `/public/committee`, `/public/navigation` —
published CMS content only; a `Draft`/unpublished slug correctly 404s.

## Frontend-side proxy routes (Next.js, not REAK.Api)

A handful of `web/src/app/api/...` Route Handlers exist purely to keep the browser same-origin:
`/media/:path*` (rewrite to REAK.Api's file storage), `/api/reference/[...path]` (cached
reference-data passthrough), collaboration file download, listing document download, portal
listing media. None of these are REAK.Api endpoints — they're thin server-side proxies in front
of the ones above.
