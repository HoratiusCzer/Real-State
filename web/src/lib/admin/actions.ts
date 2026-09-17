"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { getAccessToken } from "@/lib/auth/session";
import {
  cmsPagesApi, cmsNewsApi, cmsNoticesApi, cmsEventsApi, cmsResourcesApi, cmsCommitteeApi, cmsNavigationApi, cmsSettingsApi,
  adminMembershipApplicationsApi, adminInvitationsApi, adminMemberEntitiesApi, adminProfilesApi, adminMatchRuleSetsApi,
  featureFlagsApi,
} from "./api";

async function requireAccessToken(): Promise<string> {
  const token = await getAccessToken();
  if (!token) throw new Error("Not authenticated.");
  return token;
}

function fieldsFrom(formData: FormData, ...names: string[]) {
  const out: Record<string, string> = {};
  for (const name of names) {
    const value = String(formData.get(name) ?? "").trim();
    if (value) out[name] = value;
  }
  return out;
}

// ---- CMS: Pages ----
export async function createPageAction(formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "slug", "title", "body", "seoTitle", "seoDescription", "ogImageUrl");
  const result = await cmsPagesApi.create(accessToken, { slug: f.slug, title: f.title, ...f });
  revalidatePath("/admin/cms/pages");
  if (result.ok) redirect(`/admin/cms/pages/${result.data.id}`);
}

export async function updatePageAction(id: string, formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "slug", "title", "body", "seoTitle", "seoDescription", "ogImageUrl");
  await cmsPagesApi.update(accessToken, id, { slug: f.slug, title: f.title, ...f });
  revalidatePath(`/admin/cms/pages/${id}`);
  revalidatePath("/admin/cms/pages");
}

export async function setPageStatusAction(id: string, status: number) {
  const accessToken = await requireAccessToken();
  await cmsPagesApi.setStatus(accessToken, id, status);
  revalidatePath(`/admin/cms/pages/${id}`);
  revalidatePath("/admin/cms/pages");
}

export async function deletePageAction(id: string) {
  const accessToken = await requireAccessToken();
  await cmsPagesApi.remove(accessToken, id);
  revalidatePath("/admin/cms/pages");
  redirect("/admin/cms/pages");
}

// ---- CMS: News ----
export async function createNewsAction(formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "slug", "title", "summary", "body");
  const result = await cmsNewsApi.create(accessToken, { slug: f.slug, title: f.title, ...f });
  revalidatePath("/admin/cms/news");
  if (result.ok) redirect(`/admin/cms/news/${result.data.id}`);
}

export async function updateNewsAction(id: string, formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "slug", "title", "summary", "body");
  await cmsNewsApi.update(accessToken, id, { slug: f.slug, title: f.title, ...f });
  revalidatePath(`/admin/cms/news/${id}`);
  revalidatePath("/admin/cms/news");
}

export async function setNewsStatusAction(id: string, status: number) {
  const accessToken = await requireAccessToken();
  await cmsNewsApi.setStatus(accessToken, id, status);
  revalidatePath(`/admin/cms/news/${id}`);
  revalidatePath("/admin/cms/news");
}

export async function deleteNewsAction(id: string) {
  const accessToken = await requireAccessToken();
  await cmsNewsApi.remove(accessToken, id);
  revalidatePath("/admin/cms/news");
  redirect("/admin/cms/news");
}

// ---- CMS: Notices ----
export async function createNoticeAction(formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "slug", "title", "body");
  const result = await cmsNoticesApi.create(accessToken, { slug: f.slug, title: f.title, ...f });
  revalidatePath("/admin/cms/notices");
  if (result.ok) redirect(`/admin/cms/notices/${result.data.id}`);
}

export async function updateNoticeAction(id: string, formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "slug", "title", "body");
  await cmsNoticesApi.update(accessToken, id, { slug: f.slug, title: f.title, ...f });
  revalidatePath(`/admin/cms/notices/${id}`);
  revalidatePath("/admin/cms/notices");
}

export async function setNoticeStatusAction(id: string, status: number) {
  const accessToken = await requireAccessToken();
  await cmsNoticesApi.setStatus(accessToken, id, status);
  revalidatePath(`/admin/cms/notices/${id}`);
  revalidatePath("/admin/cms/notices");
}

