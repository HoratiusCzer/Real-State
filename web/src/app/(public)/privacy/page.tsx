import type { Metadata } from "next";
import { Scale } from "lucide-react";
import { CmsPageContent } from "@/components/marketing/cms-page-content";

export const metadata: Metadata = { title: "Privacy policy" };

export default function PrivacyPage() {
  return (
    <CmsPageContent
      slug="privacy"
      icon={Scale}
      fallbackTitle="Privacy policy"
      fallbackDescription="REAK's privacy policy will appear here once the association publishes it through the Admin CMS. Legal text is not invented in the meantime."
    />
  );
}
