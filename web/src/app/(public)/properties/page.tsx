import type { Metadata } from "next";
import { Home } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Properties" };

export default function PropertiesPage() {
  return (
    <PagePlaceholder
      icon={Home}
      title="Property exchange"
      description="Public property listings will appear here once Stage 6 (Property Exchange) is built and the public_properties_enabled feature flag is turned on."
    />
  );
}
