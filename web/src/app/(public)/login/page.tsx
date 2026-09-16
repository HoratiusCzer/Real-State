import type { Metadata } from "next";
import Link from "next/link";
import { LoginForm } from "@/components/auth/login-form";

export const metadata: Metadata = { title: "Log in", robots: { index: false } };

export default function LoginPage() {
  return (
    <section className="mx-auto max-w-md px-4 py-16 sm:px-6 lg:px-8">
      <h1 className="font-heading text-3xl font-bold text-foreground">Log in</h1>
      <p className="mt-2 text-sm text-muted-foreground">
        For REAK member organizations. Not a member yet?{" "}
        <Link href="/membership/apply" className="text-primary hover:underline">
          Apply for membership
        </Link>
        .
      </p>
      <div className="mt-8">
        <LoginForm />
      </div>
    </section>
  );
}
