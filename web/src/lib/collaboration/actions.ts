"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { getAccessToken } from "@/lib/auth/session";
import { collaborationRequestsApi, collaborationsApi } from "./api";

async function requireAccessToken(): Promise<string> {
  const token = await getAccessToken();
  if (!token) throw new Error("Not authenticated.");
  return token;
}

export async function createCollaborationRequestAction(toMemberEntityId: string, formData: FormData) {
  const message = String(formData.get("message") ?? "").trim();
  const accessToken = await requireAccessToken();
  await collaborationRequestsApi.createToOrg(accessToken, toMemberEntityId, message || undefined);
  revalidatePath("/portal/collaborations");
  redirect("/portal/collaborations");
}

export async function acceptCollaborationRequestAction(requestId: string) {
  const accessToken = await requireAccessToken();
  const result = await collaborationRequestsApi.accept(accessToken, requestId);
  revalidatePath("/portal/collaborations");
  if (result.ok) redirect(`/portal/collaborations/${result.data.workspaceId}`);
}

export async function declineCollaborationRequestAction(requestId: string) {
  const accessToken = await requireAccessToken();
  await collaborationRequestsApi.decline(accessToken, requestId);
  revalidatePath("/portal/collaborations");
}

export async function cancelCollaborationRequestAction(requestId: string) {
  const accessToken = await requireAccessToken();
  await collaborationRequestsApi.cancel(accessToken, requestId);
  revalidatePath("/portal/collaborations");
}

export async function sendMessageAction(workspaceId: string, formData: FormData) {
  const body = String(formData.get("body") ?? "").trim();
  if (!body) return;
  const accessToken = await requireAccessToken();
  await collaborationsApi.sendMessage(accessToken, workspaceId, body);
  revalidatePath(`/portal/collaborations/${workspaceId}`);
}

export async function addNoteAction(workspaceId: string, formData: FormData) {
  const body = String(formData.get("body") ?? "").trim();
  if (!body) return;
  const accessToken = await requireAccessToken();
  await collaborationsApi.addNote(accessToken, workspaceId, body);
  revalidatePath(`/portal/collaborations/${workspaceId}`);
}

export async function createTaskAction(workspaceId: string, formData: FormData) {
  const title = String(formData.get("title") ?? "").trim();
  if (!title) return;
  const dueDate = String(formData.get("dueDate") ?? "").trim();
  const accessToken = await requireAccessToken();
  await collaborationsApi.createTask(accessToken, workspaceId, title, dueDate || undefined);
  revalidatePath(`/portal/collaborations/${workspaceId}`);
}

export async function updateTaskStatusAction(workspaceId: string, taskId: string, status: number) {
  const accessToken = await requireAccessToken();
  await collaborationsApi.updateTaskStatus(accessToken, workspaceId, taskId, status);
  revalidatePath(`/portal/collaborations/${workspaceId}`);
}

export async function scheduleViewingAction(workspaceId: string, formData: FormData) {
  const scheduledAt = String(formData.get("scheduledAt") ?? "").trim();
  if (!scheduledAt) return;
  const notes = String(formData.get("notes") ?? "").trim();
  const accessToken = await requireAccessToken();
  await collaborationsApi.scheduleViewing(accessToken, workspaceId, new Date(scheduledAt).toISOString(), notes || undefined);
  revalidatePath(`/portal/collaborations/${workspaceId}`);
}

const API_BASE = process.env.REAK_API_URL ?? "http://localhost:5080";

export async function uploadFileAction(workspaceId: string, formData: FormData) {
  const file = formData.get("file");
  if (!(file instanceof File) || file.size === 0) return;

  const accessToken = await requireAccessToken();
  const forward = new FormData();
  forward.set("file", file, file.name);

  await fetch(`${API_BASE}/api/collaborations/${workspaceId}/files`, {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}` },
    body: forward,
  });

  revalidatePath(`/portal/collaborations/${workspaceId}`);
}

export async function deleteFileAction(workspaceId: string, fileId: string) {
  const accessToken = await requireAccessToken();
  await collaborationsApi.deleteFile(accessToken, workspaceId, fileId);
  revalidatePath(`/portal/collaborations/${workspaceId}`);
}

export async function grantContactDisclosureAction(workspaceId: string, dataType: number) {
  const accessToken = await requireAccessToken();
  await collaborationsApi.grantContactDisclosure(accessToken, workspaceId, dataType);
  revalidatePath(`/portal/collaborations/${workspaceId}`);
}

export async function revokeContactDisclosureAction(workspaceId: string, disclosureId: string) {
  const accessToken = await requireAccessToken();
  await collaborationsApi.revokeContactDisclosure(accessToken, workspaceId, disclosureId);
  revalidatePath(`/portal/collaborations/${workspaceId}`);
}
