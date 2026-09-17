import type { Metadata } from "next";
import { Handshake } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { collaborationRequestsApi } from "@/lib/collaboration/api";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Collaborations" };

function statusBadgeVariant(status: string): "default" | "success" | "destructive" | "accent" {
  switch (status) {
    case "Accepted": return "success";
    case "Declined":
    case "Cancelled": return "destructive";
    case "Pending": return "accent";
    default: return "default";
  }
}

/** Read-only oversight (spec §4.3) — CollaborationRequestService.ListMineAsync already returns
 * every request association-wide for a system admin caller, so no new backend endpoint was needed
 * here; this page just gives that existing behavior a place to be seen. */
export default async function AdminCollaborationsPage() {
  const { accessToken } = await requireSession();
  const result = await collaborationRequestsApi.listMine(accessToken);

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Collaborations</h1>
        <p className="mt-1 text-sm text-muted-foreground">Every collaboration request association-wide, read-only.</p>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load collaborations: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={Handshake} title="No collaborations yet" description="Requests between member organizations will appear here." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((r) => (
            <li key={r.id}>
              <Card className="flex items-center justify-between gap-4 p-4">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-foreground">{r.fromMemberEntityName} → {r.toMemberEntityName}</p>
                  <p className="text-xs text-muted-foreground">
                    {r.requestedByProfileName} · {new Date(r.createdAt).toLocaleString()}{r.matchId ? " · from a match" : ""}
                  </p>
                </div>
                <Badge variant={statusBadgeVariant(r.status)}>{r.status}</Badge>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
