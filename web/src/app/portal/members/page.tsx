import type { Metadata } from "next";
import Link from "next/link";
import { Users } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { memberEntitiesApi } from "@/lib/portal/api";
import { Card } from "@/components/ui/card";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Members" };

export default async function PortalMembersPage() {
  const { accessToken } = await requireSession();
  const result = await memberEntitiesApi.list(accessToken);
  const entities = result.ok ? result.data : [];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Members</h1>
        <p className="mt-1 text-sm text-muted-foreground">Other REAK member organizations.</p>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load members: {result.error}</p>
      ) : entities.length === 0 ? (
        <EmptyState icon={Users} title="No other members yet" description="Approved member organizations will appear here." />
      ) : (
        <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {entities.map((entity) => (
            <li key={entity.id}>
              <Link href={`/portal/members/${entity.id}`}>
                <Card className="p-5 transition-colors hover:border-accent">
                  <p className="font-heading font-semibold text-foreground">{entity.name}</p>
                  {entity.description ? (
                    <p className="mt-1 line-clamp-2 text-sm text-muted-foreground">{entity.description}</p>
                  ) : null}
                </Card>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
