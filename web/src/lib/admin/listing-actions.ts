"use server";

import { revalidatePath } from "next/cache";
import { getAccessToken } from "@/lib/auth/session";
import { listingsApi } from "@/lib/listings/api";

async function requireAccessToken(): Promise<string> {
  const token = await getAccessToken();
  if (!token) throw new Error("Not authenticated.");
  return token;
}

export async function approveListingAction(id: string) {
  const accessToken = await requireAccessToken();
  await listingsApi.approve(accessToken, id);
  revalidatePath("/admin/listings");
  revalidatePath("/admin");
}

export async function rejectListingAction(id: string, formData: FormData) {
  const reason = String(formData.get("reason") ?? "").trim();
  if (!reason) return;
  const accessToken = await requireAccessToken();
  await listingsApi.reject(accessToken, id, reason);
  revalidatePath("/admin/listings");
  revalidatePath("/admin");
}
