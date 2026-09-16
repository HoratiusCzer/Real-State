/**
 * Placeholder feature flags until Stage 12 (feature flags admin UI) exists. Values are
 * hardcoded to the spec's own conservative default (§15) and must be replaced with a real read
 * from the `feature_flags` table once that stage builds the general read path — do not wire
 * real data behind this file without also removing this notice.
 *
 * `publicPropertiesEnabled` no longer lives here — Stage 6 made it live via
 * `GET /api/public/properties`'s own `enabled` field, which the API checks directly against the
 * database on every request.
 */
export const featureFlags = {
  publicMemberDirectoryEnabled: false,
} as const;
