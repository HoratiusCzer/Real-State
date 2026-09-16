import type { Metadata } from "next";
import { ClipboardList } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Requirement detail" };

export default function DemandDetailPage() {
  return (
    <PortalPlaceholder
      icon={ClipboardList}
      title="Requirement detail"
      description="Requirement detail pages are built in Stage 7."
    />
  );
}
