import type { Metadata } from "next";
import { Newspaper } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "News" };

export default function NewsPage() {
  return (
    <PagePlaceholder
      icon={Newspaper}
      title="News"
      description="Association news will appear here once published from the Admin CMS (Stage 11)."
    />
  );
}
