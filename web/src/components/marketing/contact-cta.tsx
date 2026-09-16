import { Button } from "@/components/ui/button";

export function ContactCta() {
  return (
    <section className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
      <div className="rounded-lg border border-border bg-card px-6 py-10 text-center sm:px-10">
        <h2 className="font-heading text-2xl font-bold text-foreground">
          Have a question for the association?
        </h2>
        <p className="mx-auto mt-3 max-w-xl text-base text-muted-foreground">
          Reach out and a member of the association will get back to you.
        </p>
        <div className="mt-6">
          <Button href="/contact" variant="secondary" size="lg">
            Contact REAK
          </Button>
        </div>
      </div>
    </section>
  );
}
