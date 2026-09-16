"use server";

import { authApi } from "@/lib/auth/api";
import { getAccessToken } from "@/lib/auth/session";
import type { FormState } from "@/lib/auth/actions";

export async function changePasswordAction(_prev: FormState, formData: FormData): Promise<FormState> {
  const currentPassword = String(formData.get("currentPassword") ?? "");
  const newPassword = String(formData.get("newPassword") ?? "");
  const confirmPassword = String(formData.get("confirmPassword") ?? "");

  if (newPassword.length < 8) {
    return { error: "New password must be at least 8 characters." };
  }
  if (newPassword !== confirmPassword) {
    return { error: "New passwords do not match." };
  }

  const accessToken = await getAccessToken();
  if (!accessToken) {
    return { error: "Your session has expired. Please log in again." };
  }

  const result = await authApi.changePassword(accessToken, currentPassword, newPassword);
  if (!result.ok) {
    return { error: result.error };
  }

  return { success: "Password changed." };
}
