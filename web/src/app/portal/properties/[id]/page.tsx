import type { Metadata } from "next";
import { Building2 } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Property detail" };

export default function PropertyDetailPage() {
  return (
    <PortalPlaceholder
      icon={Building2}
      title="Property detail"
      description="Property detail pages are built in Stage 6."
    />
  );
}
