import type { Metadata } from "next";
import { Home } from "lucide-react";
import { publicPropertiesApi } from "@/lib/listings/api";
import { SearchFilters } from "@/components/listings/search-filters";
import { PropertyCard } from "@/components/listings/property-card";
import { Pagination } from "@/components/ui/pagination";
import { EmptyState } from "@/components/ui/empty-state";
import { PagePlaceholder } from "@/components/marketing/page-placeholder";

export const metadata: Metadata = { title: "Properties" };

type SearchParams = Record<string, string | undefined>;

export default async function PropertiesPage({ searchParams }: { searchParams: Promise<SearchParams> }) {
  const sp = await searchParams;
  const page = Number(sp.page ?? "1") || 1;

  const result = await publicPropertiesApi.search({
    propertyTypeId: sp.propertyTypeId,
    purposeId: sp.purposeId,
    provinceId: sp.provinceId,
    minPrice: sp.minPrice,
    maxPrice: sp.maxPrice,
    page,
    pageSize: 12,
  });

  if (!result.ok) {
    return <p className="mx-auto max-w-3xl px-4 py-16 text-sm text-destructive">Couldn&apos;t load properties.</p>;
  }

  if (!result.data.enabled) {
    return (
      <PagePlaceholder
        icon={Home}
        title="Property exchange"
        description="REAK's public property exchange isn't open yet. Check back later, or log in as a member to browse the network exchange."
      />
    );
  }

  return (
    <section className="mx-auto max-w-6xl space-y-6 px-4 py-16 sm:px-6 lg:px-8">
      <div>
        <h1 className="font-heading text-3xl font-bold text-foreground">Properties</h1>
        <p className="mt-2 text-sm text-muted-foreground">Public listings from REAK member organizations.</p>
      </div>

      <SearchFilters basePath="/properties" current={sp} />

      {result.data.items.length === 0 ? (
        <EmptyState icon={Home} title="No properties found" description="Try adjusting your filters." />
      ) : (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {result.data.items.map((item) => (
              <PropertyCard
                key={item.id}
                href={`/properties/${item.id}`}
                title={item.title}
                referenceCode={item.referenceCode}
                memberEntityName={item.memberEntityName}
                propertyTypeName={item.propertyTypeName}
                purposeName={item.purposeName}
                locationLine={`${item.municipalityName}, ${item.districtName}`}
                price={item.price}
                currencyCode={item.currencyCode}
                landArea={item.landArea}
                areaUnitName={item.areaUnitName}
                bedrooms={item.bedrooms}
                bathrooms={item.bathrooms}
                imageUrl={item.primaryImageUrl}
              />
            ))}
          </div>
          <Pagination basePath="/properties" page={result.data.page} pageSize={result.data.pageSize} totalCount={result.data.totalCount} searchParams={sp} />
        </>
      )}
    </section>
  );
}
