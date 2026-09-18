import type { Metadata } from "next";
import { MembershipApplicationForm } from "@/components/auth/membership-application-form";

export const metadata: Metadata = { title: "Apply for membership", alternates: { canonical: "/membership/apply" } };

export default function MembershipApplyPage() {
  return (
    <section className="mx-auto max-w-2xl px-4 py-16 sm:px-6 lg:px-8">
      <h1 className="font-heading text-3xl font-bold text-foreground">Apply for membership</h1>
      <p className="mt-2 text-sm text-muted-foreground">
        Submit your company&apos;s details below. REAK reviews applications and follows up by email — if
        approved, you&apos;ll receive an invitation to set up your organization&apos;s account.
      </p>
      <div className="mt-8">
        <MembershipApplicationForm />
      </div>
    </section>
  );
}
