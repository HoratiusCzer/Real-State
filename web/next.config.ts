import type { NextConfig } from "next";

// Defense-in-depth response headers (Stage 13 hardening). No Content-Security-Policy here,
// deliberately: this app embeds admin-supplied image URLs (listing photos, member logos,
// committee photos) from arbitrary hosts, and getting a CSP right for Next.js's own inline
// hydration payloads without being able to verify it in an actual browser in this environment
// risks silently breaking the site for a header that's advisory anyway — the real security
// boundary is server-side (RLS, permission checks), not a browser-enforced policy. The headers
// below are all either self-evidently safe or degrade gracefully if a browser ignores them.
const securityHeaders = [
  { key: "X-Content-Type-Options", value: "nosniff" },
  { key: "X-Frame-Options", value: "DENY" },
  { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
  { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=()" },
];

const nextConfig: NextConfig = {
  async headers() {
    return [{ source: "/:path*", headers: securityHeaders }];
  },
};

export default nextConfig;
