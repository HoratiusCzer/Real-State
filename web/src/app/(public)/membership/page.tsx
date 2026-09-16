import type { Metadata } from "next";
import { Handshake } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";
import { Button } from "@/components/ui/button";

export const metadata: Metadata = { title: "Membership" };

export default function MembershipPage() {
  return (
    <PagePlaceholder
      icon={Handshake}
      title="Membership"
      description="Membership policies and benefits will appear here once published by the association."
      action={
        <Button href="/membership/apply" variant="primary" size="sm">
          Apply for membership
        </Button>
      }
    />
  );
}
