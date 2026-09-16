import { Building2, ClipboardList } from "lucide-react";
import { SectionHeading } from "./section-heading";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";

export function ExchangeExplainer() {
  return (
    <section className="border-t border-border bg-card">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
        <SectionHeading
          eyebrow="How it works"
          title="The Member Property Exchange"
          description="Listings and client requirements are two sides of the same network — the exchange is where they meet."
        />
        <div className="mt-8 grid gap-6 sm:grid-cols-2">
          <Card>
            <CardHeader>
              <Building2 className="h-8 w-8 text-accent" aria-hidden="true" />
              <CardTitle className="mt-3">Property listings</CardTitle>
            </CardHeader>
            <CardContent>
              <CardDescription>
                Member companies list properties with location, specifications, pricing, and
                media. Contact details stay private to the listing organization until
                explicitly shared through collaboration.
              </CardDescription>
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <ClipboardList className="h-8 w-8 text-accent" aria-hidden="true" />
              <CardTitle className="mt-3">Client requirements (demands)</CardTitle>
            </CardHeader>
            <CardContent>
              <CardDescription>
                Member companies register what their clients are looking for — buying,
                renting, or investing — while keeping the client&apos;s identity private.
              </CardDescription>
            </CardContent>
          </Card>
        </div>
      </div>
    </section>
  );
}
