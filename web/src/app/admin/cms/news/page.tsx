import type { Metadata } from "next";
import Link from "next/link";
import { Newspaper } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { cmsNewsApi } from "@/lib/admin/api";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { EmptyState } from "@/components/ui/empty-state";
import { contentStatusBadgeVariant } from "@/lib/admin/status-badge";

export const metadata: Metadata = { title: "News" };

export default async function AdminNewsListPage() {
  const { accessToken } = await requireSession();
  const result = await cmsNewsApi.list(accessToken);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">News</h1>
          <p className="mt-1 text-sm text-muted-foreground">Association news articles (spec §4.3).</p>
        </div>
        <Button href="/admin/cms/news/new" size="sm">New article</Button>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load news: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={Newspaper} title="No articles yet" description="Create the first news article." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((n) => (
            <li key={n.id}>
              <Link href={`/admin/cms/news/${n.id}`}>
                <Card className="flex items-center justify-between gap-4 p-4 transition-colors hover:border-accent">
                  <div className="min-w-0">
                    <p className="truncate text-sm font-medium text-foreground">{n.title}</p>
                    <p className="text-xs text-muted-foreground">/{n.slug} · {n.authorProfileName}</p>
                  </div>
                  <Badge variant={contentStatusBadgeVariant(n.status)}>{n.status}</Badge>
                </Card>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
