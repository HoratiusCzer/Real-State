"use server";

import { revalidatePath } from "next/cache";
import { getAccessToken } from "@/lib/auth/session";
import { memberEntitiesApi, notificationsApi, profileApi } from "./api";
import type { FormState } from "@/lib/auth/actions";

async function requireAccessToken(): Promise<string> {
  const token = await getAccessToken();
  if (!token) {
    throw new Error("Not authenticated.");
  }
  return token;
}

export async function markNotificationReadAction(id: string) {
  const accessToken = await requireAccessToken();
  await notificationsApi.markRead(accessToken, id);
  revalidatePath("/portal/notifications");
}

export async function markAllNotificationsReadAction() {
  const accessToken = await requireAccessToken();
  await notificationsApi.markAllRead(accessToken);
  revalidatePath("/portal/notifications");
}

export async function updateProfileAction(_prev: FormState, formData: FormData): Promise<FormState> {
  const fullName = String(formData.get("fullName") ?? "").trim();
  const phone = String(formData.get("phone") ?? "").trim();

  if (!fullName) {
    return { error: "Name is required." };
  }

  const accessToken = await requireAccessToken();
  const result = await profileApi.update(accessToken, { fullName, phone: phone || undefined });
  if (!result.ok) {
    return { error: result.error };
  }

  revalidatePath("/portal", "layout");
  return { success: "Profile updated." };
}

export async function updateOrganizationAction(id: string, _prev: FormState, formData: FormData): Promise<FormState> {
  const name = String(formData.get("name") ?? "").trim();
  const description = String(formData.get("description") ?? "").trim();
  const website = String(formData.get("website") ?? "").trim();
  const phone = String(formData.get("phone") ?? "").trim();
  const email = String(formData.get("email") ?? "").trim();
  const address = String(formData.get("address") ?? "").trim();

  if (!name) {
    return { error: "Organization name is required." };
  }

  const accessToken = await requireAccessToken();
  const result = await memberEntitiesApi.update(accessToken, id, {
    name,
    description: description || undefined,
    website: website || undefined,
    phone: phone || undefined,
    email: email || undefined,
    address: address || undefined,
  });

  if (!result.ok) {
    return { error: result.error };
  }

  revalidatePath("/portal/organization");
  revalidatePath("/portal/dashboard");
  return { success: "Organization updated." };
}
