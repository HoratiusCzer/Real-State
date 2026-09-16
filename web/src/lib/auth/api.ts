import { apiGet, apiPost, type ApiResult } from "../api-client";

export type { ApiResult };

export type TokenPair = {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
};

export type MeResponse = {
  profile: { id: string; email: string; fullName: string; phone: string | null; lastLoginAt: string | null };
  memberships: { memberEntityId: string; memberEntityName: string }[];
  permissions: string[];
  isSystemAdmin: boolean;
};

export const authApi = {
  login: (email: string, password: string) => apiPost<TokenPair>("/api/auth/login", { email, password }),
  refresh: (refreshToken: string) => apiPost<TokenPair>("/api/auth/refresh", { refreshToken }),
  logout: (refreshToken: string) => apiPost<void>("/api/auth/logout", { refreshToken }),
  forgotPassword: (email: string) => apiPost<{ message: string }>("/api/auth/forgot-password", { email }),
  resetPassword: (token: string, newPassword: string) =>
    apiPost<void>("/api/auth/reset-password", { token, newPassword }),
  changePassword: (accessToken: string, currentPassword: string, newPassword: string) =>
    apiPost<void>("/api/auth/change-password", { currentPassword, newPassword }, accessToken),
  me: (accessToken: string) => apiGet<MeResponse>("/api/auth/me", accessToken),
};

export const invitationsApi = {
  lookup: (token: string) =>
    apiGet<{
      found: boolean;
      email: string | null;
      roleName: string | null;
      memberEntityName: string | null;
      isNewAccount: boolean;
      isExpiredOrUsed: boolean;
    }>(`/api/invitations/${encodeURIComponent(token)}`),
  accept: (token: string, password: string | undefined) =>
    apiPost<void>(`/api/invitations/${encodeURIComponent(token)}/accept`, { password }),
};

export const membershipApplicationsApi = {
  submit: (input: { companyName: string; contactName: string; email: string; phone: string; message?: string }) =>
    apiPost<{ id: string; status: number }>("/api/membership-applications", input),
};
