import { SectionHeading } from "./section-heading";

const steps = [
  {
    step: "1",
    title: "List or register a requirement",
    description: "A member submits a property listing, or a client requirement (\"demand\").",
  },
  {
    step: "2",
    title: "The network matches it",
    description:
      "Once matching rules are configured and published by the association, the system evaluates listings against demands and shows an explainable match score.",
  },
  {
    step: "3",
    title: "Members collaborate",
    description:
      "A member can request collaboration on a match. If accepted, a shared workspace opens for messages, files, and notes.",
  },
  {
    step: "4",
    title: "Contact is shared explicitly",
    description:
      "Contact details are disclosed only when a member explicitly grants access within the collaboration — never automatically.",
  },
];

export function HowItWorks() {
  return (
    <section className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
      <SectionHeading eyebrow="How the network works" title="From listing to collaboration" />
      <ol className="mt-8 grid gap-8 sm:grid-cols-2 lg:grid-cols-4">
        {steps.map((item) => (
          <li key={item.step}>
            <span className="flex h-9 w-9 items-center justify-center rounded-full bg-primary font-heading text-sm font-bold text-on-primary">
              {item.step}
            </span>
            <h3 className="mt-3 font-heading text-base font-semibold text-foreground">
              {item.title}
            </h3>
            <p className="mt-1.5 text-sm text-muted-foreground">{item.description}</p>
          </li>
        ))}
      </ol>
    </section>
  );
}
