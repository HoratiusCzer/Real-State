import type { Metadata } from "next";
import { History } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Audit Logs" };

export default function AdminAuditLogsPage() {
  return (
    <PortalPlaceholder
      icon={History}
      title="Audit Logs"
      description="Audit logging is Stage 12 (spec §2.6: 'Feature flags + reports + audit'), not Stage 11. The AuditLogs table and its append-only RLS trigger already exist and were verified in Stage 3/7's testing; the write path (actually recording events) and this viewer are both built next stage."
    />
  );
}
