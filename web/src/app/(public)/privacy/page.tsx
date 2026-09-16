import type { Metadata } from "next";
import { Scale } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Privacy policy" };

export default function PrivacyPage() {
  return (
    <PagePlaceholder
      icon={Scale}
      title="Privacy policy"
      description="REAK's privacy policy will appear here once the association publishes it through the Admin CMS. Legal text is not invented in the meantime."
    />
  );
}
