import type { Metadata } from "next";
import { Flag } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Feature Flags" };

export default function AdminFeatureFlagsPage() {
  return (
    <PortalPlaceholder
      icon={Flag}
      title="Feature Flags"
      description="Feature flag management is Stage 12 (spec §2.6: 'Feature flags + reports + audit'), not Stage 11. The FeatureFlags table already exists and is read at runtime (e.g. property_moderation_required); an admin UI to toggle them is built next stage."
    />
  );
}
