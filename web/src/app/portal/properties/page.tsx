import type { Metadata } from "next";
import { Building2 } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Properties" };

export default function PortalPropertiesPage() {
  return (
    <PortalPlaceholder
      icon={Building2}
      title="Properties"
      description="Server-side search, filtering, and browsing of the shared property exchange is built in Stage 6."
    />
  );
}
