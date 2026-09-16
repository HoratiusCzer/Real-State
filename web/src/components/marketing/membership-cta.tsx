import { Button } from "@/components/ui/button";

export function MembershipCta() {
  return (
    <section className="border-t border-border bg-primary">
      <div className="mx-auto max-w-6xl px-4 py-16 text-center sm:px-6 lg:px-8">
        <h2 className="font-heading text-2xl font-bold text-on-primary sm:text-3xl">
          Join the REAK member network
        </h2>
        <p className="mx-auto mt-3 max-w-xl text-base text-on-primary/80">
          Submit a membership application and an association administrator will review it.
        </p>
        <div className="mt-8">
          <Button href="/membership/apply" variant="primary" size="lg">
            Apply for membership
          </Button>
        </div>
      </div>
    </section>
  );
}
