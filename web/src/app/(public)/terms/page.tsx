import type { Metadata } from "next";
import { Scale } from "lucide-react";
import { CmsPageContent } from "@/components/marketing/cms-page-content";

export const metadata: Metadata = { title: "Terms of use" };

export default function TermsPage() {
  return (
    <CmsPageContent
      slug="terms"
      icon={Scale}
      fallbackTitle="Terms of use"
      fallbackDescription="REAK's terms of use will appear here once the association publishes them through the Admin CMS. Legal text is not invented in the meantime."
    />
  );
}
