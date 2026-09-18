import { NextRequest, NextResponse } from "next/server";
import { getAccessToken } from "@/lib/auth/session";

const API_BASE = process.env.REAK_API_URL ?? "http://localhost:5080";

/** Same-origin proxy for authenticated listing-document downloads (Stage 14 fix — the page linking
 * here had pointed at a route that never existed: /api/listings/... isn't a Next.js route at all,
 * only /api/portal/listings/... proxies are, and this one specifically didn't exist yet). Same
 * pattern as the collaboration-file download proxy: a plain <a href> can't attach the httpOnly
 * access-token cookie as a Bearer header, so this route does it server-side and streams through. */
export async function GET(_request: NextRequest, { params }: { params: Promise<{ id: string; documentId: string }> }) {
  const { id, documentId } = await params;
  const accessToken = await getAccessToken();
  if (!accessToken) {
    return NextResponse.json({ error: "Your session has expired. Please log in again." }, { status: 401 });
  }

  const res = await fetch(`${API_BASE}/api/listings/${id}/documents/${documentId}/download`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (!res.ok) {
    return NextResponse.json({ error: "Document not found." }, { status: res.status });
  }

  return new NextResponse(res.body, {
    status: 200,
    headers: {
      "Content-Type": res.headers.get("Content-Type") ?? "application/octet-stream",
      "Content-Disposition": res.headers.get("Content-Disposition") ?? "attachment",
    },
  });
}
