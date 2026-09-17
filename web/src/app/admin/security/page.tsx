import type { Metadata } from "next";
import { Lock } from "lucide-react";
import { PortalPlaceholder } from "@/components/portal/portal-placeholder";

export const metadata: Metadata = { title: "Security" };

export default function AdminSecurityPage() {
  return (
    <PortalPlaceholder
      icon={Lock}
      title="Security"
      description="Spec §4.3 lists a Security screen without specifying its contents beyond what's already covered elsewhere (RLS, RBAC, session/suspension handling — all built and tested in Stages 3-4). Deferred until there's a concrete, spec-driven requirement to build against, rather than guessing at scope."
    />
  );
}
