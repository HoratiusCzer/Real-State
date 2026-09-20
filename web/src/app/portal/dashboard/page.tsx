import type { Metadata } from "next";
import Link from "next/link";
import { requireSession } from "@/lib/auth/session";
import { dashboardApi } from "@/lib/portal/api";
import { StatTile } from "@/components/portal/stat-tile";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { EmptyState } from "@/components/ui/empty-state";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { FolderClock, Building2, ClipboardList, Sparkles, Handshake } from "lucide-react";

export const metadata: Metadata = { title: "Dashboard" };

export default async function DashboardPage() {
  const { user, accessToken } = await requireSession();
  const hasOrganization = user.memberships.length > 0;
  const result = await dashboardApi.summary(accessToken);
  const summary = result.ok
    ? result.data
    : {
        activePropertiesCount: 0,
        draftPropertiesCount: 0,
        activeRequirementsCount: 0,
        potentialMatchesCount: 0,
        pendingCollaborationRequestsCount: 0,
        savedPropertiesCount: null,
        expiringItemsCount: 0,
        recentProperties: [],
      };

  return (
    <div className="space-y-8">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Dashboard</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          A snapshot of your organization&apos;s activity on the REAK exchange.
        </p>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        <StatTile label="Active properties" value={summary.activePropertiesCount} />
        <StatTile label="Draft properties" value={summary.draftPropertiesCount} />
        <StatTile label="Active requirements" value={summary.activeRequirementsCount} />
        <StatTile label="Potential matches" value={summary.potentialMatchesCount} />
        <StatTile label="Collaboration requests" value={summary.pendingCollaborationRequestsCount} />
        <StatTile
          label="Saved properties"
          value={summary.savedPropertiesCount}
          unavailableNote="Coming with Property Exchange"
        />
        <StatTile label="Expiring items" value={summary.expiringItemsCount} />
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Recent properties</CardTitle>
          </CardHeader>
          <CardContent>
            {summary.recentProperties.length > 0 ? (
              <ul className="divide-y divide-border">
                {summary.recentProperties.map((p) => (
                  <li key={p.id} className="flex items-center justify-between gap-4 py-3">
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium text-foreground">{p.title}</p>
                      <p className="text-xs text-muted-foreground">{p.referenceCode}</p>
                    </div>
                    <Badge>{p.status}</Badge>
                  </li>
                ))}
              </ul>
            ) : (
              <EmptyState
                icon={FolderClock}
                title="No properties yet"
                description={
                  hasOrganization
                    ? "Create your organization's first property listing to see it here."
                    : "Your account isn't linked to a REAK member organization, so there's nothing to show here."
                }
              />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Quick actions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <Button href="/portal/properties" variant="secondary" className="w-full justify-start">
              <Building2 className="h-4 w-4" /> Browse properties
            </Button>
            <Button href="/portal/demands" variant="secondary" className="w-full justify-start">
              <ClipboardList className="h-4 w-4" /> View requirements
            </Button>
            <Button href="/portal/matches" variant="secondary" className="w-full justify-start">
              <Sparkles className="h-4 w-4" /> Review matches
            </Button>
            <Button href="/portal/collaborations" variant="secondary" className="w-full justify-start">
              <Handshake className="h-4 w-4" /> Collaborations
            </Button>
          </CardContent>
        </Card>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Dashboard data couldn&apos;t be loaded: {result.error}</p>
      ) : null}

      <p className="text-center">
        <Link href="/portal/organization" className="text-xs text-muted-foreground hover:underline">
          Manage your organization
        </Link>
      </p>
    </div>
  );
}
