import type { Metadata } from "next";
import { Building2 } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "New property" };

export default function NewPropertyPage() {
  return (
    <PortalPlaceholder
      icon={Building2}
      title="New property"
      description="The multi-step listing creation flow (spec §8.2) is built in Stage 6."
    />
  );
}
