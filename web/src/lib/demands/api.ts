import { apiGet, apiPost, apiPut, apiDelete } from "../api-client";
import type { DemandSearchResult, DemandDetail, DemandContact, CreateDemandInput, UpdateDemandInput, DemandLocationInput } from "./types";

export type DemandSearchParams = Record<string, string | number | boolean | undefined>;

function toQueryString(params: DemandSearchParams): string {
  const usp = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") usp.set(key, String(value));
  }
  const qs = usp.toString();
  return qs ? `?${qs}` : "";
}

export const demandsApi = {
  search: (accessToken: string, params: DemandSearchParams) =>
    apiGet<DemandSearchResult>(`/api/demands${toQueryString(params)}`, accessToken),
  get: (accessToken: string, id: string) => apiGet<DemandDetail>(`/api/demands/${id}`, accessToken),
  create: (accessToken: string, input: CreateDemandInput) => apiPost<{ id: string }>("/api/demands", input, accessToken),
  update: (accessToken: string, id: string, input: UpdateDemandInput) => apiPut<void>(`/api/demands/${id}`, input, accessToken),
  publish: (accessToken: string, id: string) => apiPost<void>(`/api/demands/${id}/publish`, undefined, accessToken),
  fulfill: (accessToken: string, id: string) => apiPost<void>(`/api/demands/${id}/fulfill`, undefined, accessToken),
  archive: (accessToken: string, id: string) => apiPost<void>(`/api/demands/${id}/archive`, undefined, accessToken),
  remove: (accessToken: string, id: string) => apiDelete<void>(`/api/demands/${id}`, accessToken),
  replacePropertyTypes: (accessToken: string, id: string, propertyTypeIds: string[]) =>
    apiPut<void>(`/api/demands/${id}/property-types`, { propertyTypeIds }, accessToken),
  replaceLocations: (accessToken: string, id: string, locations: DemandLocationInput[]) =>
    apiPut<void>(`/api/demands/${id}/locations`, { locations }, accessToken),
  replaceAmenities: (accessToken: string, id: string, amenityIds: string[]) =>
    apiPut<void>(`/api/demands/${id}/amenities`, { amenityIds }, accessToken),
  updateVisibility: (accessToken: string, id: string, input: { networkVisibility: number; selectedMemberEntityIds?: string[] }) =>
    apiPut<void>(`/api/demands/${id}/visibility`, input, accessToken),
  getContact: (accessToken: string, id: string) => apiGet<DemandContact>(`/api/demands/${id}/contact`, accessToken),
  updateContact: (accessToken: string, id: string, contact: DemandContact) =>
    apiPut<void>(`/api/demands/${id}/contact`, contact, accessToken),
};
