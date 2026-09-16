import type { Metadata } from "next";
import { Sparkles } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Match detail" };

export default function MatchDetailPage() {
  return (
    <PortalPlaceholder
      icon={Sparkles}
      title="Match detail"
      description="Match detail and explanation pages are built in Stage 8."
    />
  );
}
