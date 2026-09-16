import type { Metadata } from "next";
import { ForgotPasswordForm } from "@/components/auth/forgot-password-form";

export const metadata: Metadata = { title: "Forgot password", robots: { index: false } };

export default function ForgotPasswordPage() {
  return (
    <section className="mx-auto max-w-md px-4 py-16 sm:px-6 lg:px-8">
      <h1 className="font-heading text-3xl font-bold text-foreground">Forgot password</h1>
      <p className="mt-2 text-sm text-muted-foreground">
        Enter the email on your REAK account and we&apos;ll send a link to reset your password.
      </p>
      <div className="mt-8">
        <ForgotPasswordForm />
      </div>
    </section>
  );
}
