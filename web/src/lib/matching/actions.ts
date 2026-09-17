"use server";

import { revalidatePath } from "next/cache";
import { getAccessToken } from "@/lib/auth/session";
import { matchesApi } from "./api";

async function requireAccessToken(): Promise<string> {
  const token = await getAccessToken();
  if (!token) throw new Error("Not authenticated.");
  return token;
}

export async function recordMatchActionAction(id: string, actionType: number, formData: FormData) {
  const notes = String(formData.get("notes") ?? "").trim();
  const accessToken = await requireAccessToken();
  await matchesApi.recordAction(accessToken, id, actionType, notes || undefined);
  revalidatePath(`/portal/matches/${id}`);
  revalidatePath("/portal/matches");
  revalidatePath("/portal/dashboard");
}
