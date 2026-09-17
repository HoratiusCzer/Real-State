import { apiGet, apiPost, apiPut, apiPatch, apiDelete } from "../api-client";
import type {
  PageItem, NewsItem, NoticeItem, EventItem, ResourceItem, CommitteeMemberItem, NavigationItemItem, SiteSettingItem,
  AdminDashboardSummary, MembershipApplication, AdminInvitation, AdminMemberEntity, AdminProfile, AdminRole,
} from "./types";
import type { MatchRuleSet } from "../matching/types";

export const adminDashboardApi = {
  summary: (accessToken: string) => apiGet<AdminDashboardSummary>("/api/dashboard/admin-summary", accessToken),
};

export const cmsPagesApi = {
  list: (accessToken: string) => apiGet<PageItem[]>("/api/cms/pages", accessToken),
  get: (accessToken: string, id: string) => apiGet<PageItem>(`/api/cms/pages/${id}`, accessToken),
  create: (accessToken: string, input: { slug: string; title: string; body?: string; seoTitle?: string; seoDescription?: string; ogImageUrl?: string }) =>
    apiPost<PageItem>("/api/cms/pages", input, accessToken),
  update: (accessToken: string, id: string, input: { slug: string; title: string; body?: string; seoTitle?: string; seoDescription?: string; ogImageUrl?: string }) =>
    apiPut<PageItem>(`/api/cms/pages/${id}`, input, accessToken),
  setStatus: (accessToken: string, id: string, status: number) => apiPatch<void>(`/api/cms/pages/${id}/status`, { status }, accessToken),
  remove: (accessToken: string, id: string) => apiDelete<void>(`/api/cms/pages/${id}`, accessToken),
};

export const cmsNewsApi = {
  list: (accessToken: string) => apiGet<NewsItem[]>("/api/cms/news", accessToken),
  get: (accessToken: string, id: string) => apiGet<NewsItem>(`/api/cms/news/${id}`, accessToken),
  create: (accessToken: string, input: { slug: string; title: string; summary?: string; body?: string }) => apiPost<NewsItem>("/api/cms/news", input, accessToken),
  update: (accessToken: string, id: string, input: { slug: string; title: string; summary?: string; body?: string }) => apiPut<NewsItem>(`/api/cms/news/${id}`, input, accessToken),
  setStatus: (accessToken: string, id: string, status: number) => apiPatch<void>(`/api/cms/news/${id}/status`, { status }, accessToken),
  remove: (accessToken: string, id: string) => apiDelete<void>(`/api/cms/news/${id}`, accessToken),
};

export const cmsNoticesApi = {
  list: (accessToken: string) => apiGet<NoticeItem[]>("/api/cms/notices", accessToken),
  get: (accessToken: string, id: string) => apiGet<NoticeItem>(`/api/cms/notices/${id}`, accessToken),
  create: (accessToken: string, input: { slug: string; title: string; body?: string }) => apiPost<NoticeItem>("/api/cms/notices", input, accessToken),
  update: (accessToken: string, id: string, input: { slug: string; title: string; body?: string }) => apiPut<NoticeItem>(`/api/cms/notices/${id}`, input, accessToken),
  setStatus: (accessToken: string, id: string, status: number) => apiPatch<void>(`/api/cms/notices/${id}/status`, { status }, accessToken),
  remove: (accessToken: string, id: string) => apiDelete<void>(`/api/cms/notices/${id}`, accessToken),
};

export const cmsEventsApi = {
  list: (accessToken: string) => apiGet<EventItem[]>("/api/cms/events", accessToken),
  get: (accessToken: string, id: string) => apiGet<EventItem>(`/api/cms/events/${id}`, accessToken),
  create: (accessToken: string, input: { slug: string; title: string; body?: string; eventDate?: string; location?: string }) => apiPost<EventItem>("/api/cms/events", input, accessToken),
  update: (accessToken: string, id: string, input: { slug: string; title: string; body?: string; eventDate?: string; location?: string }) => apiPut<EventItem>(`/api/cms/events/${id}`, input, accessToken),
  setStatus: (accessToken: string, id: string, status: number) => apiPatch<void>(`/api/cms/events/${id}/status`, { status }, accessToken),
  remove: (accessToken: string, id: string) => apiDelete<void>(`/api/cms/events/${id}`, accessToken),
};

export const cmsResourcesApi = {
  list: (accessToken: string) => apiGet<ResourceItem[]>("/api/cms/resources", accessToken),
  create: (accessToken: string, input: { title: string; description?: string; linkUrl?: string }) => apiPost<ResourceItem>("/api/cms/resources", input, accessToken),
  update: (accessToken: string, id: string, input: { title: string; description?: string; linkUrl?: string }) => apiPut<ResourceItem>(`/api/cms/resources/${id}`, input, accessToken),
  setStatus: (accessToken: string, id: string, status: number) => apiPatch<void>(`/api/cms/resources/${id}/status`, { status }, accessToken),
  remove: (accessToken: string, id: string) => apiDelete<void>(`/api/cms/resources/${id}`, accessToken),
};

