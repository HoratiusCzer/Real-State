import { FileEdit } from "lucide-react";
import { SectionHeading } from "./section-heading";
import { EmptyState } from "@/components/ui/empty-state";

export function IntroSection() {
  return (
    <section className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
      <SectionHeading eyebrow="About" title="Introducing REAK" />
      <div className="mt-6">
        <EmptyState
          icon={FileEdit}
          title="Introduction content not yet configured"
          description="This section will present REAK's introduction once an association administrator publishes it from the Admin CMS."
        />
      </div>
    </section>
  );
}
