import type { Metadata } from "next";
import Link from "next/link";
import { FileText } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { cmsPagesApi } from "@/lib/admin/api";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { EmptyState } from "@/components/ui/empty-state";
import { contentStatusBadgeVariant } from "@/lib/admin/status-badge";

export const metadata: Metadata = { title: "Pages" };

export default async function AdminPagesListPage() {
  const { accessToken } = await requireSession();
  const result = await cmsPagesApi.list(accessToken);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">Pages</h1>
          <p className="mt-1 text-sm text-muted-foreground">Homepage, about, mission, legal pages, and other static content (spec §4.3).</p>
        </div>
        <Button href="/admin/cms/pages/new" size="sm">New page</Button>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load pages: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={FileText} title="No pages yet" description="Create the first page — About, Privacy, Terms, etc." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((p) => (
            <li key={p.id}>
              <Link href={`/admin/cms/pages/${p.id}`}>
                <Card className="flex items-center justify-between gap-4 p-4 transition-colors hover:border-accent">
                  <div className="min-w-0">
                    <p className="truncate text-sm font-medium text-foreground">{p.title}</p>
                    <p className="text-xs text-muted-foreground">/{p.slug}</p>
                  </div>
                  <Badge variant={contentStatusBadgeVariant(p.status)}>{p.status}</Badge>
                </Card>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
