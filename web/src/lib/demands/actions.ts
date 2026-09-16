"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { getAccessToken } from "@/lib/auth/session";
import { demandsApi } from "./api";
import type { CreateDemandInput, UpdateDemandInput, DemandContact, DemandLocationInput } from "./types";

async function requireAccessToken(): Promise<string> {
  const token = await getAccessToken();
  if (!token) throw new Error("Not authenticated.");
  return token;
}

export async function createDemandAction(input: CreateDemandInput): Promise<{ ok: boolean; id?: string; error?: string }> {
  const accessToken = await requireAccessToken();
  const result = await demandsApi.create(accessToken, input);
  if (!result.ok) return { ok: false, error: result.error };

  revalidatePath("/portal/demands");
  revalidatePath("/portal/dashboard");
  return { ok: true, id: result.data.id };
}

export async function updateDemandAction(id: string, input: UpdateDemandInput): Promise<{ ok: boolean; error?: string }> {
  const accessToken = await requireAccessToken();
  const result = await demandsApi.update(accessToken, id, input);
  if (!result.ok) return { ok: false, error: result.error };

  revalidatePath(`/portal/demands/${id}`);
  revalidatePath("/portal/demands");
  return { ok: true };
}

export async function updatePropertyTypesAction(id: string, propertyTypeIds: string[]) {
  const accessToken = await requireAccessToken();
  await demandsApi.replacePropertyTypes(accessToken, id, propertyTypeIds);
  revalidatePath(`/portal/demands/${id}`);
}

export async function updateLocationsAction(id: string, locations: DemandLocationInput[]) {
  const accessToken = await requireAccessToken();
  await demandsApi.replaceLocations(accessToken, id, locations);
  revalidatePath(`/portal/demands/${id}`);
}

export async function updateDemandAmenitiesAction(id: string, amenityIds: string[]) {
  const accessToken = await requireAccessToken();
  await demandsApi.replaceAmenities(accessToken, id, amenityIds);
  revalidatePath(`/portal/demands/${id}`);
}

export async function updateDemandContactAction(id: string, contact: DemandContact) {
  const accessToken = await requireAccessToken();
  await demandsApi.updateContact(accessToken, id, contact);
  revalidatePath(`/portal/demands/${id}`);
}

export async function updateDemandVisibilityAction(id: string, input: { networkVisibility: number; selectedMemberEntityIds?: string[] }) {
  const accessToken = await requireAccessToken();
  await demandsApi.updateVisibility(accessToken, id, input);
  revalidatePath(`/portal/demands/${id}`);
}

async function transition(id: string, fn: (token: string) => Promise<{ ok: boolean }>) {
  const accessToken = await requireAccessToken();
  await fn(accessToken);
  revalidatePath(`/portal/demands/${id}`);
  revalidatePath("/portal/demands");
  revalidatePath("/portal/dashboard");
}

export async function publishDemandAction(id: string) {
  await transition(id, (token) => demandsApi.publish(token, id));
}

export async function fulfillDemandAction(id: string) {
  await transition(id, (token) => demandsApi.fulfill(token, id));
}

export async function archiveDemandAction(id: string) {
  await transition(id, (token) => demandsApi.archive(token, id));
}

export async function deleteDemandAction(id: string) {
  const accessToken = await requireAccessToken();
  await demandsApi.remove(accessToken, id);
  revalidatePath("/portal/demands");
  revalidatePath("/portal/dashboard");
  redirect("/portal/demands");
}
