import type { Metadata } from "next";
import { ClipboardList } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Requirements" };

export default function DemandsPage() {
  return (
    <PortalPlaceholder
      icon={ClipboardList}
      title="Requirements"
      description="Client requirement (demand) registration and search (spec §9) are built in Stage 7."
    />
  );
}
