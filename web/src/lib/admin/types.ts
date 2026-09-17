// Mirrors REAK.Api.Models.Enums.ContentStatus
export const CONTENT_STATUS = { Draft: 1, Review: 2, Published: 3, Archived: 4 } as const;
export const CONTENT_STATUS_LABELS: Record<number, string> = { 1: "Draft", 2: "Review", 3: "Published", 4: "Archived" };

export type PageItem = {
  id: string; slug: string; title: string; body: string | null; status: string;
  seoTitle: string | null; seoDescription: string | null; ogImageUrl: string | null;
  publishedAt: string | null; createdAt: string; updatedAt: string | null;
};
export type NewsItem = {
  id: string; slug: string; title: string; summary: string | null; body: string | null; status: string;
  publishedAt: string | null; authorProfileName: string; createdAt: string; updatedAt: string | null;
};
export type NoticeItem = {
  id: string; slug: string; title: string; body: string | null; status: string;
  publishedAt: string | null; createdAt: string; updatedAt: string | null;
};
export type EventItem = {
  id: string; slug: string; title: string; body: string | null; eventDate: string | null; location: string | null;
  status: string; publishedAt: string | null; createdAt: string; updatedAt: string | null;
};
export type ResourceItem = { id: string; title: string; description: string | null; linkUrl: string | null; status: string; createdAt: string };
export type CommitteeMemberItem = { id: string; name: string; title: string; photoUrl: string | null; sortOrder: number; isActive: boolean };
export type NavigationItemItem = { id: string; label: string; url: string; parentId: string | null; sortOrder: number; isActive: boolean };
export type SiteSettingItem = { id: string; key: string; value: string | null; description: string | null; updatedAt: string };

export type AdminDashboardSummary = {
  pendingMembershipApplications: number;
  totalActiveMembers: number;
  totalActiveUsers: number;
  pendingInvitations: number;
  pendingModerationCount: number;
  pendingCollaborationRequests: number;
  cmsDraftCount: number;
  cmsPublishedCount: number;
};

// Mirrors REAK.Api.Models.Enums.MembershipApplicationStatus — serialized as a raw number since
// MembershipApplicationsController.List returns entities directly rather than a string-status DTO.
export const MEMBERSHIP_APPLICATION_STATUS_LABELS: Record<number, string> = { 1: "Pending", 2: "Approved", 3: "Rejected" };

export type MembershipApplication = {
  id: string; companyName: string; contactName: string; email: string; phone: string;
  message: string | null; status: number; reviewedByProfileId: string | null; reviewedAt: string | null;
  rejectionReason: string | null; createdAt: string;
};

export type AdminInvitation = {
  id: string; email: string; status: number; memberEntityId: string | null; memberEntityName: string | null;
  roleName: string; invitedByName: string; expiresAt: string; acceptedAt: string | null; createdAt: string;
};

export type AdminMemberEntity = { id: string; name: string; description: string | null; website: string | null; logoUrl: string | null; isActive: boolean; createdAt: string };

export type AdminProfile = {
  id: string; email: string; fullName: string; phone: string | null; isActive: boolean;
  lastLoginAt: string | null; createdAt: string; memberships: string[]; roles: string[];
};

export type AdminRole = { id: string; name: string; scope: string; permissions: string[] };
