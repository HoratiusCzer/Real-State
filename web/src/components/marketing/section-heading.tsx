import { cn } from "@/lib/utils";

export function SectionHeading({
  eyebrow,
  title,
  description,
  className,
}: {
  eyebrow?: string;
  title: string;
  description?: string;
  className?: string;
}) {
  return (
    <div className={cn("max-w-2xl", className)}>
      {eyebrow ? (
        <p className="text-sm font-semibold uppercase tracking-wide text-accent">{eyebrow}</p>
      ) : null}
      <h2 className="mt-2 font-heading text-2xl font-bold text-foreground sm:text-3xl">
        {title}
      </h2>
      {description ? (
        <p className="mt-3 text-base text-muted-foreground">{description}</p>
      ) : null}
    </div>
  );
}
