import type { Metadata } from "next";
import { History } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { auditLogsApi } from "@/lib/admin/api";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input, Label } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { EmptyState } from "@/components/ui/empty-state";
import { Pagination } from "@/components/ui/pagination";

export const metadata: Metadata = { title: "Audit Logs" };

type SearchParams = Record<string, string | undefined>;

export default async function AdminAuditLogsPage({ searchParams }: { searchParams: Promise<SearchParams> }) {
  const sp = await searchParams;
  const { accessToken } = await requireSession();
  const page = Number(sp.page ?? "1") || 1;

  const [logsResult, entityTypesResult] = await Promise.all([
    auditLogsApi.list(accessToken, { entityType: sp.entityType, action: sp.action, page }),
    auditLogsApi.entityTypes(accessToken),
  ]);
  const entityTypes = entityTypesResult.ok ? entityTypesResult.data : [];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Audit Logs</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Append-only trail (spec §19) — login/security, role, member, listing, demand, moderation,
          collaboration, contact-disclosure, feature-flag, and configuration events.
        </p>
      </div>

      <form className="flex flex-wrap items-end gap-3">
        <div>
          <Label htmlFor="entityType">Entity type</Label>
          <select id="entityType" name="entityType" defaultValue={sp.entityType ?? ""} className="flex h-11 w-48 rounded-md border border-border bg-card px-3 text-sm text-foreground">
            <option value="">All</option>
            {entityTypes.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </select>
        </div>
        <div>
          <Label htmlFor="action">Action</Label>
          <Input id="action" name="action" defaultValue={sp.action ?? ""} placeholder="e.g. ListingApproved" className="w-56" />
        </div>
        <SubmitButton variant="secondary">Filter</SubmitButton>
      </form>

      {!logsResult.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load audit logs: {logsResult.error}</p>
      ) : logsResult.data.items.length === 0 ? (
        <EmptyState icon={History} title="No audit entries found" description="Try a different filter, or check back once more activity has occurred." />
      ) : (
        <>
          <ul className="space-y-2">
            {logsResult.data.items.map((a) => (
              <li key={a.id}>
                <Card className="p-4">
                  <div className="flex items-start justify-between gap-4">
                    <div className="min-w-0">
                      <div className="flex items-center gap-2">
                        <Badge variant="accent">{a.action}</Badge>
                        <span className="text-xs text-muted-foreground">{a.entityType}</span>
                      </div>
                      {a.summary ? <p className="mt-1 text-sm text-foreground">{a.summary}</p> : null}
                      <p className="mt-1 text-xs text-muted-foreground">
                        {a.actorName ?? "System"} · {new Date(a.createdAt).toLocaleString()}{a.ipAddress ? ` · ${a.ipAddress}` : ""}
                      </p>
                    </div>
                  </div>
                </Card>
              </li>
            ))}
          </ul>
          <Pagination basePath="/admin/audit-logs" page={logsResult.data.page} pageSize={logsResult.data.pageSize} totalCount={logsResult.data.totalCount} searchParams={sp} />
        </>
      )}
    </div>
  );
}
