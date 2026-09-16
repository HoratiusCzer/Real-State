import { Handshake, Search, ShieldCheck, Users } from "lucide-react";
import { SectionHeading } from "./section-heading";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";

const benefits = [
  {
    icon: Search,
    title: "Shared property exchange",
    description:
      "List properties and browse listings from other member companies in one network, subject to each listing's visibility settings.",
  },
  {
    icon: Users,
    title: "Client requirement matching",
    description:
      "Register client requirements and get matched against member listings — and vice versa — through configurable, explainable matching rules.",
  },
  {
    icon: Handshake,
    title: "Secure collaboration",
    description:
      "Coordinate on a match with another member in a dedicated workspace, with contact details disclosed only when you explicitly choose to share them.",
  },
  {
    icon: ShieldCheck,
    title: "Verified membership",
    description:
      "Every member organization is reviewed and approved by the association before joining the network.",
  },
];

export function Benefits() {
  return (
    <section className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
      <SectionHeading eyebrow="Membership" title="What member companies get" />
      <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {benefits.map((benefit) => (
          <Card key={benefit.title}>
            <CardHeader>
              <benefit.icon className="h-8 w-8 text-accent" aria-hidden="true" />
              <CardTitle className="mt-3">{benefit.title}</CardTitle>
            </CardHeader>
            <CardContent>
              <CardDescription>{benefit.description}</CardDescription>
            </CardContent>
          </Card>
        ))}
      </div>
    </section>
  );
}