export async function deleteNoticeAction(id: string) {
  const accessToken = await requireAccessToken();
  await cmsNoticesApi.remove(accessToken, id);
  revalidatePath("/admin/cms/notices");
  redirect("/admin/cms/notices");
}

// ---- CMS: Events ----
export async function createEventAction(formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "slug", "title", "body", "eventDate", "location");
  const result = await cmsEventsApi.create(accessToken, { slug: f.slug, title: f.title, ...f, eventDate: f.eventDate ? new Date(f.eventDate).toISOString() : undefined });
  revalidatePath("/admin/cms/events");
  if (result.ok) redirect(`/admin/cms/events/${result.data.id}`);
}

export async function updateEventAction(id: string, formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "slug", "title", "body", "eventDate", "location");
  await cmsEventsApi.update(accessToken, id, { slug: f.slug, title: f.title, ...f, eventDate: f.eventDate ? new Date(f.eventDate).toISOString() : undefined });
  revalidatePath(`/admin/cms/events/${id}`);
  revalidatePath("/admin/cms/events");
}

export async function setEventStatusAction(id: string, status: number) {
  const accessToken = await requireAccessToken();
  await cmsEventsApi.setStatus(accessToken, id, status);
  revalidatePath(`/admin/cms/events/${id}`);
  revalidatePath("/admin/cms/events");
}

export async function deleteEventAction(id: string) {
  const accessToken = await requireAccessToken();
  await cmsEventsApi.remove(accessToken, id);
  revalidatePath("/admin/cms/events");
  redirect("/admin/cms/events");
}

// ---- CMS: Resources ----
export async function createResourceAction(formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "title", "description", "linkUrl");
  await cmsResourcesApi.create(accessToken, { title: f.title, ...f });
  revalidatePath("/admin/cms/resources");
}

export async function setResourceStatusAction(id: string, status: number) {
  const accessToken = await requireAccessToken();
  await cmsResourcesApi.setStatus(accessToken, id, status);
  revalidatePath("/admin/cms/resources");
}

export async function deleteResourceAction(id: string) {
  const accessToken = await requireAccessToken();
  await cmsResourcesApi.remove(accessToken, id);
  revalidatePath("/admin/cms/resources");
}

// ---- CMS: Committee ----
export async function createCommitteeMemberAction(formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "name", "title", "photoUrl", "sortOrder");
  await cmsCommitteeApi.create(accessToken, { name: f.name, title: f.title, photoUrl: f.photoUrl, sortOrder: Number(f.sortOrder ?? 0) });
  revalidatePath("/admin/cms/committee");
}

export async function toggleCommitteeMemberActiveAction(id: string) {
  const accessToken = await requireAccessToken();
  await cmsCommitteeApi.toggleActive(accessToken, id);
  revalidatePath("/admin/cms/committee");
}

export async function deleteCommitteeMemberAction(id: string) {
  const accessToken = await requireAccessToken();
  await cmsCommitteeApi.remove(accessToken, id);
  revalidatePath("/admin/cms/committee");
}

// ---- CMS: Navigation ----
export async function createNavigationItemAction(formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "label", "url", "parentId", "sortOrder");
  await cmsNavigationApi.create(accessToken, { label: f.label, url: f.url, parentId: f.parentId, sortOrder: Number(f.sortOrder ?? 0) });
  revalidatePath("/admin/cms/navigation");
}

export async function toggleNavigationItemActiveAction(id: string) {
  const accessToken = await requireAccessToken();
  await cmsNavigationApi.toggleActive(accessToken, id);
  revalidatePath("/admin/cms/navigation");
}

export async function deleteNavigationItemAction(id: string) {
  const accessToken = await requireAccessToken();
  await cmsNavigationApi.remove(accessToken, id);
  revalidatePath("/admin/cms/navigation");
}

// ---- CMS: Site settings ----
export async function upsertSiteSettingAction(formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "key", "value", "description");
  await cmsSettingsApi.upsert(accessToken, { key: f.key, ...f });
  revalidatePath("/admin/cms/settings");
}

export async function deleteSiteSettingAction(id: string) {
  const accessToken = await requireAccessToken();
  await cmsSettingsApi.remove(accessToken, id);
  revalidatePath("/admin/cms/settings");
}

