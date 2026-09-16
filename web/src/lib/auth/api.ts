/**
 * Server-only client for REAK.Api's auth surface. Every call here runs on the Next.js server
 * (Server Actions / Server Components) — the browser never talks to the API directly and never
 * sees the API's own error bodies verbatim, so token handling and error shaping stay in one
 * place. Base URL comes from REAK_API_URL (server-only env var, not NEXT_PUBLIC_*) so it is
 * never bundled into client JavaScript.
 */
const API_BASE = process.env.REAK_API_URL ?? "http://localhost:5080";

export type TokenPair = {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
};

export type ApiResult<T> = { ok: true; data: T } | { ok: false; error: string; status: number };

async function post<T>(path: string, body: unknown, accessToken?: string): Promise<ApiResult<T>> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
    },
    body: JSON.stringify(body),
    cache: "no-store",
  });

  if (res.status === 204) {
    return { ok: true, data: undefined as T };
  }

  let payload: unknown = null;
  try {
    payload = await res.json();
  } catch {
    // No JSON body (e.g. some error responses) — fall through with a generic message below.
  }

  if (!res.ok) {
    const message =
      payload && typeof payload === "object" && "error" in payload && typeof (payload as { error?: unknown }).error === "string"
        ? (payload as { error: string }).error
        : "Something went wrong. Please try again.";
    return { ok: false, error: message, status: res.status };
  }

  return { ok: true, data: payload as T };
}

async function get<T>(path: string, accessToken?: string): Promise<ApiResult<T>> {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
    cache: "no-store",
  });

  if (!res.ok) {
    return { ok: false, error: "Request failed.", status: res.status };
  }

  return { ok: true, data: (await res.json()) as T };
}

export const authApi = {
  login: (email: string, password: string) => post<TokenPair>("/api/auth/login", { email, password }),
  refresh: (refreshToken: string) => post<TokenPair>("/api/auth/refresh", { refreshToken }),
  logout: (refreshToken: string) => post<void>("/api/auth/logout", { refreshToken }),
  forgotPassword: (email: string) => post<{ message: string }>("/api/auth/forgot-password", { email }),
  resetPassword: (token: string, newPassword: string) =>
    post<void>("/api/auth/reset-password", { token, newPassword }),
  me: (accessToken: string) => get<MeResponse>("/api/auth/me", accessToken),
};

export type MeResponse = {
  profile: { id: string; email: string; fullName: string; phone: string | null; lastLoginAt: string | null };
  memberships: { memberEntityId: string; memberEntityName: string }[];
  permissions: string[];
  isSystemAdmin: boolean;
};

export const invitationsApi = {
  lookup: (token: string) =>
    get<{
      found: boolean;
      email: string | null;
      roleName: string | null;
      memberEntityName: string | null;
      isNewAccount: boolean;
      isExpiredOrUsed: boolean;
    }>(`/api/invitations/${encodeURIComponent(token)}`),
  accept: (token: string, password: string | undefined) =>
    post<void>(`/api/invitations/${encodeURIComponent(token)}/accept`, { password }),
};

export const membershipApplicationsApi = {
  submit: (input: { companyName: string; contactName: string; email: string; phone: string; message?: string }) =>
    post<{ id: string; status: number }>("/api/membership-applications", input),
};
