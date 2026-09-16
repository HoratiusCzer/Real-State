import type { Metadata } from "next";
import { requireSession } from "@/lib/auth/session";
import { PropertyWizard } from "@/components/listings/wizard/property-wizard";
import { EmptyState } from "@/components/ui/empty-state";
import { Building2 } from "lucide-react";

export const metadata: Metadata = { title: "New property" };

export default async function NewPropertyPage() {
  const { user } = await requireSession();

  if (user.memberships.length === 0) {
    return (
      <EmptyState
        icon={Building2}
        title="No organization yet"
        description="Your account isn't linked to a REAK member organization, so you can't create a listing."
      />
    );
  }

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">New property</h1>
        <p className="mt-1 text-sm text-muted-foreground">List a property on the REAK exchange.</p>
      </div>
      <PropertyWizard />
    </div>
  );
}
