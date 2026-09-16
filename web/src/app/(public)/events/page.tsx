import type { Metadata } from "next";
import { CalendarDays } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Events" };

export default function EventsPage() {
  return (
    <PagePlaceholder
      icon={CalendarDays}
      title="Events"
      description="Association events will appear here once published from the Admin CMS."
    />
  );
}
