import type { Metadata } from "next";
import { Database } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Reference Data" };

export default function AdminReferenceDataPage() {
  return (
    <PortalPlaceholder
      icon={Database}
      title="Reference Data"
      description="Property types, subtypes, purposes, amenities, Nepal's location hierarchy, area units, and currencies are all readable and creatable via the API today (Stage 6's ReferenceDataController — settings.manage). A dedicated edit/disable/reorder screen for all eleven tables is deliberately deferred — most of this is fixed geography or taxonomy set up once, not day-to-day editorial content."
    />
  );
}
