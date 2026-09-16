import type { Metadata } from "next";
import { Building } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { memberEntitiesApi } from "@/lib/portal/api";
import { OrganizationForm } from "@/components/portal/organization-form";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Organization" };

export default async function PortalOrganizationPage() {
  const { user, accessToken } = await requireSession();
  const membership = user.memberships[0];

  if (!membership) {
    return (
      <div className="space-y-4">
        <h1 className="font-heading text-2xl font-bold text-foreground">Organization</h1>
        <EmptyState
          icon={Building}
          title="No organization yet"
          description="Your account isn't linked to a REAK member organization. Contact your administrator if this seems wrong."
        />
      </div>
    );
  }

  const result = await memberEntitiesApi.get(accessToken, membership.memberEntityId);
  const canEdit = user.permissions.includes("members.update");

  return (
    <div className="max-w-2xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Organization</h1>
        <p className="mt-1 text-sm text-muted-foreground">{membership.memberEntityName}</p>
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load organization details: {result.error}</p>
      ) : canEdit ? (
        <OrganizationForm entity={result.data} />
      ) : (
        <p className="text-sm text-muted-foreground">
          Only your organization&apos;s Member Admin can edit these details.
        </p>
      )}
    </div>
  );
}
