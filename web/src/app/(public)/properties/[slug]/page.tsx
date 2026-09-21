import type { Metadata } from "next";
import Link from "next/link";
import Image from "next/image";
import { notFound } from "next/navigation";
import { Home } from "lucide-react";
import { publicPropertiesApi } from "@/lib/listings/api";
import { formatListingArea } from "@/lib/listings/land-area";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

/** REAK.Api's listings have no separate "slug" field (spec never requires one — only a
 * ReferenceCode and a UUID, unlike the CMS content types that do have Slug). This route's [slug]
 * segment is the listing's id, same as /portal/properties/[id]. */
export async function generateMetadata({ params }: { params: Promise<{ slug: string }> }): Promise<Metadata> {
  const { slug } = await params;
  const result = await publicPropertiesApi.get(slug);
  if (!result.ok) return { title: "Property" };

  const p = result.data;
  const description = `${p.propertyTypeName} for ${p.purposeName.toLowerCase()} in ${p.municipalityName}, ${p.districtName} — ${p.currencyCode} ${p.price.toLocaleString()}.`;
  return {
    title: p.title,
    description,
    alternates: { canonical: `/properties/${slug}` },
    openGraph: { title: p.title, description, type: "website", url: `/properties/${slug}`, images: p.mediaUrls[0] ? [p.mediaUrls[0]] : undefined },
  };
}

export default async function PropertyDetailPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const result = await publicPropertiesApi.get(slug);

  if (!result.ok) {
    if (result.status === 404) notFound();
    return (
      <PagePlaceholder
        icon={Home}
        title="Property"
        description="REAK's public property exchange isn't open yet. Check back later, or log in as a member to browse the network exchange."
      />
    );
  }

  const p = result.data;

  return (
    <section className="mx-auto max-w-3xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <div>
        <h1 className="font-heading text-3xl font-bold text-foreground">{p.title}</h1>
        <p className="mt-1 text-sm text-muted-foreground">{p.referenceCode} · {p.memberEntityName}</p>
      </div>

      {p.mediaUrls.length > 0 ? (
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
          {p.mediaUrls.map((url) => (
            <div key={url} className="relative aspect-square overflow-hidden rounded-md">
              <Image src={url} alt={p.title} fill sizes="(max-width: 640px) 50vw, 33vw" className="object-cover" />
            </div>
          ))}
        </div>
      ) : null}

      <Card>
        <CardHeader><CardTitle>Details</CardTitle></CardHeader>
        <CardContent className="grid grid-cols-2 gap-x-6 gap-y-2 text-sm sm:grid-cols-3">
          <Detail label="Price" value={`${p.currencyCode} ${p.price.toLocaleString()}${p.isPriceNegotiable ? " (negotiable)" : ""}`} />
          <Detail label="Type" value={`${p.propertyTypeName}${p.propertySubtypeName ? ` / ${p.propertySubtypeName}` : ""}`} />
          <Detail label="Purpose" value={p.purposeName} />
          <Detail label="Land area" value={formatListingArea(p)} />
          <Detail
            label="Location"
            value={[p.localityName, `Ward ${p.wardNumber}`, p.municipalityName, p.districtName, p.provinceName].filter(Boolean).join(", ")}
          />
          {p.bedrooms != null ? <Detail label="Bedrooms" value={String(p.bedrooms)} /> : null}
          {p.bathrooms != null ? <Detail label="Bathrooms" value={String(p.bathrooms)} /> : null}
        </CardContent>
      </Card>

      {p.description ? (
        <Card>
          <CardHeader><CardTitle>Description</CardTitle></CardHeader>
          <CardContent><p className="whitespace-pre-wrap text-sm text-foreground">{p.description}</p></CardContent>
        </Card>
      ) : null}

      {p.amenityNames.length > 0 ? (
        <Card>
          <CardHeader><CardTitle>Amenities</CardTitle></CardHeader>
          <CardContent className="flex flex-wrap gap-2">
            {p.amenityNames.map((a) => <Badge key={a}>{a}</Badge>)}
          </CardContent>
        </Card>
      ) : null}

      <p className="text-sm text-muted-foreground">
        Listed by {p.memberEntityName}, a REAK member organization.{" "}
        <Link href="/membership/apply" className="text-primary hover:underline">Apply for REAK membership</Link> to contact member
        organizations directly through the exchange.
      </p>
    </section>
  );
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="text-foreground">{value}</p>
    </div>
  );
}
