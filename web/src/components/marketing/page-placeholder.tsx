import type { LucideIcon } from "lucide-react";
import { FileQuestion } from "lucide-react";
import { EmptyState } from "@/components/ui/empty-state";

export function PagePlaceholder({
  title,
  description,
  icon: Icon = FileQuestion,
  action,
}: {
  title: string;
  description: string;
  icon?: LucideIcon;
  action?: React.ReactNode;
}) {
  return (
    <section className="mx-auto max-w-3xl px-4 py-16 sm:px-6 lg:px-8">
      <h1 className="font-heading text-3xl font-bold text-foreground">{title}</h1>
      <div className="mt-8">
        <EmptyState icon={Icon} title="Not yet configured" description={description} action={action} />
      </div>
    </section>
  );
}
