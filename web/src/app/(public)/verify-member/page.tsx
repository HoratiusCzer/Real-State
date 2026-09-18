import type { Metadata } from "next";
import { ShieldCheck } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Verify a member", alternates: { canonical: "/verify-member" } };

export default function VerifyMemberPage() {
  return (
    <PagePlaceholder
      icon={ShieldCheck}
      title="Verify a member"
      description="Member verification will be available once the member directory and public data projection are built."
    />
  );
}
