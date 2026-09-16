import type { Metadata } from "next";
import { Bell } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Notice" };

export default async function NoticePage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  await params;
  return (
    <PagePlaceholder
      icon={Bell}
      title="Notice"
      description="This notice will appear here once the CMS is built and content is published."
    />
  );
}
