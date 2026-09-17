import { apiGet, apiPost, apiPut, apiDelete } from "../api-client";
import type {
  CollaborationRequest,
  CollaborationWorkspace,
  CollaborationMessage,
  CollaborationNote,
  CollaborationTask,
  CollaborationViewing,
  CollaborationFile,
  CollaborationActivity,
} from "./types";

export const collaborationRequestsApi = {
  listMine: (accessToken: string) => apiGet<CollaborationRequest[]>("/api/collaboration-requests", accessToken),
  createToOrg: (accessToken: string, toMemberEntityId: string, message?: string) =>
    apiPost<{ id: string }>("/api/collaboration-requests/to-org", { toMemberEntityId, message }, accessToken),
  accept: (accessToken: string, id: string) => apiPost<{ workspaceId: string }>(`/api/collaboration-requests/${id}/accept`, undefined, accessToken),
  decline: (accessToken: string, id: string) => apiPost<void>(`/api/collaboration-requests/${id}/decline`, undefined, accessToken),
  cancel: (accessToken: string, id: string) => apiPost<void>(`/api/collaboration-requests/${id}/cancel`, undefined, accessToken),
};

export const collaborationsApi = {
  get: (accessToken: string, id: string) => apiGet<CollaborationWorkspace>(`/api/collaborations/${id}`, accessToken),
  listMessages: (accessToken: string, id: string) => apiGet<CollaborationMessage[]>(`/api/collaborations/${id}/messages`, accessToken),
  sendMessage: (accessToken: string, id: string, body: string) => apiPost<void>(`/api/collaborations/${id}/messages`, { body }, accessToken),
  listNotes: (accessToken: string, id: string) => apiGet<CollaborationNote[]>(`/api/collaborations/${id}/notes`, accessToken),
  addNote: (accessToken: string, id: string, body: string) => apiPost<void>(`/api/collaborations/${id}/notes`, { body }, accessToken),
  listTasks: (accessToken: string, id: string) => apiGet<CollaborationTask[]>(`/api/collaborations/${id}/tasks`, accessToken),
  createTask: (accessToken: string, id: string, title: string, dueDate?: string) =>
    apiPost<void>(`/api/collaborations/${id}/tasks`, { title, dueDate: dueDate || undefined }, accessToken),
  updateTaskStatus: (accessToken: string, id: string, taskId: string, status: number) =>
    apiPut<void>(`/api/collaborations/${id}/tasks/${taskId}/status`, { status }, accessToken),
  listViewings: (accessToken: string, id: string) => apiGet<CollaborationViewing[]>(`/api/collaborations/${id}/viewings`, accessToken),
  scheduleViewing: (accessToken: string, id: string, scheduledAt: string, notes?: string) =>
    apiPost<void>(`/api/collaborations/${id}/viewings`, { scheduledAt, notes }, accessToken),
  listFiles: (accessToken: string, id: string) => apiGet<CollaborationFile[]>(`/api/collaborations/${id}/files`, accessToken),
  deleteFile: (accessToken: string, id: string, fileId: string) => apiDelete<void>(`/api/collaborations/${id}/files/${fileId}`, accessToken),
  listActivities: (accessToken: string, id: string) => apiGet<CollaborationActivity[]>(`/api/collaborations/${id}/activities`, accessToken),
  grantContactDisclosure: (accessToken: string, id: string, dataType: number) =>
    apiPost<void>(`/api/collaborations/${id}/contact-disclosures`, { dataType }, accessToken),
  revokeContactDisclosure: (accessToken: string, id: string, disclosureId: string) =>
    apiDelete<void>(`/api/collaborations/${id}/contact-disclosures/${disclosureId}`, accessToken),
};
