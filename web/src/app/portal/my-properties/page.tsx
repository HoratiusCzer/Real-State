import type { Metadata } from "next";
import Link from "next/link";
import { Home, Plus } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { listingsApi } from "@/lib/listings/api";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { EmptyState } from "@/components/ui/empty-state";
import { statusBadgeVariant, statusLabel } from "@/lib/listings/status-badge";

export const metadata: Metadata = { title: "My Properties" };

export default async function MyPropertiesPage() {
  const { user, accessToken } = await requireSession();
  const membership = user.memberships[0];

  if (!membership) {
    return (
      <div className="space-y-4">
        <h1 className="font-heading text-2xl font-bold text-foreground">My properties</h1>
        <EmptyState icon={Home} title="No organization yet" description="Your account isn't linked to a member organization." />
      </div>
    );
  }

  const result = await listingsApi.search(accessToken, { memberEntityId: membership.memberEntityId, pageSize: 50 });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-heading text-2xl font-bold text-foreground">My properties</h1>
          <p className="mt-1 text-sm text-muted-foreground">{membership.memberEntityName}&apos;s listings, every status.</p>
        </div>
        <Button href="/portal/properties/new">
          <Plus className="h-4 w-4" /> New listing
        </Button>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load listings: {result.error}</p>
      ) : result.data.items.length === 0 ? (
        <EmptyState icon={Home} title="No listings yet" description="Create your organization's first property listing." />
      ) : (
        <ul className="space-y-2">
          {result.data.items.map((item) => (
            <li key={item.id}>
              <Link href={`/portal/properties/${item.id}`}>
                <Card className="flex items-center justify-between gap-4 p-4 transition-colors hover:border-accent">
                  <div className="min-w-0">
                    <p className="truncate font-medium text-foreground">{item.title}</p>
                    <p className="text-xs text-muted-foreground">
                      {item.referenceCode} · {item.currencyCode} {item.price.toLocaleString()}
                    </p>
                  </div>
                  <Badge variant={statusBadgeVariant(item.status)}>{statusLabel(item.status)}</Badge>
                </Card>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
