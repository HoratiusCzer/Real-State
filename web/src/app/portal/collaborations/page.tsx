import type { Metadata } from "next";
import { Handshake } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Collaborations" };

export default function CollaborationsPage() {
  return (
    <PortalPlaceholder
      icon={Handshake}
      title="Collaborations"
      description="Collaboration requests, workspaces, and contact disclosure (spec §13) are built in Stage 9."
    />
  );
}
