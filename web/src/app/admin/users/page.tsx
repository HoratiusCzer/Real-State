import type { Metadata } from "next";
import { Users } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { adminProfilesApi } from "@/lib/admin/api";
import { suspendProfileAction, reactivateProfileAction } from "@/lib/admin/actions";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { SubmitButton } from "@/components/auth/submit-button";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Users" };

export default async function AdminUsersPage({ searchParams }: { searchParams: Promise<{ search?: string }> }) {
  const { search } = await searchParams;
  const { accessToken } = await requireSession();
  const result = await adminProfilesApi.list(accessToken, search);

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Users</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Every profile, regardless of organization (spec §4.3). Suspension revokes access immediately, even
          for an already-active session.
        </p>
      </div>

      <form className="flex gap-2">
        <Input name="search" placeholder="Search by name or email…" defaultValue={search ?? ""} />
        <SubmitButton variant="secondary">Search</SubmitButton>
      </form>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load users: {result.error}</p>
      ) : result.data.length === 0 ? (
        <EmptyState icon={Users} title="No users found" description="Try a different search." />
      ) : (
        <ul className="space-y-2">
          {result.data.map((p) => (
            <li key={p.id}>
              <Card className="flex items-center justify-between gap-4 p-4">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-foreground">{p.fullName}</p>
                  <p className="text-xs text-muted-foreground">{p.email}</p>
                  <p className="text-xs text-muted-foreground">
                    {p.roles.join(", ") || "No roles"}{p.memberships.length > 0 ? ` · ${p.memberships.join(", ")}` : ""}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge variant={p.isActive ? "success" : "destructive"}>{p.isActive ? "Active" : "Suspended"}</Badge>
                  {p.isActive ? (
                    <form action={suspendProfileAction.bind(null, p.id)}>
                      <SubmitButton size="sm" variant="destructive">Suspend</SubmitButton>
                    </form>
                  ) : (
                    <form action={reactivateProfileAction.bind(null, p.id)}>
                      <SubmitButton size="sm" variant="secondary">Reactivate</SubmitButton>
                    </form>
                  )}
                </div>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
