import type { Metadata } from "next";
import Link from "next/link";
import { Sparkles } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { matchesApi } from "@/lib/matching/api";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { EmptyState } from "@/components/ui/empty-state";
import { Pagination } from "@/components/ui/pagination";
import { matchStatusBadgeVariant } from "@/lib/matching/status-badge";

export const metadata: Metadata = { title: "Matches" };

type SearchParams = Record<string, string | undefined>;

export default async function MatchesPage({ searchParams }: { searchParams: Promise<SearchParams> }) {
  const sp = await searchParams;
  const { accessToken } = await requireSession();
  const page = Number(sp.page ?? "1") || 1;

  const result = await matchesApi.search(accessToken, { status: sp.status, page, pageSize: 20 });

  if (!result.ok) {
    return <p className="text-sm text-destructive">Couldn&apos;t load matches: {result.error}</p>;
  }

  if (!result.data.configured) {
    return (
      <div className="space-y-4">
        <h1 className="font-heading text-2xl font-bold text-foreground">Matches</h1>
        <EmptyState
          icon={Sparkles}
          title="Matching is not yet configured by REAK"
          description="An administrator hasn't published a matching rule set yet. Once they do, compatible properties and requirements will be scored and explained here (spec §12)."
        />
      </div>
    );
  }

  const { result: search } = result.data;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Matches</h1>
        <p className="mt-1 text-sm text-muted-foreground">Properties and requirements the matching engine has scored as compatible.</p>
      </div>

      {search.items.length === 0 ? (
        <EmptyState
          icon={Sparkles}
          title="No matches yet"
          description="Matches appear here once your organization's approved listings or active requirements are compatible with the opposite side."
        />
      ) : (
        <>
          <ul className="space-y-2">
            {search.items.map((m) => (
              <li key={m.id}>
                <Link href={`/portal/matches/${m.id}`}>
                  <Card className="flex items-center justify-between gap-4 p-4 transition-colors hover:border-accent">
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium text-foreground">
                        {m.listingTitle} <span className="text-muted-foreground">×</span> {m.demandTitle}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {m.listingMemberEntityName} · {m.demandMemberEntityName}
                      </p>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className="font-heading text-lg font-semibold text-foreground">{m.score.toFixed(0)}%</span>
                      <Badge variant={matchStatusBadgeVariant(m.status)}>{m.status}</Badge>
                    </div>
                  </Card>
                </Link>
              </li>
            ))}
          </ul>
          <Pagination basePath="/portal/matches" page={search.page} pageSize={search.pageSize} totalCount={search.totalCount} searchParams={sp} />
        </>
      )}
    </div>
  );
}
