"use client";

import { useEffect, useState } from "react";
import type { PropertyTypeWithSubtypes, NamedRef, WardRef, AreaUnitRef, CurrencyRef } from "@/lib/listings/types";

/** Fetches through the same-origin /api/reference/* proxy (see that route's header comment) so
 * the browser never calls REAK.Api cross-origin. */
function useFetch<T>(path: string | null, initial: T): T {
  const [data, setData] = useState<T>(initial);
  useEffect(() => {
    let cancelled = false;
    if (!path) {
      Promise.resolve().then(() => { if (!cancelled) setData(initial); });
      return () => { cancelled = true; };
    }
    // no-store: this data is admin-editable at runtime (Admin Portal reference-data screens) —
    // without this, the browser's own HTTP cache would keep serving a stale cascading-dropdown
    // list after an admin adds/edits a district, municipality, etc., with no visible error, just
    // wrong options silently missing. The proxy route (app/api/reference/[...path]/route.ts) is
    // fixed the same way on its own fetch to REAK.Api.
    fetch(path, { cache: "no-store" })
      .then((r) => r.json())
      .then((d) => { if (!cancelled) setData(d); })
      .catch(() => { if (!cancelled) setData(initial); });
    return () => { cancelled = true; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [path]);
  return data;
}

export function usePropertyTypes() {
  return useFetch<PropertyTypeWithSubtypes[]>("/api/reference/property-types", []);
}
export function usePurposes() {
  return useFetch<NamedRef[]>("/api/reference/purposes", []);
}
export function useAmenities() {
  return useFetch<NamedRef[]>("/api/reference/amenities", []);
}
export function useAreaUnits() {
  return useFetch<AreaUnitRef[]>("/api/reference/area-units", []);
}
export function useCurrencies() {
  return useFetch<CurrencyRef[]>("/api/reference/currencies", []);
}
export function useProvinces() {
  return useFetch<NamedRef[]>("/api/reference/provinces", []);
}
export function useDistricts(provinceId: string) {
  return useFetch<NamedRef[]>(provinceId ? `/api/reference/districts?provinceId=${provinceId}` : null, []);
}
export function useMunicipalities(districtId: string) {
  return useFetch<NamedRef[]>(districtId ? `/api/reference/municipalities?districtId=${districtId}` : null, []);
}
export function useWards(municipalityId: string) {
  return useFetch<WardRef[]>(municipalityId ? `/api/reference/wards?municipalityId=${municipalityId}` : null, []);
}
export function useLocalities(wardId: string) {
  return useFetch<NamedRef[]>(wardId ? `/api/reference/localities?wardId=${wardId}` : null, []);
}
