import type { Metadata } from "next";
import { Bell } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Notices" };

export default function NoticesPage() {
  return (
    <PagePlaceholder
      icon={Bell}
      title="Notices"
      description="Official association notices will appear here once published from the Admin CMS."
    />
  );
}
