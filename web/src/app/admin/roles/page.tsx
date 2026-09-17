import type { Metadata } from "next";
import { requireSession } from "@/lib/auth/session";
import { adminRolesApi } from "@/lib/admin/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

export const metadata: Metadata = { title: "Roles & Permissions" };

export default async function AdminRolesPage() {
  const { accessToken } = await requireSession();
  const result = await adminRolesApi.list(accessToken);

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Roles &amp; Permissions</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Read-only (spec §4.3 lists this as an Admin Portal screen, but editing the RBAC matrix live is
          security-sensitive enough to defer deliberately — see DEVELOPMENT_PLAN.md). Adjust the seeded
          role/permission matrix in <code>DatabaseSeeder.cs</code> for now.
        </p>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load roles: {result.error}</p>
      ) : (
        <ul className="space-y-3">
          {result.data.map((r) => (
            <li key={r.id}>
              <Card>
                <CardHeader className="flex flex-row items-center justify-between gap-4 space-y-0">
                  <CardTitle>{r.name}</CardTitle>
                  <Badge variant={r.scope === "System" ? "primary" : "accent"}>{r.scope}</Badge>
                </CardHeader>
                <CardContent className="flex flex-wrap gap-1.5">
                  {r.permissions.length === 0 ? (
                    <span className="text-sm text-muted-foreground">No permissions granted.</span>
                  ) : (
                    r.permissions.map((p) => (
                      <Badge key={p} variant="default">{p}</Badge>
                    ))
                  )}
                </CardContent>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
