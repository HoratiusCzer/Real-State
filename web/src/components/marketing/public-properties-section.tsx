import { Home } from "lucide-react";
import { SectionHeading } from "./section-heading";
import { EmptyState } from "@/components/ui/empty-state";
import { PropertyCard } from "@/components/listings/property-card";
import { publicPropertiesApi } from "@/lib/listings/api";

export async function PublicPropertiesSection() {
  const result = await publicPropertiesApi.search({ page: 1, pageSize: 3 });
  if (!result.ok || !result.data.enabled) {
    return null;
  }

  return (
    <section className="border-t border-border bg-card">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
        <SectionHeading eyebrow="Public exchange" title="Featured properties" />
        <div className="mt-6">
          {result.data.items.length === 0 ? (
            <EmptyState icon={Home} title="No public listings yet" />
          ) : (
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
          )}
        </div>
      </div>
    </section>
  );
}
