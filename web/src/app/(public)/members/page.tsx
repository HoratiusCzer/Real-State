import type { Metadata } from "next";
import { Building2 } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Members" };

export default function MembersPage() {
  return (
    <PagePlaceholder
      icon={Building2}
      title="Member directory"
      description="Verified member organizations will be listed here once membership onboarding is live (Stage 4+)."
    />
  );
}
