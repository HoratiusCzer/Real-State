import type { Metadata } from "next";
import { Sparkles } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Matches" };

export default function MatchesPage() {
  return (
    <PortalPlaceholder
      icon={Sparkles}
      title="Matches"
      description="The bidirectional matching engine (spec §12) is built in Stage 8."
    />
  );
}
