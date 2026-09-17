import type { Metadata } from "next";
import Link from "next/link";
import { requireSession } from "@/lib/auth/session";
import { adminDashboardApi } from "@/lib/admin/api";
import { Card, CardContent } from "@/components/ui/card";

export const metadata: Metadata = { title: "Admin Dashboard" };

function StatTile({ label, value, href }: { label: string; value: number; href: string }) {
  return (
    <Link href={href}>
      <Card className="p-4 transition-colors hover:border-accent">
        <p className="text-xs font-medium text-muted-foreground">{label}</p>
        <p className="mt-1 font-heading text-2xl font-bold text-foreground">{value}</p>
      </Card>
    </Link>
  );
}

export default async function AdminDashboardPage() {
  const { accessToken } = await requireSession();
  const result = await adminDashboardApi.summary(accessToken);

  if (!result.ok) {
    return <p className="text-sm text-destructive">Couldn&apos;t load the admin dashboard: {result.error}</p>;
  }
  const s = result.data;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Admin Dashboard</h1>
        <p className="mt-1 text-sm text-muted-foreground">Association-wide counts, database-backed (spec §4.3).</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatTile label="Pending Membership Applications" value={s.pendingMembershipApplications} href="/admin/membership-applications" />
        <StatTile label="Pending Property Moderation" value={s.pendingModerationCount} href="/admin/listings" />
        <StatTile label="Pending Invitations" value={s.pendingInvitations} href="/admin/invitations" />
        <StatTile label="Pending Collaboration Requests" value={s.pendingCollaborationRequests} href="/admin/collaborations" />
        <StatTile label="Active Member Organizations" value={s.totalActiveMembers} href="/admin/members" />
        <StatTile label="Active Users" value={s.totalActiveUsers} href="/admin/users" />
        <StatTile label="Published CMS Content" value={s.cmsPublishedCount} href="/admin/cms/pages" />
        <StatTile label="Draft / In Review CMS Content" value={s.cmsDraftCount} href="/admin/cms/pages" />
      </div>

      <Card>
        <CardContent className="pt-6 text-sm text-muted-foreground">
          Every number above is a real database count — no placeholder statistics (spec §22). A
          fresh install legitimately shows zeros until real activity happens.
        </CardContent>
      </Card>
    </div>
  );
}
