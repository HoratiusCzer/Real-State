import Link from "next/link";
import type { LucideIcon } from "lucide-react";
import { SectionHeading } from "./section-heading";
import { EmptyState } from "@/components/ui/empty-state";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export type ContentFeedItem = { href: string; title: string; dateLabel: string };

export function ContentFeedSection({
  eyebrow,
  title,
  icon,
  emptyTitle,
  emptyDescription,
  viewAllHref,
  viewAllLabel,
  items = [],
}: {
  eyebrow: string;
  title: string;
  icon: LucideIcon;
  emptyTitle: string;
  emptyDescription: string;
  viewAllHref: string;
  viewAllLabel: string;
  items?: ContentFeedItem[];
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
        {items.length === 0 ? (
          <EmptyState icon={icon} title={emptyTitle} description={emptyDescription} />
        ) : (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            {items.map((item) => (
              <Link key={item.href} href={item.href}>
                <Card className="p-4 transition-colors hover:border-accent">
                  <p className="text-sm font-medium text-foreground">{item.title}</p>
                  <p className="mt-1 text-xs text-muted-foreground">{item.dateLabel}</p>
                </Card>
              </Link>
            ))}
          </div>
        )}
      </div>
    </section>
  );
}
