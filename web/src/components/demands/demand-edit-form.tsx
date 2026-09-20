"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Label } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Alert } from "@/components/ui/alert";
import {
  usePurposes, usePropertyTypes, useAmenities, useAreaUnits, useCurrencies, useProvinces,
} from "@/components/listings/wizard/use-reference-data";
import { updateDemandAction, updateDemandAmenitiesAction, updateLocationsAction, updatePropertyTypesAction } from "@/lib/demands/actions";
import type { DemandDetail } from "@/lib/demands/types";

const selectClass =
  "flex h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";
const inputClass = selectClass;
const textareaClass =
  "flex w-full rounded-md border border-border bg-card px-3 py-2 text-sm text-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";

export function DemandEditForm({ demand, initialPropertyTypeIds }: { demand: DemandDetail; initialPropertyTypeIds: string[] }) {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const [title, setTitle] = useState(demand.title);
  const [purposeId, setPurposeId] = useState(demand.purposeId);
  const [propertyTypeIds, setPropertyTypeIds] = useState<string[]>(initialPropertyTypeIds);
  const [amenityIds, setAmenityIds] = useState<string[]>(demand.amenityIds);
  const firstLocation = demand.locations[0];
  const [provinceId, setProvinceId] = useState(firstLocation?.provinceId ?? "");
  const [currencyId, setCurrencyId] = useState(demand.currencyId ?? "");
  const [minBudget, setMinBudget] = useState(demand.minBudget != null ? String(demand.minBudget) : "");
  const [maxBudget, setMaxBudget] = useState(demand.maxBudget != null ? String(demand.maxBudget) : "");
  const [areaUnitId, setAreaUnitId] = useState(demand.areaUnitId ?? "");
  const [minArea, setMinArea] = useState(demand.minArea != null ? String(demand.minArea) : "");
  const [maxArea, setMaxArea] = useState(demand.maxArea != null ? String(demand.maxArea) : "");
  const [minBedrooms, setMinBedrooms] = useState(demand.minBedrooms != null ? String(demand.minBedrooms) : "");
  const [minBathrooms, setMinBathrooms] = useState(demand.minBathrooms != null ? String(demand.minBathrooms) : "");
  const [description, setDescription] = useState(demand.description ?? "");
  const [internalNotes, setInternalNotes] = useState(demand.internalNotes ?? "");
  const [expiresAt, setExpiresAt] = useState(demand.expiresAt ? demand.expiresAt.slice(0, 10) : "");

  const purposes = usePurposes();
  const propertyTypes = usePropertyTypes();
  const amenities = useAmenities();
  const areaUnits = useAreaUnits();
  const currencies = useCurrencies();
  const provinces = useProvinces();

  async function handleSave() {
    setError(null);
    setBusy(true);

    const [demandResult] = await Promise.all([
      updateDemandAction(demand.id, {
        title, purposeId, description: description || undefined, internalNotes: internalNotes || undefined,
        currencyId: currencyId || undefined, minBudget: minBudget ? Number(minBudget) : undefined, maxBudget: maxBudget ? Number(maxBudget) : undefined,
        areaUnitId: areaUnitId || undefined, minArea: minArea ? Number(minArea) : undefined, maxArea: maxArea ? Number(maxArea) : undefined,
        minBedrooms: minBedrooms ? Number(minBedrooms) : undefined, minBathrooms: minBathrooms ? Number(minBathrooms) : undefined,
        expiresAt: expiresAt || undefined,
      }),
      updatePropertyTypesAction(demand.id, propertyTypeIds),
      updateDemandAmenitiesAction(demand.id, amenityIds),
      provinceId ? updateLocationsAction(demand.id, [{ provinceId }]) : Promise.resolve(),
    ]);

    setBusy(false);
    if (!demandResult.ok) {
      setError(demandResult.error ?? "Couldn't save changes.");
      return;
    }

    router.push(`/portal/demands/${demand.id}`);
  }

  return (
    <div className="space-y-6">
      {error ? <Alert variant="destructive">{error}</Alert> : null}

      <Section title="Basic information">
        <div className="space-y-3">
          <div><Label>Title</Label><input className={inputClass} value={title} onChange={(e) => setTitle(e.target.value)} /></div>
          <div><Label>Purpose</Label><select className={selectClass} value={purposeId} onChange={(e) => setPurposeId(e.target.value)}>{purposes.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}</select></div>
        </div>
      </Section>

      <Section title="Property types">
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
          {propertyTypes.map((t) => (
            <label key={t.id} className="flex items-center gap-2 text-sm text-foreground">
              <input
                type="checkbox"
                checked={propertyTypeIds.includes(t.id)}
                onChange={(e) => setPropertyTypeIds((ids) => (e.target.checked ? [...ids, t.id] : ids.filter((id) => id !== t.id)))}
              />
              {t.name}
            </label>
          ))}
        </div>
      </Section>

      <Section title="Location">
        <select className={selectClass} value={provinceId} onChange={(e) => setProvinceId(e.target.value)}>
          <option value="">Not specified</option>
          {provinces.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
        </select>
        <p className="mt-2 text-xs text-muted-foreground">Editing here sets a single province-level location. For district/ward-level precision, use the requirement&apos;s API directly or recreate it.</p>
      </Section>

      <Section title="Budget & area">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div><Label>Currency</Label><select className={selectClass} value={currencyId} onChange={(e) => setCurrencyId(e.target.value)}><option value="">Not specified</option>{currencies.map((c) => <option key={c.id} value={c.id}>{c.code}</option>)}</select></div>
          <div><Label>Min budget</Label><input className={inputClass} type="number" value={minBudget} onChange={(e) => setMinBudget(e.target.value)} /></div>
          <div><Label>Max budget</Label><input className={inputClass} type="number" value={maxBudget} onChange={(e) => setMaxBudget(e.target.value)} /></div>
          <div><Label>Area unit</Label><select className={selectClass} value={areaUnitId} onChange={(e) => setAreaUnitId(e.target.value)}><option value="">Not specified</option>{areaUnits.map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}</select></div>
          <div><Label>Min area</Label><input className={inputClass} type="number" value={minArea} onChange={(e) => setMinArea(e.target.value)} /></div>
          <div><Label>Max area</Label><input className={inputClass} type="number" value={maxArea} onChange={(e) => setMaxArea(e.target.value)} /></div>
          <div><Label>Min bedrooms</Label><input className={inputClass} type="number" value={minBedrooms} onChange={(e) => setMinBedrooms(e.target.value)} /></div>
          <div><Label>Min bathrooms</Label><input className={inputClass} type="number" value={minBathrooms} onChange={(e) => setMinBathrooms(e.target.value)} /></div>
        </div>
      </Section>

      <Section title="Amenities">
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
          {amenities.map((a) => (
            <label key={a.id} className="flex items-center gap-2 text-sm text-foreground">
              <input
                type="checkbox"
                checked={amenityIds.includes(a.id)}
                onChange={(e) => setAmenityIds((ids) => (e.target.checked ? [...ids, a.id] : ids.filter((id) => id !== a.id)))}
              />
              {a.name}
            </label>
          ))}
        </div>
      </Section>

      <Section title="Description">
        <textarea className={textareaClass} rows={5} value={description} onChange={(e) => setDescription(e.target.value)} />
        <Label className="mt-3 block">Internal notes</Label>
        <textarea className={textareaClass} rows={3} value={internalNotes} onChange={(e) => setInternalNotes(e.target.value)} />
      </Section>

      <Section title="Expiry">
        <input
          className={inputClass}
          type="date"
          min={new Date().toISOString().slice(0, 10)}
          value={expiresAt}
          onChange={(e) => setExpiresAt(e.target.value)}
        />
      </Section>

      <div className="flex gap-3">
        <Button onClick={handleSave} disabled={busy}>{busy ? "Saving…" : "Save changes"}</Button>
        <Button href={`/portal/demands/${demand.id}`} variant="secondary">Cancel</Button>
      </div>
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="rounded-lg border border-border bg-card p-5">
      <h2 className="mb-3 font-heading text-sm font-semibold text-foreground">{title}</h2>
      {children}
    </div>
  );
}
