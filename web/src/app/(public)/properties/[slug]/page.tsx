import type { Metadata } from "next";
import { Home } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Property" };

export default async function PropertyDetailPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  await params;
  return (
    <PagePlaceholder
      icon={Home}
      title="Property"
      description="This listing will appear here once the Property Exchange (Stage 6) is built."
    />
  );
}
