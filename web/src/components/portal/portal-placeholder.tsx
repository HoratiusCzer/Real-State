import type { LucideIcon } from "lucide-react";
import { FileQuestion } from "lucide-react";
import { EmptyState } from "@/components/ui/empty-state";

export function PortalPlaceholder({
  title,
  description,
  icon: Icon = FileQuestion,
}: {
  title: string;
  description: string;
  icon?: LucideIcon;
}) {
  return (
    <div className="space-y-4">
      <h1 className="font-heading text-2xl font-bold text-foreground">{title}</h1>
      <EmptyState icon={Icon} title="Not built yet" description={description} />
    </div>
  );
}
