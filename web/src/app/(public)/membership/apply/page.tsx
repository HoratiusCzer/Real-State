import type { Metadata } from "next";
import { ClipboardList } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Apply for membership" };

export default function MembershipApplyPage() {
  return (
    <PagePlaceholder
      icon={ClipboardList}
      title="Apply for membership"
      description="The membership application form will go live once the database and submission backend are built (Stage 3-4). Showing a non-functional form here would be misleading, so this page intentionally has no form yet."
    />
  );
}
