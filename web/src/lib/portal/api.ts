import { apiGet, apiPatch, apiPost, apiPut } from "../api-client";

export type DashboardSummary = {
  activePropertiesCount: number;
  draftPropertiesCount: number;
  activeRequirementsCount: number;
  potentialMatchesCount: number;
  pendingCollaborationRequestsCount: number;
  savedPropertiesCount: number | null;
  expiringItemsCount: number;
  recentProperties: { id: string; referenceCode: string; title: string; status: string; createdAt: string }[];
};

export type MemberEntitySummary = {
  id: string;
  name: string;
  description: string | null;
  website: string | null;
  logoUrl: string | null;
  createdAt: string;
};

export type MemberEntityDetail = MemberEntitySummary & {
  phone: string | null;
  email: string | null;
  address: string | null;
};

export type NotificationItem = {
  id: string;
  type: number;
  title: string;
  body: string | null;
  linkUrl: string | null;
  isRead: boolean;
  createdAt: string;
};

export const dashboardApi = {
  summary: (accessToken: string) => apiGet<DashboardSummary>("/api/dashboard/summary", accessToken),
};

export const memberEntitiesApi = {
  list: (accessToken: string) => apiGet<MemberEntitySummary[]>("/api/member-entities", accessToken),
  get: (accessToken: string, id: string) => apiGet<MemberEntityDetail>(`/api/member-entities/${id}`, accessToken),
  update: (
    accessToken: string,
    id: string,
    input: { name: string; description?: string; website?: string; phone?: string; email?: string; address?: string }
  ) => apiPut<void>(`/api/member-entities/${id}`, input, accessToken),
};

export const notificationsApi = {
  list: (accessToken: string) => apiGet<NotificationItem[]>("/api/notifications", accessToken),
  unreadCount: (accessToken: string) => apiGet<{ count: number }>("/api/notifications/unread-count", accessToken),
  markRead: (accessToken: string, id: string) => apiPost<void>(`/api/notifications/${id}/read`, undefined, accessToken),
  markAllRead: (accessToken: string) => apiPost<void>("/api/notifications/read-all", undefined, accessToken),
};

export const profileApi = {
  update: (accessToken: string, input: { fullName: string; phone?: string }) =>
    apiPatch<void>("/api/profiles/me", input, accessToken),
};

export type RoleSummary = { id: string; name: string; scope: string };

export const rolesApi = {
  list: (accessToken: string) => apiGet<RoleSummary[]>("/api/reference/roles", accessToken),
};

export const invitationsApi = {
  create: (accessToken: string, input: { email: string; memberEntityId: string; roleId: string }) =>
    apiPost<{ id: string }>("/api/invitations", input, accessToken),
};
