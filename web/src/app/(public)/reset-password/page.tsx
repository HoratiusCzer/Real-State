import type { Metadata } from "next";
import { KeyRound } from "lucide-react";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Reset password", robots: { index: false } };

export default function ResetPasswordPage() {
  return (
    <PagePlaceholder
      icon={KeyRound}
      title="Reset password"
      description="Password reset will be built alongside authentication in Stage 4."
    />
  );
}
