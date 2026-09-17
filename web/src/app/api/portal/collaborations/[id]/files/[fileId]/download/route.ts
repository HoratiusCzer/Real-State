import { NextRequest, NextResponse } from "next/server";
import { getAccessToken } from "@/lib/auth/session";

const API_BASE = process.env.REAK_API_URL ?? "http://localhost:5080";

/**
 * Same-origin proxy for authenticated collaboration file downloads. The browser can't hit
 * REAK.Api directly with a plain <a href> — it has no way to attach the httpOnly access-token
 * cookie as a Bearer header — so this route reads the token server-side and streams the
 * response straight through.
 */
export async function GET(_request: NextRequest, { params }: { params: Promise<{ id: string; fileId: string }> }) {
  const { id, fileId } = await params;
  const accessToken = await getAccessToken();
  if (!accessToken) {
    return NextResponse.json({ error: "Your session has expired. Please log in again." }, { status: 401 });
  }

  const res = await fetch(`${API_BASE}/api/collaborations/${id}/files/${fileId}/download`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (!res.ok) {
    return NextResponse.json({ error: "File not found." }, { status: res.status });
  }

  return new NextResponse(res.body, {
    status: 200,
    headers: {
      "Content-Type": res.headers.get("Content-Type") ?? "application/octet-stream",
      "Content-Disposition": res.headers.get("Content-Disposition") ?? "attachment",
    },
  });
}
