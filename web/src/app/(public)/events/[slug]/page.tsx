import type { Metadata } from "next";
import { CalendarDays } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Event" };

export default async function EventPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  await params;
  return (
    <PagePlaceholder
      icon={CalendarDays}
      title="Event"
      description="This event will appear here once the CMS is built and content is published."
    />
  );
}
