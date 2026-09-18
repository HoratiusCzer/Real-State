import { apiGet } from "../api-client";
import type { PropertyTypeWithSubtypes, NamedRef } from "./types";

// Property types/purposes/locations are admin-managed and change rarely (Stage 11 left their
// management API-only, precisely because they're not day-to-day editorial content) — a 1-hour
// cache (spec §25 "caching where appropriate") means every search-filter render and wizard step
// doesn't re-hit the database for data that's virtually always identical to the last request. An
// admin's edit still shows up within the hour; nothing here is security- or freshness-sensitive.
const REFERENCE_DATA_REVALIDATE_SECONDS = 3600;

/** Server-side reference-data reads for Server Components (search filter dropdowns). The
 * client-side wizard instead uses the /api/reference/* same-origin proxy — see that route's
 * header comment for why the two paths differ. */
export const referenceApi = {
  propertyTypes: () => apiGet<PropertyTypeWithSubtypes[]>("/api/reference/property-types", undefined, REFERENCE_DATA_REVALIDATE_SECONDS),
  purposes: () => apiGet<NamedRef[]>("/api/reference/purposes", undefined, REFERENCE_DATA_REVALIDATE_SECONDS),
  provinces: () => apiGet<NamedRef[]>("/api/reference/provinces", undefined, REFERENCE_DATA_REVALIDATE_SECONDS),
  districts: (provinceId: string) => apiGet<NamedRef[]>(`/api/reference/districts?provinceId=${provinceId}`, undefined, REFERENCE_DATA_REVALIDATE_SECONDS),
};
