import type { Metadata } from "next";
import { Building2 } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { listingsApi } from "@/lib/listings/api";
import { SearchFilters } from "@/components/listings/search-filters";
import { PropertyCard } from "@/components/listings/property-card";
import { Pagination } from "@/components/ui/pagination";
import { EmptyState } from "@/components/ui/empty-state";

export const metadata: Metadata = { title: "Properties" };

type SearchParams = Record<string, string | undefined>;

export default async function PortalPropertiesPage({ searchParams }: { searchParams: Promise<SearchParams> }) {
  const sp = await searchParams;
  const { accessToken } = await requireSession();

  const page = Number(sp.page ?? "1") || 1;
  const result = await listingsApi.search(accessToken, {
    propertyTypeId: sp.propertyTypeId,
    purposeId: sp.purposeId,
    provinceId: sp.provinceId,
    minPrice: sp.minPrice,
    maxPrice: sp.maxPrice,
    status: "Approved",
    page,
    pageSize: 12,
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-foreground">Properties</h1>
        <p className="mt-1 text-sm text-muted-foreground">Browse the shared REAK property exchange.</p>
      </div>

      <SearchFilters basePath="/portal/properties" current={sp} />

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load properties: {result.error}</p>
      ) : result.data.items.length === 0 ? (
        <EmptyState
          icon={Building2}
          title="No properties found"
          description="Try adjusting your filters, or check back once member organizations start listing properties."
        />
      ) : (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {result.data.items.map((item) => (
              <PropertyCard
                key={item.id}
                href={`/portal/properties/${item.id}`}
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
          <Pagination basePath="/portal/properties" page={result.data.page} pageSize={result.data.pageSize} totalCount={result.data.totalCount} searchParams={sp} />
        </>
      )}
    </div>
  );
}
