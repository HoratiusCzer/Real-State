import Link from "next/link";
import { Home } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { statusBadgeVariant, statusLabel } from "@/lib/listings/status-badge";

export function PropertyCard({
  href,
  title,
  referenceCode,
  memberEntityName,
  propertyTypeName,
  purposeName,
  locationLine,
  price,
  currencyCode,
  landArea,
  areaUnitName,
  bedrooms,
  bathrooms,
  imageUrl,
  status,
}: {
  href: string;
  title: string;
  referenceCode: string;
  memberEntityName: string;
  propertyTypeName: string;
  purposeName: string;
  locationLine: string;
  price: number;
  currencyCode: string;
  landArea: number;
  areaUnitName: string;
  bedrooms?: number | null;
  bathrooms?: number | null;
  imageUrl?: string | null;
  status?: string;
}) {
  return (
    <Link href={href}>
      <Card className="overflow-hidden transition-colors hover:border-accent">
        <div className="flex h-40 items-center justify-center bg-muted">
          {imageUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img src={imageUrl} alt={title} className="h-full w-full object-cover" />
          ) : (
            <Home className="h-10 w-10 text-muted-foreground" aria-hidden="true" />
          )}
        </div>
        <div className="p-4">
          <div className="flex items-start justify-between gap-2">
            <p className="font-heading font-semibold text-foreground">{title}</p>
            {status ? <Badge variant={statusBadgeVariant(status)}>{statusLabel(status)}</Badge> : null}
          </div>
          <p className="mt-1 text-xs text-muted-foreground">{referenceCode} · {memberEntityName}</p>
          <p className="mt-2 text-lg font-semibold text-foreground">
            {currencyCode} {price.toLocaleString()}
          </p>
          <p className="mt-1 text-sm text-muted-foreground">{locationLine}</p>
          <p className="mt-1 text-xs text-muted-foreground">
            {propertyTypeName} · {purposeName} · {landArea} {areaUnitName}
            {bedrooms != null ? ` · ${bedrooms} bed` : ""}
            {bathrooms != null ? ` · ${bathrooms} bath` : ""}
          </p>
        </div>
      </Card>
    </Link>
  );
}
