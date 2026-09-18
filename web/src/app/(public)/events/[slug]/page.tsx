import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { publicContentApi } from "@/lib/public-content/api";

export async function generateMetadata({ params }: { params: Promise<{ slug: string }> }): Promise<Metadata> {
  const { slug } = await params;
  const result = await publicContentApi.getEvent(slug);
  if (!result.ok) return { title: "Event" };

  const { title, location } = result.data;
  return {
    title,
    description: location ?? undefined,
    alternates: { canonical: `/events/${slug}` },
    openGraph: { title, description: location ?? undefined, type: "article", url: `/events/${slug}` },
  };
}

export default async function EventPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const result = await publicContentApi.getEvent(slug);
  if (!result.ok) notFound();
  const evt = result.data;

  return (
    <article className="mx-auto max-w-3xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <div>
        <h1 className="font-heading text-3xl font-bold text-foreground">{evt.title}</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          {evt.eventDate ? new Date(evt.eventDate).toLocaleString() : "Date TBA"}{evt.location ? ` · ${evt.location}` : ""}
        </p>
      </div>
      {evt.body ? <div className="whitespace-pre-wrap text-sm leading-relaxed text-foreground">{evt.body}</div> : null}
    </article>
  );
}
