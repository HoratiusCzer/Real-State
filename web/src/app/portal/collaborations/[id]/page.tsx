import type { Metadata } from "next";
import { Handshake } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Collaboration workspace" };

export default function CollaborationDetailPage() {
  return (
    <PortalPlaceholder
      icon={Handshake}
      title="Collaboration workspace"
      description="Collaboration workspaces (messages, files, notes, tasks, contact disclosure) are built in Stage 9."
    />
  );
}
