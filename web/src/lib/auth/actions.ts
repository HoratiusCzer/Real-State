"use server";

import { redirect } from "next/navigation";
import { authApi, invitationsApi, membershipApplicationsApi } from "./api";
import { clearSessionCookies, getRefreshToken, setSessionCookies } from "./session";

export type FormState = { error?: string; success?: string } | undefined;

export async function loginAction(_prev: FormState, formData: FormData): Promise<FormState> {
  const email = String(formData.get("email") ?? "").trim();
  const password = String(formData.get("password") ?? "");

  if (!email || !password) {
    return { error: "Email and password are required." };
  }

  const result = await authApi.login(email, password);
  if (!result.ok) {
    return { error: result.error };
  }

  await setSessionCookies(result.data);
  redirect("/portal");
}

export async function logoutAction() {
  const refreshToken = await getRefreshToken();
  if (refreshToken) {
    await authApi.logout(refreshToken);
  }
  await clearSessionCookies();
  redirect("/login");
}

export async function forgotPasswordAction(_prev: FormState, formData: FormData): Promise<FormState> {
  const email = String(formData.get("email") ?? "").trim();
  if (!email) {
    return { error: "Email is required." };
  }

  const result = await authApi.forgotPassword(email);
  if (!result.ok) {
    return { error: result.error };
  }

  return { success: result.data.message };
}

export async function resetPasswordAction(_prev: FormState, formData: FormData): Promise<FormState> {
  const token = String(formData.get("token") ?? "");
  const newPassword = String(formData.get("newPassword") ?? "");
  const confirmPassword = String(formData.get("confirmPassword") ?? "");

  if (!token) {
    return { error: "This reset link is missing its token." };
  }
  if (newPassword.length < 8) {
    return { error: "Password must be at least 8 characters." };
  }
  if (newPassword !== confirmPassword) {
    return { error: "Passwords do not match." };
  }

  const result = await authApi.resetPassword(token, newPassword);
  if (!result.ok) {
    return { error: result.error };
  }

  return { success: "Password reset. You can now log in with your new password." };
}

export async function acceptInvitationAction(token: string, _prev: FormState, formData: FormData): Promise<FormState> {
  const password = formData.get("password");
  const passwordStr = password ? String(password) : undefined;

  if (passwordStr) {
    const confirmPassword = String(formData.get("confirmPassword") ?? "");
    if (passwordStr.length < 8) {
      return { error: "Password must be at least 8 characters." };
    }
    if (passwordStr !== confirmPassword) {
      return { error: "Passwords do not match." };
    }
  }

  const result = await invitationsApi.accept(token, passwordStr);
  if (!result.ok) {
    return { error: result.error };
  }

  return { success: "Invitation accepted. You can now log in." };
}

export async function submitMembershipApplicationAction(_prev: FormState, formData: FormData): Promise<FormState> {
  const companyName = String(formData.get("companyName") ?? "").trim();
  const contactName = String(formData.get("contactName") ?? "").trim();
  const email = String(formData.get("email") ?? "").trim();
  const phone = String(formData.get("phone") ?? "").trim();
  const message = String(formData.get("message") ?? "").trim();

  if (!companyName || !contactName || !email || !phone) {
    return { error: "Company name, contact name, email, and phone are required." };
  }

  const result = await membershipApplicationsApi.submit({
    companyName,
    contactName,
    email,
    phone,
    message: message || undefined,
  });

  if (!result.ok) {
    return { error: result.error };
  }

  return { success: "Application submitted. REAK will review it and follow up by email." };
}
