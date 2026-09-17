import { NextResponse } from "next/server";
import { getAccessToken } from "@/lib/auth/session";

const API_BASE = process.env.REAK_API_URL ?? "http://localhost:5080";

/** Same-origin proxy for the member-organizations CSV export (member_export_enabled, Stage 12) —
 * same reasoning as the collaboration file download proxy: a plain <a href> can't attach the
 * httpOnly access-token cookie as a Bearer header, so this route does it server-side and streams
 * the response straight through. */
export async function GET() {
  const accessToken = await getAccessToken();
  if (!accessToken) {
    return NextResponse.json({ error: "Your session has expired. Please log in again." }, { status: 401 });
  }

  const res = await fetch(`${API_BASE}/api/member-entities/export`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (!res.ok) {
    const body = await res.text();
    return new NextResponse(body, { status: res.status, headers: { "Content-Type": "application/json" } });
  }

  return new NextResponse(res.body, {
    status: 200,
    headers: {
      "Content-Type": res.headers.get("Content-Type") ?? "text/csv",
      "Content-Disposition": res.headers.get("Content-Disposition") ?? "attachment; filename=member-organizations.csv",
    },
  });
}
