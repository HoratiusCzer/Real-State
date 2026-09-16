import type { Metadata } from "next";
import { ClipboardList } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Edit requirement" };

export default function EditDemandPage() {
  return (
    <PortalPlaceholder
      icon={ClipboardList}
      title="Edit requirement"
      description="Requirement editing is built in Stage 7."
    />
  );
}
