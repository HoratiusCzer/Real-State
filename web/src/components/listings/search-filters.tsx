import { Label } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { referenceApi } from "@/lib/listings/reference-api";

const selectClass =
  "flex h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";
const inputClass = selectClass;

/** A plain GET form — filtering stays entirely server-side (spec §8.1: "never download the full
 * dataset to the browser for client-side filtering"), and the result is a shareable/bookmarkable
 * URL with no client JavaScript required at all. */
export async function SearchFilters({ basePath, current }: { basePath: string; current: Record<string, string | undefined> }) {
  const [typesResult, purposesResult, provincesResult] = await Promise.all([
    referenceApi.propertyTypes(),
    referenceApi.purposes(),
    referenceApi.provinces(),
  ]);

  const types = typesResult.ok ? typesResult.data : [];
  const purposes = purposesResult.ok ? purposesResult.data : [];
  const provinces = provincesResult.ok ? provincesResult.data : [];

  return (
    <form method="get" action={basePath} className="grid grid-cols-2 gap-3 rounded-lg border border-border bg-card p-4 sm:grid-cols-4 lg:grid-cols-6">
      <div>
        <Label htmlFor="propertyTypeId">Type</Label>
        <select id="propertyTypeId" name="propertyTypeId" defaultValue={current.propertyTypeId ?? ""} className={selectClass}>
          <option value="">Any</option>
          {types.map((t) => (
            <option key={t.id} value={t.id}>{t.name}</option>
          ))}
        </select>
      </div>

      <div>
        <Label htmlFor="purposeId">Purpose</Label>
        <select id="purposeId" name="purposeId" defaultValue={current.purposeId ?? ""} className={selectClass}>
          <option value="">Any</option>
          {purposes.map((p) => (
            <option key={p.id} value={p.id}>{p.name}</option>
          ))}
        </select>
      </div>

      <div>
        <Label htmlFor="provinceId">Province</Label>
        <select id="provinceId" name="provinceId" defaultValue={current.provinceId ?? ""} className={selectClass}>
          <option value="">Any</option>
          {provinces.map((p) => (
            <option key={p.id} value={p.id}>{p.name}</option>
          ))}
        </select>
      </div>

      <div>
        <Label htmlFor="minPrice">Min price</Label>
        <input id="minPrice" name="minPrice" type="number" min={0} defaultValue={current.minPrice ?? ""} className={inputClass} />
      </div>

      <div>
        <Label htmlFor="maxPrice">Max price</Label>
        <input id="maxPrice" name="maxPrice" type="number" min={0} defaultValue={current.maxPrice ?? ""} className={inputClass} />
      </div>

      <div className="flex items-end">
        <Button type="submit" className="w-full">Search</Button>
      </div>
    </form>
  );
}
