import type { Metadata } from "next";
import { Building } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { listingsApi } from "@/lib/listings/api";
import { approveListingAction, rejectListingAction } from "@/lib/admin/listing-actions";
import { Card } from "@/components/ui/card";
import { Textarea } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Property Moderation" };

export default async function AdminListingsPage() {
  const { accessToken } = await requireSession();
  const result = await listingsApi.search(accessToken, { status: "PendingReview", pageSize: 50 });

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Property Moderation</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Listings awaiting review (spec §4.3). Only shown when <code>property_moderation_required</code> is
          enabled — otherwise listings publish immediately on submission and never reach this queue.
        </p>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load listings: {result.error}</p>
      ) : result.data.items.length === 0 ? (
        <EmptyState icon={Building} title="Nothing pending review" description="Submitted listings awaiting moderation will appear here." />
      ) : (
        <ul className="space-y-3">
          {result.data.items.map((l) => (
            <li key={l.id}>
              <Card className="p-4">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-foreground">{l.title}</p>
                  <p className="text-xs text-muted-foreground">
                    {l.referenceCode} · {l.memberEntityName} · {l.propertyTypeName} · {l.provinceName}
                  </p>
                </div>
                <div className="mt-3 flex flex-wrap items-start gap-2">
                  <form action={approveListingAction.bind(null, l.id)}>
                    <SubmitButton size="sm">Approve</SubmitButton>
                  </form>
                  <form action={rejectListingAction.bind(null, l.id)} className="flex items-start gap-2">
                    <Textarea name="reason" placeholder="Reason for rejection" rows={1} required className="w-64" />
                    <SubmitButton size="sm" variant="destructive">Reject</SubmitButton>
                  </form>
                </div>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
