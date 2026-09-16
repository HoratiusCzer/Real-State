import { NextRequest, NextResponse } from "next/server";
import { getAccessToken } from "@/lib/auth/session";

const API_BASE = process.env.REAK_API_URL ?? "http://localhost:5080";

/** Same as the media proxy, for the wizard's private-document upload step. */
export async function POST(request: NextRequest, { params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const accessToken = await getAccessToken();
  if (!accessToken) {
    return NextResponse.json({ error: "Your session has expired. Please log in again." }, { status: 401 });
  }

  const formData = await request.formData();
  const res = await fetch(`${API_BASE}/api/listings/${id}/documents`, {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}` },
    body: formData,
  });

  const body = await res.text();
  return new NextResponse(body, { status: res.status, headers: { "Content-Type": "application/json" } });
}
