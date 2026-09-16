/**
 * Placeholder feature flags until Stage 3 (database) and Stage 12 (feature flags admin UI)
 * exist. Values are hardcoded to the spec's own conservative defaults (§15) and must be
 * replaced with a real read from the `feature_flags` table once the backend exists — do not
 * wire real data behind this file without also removing this notice.
 */
export const featureFlags = {
  publicPropertiesEnabled: false,
  publicMemberDirectoryEnabled: false,
} as const;
