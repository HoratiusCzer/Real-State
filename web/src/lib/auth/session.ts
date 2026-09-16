import { cookies } from "next/headers";
import { authApi, type MeResponse, type TokenPair } from "./api";

const ACCESS_COOKIE = "reak_access";
const REFRESH_COOKIE = "reak_refresh";

/** Both tokens are httpOnly so client-side JavaScript can never read them — the browser only
 * ever gets a session cookie, never the JWT itself (mitigates token theft via XSS). Frontend
 * code never verifies or decodes either token; REAK.Api and its RLS remain the sole authority
 * (spec §2.5 — frontend checks are UX only). */
export async function setSessionCookies(tokens: TokenPair) {
  const store = await cookies();
  const secure = process.env.NODE_ENV === "production";

  store.set(ACCESS_COOKIE, tokens.accessToken, {
    httpOnly: true,
    secure,
    sameSite: "lax",
    path: "/",
    expires: new Date(tokens.accessTokenExpiresAt),
  });
  store.set(REFRESH_COOKIE, tokens.refreshToken, {
    httpOnly: true,
    secure,
    sameSite: "lax",
    path: "/",
    expires: new Date(tokens.refreshTokenExpiresAt),
  });
}

export async function clearSessionCookies() {
  const store = await cookies();
  store.delete(ACCESS_COOKIE);
  store.delete(REFRESH_COOKIE);
}

export async function getRefreshToken(): Promise<string | undefined> {
  const store = await cookies();
  return store.get(REFRESH_COOKIE)?.value;
}

/** Reads the current user from REAK.Api using the access-token cookie. Does not attempt a
 * silent refresh — Server Components cannot set response cookies mid-render (see Next.js's
 * cookies() docs), and Proxy is documented as unsuitable for full session management, so an
 * expired access token here just means "signed out"; the user logs in again. A short (15 min)
 * access-token lifetime keeps this acceptable for Stage 4's minimal authenticated placeholder —
 * genuine session persistence is Stage 5 (Member Portal) territory. */
export async function getCurrentUser(): Promise<MeResponse | null> {
  const store = await cookies();
  const accessToken = store.get(ACCESS_COOKIE)?.value;
  if (!accessToken) {
    return null;
  }

  const result = await authApi.me(accessToken);
  return result.ok ? result.data : null;
}
