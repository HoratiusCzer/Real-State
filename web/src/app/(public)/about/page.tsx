import type { Metadata } from "next";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "About" };

export default function AboutPage() {
  return (
    <PagePlaceholder
      title="About REAK"
      description="Association background will appear here once published from the Admin CMS."
    />
  );
}
