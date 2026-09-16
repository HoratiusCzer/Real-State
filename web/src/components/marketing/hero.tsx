import { Button } from "@/components/ui/button";

export function Hero() {
  return (
    <section className="border-b border-border bg-card">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 sm:py-24 lg:px-8">
        <div className="max-w-2xl">
          <p className="text-sm font-semibold uppercase tracking-wide text-accent">
            REAK
          </p>
          <h1 className="mt-3 font-heading text-4xl font-bold tracking-tight text-foreground sm:text-5xl">
            A trusted network for real estate professionals
          </h1>
          <p className="mt-5 text-lg text-muted-foreground">
            REAK connects member real estate companies through a shared property exchange,
            client requirement matching, and secure collaboration — under one association.
          </p>
          <div className="mt-8 flex flex-col gap-3 sm:flex-row">
            <Button href="/membership/apply" size="lg">
              Apply for membership
            </Button>
            <Button href="/about" variant="secondary" size="lg">
              About REAK
            </Button>
          </div>
        </div>
      </div>
    </section>
  );
}
