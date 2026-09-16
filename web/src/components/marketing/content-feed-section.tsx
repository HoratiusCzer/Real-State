import type { LucideIcon } from "lucide-react";
import { SectionHeading } from "./section-heading";
import { EmptyState } from "@/components/ui/empty-state";
import { Button } from "@/components/ui/button";

export function ContentFeedSection({
  eyebrow,
  title,
  icon,
  emptyTitle,
  emptyDescription,
  viewAllHref,
  viewAllLabel,
}: {
  eyebrow: string;
  title: string;
  icon: LucideIcon;
  emptyTitle: string;
  emptyDescription: string;
  viewAllHref: string;
  viewAllLabel: string;
}) {
  return (
    <section className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <SectionHeading eyebrow={eyebrow} title={title} />
        <Button href={viewAllHref} variant="ghost" size="sm">
          {viewAllLabel}
        </Button>
      </div>
      <div className="mt-6">
        <EmptyState icon={icon} title={emptyTitle} description={emptyDescription} />
      </div>
    </section>
  );
}
