import type { Metadata } from "next";
import { ClipboardList } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "New requirement" };

export default function NewDemandPage() {
  return (
    <PortalPlaceholder
      icon={ClipboardList}
      title="New requirement"
      description="Requirement creation is built in Stage 7."
    />
  );
}
