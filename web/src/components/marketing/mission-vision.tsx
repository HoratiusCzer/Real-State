import { Compass, Target } from "lucide-react";
import { SectionHeading } from "./section-heading";
import { EmptyState } from "@/components/ui/empty-state";

export function MissionVision() {
  return (
    <section className="border-t border-border bg-card">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
        <SectionHeading eyebrow="Purpose" title="Mission &amp; vision" />
        <div className="mt-6 grid gap-6 sm:grid-cols-2">
          <EmptyState icon={Target} title="Mission not yet configured" />
          <EmptyState icon={Compass} title="Vision not yet configured" />
        </div>
      </div>
    </section>
  );
}
