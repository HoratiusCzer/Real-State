import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Building2, Globe } from "lucide-react";
import { publicContentApi } from "@/lib/public-content/api";
import { Card, CardContent } from "@/components/ui/card";

/** MemberEntity has no separate Slug column (same reasoning as /properties/[slug] — see that
 * route's comment) — this [slug] segment is the organization's id. */
export async function generateMetadata({ params }: { params: Promise<{ slug: string }> }): Promise<Metadata> {
  const { slug } = await params;
  const result = await publicContentApi.getMember(slug);
  if (!result.ok) return { title: "Member profile" };

  const { name, description } = result.data;
  return {
    title: name,
    description: description ?? undefined,
    alternates: { canonical: `/members/${slug}` },
    openGraph: { title: name, description: description ?? undefined, type: "website", url: `/members/${slug}` },
  };
}

export default async function MemberProfilePage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const result = await publicContentApi.getMember(slug);
  if (!result.ok) notFound();
  const member = result.data;

  return (
    <section className="mx-auto max-w-2xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <div className="flex items-center gap-4">
        {member.logoUrl ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img src={member.logoUrl} alt={member.name} className="h-16 w-16 rounded-md object-cover" />
        ) : (
          <div className="flex h-16 w-16 items-center justify-center rounded-md bg-muted text-muted-foreground">
            <Building2 className="h-8 w-8" aria-hidden="true" />
          </div>
        )}
        <h1 className="font-heading text-2xl font-bold text-foreground">{member.name}</h1>
      </div>

      {member.description ? <p className="text-sm text-foreground">{member.description}</p> : null}

      {member.website ? (
        <Card>
          <CardContent className="pt-6">
            <p className="flex items-center gap-2 text-sm text-foreground">
              <Globe className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
              <a href={member.website} target="_blank" rel="noopener noreferrer" className="text-primary hover:underline">
                {member.website}
              </a>
            </p>
          </CardContent>
        </Card>
      ) : null}
    </section>
  );
}
