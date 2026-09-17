import type { Metadata } from "next";
import { requireSession } from "@/lib/auth/session";
import { reportsApi } from "@/lib/admin/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export const metadata: Metadata = { title: "Reports" };

function Breakdown({ items }: { items: { status: string; count: number }[] }) {
  if (items.length === 0) return <p className="text-sm text-muted-foreground">No data yet.</p>;
  const total = items.reduce((sum, i) => sum + i.count, 0);
  return (
    <ul className="space-y-2">
      {items.map((i) => (
        <li key={i.status} className="flex items-center justify-between gap-4 text-sm">
          <span className="text-foreground">{i.status}</span>
          <div className="flex flex-1 items-center gap-2">
            <div className="h-2 flex-1 rounded-full bg-muted">
              <div className="h-2 rounded-full bg-accent" style={{ width: `${total === 0 ? 0 : (i.count / total) * 100}%` }} />
            </div>
            <span className="w-8 text-right font-medium text-foreground">{i.count}</span>
          </div>
        </li>
      ))}
    </ul>
  );
}

export default async function AdminReportsPage() {
  const { accessToken } = await requireSession();
  const result = await reportsApi.summary(accessToken);

  if (!result.ok) {
    return <p className="text-sm text-destructive">Couldn&apos;t load reports: {result.error}</p>;
  }
  const r = result.data;

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Reports</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Spec §4.3 names a Reports screen without a dedicated content spec (unlike feature flags
          and audit logging) — real, database-backed breakdowns of what already exists, deliberately
          modest rather than invented metrics.
        </p>
      </div>

      <div className="grid gap-6 sm:grid-cols-2">
        <Card>
          <CardHeader><CardTitle>Properties by status</CardTitle></CardHeader>
          <CardContent><Breakdown items={r.listingsByStatus} /></CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Requirements by status</CardTitle></CardHeader>
          <CardContent><Breakdown items={r.demandsByStatus} /></CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Matches by status</CardTitle></CardHeader>
          <CardContent><Breakdown items={r.matchesByStatus} /></CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle>Collaboration requests by status</CardTitle></CardHeader>
          <CardContent><Breakdown items={r.collaborationsByStatus} /></CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader><CardTitle>Member growth (last 12 months)</CardTitle></CardHeader>
        <CardContent>
          {r.memberGrowth.length === 0 ? (
            <p className="text-sm text-muted-foreground">No new member organizations in this window.</p>
          ) : (
            <ul className="space-y-1">
              {r.memberGrowth.map((m) => (
                <li key={m.month} className="flex items-center justify-between text-sm">
                  <span className="text-foreground">{m.month}</span>
                  <span className="font-medium text-foreground">{m.count}</span>
                </li>
              ))}
            </ul>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