export const cmsCommitteeApi = {
  list: (accessToken: string) => apiGet<CommitteeMemberItem[]>("/api/cms/committee", accessToken),
  create: (accessToken: string, input: { name: string; title: string; photoUrl?: string; sortOrder: number }) => apiPost<CommitteeMemberItem>("/api/cms/committee", input, accessToken),
  update: (accessToken: string, id: string, input: { name: string; title: string; photoUrl?: string; sortOrder: number }) => apiPut<CommitteeMemberItem>(`/api/cms/committee/${id}`, input, accessToken),
  toggleActive: (accessToken: string, id: string) => apiPost<CommitteeMemberItem>(`/api/cms/committee/${id}/toggle-active`, undefined, accessToken),
  remove: (accessToken: string, id: string) => apiDelete<void>(`/api/cms/committee/${id}`, accessToken),
};

export const cmsNavigationApi = {
  list: (accessToken: string) => apiGet<NavigationItemItem[]>("/api/cms/navigation", accessToken),
  create: (accessToken: string, input: { label: string; url: string; parentId?: string; sortOrder: number }) => apiPost<NavigationItemItem>("/api/cms/navigation", input, accessToken),
  toggleActive: (accessToken: string, id: string) => apiPost<NavigationItemItem>(`/api/cms/navigation/${id}/toggle-active`, undefined, accessToken),
  remove: (accessToken: string, id: string) => apiDelete<void>(`/api/cms/navigation/${id}`, accessToken),
};

export const cmsSettingsApi = {
  list: (accessToken: string) => apiGet<SiteSettingItem[]>("/api/cms/settings", accessToken),
  upsert: (accessToken: string, input: { key: string; value?: string; description?: string }) => apiPut<SiteSettingItem>("/api/cms/settings", input, accessToken),
  remove: (accessToken: string, id: string) => apiDelete<void>(`/api/cms/settings/${id}`, accessToken),
};

export const adminMembershipApplicationsApi = {
  list: (accessToken: string) => apiGet<MembershipApplication[]>("/api/membership-applications", accessToken),
  approve: (accessToken: string, id: string) => apiPost<void>(`/api/membership-applications/${id}/approve`, undefined, accessToken),
  reject: (accessToken: string, id: string, reason: string) => apiPost<void>(`/api/membership-applications/${id}/reject`, { reason }, accessToken),
};

export const adminInvitationsApi = {
  list: (accessToken: string) => apiGet<AdminInvitation[]>("/api/invitations", accessToken),
  create: (accessToken: string, input: { email: string; memberEntityId?: string; roleId: string }) => apiPost<{ id: string }>("/api/invitations", input, accessToken),
  revoke: (accessToken: string, id: string) => apiPost<void>(`/api/invitations/${id}/revoke`, undefined, accessToken),
};

export const adminMemberEntitiesApi = {
  list: (accessToken: string) => apiGet<AdminMemberEntity[]>("/api/member-entities?includeInactive=true", accessToken),
  suspend: (accessToken: string, id: string) => apiPost<void>(`/api/member-entities/${id}/suspend`, undefined, accessToken),
  reactivate: (accessToken: string, id: string) => apiPost<void>(`/api/member-entities/${id}/reactivate`, undefined, accessToken),
};

export const adminProfilesApi = {
  list: (accessToken: string, search?: string) => apiGet<AdminProfile[]>(`/api/profiles${search ? `?search=${encodeURIComponent(search)}` : ""}`, accessToken),
  suspend: (accessToken: string, id: string) => apiPost<void>(`/api/profiles/${id}/suspend`, undefined, accessToken),
  reactivate: (accessToken: string, id: string) => apiPost<void>(`/api/profiles/${id}/reactivate`, undefined, accessToken),
};

export const adminRolesApi = {
  list: (accessToken: string) => apiGet<AdminRole[]>("/api/reference/roles", accessToken),
};

export const adminMatchRuleSetsApi = {
  list: (accessToken: string) => apiGet<MatchRuleSet[]>("/api/match-rule-sets", accessToken),
  get: (accessToken: string, id: string) => apiGet<MatchRuleSet>(`/api/match-rule-sets/${id}`, accessToken),
  create: (accessToken: string, input: { name: string; description?: string }) => apiPost<MatchRuleSet>("/api/match-rule-sets", input, accessToken),
  addRule: (accessToken: string, id: string, input: { criterion: number; weight: number; isRequired: boolean; toleranceValue?: number; sortOrder: number }) =>
    apiPost<void>(`/api/match-rule-sets/${id}/rules`, input, accessToken),
  removeRule: (accessToken: string, id: string, ruleId: string) => apiDelete<void>(`/api/match-rule-sets/${id}/rules/${ruleId}`, accessToken),
  publish: (accessToken: string, id: string) => apiPost<void>(`/api/match-rule-sets/${id}/publish`, undefined, accessToken),
  archive: (accessToken: string, id: string) => apiPost<void>(`/api/match-rule-sets/${id}/archive`, undefined, accessToken),
};
