import type { Metadata } from "next";
import { Building2 } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Edit property" };

export default function EditPropertyPage() {
  return (
    <PortalPlaceholder
      icon={Building2}
      title="Edit property"
      description="Property editing is built in Stage 6."
    />
  );
}
