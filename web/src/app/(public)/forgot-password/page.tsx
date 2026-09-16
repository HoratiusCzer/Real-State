import type { Metadata } from "next";
import { KeyRound } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Forgot password", robots: { index: false } };

export default function ForgotPasswordPage() {
  return (
    <PagePlaceholder
      icon={KeyRound}
      title="Forgot password"
      description="Password reset will be built alongside authentication in Stage 4."
    />
  );
}
