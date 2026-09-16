import type { Metadata } from "next";
import { Building2 } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Member profile" };

export default async function MemberProfilePage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  await params;
  return (
    <PagePlaceholder
      icon={Building2}
      title="Member profile"
      description="This member's public profile will appear here once the member directory is live."
    />
  );
}
