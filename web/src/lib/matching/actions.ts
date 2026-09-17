"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { getAccessToken } from "@/lib/auth/session";
import { matchesApi } from "./api";

async function requireAccessToken(): Promise<string> {
  const token = await getAccessToken();
  if (!token) throw new Error("Not authenticated.");
  return token;
}

const REQUEST_COLLABORATION_ACTION_TYPE = 4;

export async function recordMatchActionAction(id: string, actionType: number, formData: FormData) {
  const notes = String(formData.get("notes") ?? "").trim();
  const accessToken = await requireAccessToken();
  const result = await matchesApi.recordAction(accessToken, id, actionType, notes || undefined);
  revalidatePath(`/portal/matches/${id}`);
  revalidatePath("/portal/matches");
  revalidatePath("/portal/dashboard");

  if (actionType === REQUEST_COLLABORATION_ACTION_TYPE && result.ok && result.data.collaborationRequestId) {
    revalidatePath("/portal/collaborations");
    redirect("/portal/collaborations");
  }
}
