import { apiGet, apiPost, apiPut, apiDelete } from "../api-client";
import type {
  ListingSearchResult, ListingDetail, ListingContact,
  CreateListingInput, UpdateListingInput,
  PublicPropertySearchResult, PublicPropertyDetail,
} from "./types";

export type ListingSearchParams = Record<string, string | number | boolean | undefined>;

function toQueryString(params: ListingSearchParams): string {
  const usp = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") usp.set(key, String(value));
  }
  const qs = usp.toString();
  return qs ? `?${qs}` : "";
}

export const listingsApi = {
  search: (accessToken: string, params: ListingSearchParams) =>
    apiGet<ListingSearchResult>(`/api/listings${toQueryString(params)}`, accessToken),
  get: (accessToken: string, id: string) => apiGet<ListingDetail>(`/api/listings/${id}`, accessToken),
  create: (accessToken: string, input: CreateListingInput) => apiPost<{ id: string }>("/api/listings", input, accessToken),
  update: (accessToken: string, id: string, input: UpdateListingInput) => apiPut<void>(`/api/listings/${id}`, input, accessToken),
  submit: (accessToken: string, id: string) => apiPost<void>(`/api/listings/${id}/submit`, undefined, accessToken),
  approve: (accessToken: string, id: string) => apiPost<void>(`/api/listings/${id}/approve`, undefined, accessToken),
  reject: (accessToken: string, id: string, reason: string) => apiPost<void>(`/api/listings/${id}/reject`, { reason }, accessToken),
  archive: (accessToken: string, id: string) => apiPost<void>(`/api/listings/${id}/archive`, undefined, accessToken),
  remove: (accessToken: string, id: string) => apiDelete<void>(`/api/listings/${id}`, accessToken),
  replaceAmenities: (accessToken: string, id: string, amenityIds: string[]) =>
    apiPut<void>(`/api/listings/${id}/amenities`, { amenityIds }, accessToken),
  updateVisibility: (
    accessToken: string,
    id: string,
    input: { networkVisibility: number; isPublicVisible: boolean; selectedMemberEntityIds?: string[] }
  ) => apiPut<void>(`/api/listings/${id}/visibility`, input, accessToken),
  getContact: (accessToken: string, id: string) => apiGet<ListingContact>(`/api/listings/${id}/contact`, accessToken),
  updateContact: (accessToken: string, id: string, contact: ListingContact) =>
    apiPut<void>(`/api/listings/${id}/contact`, contact, accessToken),
  save: (accessToken: string, id: string) => apiPost<void>(`/api/listings/${id}/save`, undefined, accessToken),
  unsave: (accessToken: string, id: string) => apiDelete<void>(`/api/listings/${id}/save`, accessToken),
  listSaved: (accessToken: string) =>
    apiGet<
      {
        listingId: string;
        createdAt: string;
        listing: { id: string; referenceCode: string; title: string; price: number; currencyCode: string; status: string };
      }[]
    >("/api/listings/saved", accessToken),
  deleteMedia: (accessToken: string, id: string, mediaId: string) =>
    apiDelete<void>(`/api/listings/${id}/media/${mediaId}`, accessToken),
  deleteDocument: (accessToken: string, id: string, documentId: string) =>
    apiDelete<void>(`/api/listings/${id}/documents/${documentId}`, accessToken),
};

export const publicPropertiesApi = {
  search: (params: ListingSearchParams) => apiGet<PublicPropertySearchResult>(`/api/public/properties${toQueryString(params)}`),
  get: (id: string) => apiGet<PublicPropertyDetail>(`/api/public/properties/${id}`),
};
