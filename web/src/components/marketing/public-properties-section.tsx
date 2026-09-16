import { Home } from "lucide-react";
import { SectionHeading } from "./section-heading";
import { EmptyState } from "@/components/ui/empty-state";
import { featureFlags } from "@/lib/feature-flags";

export function PublicPropertiesSection() {
  if (!featureFlags.publicPropertiesEnabled) {
    return null;
  }

  return (
    <section className="border-t border-border bg-card">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
        <SectionHeading eyebrow="Public exchange" title="Featured properties" />
        <div className="mt-6">
          <EmptyState icon={Home} title="No public listings yet" />
        </div>
      </div>
    </section>
  );
}
