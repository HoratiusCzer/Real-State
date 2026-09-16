import type { Metadata } from "next";
import { ClipboardList } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { DemandWizard } from "@/components/demands/demand-wizard";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "New requirement" };

export default async function NewDemandPage() {
  const { user } = await requireSession();

  if (user.memberships.length === 0) {
    return (
      <EmptyState
        icon={ClipboardList}
        title="No organization yet"
        description="Your account isn't linked to a REAK member organization, so you can't register a requirement."
      />
    );
  }

  return (
    <div className="max-w-3xl space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">New requirement</h1>
        <p className="mt-1 text-sm text-muted-foreground">Register a client&apos;s requirement on the REAK exchange.</p>
      </div>
      <DemandWizard />
    </div>
  );
}
