import type { Metadata } from "next";
import { CmsPageContent } from "@/components/marketing/cms-page-content";

export const metadata: Metadata = { title: "About", alternates: { canonical: "/about" } };

export default function AboutPage() {
  return (
    <CmsPageContent
      slug="about"
      fallbackTitle="About REAK"
      fallbackDescription="Association background will appear here once published from the Admin CMS."
    />
  );
}
