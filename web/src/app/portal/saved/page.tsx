import type { Metadata } from "next";
import Link from "next/link";
import { Bookmark } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { listingsApi } from "@/lib/listings/api";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { EmptyState } from "@/components/ui/empty-state";
import { statusBadgeVariant, statusLabel } from "@/lib/listings/status-badge";

export const metadata: Metadata = { title: "Saved" };

export default async function SavedPage() {
  const { accessToken } = await requireSession();
  const result = await listingsApi.listSaved(accessToken);

  return (
    <div className="space-y-6">
      <h1 className="font-heading text-2xl font-bold text-foreground">Saved properties</h1>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load your saved properties: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={Bookmark} title="Nothing saved yet" description="Save a property from its detail page to find it here later." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((s) => (
            <li key={s.listingId}>
              <Link href={`/portal/properties/${s.listingId}`}>
                <Card className="flex items-center justify-between gap-4 p-4 transition-colors hover:border-accent">
                  <div className="min-w-0">
                    <p className="truncate font-medium text-foreground">{s.listing.title}</p>
                    <p className="text-xs text-muted-foreground">
                      {s.listing.referenceCode} · {s.listing.currencyCode} {s.listing.price.toLocaleString()} · Saved{" "}
                      {new Date(s.createdAt).toLocaleDateString()}
                    </p>
                  </div>
                  <Badge variant={statusBadgeVariant(s.listing.status)}>{statusLabel(s.listing.status)}</Badge>
                </Card>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
