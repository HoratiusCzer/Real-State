import type { Metadata } from "next";
import { FileText } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Resources" };

export default function ResourcesPage() {
  return (
    <PagePlaceholder
      icon={FileText}
      title="Resources"
      description="Association resources will appear here once published from the Admin CMS."
    />
  );
}
