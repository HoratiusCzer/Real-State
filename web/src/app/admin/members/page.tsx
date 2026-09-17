import type { Metadata } from "next";
import { Building2, Download } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { adminMemberEntitiesApi, featureFlagsApi } from "@/lib/admin/api";
import { suspendMemberEntityAction, reactivateMemberEntityAction } from "@/lib/admin/actions";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { SubmitButton } from "@/components/auth/submit-button";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Member Organizations" };

export default async function AdminMembersPage() {
  const { accessToken } = await requireSession();
  const [result, flagsResult] = await Promise.all([
    adminMemberEntitiesApi.list(accessToken),
    featureFlagsApi.list(accessToken),
  ]);
  const exportEnabled = flagsResult.ok && flagsResult.data.some((f) => f.key === "member_export_enabled" && f.isEnabled);

  return (
    <div className="max-w-3xl space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">Member Organizations</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Every REAK member organization, including suspended ones (spec §4.3). Suspending an org hides it
            from the member directory — it does not suspend its individual users; do that from Users.
          </p>
        </div>
        {exportEnabled ? (
          <Button href="/api/portal/admin/members/export" variant="secondary" size="sm">
            <Download className="h-4 w-4" /> Export CSV
          </Button>
        ) : null}
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load organizations: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={Building2} title="No member organizations yet" description="Organizations are created when a membership application is approved." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((m) => (
            <li key={m.id}>
              <Card className="flex items-center justify-between gap-4 p-4">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-foreground">{m.name}</p>
                  {m.description ? <p className="truncate text-xs text-muted-foreground">{m.description}</p> : null}
                </div>
                <div className="flex items-center gap-2">
                  <Badge variant={m.isActive ? "success" : "destructive"}>{m.isActive ? "Active" : "Suspended"}</Badge>
                  {m.isActive ? (
                    <form action={suspendMemberEntityAction.bind(null, m.id)}>
                      <SubmitButton size="sm" variant="destructive">Suspend</SubmitButton>
                    </form>
                  ) : (
                    <form action={reactivateMemberEntityAction.bind(null, m.id)}>
                      <SubmitButton size="sm" variant="secondary">Reactivate</SubmitButton>
                    </form>
                  )}
                </div>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
