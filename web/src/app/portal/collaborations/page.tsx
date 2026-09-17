import type { Metadata } from "next";
import Link from "next/link";
import { Handshake } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { collaborationRequestsApi } from "@/lib/collaboration/api";
import { acceptCollaborationRequestAction, declineCollaborationRequestAction, cancelCollaborationRequestAction } from "@/lib/collaboration/actions";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { EmptyState } from "@/components/ui/empty-state";
import { SubmitButton } from "@/components/auth/submit-button";

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

export default async function CollaborationsPage() {
  const { user, accessToken } = await requireSession();
  const result = await collaborationRequestsApi.listMine(accessToken);

  if (!result.ok) {
    return <p className="text-sm text-destructive">Couldn&apos;t load collaborations: {result.error}</p>;
  }

  const myOrgIds = new Set(user.memberships.map((m) => m.memberEntityId));
  const requests = result.data;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Collaborations</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Requests to coordinate on a match or a standalone introduction, and the shared workspaces they create once accepted (spec §13).
        </p>
      </div>

      {requests.length === 0 ? (
        <EmptyState
          icon={Handshake}
          title="No collaborations yet"
          description="Request collaboration from a match, or from another organization's member page, to start coordinating here."
        />
      ) : (
        <ul className="space-y-2">
          {requests.map((r) => {
            const isRecipient = myOrgIds.has(r.toMemberEntityId);
            const isRequester = myOrgIds.has(r.fromMemberEntityId);
            const counterpartName = isRequester ? r.toMemberEntityName : r.fromMemberEntityName;

            return (
              <li key={r.id}>
                <Card className="p-4">
                  <div className="flex items-start justify-between gap-4">
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-foreground">{counterpartName}</p>
                      <p className="text-xs text-muted-foreground">
                        {isRequester ? "You requested" : "They requested"} · {new Date(r.createdAt).toLocaleString()}
                        {r.matchId ? " · from a match" : ""}
                      </p>
                      {r.message ? <p className="mt-1 text-sm text-foreground">{r.message}</p> : null}
                    </div>
                    <Badge variant={statusBadgeVariant(r.status)}>{r.status}</Badge>
                  </div>

                  <div className="mt-3 flex flex-wrap items-center gap-2">
                    {r.status === "Accepted" && r.workspaceId ? (
                      <Link href={`/portal/collaborations/${r.workspaceId}`} className="text-sm font-medium text-primary hover:underline">
                        Open workspace →
                      </Link>
                    ) : null}
                    {r.status === "Pending" && isRecipient ? (
                      <>
                        <form action={acceptCollaborationRequestAction.bind(null, r.id)}>
                          <SubmitButton size="sm">Accept</SubmitButton>
                        </form>
                        <form action={declineCollaborationRequestAction.bind(null, r.id)}>
                          <SubmitButton size="sm" variant="secondary">Decline</SubmitButton>
                        </form>
                      </>
                    ) : null}
                    {r.status === "Pending" && isRequester && !isRecipient ? (
                      <form action={cancelCollaborationRequestAction.bind(null, r.id)}>
                        <SubmitButton size="sm" variant="secondary">Cancel request</SubmitButton>
                      </form>
                    ) : null}
                  </div>
                </Card>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}
