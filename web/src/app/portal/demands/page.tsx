import type { Metadata } from "next";
import Link from "next/link";
import { ClipboardList, Plus } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { demandsApi } from "@/lib/demands/api";
import { referenceApi } from "@/lib/listings/reference-api";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/input";
import { EmptyState } from "@/components/ui/empty-state";
import { Pagination } from "@/components/ui/pagination";
import { demandStatusBadgeVariant } from "@/lib/demands/status-badge";

export const metadata: Metadata = { title: "Requirements" };

const selectClass =
  "flex h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";

type SearchParams = Record<string, string | undefined>;

export default async function DemandsPage({ searchParams }: { searchParams: Promise<SearchParams> }) {
  const sp = await searchParams;
  const { accessToken } = await requireSession();
  const page = Number(sp.page ?? "1") || 1;

  const [result, purposesResult] = await Promise.all([
    demandsApi.search(accessToken, { purposeId: sp.purposeId, page, pageSize: 20 }),
    referenceApi.purposes(),
  ]);
  const purposes = purposesResult.ok ? purposesResult.data : [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">Requirements</h1>
          <p className="mt-1 text-sm text-muted-foreground">Client requirements registered by REAK members, visibility permitting.</p>
        </div>
        <Button href="/portal/demands/new">
          <Plus className="h-4 w-4" /> New requirement
        </Button>
      </div>

      <form method="get" action="/portal/demands" className="flex items-end gap-3 rounded-lg border border-border bg-card p-4">
        <div className="w-56">
          <Label htmlFor="purposeId">Purpose</Label>
          <select id="purposeId" name="purposeId" defaultValue={sp.purposeId ?? ""} className={selectClass}>
            <option value="">Any</option>
            {purposes.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
          </select>
        </div>
        <Button type="submit" variant="secondary">Filter</Button>
      </form>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load requirements: {result.error}</p>
      ) : result.data.items.length === 0 ? (
        <EmptyState icon={ClipboardList} title="No requirements found" description="Try adjusting your filters, or register a new requirement." />
      ) : (
        <>
          <ul className="space-y-2">
            {result.data.items.map((item) => (
              <li key={item.id}>
                <Link href={`/portal/demands/${item.id}`}>
                  <Card className="flex items-center justify-between gap-4 p-4 transition-colors hover:border-accent">
                    <div className="min-w-0">
                      <p className="truncate font-medium text-foreground">{item.title}</p>
                      <p className="text-xs text-muted-foreground">
                        {item.referenceCode} · {item.memberEntityName} · {item.purposeName}
                        {item.minBudget || item.maxBudget
                          ? ` · ${item.currencyCode ?? ""} ${item.minBudget?.toLocaleString() ?? "?"}–${item.maxBudget?.toLocaleString() ?? "?"}`
                          : ""}
                      </p>
                      {item.propertyTypeNames.length > 0 || item.locationSummaries.length > 0 ? (
                        <p className="mt-0.5 text-xs text-muted-foreground">
                          {item.propertyTypeNames.join(", ")}
                          {item.propertyTypeNames.length > 0 && item.locationSummaries.length > 0 ? " · " : ""}
                          {item.locationSummaries.join(", ")}
                        </p>
                      ) : null}
                    </div>
                    <Badge variant={demandStatusBadgeVariant(item.status)}>{item.status}</Badge>
                  </Card>
                </Link>
              </li>
            ))}
          </ul>
          <Pagination basePath="/portal/demands" page={result.data.page} pageSize={result.data.pageSize} totalCount={result.data.totalCount} searchParams={sp} />
        </>
      )}
    </div>
  );
}
