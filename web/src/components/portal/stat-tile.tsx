import { Card } from "@/components/ui/card";
import { cn } from "@/lib/utils";

/**
 * Stat tile per the dataviz skill's figures contract: sentence-case label (no trailing colon),
 * semibold proportional-figure value. `value: null` means the underlying feature doesn't exist
 * yet (e.g. Saved Properties, no Stage 6 saved-listings table) and is rendered as a muted em
 * dash with a caption — visually distinct from a real, database-backed zero, which renders as an
 * ordinary "0" in full-weight ink. Never collapse those two states into the same look.
 */
export function StatTile({
  label,
  value,
  unavailableNote,
  className,
}: {
  label: string;
  value: number | null;
  unavailableNote?: string;
  className?: string;
}) {
  const isAvailable = value !== null;

  return (
    <Card className={cn("p-5", className)}>
      <p className="text-sm text-muted-foreground">{label}</p>
      {isAvailable ? (
        <p className="mt-1 font-sans text-3xl font-semibold text-foreground">{value.toLocaleString()}</p>
      ) : (
        <>
          <p className="mt-1 font-sans text-3xl font-semibold text-muted-foreground/50" aria-hidden="true">
            —
          </p>
          <p className="text-xs text-muted-foreground">{unavailableNote ?? "Not available yet"}</p>
        </>
      )}
    </Card>
  );
}
