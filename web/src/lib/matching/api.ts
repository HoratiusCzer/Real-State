import { apiGet, apiPost } from "../api-client";
import type { MatchSearchResponse, MatchDetail } from "./types";

export type MatchSearchParams = Record<string, string | number | undefined>;

function toQueryString(params: MatchSearchParams): string {
  const usp = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") usp.set(key, String(value));
  }
  const qs = usp.toString();
  return qs ? `?${qs}` : "";
}

export const matchesApi = {
  search: (accessToken: string, params: MatchSearchParams) =>
    apiGet<MatchSearchResponse>(`/api/matches${toQueryString(params)}`, accessToken),
  get: (accessToken: string, id: string) => apiGet<MatchDetail>(`/api/matches/${id}`, accessToken),
  recordAction: (accessToken: string, id: string, actionType: number, notes?: string) =>
    apiPost<{ recorded: boolean; collaborationNote: string | null }>(`/api/matches/${id}/actions`, { actionType, notes }, accessToken),
};
