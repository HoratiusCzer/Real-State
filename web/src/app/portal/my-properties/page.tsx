import type { Metadata } from "next";
import { Home } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "My Properties" };

export default function MyPropertiesPage() {
  return (
    <PortalPlaceholder
      icon={Home}
      title="My properties"
      description="Your organization's own listings management is built in Stage 6."
    />
  );
}
