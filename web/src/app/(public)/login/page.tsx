import type { Metadata } from "next";
import { LogIn } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Log in", robots: { index: false } };

export default function LoginPage() {
  return (
    <PagePlaceholder
      icon={LogIn}
      title="Log in"
      description="Real authentication will be built in Stage 4. A non-functional login form isn't shown here to avoid a fake-login flow."
    />
  );
}
