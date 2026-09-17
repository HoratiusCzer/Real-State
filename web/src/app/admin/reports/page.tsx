import type { Metadata } from "next";
import { BarChart3 } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Reports" };

export default function AdminReportsPage() {
  return (
    <PortalPlaceholder
      icon={BarChart3}
      title="Reports"
      description="Reporting is Stage 12 (spec §2.6: 'Feature flags + reports + audit'), not Stage 11. The Admin Dashboard already surfaces the key real-time counts; deeper historical reports are built next stage."
    />
  );
}
