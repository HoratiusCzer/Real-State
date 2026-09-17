import type { Metadata } from "next";
import Link from "next/link";
import { CalendarDays } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { cmsEventsApi } from "@/lib/admin/api";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { EmptyState } from "@/components/ui/empty-state";
import { contentStatusBadgeVariant } from "@/lib/admin/status-badge";

export const metadata: Metadata = { title: "Events" };

export default async function AdminEventsListPage() {
  const { accessToken } = await requireSession();
  const result = await cmsEventsApi.list(accessToken);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">Events</h1>
          <p className="mt-1 text-sm text-muted-foreground">Association events (spec §4.3).</p>
        </div>
        <Button href="/admin/cms/events/new" size="sm">New event</Button>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load events: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={CalendarDays} title="No events yet" description="Create the first event." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((e) => (
            <li key={e.id}>
              <Link href={`/admin/cms/events/${e.id}`}>
                <Card className="flex items-center justify-between gap-4 p-4 transition-colors hover:border-accent">
                  <div className="min-w-0">
                    <p className="truncate text-sm font-medium text-foreground">{e.title}</p>
                    <p className="text-xs text-muted-foreground">
                      /{e.slug}{e.eventDate ? ` · ${new Date(e.eventDate).toLocaleDateString()}` : ""}{e.location ? ` · ${e.location}` : ""}
                    </p>
                  </div>
                  <Badge variant={contentStatusBadgeVariant(e.status)}>{e.status}</Badge>
                </Card>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
