import type { Metadata } from "next";
import Link from "next/link";
import { Bell } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { cmsNoticesApi } from "@/lib/admin/api";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { EmptyState } from "@/components/ui/empty-state";
import { contentStatusBadgeVariant } from "@/lib/admin/status-badge";

export const metadata: Metadata = { title: "Notices" };

export default async function AdminNoticesListPage() {
  const { accessToken } = await requireSession();
  const result = await cmsNoticesApi.list(accessToken);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">Notices</h1>
          <p className="mt-1 text-sm text-muted-foreground">Official association notices (spec §4.3).</p>
        </div>
        <Button href="/admin/cms/notices/new" size="sm">New notice</Button>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load notices: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={Bell} title="No notices yet" description="Create the first notice." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((n) => (
            <li key={n.id}>
              <Link href={`/admin/cms/notices/${n.id}`}>
                <Card className="flex items-center justify-between gap-4 p-4 transition-colors hover:border-accent">
                  <div className="min-w-0">
                    <p className="truncate text-sm font-medium text-foreground">{n.title}</p>
                    <p className="text-xs text-muted-foreground">/{n.slug}</p>
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
