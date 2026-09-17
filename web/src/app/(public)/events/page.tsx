import type { Metadata } from "next";
import Link from "next/link";
import { CalendarDays } from "lucide-react";
import { publicContentApi } from "@/lib/public-content/api";
import { Card } from "@/components/ui/card";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Events" };

export default async function EventsPage() {
  const result = await publicContentApi.listEvents();
  const items = result.ok ? result.data : [];

  return (
    <section className="mx-auto max-w-4xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <div>
        <h1 className="font-heading text-3xl font-bold text-foreground">Events</h1>
        <p className="mt-2 text-sm text-muted-foreground">Upcoming and past association events.</p>
      </div>

      {items.length === 0 ? (
        <EmptyState icon={CalendarDays} title="No events published yet" description="Association events will appear here once published from the Admin CMS." />
      ) : (
        <ul className="space-y-3">
          {items.map((e) => (
            <li key={e.slug}>
              <Link href={`/events/${e.slug}`}>
                <Card className="p-4 transition-colors hover:border-accent">
                  <p className="text-sm font-medium text-foreground">{e.title}</p>
                  <p className="mt-1 text-xs text-muted-foreground">
                    {e.eventDate ? new Date(e.eventDate).toLocaleString() : "Date TBA"}{e.location ? ` · ${e.location}` : ""}
                  </p>
                </Card>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
