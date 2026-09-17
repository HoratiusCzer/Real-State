import { apiGet } from "../api-client";
import type {
  PublicPage, PublicNewsSummary, PublicNewsDetail, PublicNoticeSummary, PublicNoticeDetail,
  PublicEventSummary, PublicEventDetail, PublicResource, PublicCommitteeMember,
} from "./types";
import type { PublicMemberListResult, PublicMemberSummary } from "./member-types";

export const publicContentApi = {
  getPage: (slug: string) => apiGet<PublicPage>(`/api/public/pages/${slug}`),
  listNews: () => apiGet<PublicNewsSummary[]>("/api/public/news"),
  getNews: (slug: string) => apiGet<PublicNewsDetail>(`/api/public/news/${slug}`),
  listNotices: () => apiGet<PublicNoticeSummary[]>("/api/public/notices"),
  getNotice: (slug: string) => apiGet<PublicNoticeDetail>(`/api/public/notices/${slug}`),
  listEvents: () => apiGet<PublicEventSummary[]>("/api/public/events"),
  getEvent: (slug: string) => apiGet<PublicEventDetail>(`/api/public/events/${slug}`),
  listResources: () => apiGet<PublicResource[]>("/api/public/resources"),
  listCommittee: () => apiGet<PublicCommitteeMember[]>("/api/public/committee"),
  listMembers: () => apiGet<PublicMemberListResult>("/api/public/members"),
  getMember: (id: string) => apiGet<PublicMemberSummary>(`/api/public/members/${id}`),
};
