import { apiGet } from "../api-client";
import type { PropertyTypeWithSubtypes, NamedRef } from "./types";

/** Server-side reference-data reads for Server Components (search filter dropdowns). The
 * client-side wizard instead uses the /api/reference/* same-origin proxy — see that route's
 * header comment for why the two paths differ. */
export const referenceApi = {
  propertyTypes: () => apiGet<PropertyTypeWithSubtypes[]>("/api/reference/property-types"),
  purposes: () => apiGet<NamedRef[]>("/api/reference/purposes"),
  provinces: () => apiGet<NamedRef[]>("/api/reference/provinces"),
  districts: (provinceId: string) => apiGet<NamedRef[]>(`/api/reference/districts?provinceId=${provinceId}`),
};
