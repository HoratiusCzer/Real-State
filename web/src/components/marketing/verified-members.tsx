import { Building2 } from "lucide-react";
import { SectionHeading } from "./section-heading";
import { EmptyState } from "@/components/ui/empty-state";
import { Button } from "@/components/ui/button";

export function VerifiedMembers() {
  return (
    <section className="border-t border-border bg-card">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
        <SectionHeading eyebrow="Network" title="Verified members" />
        <div className="mt-6">
          <EmptyState
            icon={Building2}
            title="No verified members published yet"
            description="Member organizations approved by the association will appear here once membership onboarding is live."
            action={
              <Button href="/members" variant="secondary" size="sm">
                View member directory
              </Button>
            }
          />
        </div>
      </div>
    </section>
  );
}
