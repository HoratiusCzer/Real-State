import type { Metadata } from "next";
import { Mail } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Contact", alternates: { canonical: "/contact" } };

export default function ContactPage() {
  return (
    <PagePlaceholder
      icon={Mail}
      title="Contact"
      description="Association contact details and a contact form will appear here once configured. A non-functional form isn't shown to avoid a fake submit flow."
    />
  );
}
