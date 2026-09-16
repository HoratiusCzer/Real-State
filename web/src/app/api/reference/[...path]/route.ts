import { NextRequest, NextResponse } from "next/server";

const API_BASE = process.env.REAK_API_URL ?? "http://localhost:5080";

/**
 * Same-origin proxy for REAK.Api's public reference-data reads (property types, purposes,
 * amenities, area units, currencies, Nepal location hierarchy) — needed so the listing wizard's
 * client-side cascading dropdowns (province -> district -> municipality -> ward -> locality) can
 * fetch fresh options without the browser calling REAK.Api cross-origin. Keeps every browser
 * request same-origin to the Next.js server, avoiding CORS entirely, consistent with the
 * server-only API client used everywhere else (spec §2.5 — the browser is never REAK.Api's
 * direct caller). Read-only, and only forwards to REAK.Api's own [AllowAnonymous] GET endpoints
 * — never a general-purpose proxy.
 */
export async function GET(request: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
  const { path } = await params;
  const search = request.nextUrl.search;
  const res = await fetch(`${API_BASE}/api/reference/${path.join("/")}${search}`, { cache: "no-store" });
  const body = await res.text();
  return new NextResponse(body, { status: res.status, headers: { "Content-Type": "application/json" } });
}
