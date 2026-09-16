"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { getAccessToken } from "@/lib/auth/session";
import { listingsApi } from "./api";
import type { CreateListingInput, UpdateListingInput, ListingContact } from "./types";

async function requireAccessToken(): Promise<string> {
  const token = await getAccessToken();
  if (!token) {
    throw new Error("Not authenticated.");
  }
  return token;
}

export async function createListingAction(input: CreateListingInput): Promise<{ ok: boolean; id?: string; error?: string }> {
  const accessToken = await requireAccessToken();
  const result = await listingsApi.create(accessToken, input);
  if (!result.ok) return { ok: false, error: result.error };

  revalidatePath("/portal/my-properties");
  revalidatePath("/portal/dashboard");
  return { ok: true, id: result.data.id };
}

export async function updateListingAction(id: string, input: UpdateListingInput): Promise<{ ok: boolean; error?: string }> {
  const accessToken = await requireAccessToken();
  const result = await listingsApi.update(accessToken, id, input);
  if (!result.ok) return { ok: false, error: result.error };

  revalidatePath(`/portal/properties/${id}`);
  revalidatePath("/portal/my-properties");
  return { ok: true };
}

export async function updateAmenitiesAction(id: string, amenityIds: string[]) {
  const accessToken = await requireAccessToken();
  await listingsApi.replaceAmenities(accessToken, id, amenityIds);
  revalidatePath(`/portal/properties/${id}`);
}

export async function updateContactAction(id: string, contact: ListingContact) {
  const accessToken = await requireAccessToken();
  await listingsApi.updateContact(accessToken, id, contact);
  revalidatePath(`/portal/properties/${id}`);
}

export async function updateVisibilityAction(
  id: string,
  input: { networkVisibility: number; isPublicVisible: boolean; selectedMemberEntityIds?: string[] }
) {
  const accessToken = await requireAccessToken();
  await listingsApi.updateVisibility(accessToken, id, input);
  revalidatePath(`/portal/properties/${id}`);
}

async function transition(id: string, fn: (token: string) => Promise<{ ok: boolean }>) {
  const accessToken = await requireAccessToken();
  await fn(accessToken);
  revalidatePath(`/portal/properties/${id}`);
  revalidatePath("/portal/my-properties");
  revalidatePath("/portal/dashboard");
}

export async function submitListingAction(id: string) {
  await transition(id, (token) => listingsApi.submit(token, id));
}

export async function approveListingAction(id: string) {
  await transition(id, (token) => listingsApi.approve(token, id));
}

export async function rejectListingAction(id: string, reason: string) {
  await transition(id, (token) => listingsApi.reject(token, id, reason));
}

export async function rejectListingFormAction(id: string, formData: FormData) {
  const reason = String(formData.get("reason") ?? "").trim();
  if (!reason) return;
  await rejectListingAction(id, reason);
}

export async function archiveListingAction(id: string) {
  await transition(id, (token) => listingsApi.archive(token, id));
}

export async function deleteListingAction(id: string) {
  const accessToken = await requireAccessToken();
  await listingsApi.remove(accessToken, id);
  revalidatePath("/portal/my-properties");
  revalidatePath("/portal/dashboard");
  redirect("/portal/my-properties");
}

export async function toggleSaveListingAction(id: string, currentlySaved: boolean) {
  const accessToken = await requireAccessToken();
  if (currentlySaved) {
    await listingsApi.unsave(accessToken, id);
  } else {
    await listingsApi.save(accessToken, id);
  }
  revalidatePath(`/portal/properties/${id}`);
  revalidatePath("/portal/properties");
  revalidatePath("/portal/saved");
  revalidatePath("/portal/dashboard");
}

export async function deleteMediaAction(listingId: string, mediaId: string) {
  const accessToken = await requireAccessToken();
  await listingsApi.deleteMedia(accessToken, listingId, mediaId);
  revalidatePath(`/portal/properties/${listingId}`);
}

export async function deleteDocumentAction(listingId: string, documentId: string) {
  const accessToken = await requireAccessToken();
  await listingsApi.deleteDocument(accessToken, listingId, documentId);
  revalidatePath(`/portal/properties/${listingId}`);
}
