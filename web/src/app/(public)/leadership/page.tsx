import type { Metadata } from "next";
import { Users } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Leadership" };

export default function LeadershipPage() {
  return (
    <PagePlaceholder
      icon={Users}
      title="Leadership"
      description="Committee and leadership information will appear here once published by the association."
    />
  );
}
