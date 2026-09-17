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

export type FeatureFlag = { id: string; key: string; isEnabled: boolean; updatedAt: string };

// Mirrors REAK.Api.Data.DatabaseSeeder.FeatureFlagKeys (spec §15's 13 flags). "reserved" flags have
// no feature in this codebase to gate yet (documented in DEVELOPMENT_PLAN.md's Stage 12 section) —
// toggling them is harmless but currently has no visible effect.
export const FEATURE_FLAG_INFO: Record<string, { label: string; description: string; reserved?: boolean }> = {
  public_properties_enabled: { label: "Public Properties", description: "Shows the public property exchange at /properties." },
  public_member_directory_enabled: { label: "Public Member Directory", description: "Shows the public member directory at /members." },
  open_registration_enabled: { label: "Open Registration", description: "Reserved — REAK has no self-service signup flow; membership is always via application + invitation.", reserved: true },
  membership_application_enabled: { label: "Membership Applications", description: "Lets visitors submit a membership application at /membership/apply." },
  property_moderation_required: { label: "Property Moderation Required", description: "Requires admin approval before a submitted listing goes live." },
  matching_enabled: { label: "Matching Engine", description: "Master switch for the matching engine, on top of whether a rule set is published." },
  collaboration_enabled: { label: "Collaboration", description: "Lets members start new collaboration requests. Existing workspaces keep working if turned off." },
  deal_tracking_enabled: { label: "Deal Tracking", description: "Reserved — no deal-tracking feature is built yet.", reserved: true },
  auto_unit_conversion: { label: "Auto Unit Conversion", description: "Reserved — no automatic area-unit conversion is built yet.", reserved: true },
  sms_notifications: { label: "SMS Notifications", description: "Reserved — no SMS provider is configured; notifications are in-app only.", reserved: true },
  whatsapp_notifications: { label: "WhatsApp Notifications", description: "Reserved — no WhatsApp provider is configured; notifications are in-app only.", reserved: true },
  email_notifications: { label: "Email Notifications", description: "Reserved — no real email provider is configured yet (dev logs only).", reserved: true },
  member_export_enabled: { label: "Member Export", description: "Lets an admin export the member organization list as CSV." },
};

export type AuditLogEntry = {
  id: string; action: string; entityType: string; entityId: string | null; summary: string | null;
  ipAddress: string | null; createdAt: string; actorName: string | null;
};
export type AuditLogSearchResult = { items: AuditLogEntry[]; totalCount: number; page: number; pageSize: number };

export type ReportsSummary = {
  listingsByStatus: { status: string; count: number }[];
  demandsByStatus: { status: string; count: number }[];
  matchesByStatus: { status: string; count: number }[];
  collaborationsByStatus: { status: string; count: number }[];
  memberGrowth: { month: string; count: number }[];
};
