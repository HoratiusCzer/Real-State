import type { Metadata } from "next";
import { Newspaper } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "News article" };

export default async function NewsArticlePage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  await params;
  return (
    <PagePlaceholder
      icon={Newspaper}
      title="News article"
      description="This article will appear here once the CMS is built and content is published."
    />
  );
}
