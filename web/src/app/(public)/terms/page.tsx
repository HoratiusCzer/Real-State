import type { Metadata } from "next";
import { Scale } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Terms of use" };

export default function TermsPage() {
  return (
    <PagePlaceholder
      icon={Scale}
      title="Terms of use"
      description="REAK's terms of use will appear here once the association publishes them through the Admin CMS. Legal text is not invented in the meantime."
    />
  );
}
