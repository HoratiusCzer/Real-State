import type { Metadata } from "next";
import { Bookmark } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Saved" };

export default function SavedPage() {
  return (
    <PortalPlaceholder
      icon={Bookmark}
      title="Saved properties"
      description="Saved filters and saved properties (spec §8.1) ship alongside property discovery in Stage 6."
    />
  );
}
