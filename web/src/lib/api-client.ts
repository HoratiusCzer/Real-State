/**
 * Shared server-only HTTP client for REAK.Api. Every call runs on the Next.js server (Server
 * Actions / Server Components) — see lib/auth/api.ts's header comment for why. Base URL comes
 * from REAK_API_URL (server-only env var, not NEXT_PUBLIC_*).
 */
const API_BASE = process.env.REAK_API_URL ?? "http://localhost:5080";

export type ApiResult<T> = { ok: true; data: T } | { ok: false; error: string; status: number };

async function request<T>(
  method: "GET" | "POST" | "PATCH" | "PUT",
  path: string,
  body: unknown | undefined,
  accessToken: string | undefined
): Promise<ApiResult<T>> {
  const res = await fetch(`${API_BASE}${path}`, {
    method,
    headers: {
      ...(body !== undefined ? { "Content-Type": "application/json" } : {}),
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
    cache: "no-store",
  });

  if (res.status === 204) {
    return { ok: true, data: undefined as T };
  }

  let payload: unknown = null;
  try {
    payload = await res.json();
  } catch {
    // No JSON body — fall through with a generic message below.
  }

  if (!res.ok) {
    const message =
      payload && typeof payload === "object" && "error" in payload && typeof (payload as { error?: unknown }).error === "string"
        ? (payload as { error: string }).error
        : res.status === 403
          ? "You don't have permission to do that."
          : res.status === 401
            ? "Your session has expired. Please log in again."
            : "Something went wrong. Please try again.";
    return { ok: false, error: message, status: res.status };
  }

  return { ok: true, data: payload as T };
}

export const apiGet = <T>(path: string, accessToken?: string) => request<T>("GET", path, undefined, accessToken);
export const apiPost = <T>(path: string, body: unknown, accessToken?: string) => request<T>("POST", path, body, accessToken);
export const apiPatch = <T>(path: string, body: unknown, accessToken?: string) => request<T>("PATCH", path, body, accessToken);
export const apiPut = <T>(path: string, body: unknown, accessToken?: string) => request<T>("PUT", path, body, accessToken);
