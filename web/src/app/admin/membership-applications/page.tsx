import type { Metadata } from "next";
import { ClipboardCheck } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { adminMembershipApplicationsApi } from "@/lib/admin/api";
import { approveMembershipApplicationAction, rejectMembershipApplicationAction } from "@/lib/admin/actions";
import { MEMBERSHIP_APPLICATION_STATUS_LABELS } from "@/lib/admin/types";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Membership Applications" };

function statusBadgeVariant(status: number): "default" | "success" | "destructive" | "accent" {
  switch (status) {
    case 2: return "success";
    case 3: return "destructive";
    default: return "accent";
  }
}

export default async function AdminMembershipApplicationsPage() {
  const { accessToken } = await requireSession();
  const result = await adminMembershipApplicationsApi.list(accessToken);

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Membership Applications</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Flow A step 1-2 (spec §2.4) — approving creates the organization and a MemberAdmin invitation.
        </p>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load applications: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={ClipboardCheck} title="No applications yet" description="Submissions from the public membership application form will appear here." />
      ) : (
        <ul className="space-y-3">
          {result.data.map((a) => (
            <li key={a.id}>
              <Card className="p-4">
                <div className="flex items-start justify-between gap-4">
                  <div className="min-w-0">
                    <p className="text-sm font-medium text-foreground">{a.companyName}</p>
                    <p className="text-xs text-muted-foreground">{a.contactName} · {a.email} · {a.phone}</p>
                    {a.message ? <p className="mt-1 text-sm text-foreground">{a.message}</p> : null}
                    {a.rejectionReason ? <p className="mt-1 text-xs text-destructive">Rejected: {a.rejectionReason}</p> : null}
                  </div>
                  <Badge variant={statusBadgeVariant(a.status)}>{MEMBERSHIP_APPLICATION_STATUS_LABELS[a.status] ?? a.status}</Badge>
                </div>
                {a.status === 1 ? (
                  <div className="mt-3 flex flex-wrap items-start gap-2">
                    <form action={approveMembershipApplicationAction.bind(null, a.id)}>
                      <SubmitButton size="sm">Approve</SubmitButton>
                    </form>
                    <form action={rejectMembershipApplicationAction.bind(null, a.id)} className="flex items-start gap-2">
                      <Textarea name="reason" placeholder="Reason for rejection" rows={1} required className="w-64" />
                      <SubmitButton size="sm" variant="destructive">Reject</SubmitButton>
                    </form>
                  </div>
                ) : null}
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
