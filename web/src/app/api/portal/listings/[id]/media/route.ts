import { NextRequest, NextResponse } from "next/server";
import { getAccessToken } from "@/lib/auth/session";

const API_BASE = process.env.REAK_API_URL ?? "http://localhost:5080";

/**
 * Same-origin proxy for the listing photo-upload wizard step. The browser needs to POST
 * multipart/form-data directly (for real upload-progress reporting via XHR, spec §8.2) but must
 * never hold the access token itself (httpOnly cookie only, per Stage 4's security model) — this
 * route reads the token server-side and forwards the request, so the token never reaches
 * client-side JavaScript.
 */
export async function POST(request: NextRequest, { params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const accessToken = await getAccessToken();
  if (!accessToken) {
    return NextResponse.json({ error: "Your session has expired. Please log in again." }, { status: 401 });
  }

  const search = request.nextUrl.search;
  const formData = await request.formData();
  const res = await fetch(`${API_BASE}/api/listings/${id}/media${search}`, {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}` },
    body: formData,
  });

  const body = await res.text();
  return new NextResponse(body, { status: res.status, headers: { "Content-Type": "application/json" } });
}