// ---- Membership applications ----
export async function approveMembershipApplicationAction(id: string) {
  const accessToken = await requireAccessToken();
  await adminMembershipApplicationsApi.approve(accessToken, id);
  revalidatePath("/admin/membership-applications");
}

export async function rejectMembershipApplicationAction(id: string, formData: FormData) {
  const accessToken = await requireAccessToken();
  const reason = String(formData.get("reason") ?? "").trim();
  await adminMembershipApplicationsApi.reject(accessToken, id, reason);
  revalidatePath("/admin/membership-applications");
}

// ---- Invitations ----
export async function createInvitationAction(formData: FormData) {
  const accessToken = await requireAccessToken();
  const email = String(formData.get("email") ?? "").trim();
  const memberEntityId = String(formData.get("memberEntityId") ?? "").trim();
  const roleId = String(formData.get("roleId") ?? "").trim();
  await adminInvitationsApi.create(accessToken, { email, roleId, memberEntityId: memberEntityId || undefined });
  revalidatePath("/admin/invitations");
}

export async function revokeInvitationAction(id: string) {
  const accessToken = await requireAccessToken();
  await adminInvitationsApi.revoke(accessToken, id);
  revalidatePath("/admin/invitations");
}

// ---- Member entities ----
export async function suspendMemberEntityAction(id: string) {
  const accessToken = await requireAccessToken();
  await adminMemberEntitiesApi.suspend(accessToken, id);
  revalidatePath("/admin/members");
}

export async function reactivateMemberEntityAction(id: string) {
  const accessToken = await requireAccessToken();
  await adminMemberEntitiesApi.reactivate(accessToken, id);
  revalidatePath("/admin/members");
}

// ---- Profiles (users) ----
export async function suspendProfileAction(id: string) {
  const accessToken = await requireAccessToken();
  await adminProfilesApi.suspend(accessToken, id);
  revalidatePath("/admin/users");
}

export async function reactivateProfileAction(id: string) {
  const accessToken = await requireAccessToken();
  await adminProfilesApi.reactivate(accessToken, id);
  revalidatePath("/admin/users");
}

// ---- Match rule sets ----
export async function createMatchRuleSetAction(formData: FormData) {
  const accessToken = await requireAccessToken();
  const f = fieldsFrom(formData, "name", "description");
  const result = await adminMatchRuleSetsApi.create(accessToken, { name: f.name, description: f.description });
  revalidatePath("/admin/match-rules");
  if (result.ok) redirect(`/admin/match-rules/${result.data.id}`);
}

export async function addMatchRuleAction(id: string, formData: FormData) {
  const accessToken = await requireAccessToken();
  const criterion = Number(formData.get("criterion"));
  const weight = Number(formData.get("weight"));
  const isRequired = formData.get("isRequired") === "on";
  const toleranceRaw = String(formData.get("toleranceValue") ?? "").trim();
  const sortOrder = Number(formData.get("sortOrder") ?? 0);
  await adminMatchRuleSetsApi.addRule(accessToken, id, { criterion, weight, isRequired, toleranceValue: toleranceRaw ? Number(toleranceRaw) : undefined, sortOrder });
  revalidatePath(`/admin/match-rules/${id}`);
}

export async function removeMatchRuleAction(id: string, ruleId: string) {
  const accessToken = await requireAccessToken();
  await adminMatchRuleSetsApi.removeRule(accessToken, id, ruleId);
  revalidatePath(`/admin/match-rules/${id}`);
}

export async function publishMatchRuleSetAction(id: string) {
  const accessToken = await requireAccessToken();
  await adminMatchRuleSetsApi.publish(accessToken, id);
  revalidatePath(`/admin/match-rules/${id}`);
  revalidatePath("/admin/match-rules");
}

export async function archiveMatchRuleSetAction(id: string) {
  const accessToken = await requireAccessToken();
  await adminMatchRuleSetsApi.archive(accessToken, id);
  revalidatePath(`/admin/match-rules/${id}`);
  revalidatePath("/admin/match-rules");
}

// ---- Feature flags ----
export async function setFeatureFlagAction(key: string, isEnabled: boolean) {
  const accessToken = await requireAccessToken();
  await featureFlagsApi.setEnabled(accessToken, key, isEnabled);
  revalidatePath("/admin/feature-flags");
}
