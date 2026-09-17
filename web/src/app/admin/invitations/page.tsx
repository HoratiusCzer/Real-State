import type { Metadata } from "next";
import { Mail } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { adminInvitationsApi, adminMemberEntitiesApi, adminRolesApi } from "@/lib/admin/api";
import { createInvitationAction, revokeInvitationAction } from "@/lib/admin/actions";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Invitations" };

const STATUS_LABELS: Record<number, string> = { 1: "Pending", 2: "Accepted", 3: "Expired", 4: "Revoked" };
function statusBadgeVariant(status: number): "default" | "success" | "destructive" | "accent" {
  switch (status) {
    case 1: return "accent";
    case 2: return "success";
    default: return "destructive";
  }
}

export default async function AdminInvitationsPage() {
  const { accessToken } = await requireSession();
  const [invitationsResult, orgsResult, rolesResult] = await Promise.all([
    adminInvitationsApi.list(accessToken),
    adminMemberEntitiesApi.list(accessToken),
    adminRolesApi.list(accessToken),
  ]);
  const orgs = orgsResult.ok ? orgsResult.data : [];
  const roles = rolesResult.ok ? rolesResult.data : [];

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Invitations</h1>
        <p className="mt-1 text-sm text-muted-foreground">Flow A step 3 (spec §2.4) — invite a person into a role, org-scoped or system-level.</p>
      </div>

      <Card>
        <CardHeader><CardTitle>Send an invitation</CardTitle></CardHeader>
        <CardContent>
          <form action={createInvitationAction} className="space-y-4">
            <div>
              <Label htmlFor="email">Email</Label>
              <Input id="email" name="email" type="email" required />
            </div>
            <div>
              <Label htmlFor="roleId">Role</Label>
              <select id="roleId" name="roleId" required className="flex h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground">
                <option value="">Select a role…</option>
                {roles.map((r) => (
                  <option key={r.id} value={r.id}>{r.name} ({r.scope})</option>
                ))}
              </select>
            </div>
            <div>
              <Label htmlFor="memberEntityId">Organization (required for an organization-scoped role)</Label>
              <select id="memberEntityId" name="memberEntityId" className="flex h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground">
                <option value="">None (system-level role)</option>
                {orgs.map((o) => (
                  <option key={o.id} value={o.id}>{o.name}</option>
                ))}
              </select>
            </div>
            <SubmitButton>Send invitation</SubmitButton>
          </form>
        </CardContent>
      </Card>

      {!invitationsResult.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load invitations: {invitationsResult.error}</p>
      ) : invitationsResult.data.length === 0 ? (
        <EmptyState icon={Mail} title="No invitations yet" description="Send the first invitation above." />
      ) : (
        <ul className="space-y-2">
          {invitationsResult.data.map((i) => (
            <li key={i.id}>
              <Card className="flex items-center justify-between gap-4 p-4">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-foreground">{i.email}</p>
                  <p className="text-xs text-muted-foreground">
                    {i.roleName}{i.memberEntityName ? ` · ${i.memberEntityName}` : ""} · invited by {i.invitedByName}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge variant={statusBadgeVariant(i.status)}>{STATUS_LABELS[i.status] ?? i.status}</Badge>
                  {i.status === 1 ? (
                    <form action={revokeInvitationAction.bind(null, i.id)}>
                      <SubmitButton size="sm" variant="destructive">Revoke</SubmitButton>
                    </form>
                  ) : null}
                </div>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
