"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Label } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Alert } from "@/components/ui/alert";
import {
  usePropertyTypes, usePurposes, useAmenities, useCurrencies,
  useProvinces, useDistricts, useMunicipalities, useWards, useLocalities,
} from "@/components/listings/wizard/use-reference-data";
import { updateAmenitiesAction, updateListingAction } from "@/lib/listings/actions";
import type { ListingDetail } from "@/lib/listings/types";
import { LandAreaFields } from "@/components/listings/land-area-fields";
import { fromListingDetail, toLandAreaPayload } from "@/lib/listings/land-area";

const selectClass =
  "flex h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";
const inputClass = selectClass;
const textareaClass =
  "flex w-full rounded-md border border-border bg-card px-3 py-2 text-sm text-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";

export function PropertyEditForm({ listing, initialAmenityIds }: { listing: ListingDetail; initialAmenityIds: string[] }) {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [amenityIds, setAmenityIds] = useState<string[]>(initialAmenityIds);

  const [propertyTypeId, setPropertyTypeId] = useState(listing.propertyTypeId);
  const [propertySubtypeId, setPropertySubtypeId] = useState(listing.propertySubtypeId ?? "");
  const [title, setTitle] = useState(listing.title);
  const [purposeId, setPurposeId] = useState(listing.purposeId);
  const [provinceId, setProvinceId] = useState(listing.provinceId);
  const [districtId, setDistrictId] = useState(listing.districtId);
  const [municipalityId, setMunicipalityId] = useState(listing.municipalityId);
  const [wardId, setWardId] = useState(listing.wardId);
  const [localityId, setLocalityId] = useState(listing.localityId ?? "");
  const [landmark, setLandmark] = useState(listing.landmark ?? "");
  const [landArea, setLandArea] = useState(() => fromListingDetail(listing));
  const [builtUpArea, setBuiltUpArea] = useState(listing.builtUpArea != null ? String(listing.builtUpArea) : "");
  const [bedrooms, setBedrooms] = useState(listing.bedrooms != null ? String(listing.bedrooms) : "");
  const [bathrooms, setBathrooms] = useState(listing.bathrooms != null ? String(listing.bathrooms) : "");
  const [floors, setFloors] = useState(listing.floors != null ? String(listing.floors) : "");
  const [parkingSpaces, setParkingSpaces] = useState(listing.parkingSpaces != null ? String(listing.parkingSpaces) : "");
  const [furnishing, setFurnishing] = useState(listing.furnishing ?? "");
  const [hasRoadAccess, setHasRoadAccess] = useState(listing.hasRoadAccess);
  const [roadWidthFeet, setRoadWidthFeet] = useState(listing.roadWidthFeet != null ? String(listing.roadWidthFeet) : "");
  const [currencyId, setCurrencyId] = useState(listing.currencyId);
  const [price, setPrice] = useState(String(listing.price));
  const [isPriceNegotiable, setIsPriceNegotiable] = useState(listing.isPriceNegotiable);
  const [description, setDescription] = useState(listing.description ?? "");
  const [internalNotes, setInternalNotes] = useState(listing.internalNotes ?? "");
  const [expiresAt, setExpiresAt] = useState(listing.expiresAt ? listing.expiresAt.slice(0, 10) : "");

  const propertyTypes = usePropertyTypes();
  const purposes = usePurposes();
  const amenities = useAmenities();
  const currencies = useCurrencies();
  const provinces = useProvinces();
  const districts = useDistricts(provinceId);
  const municipalities = useMunicipalities(districtId);
  const wards = useWards(municipalityId);
  const localities = useLocalities(wardId);
  const selectedType = propertyTypes.find((t) => t.id === propertyTypeId);

  async function handleSave() {
    setError(null);
    setBusy(true);

    const [listingResult] = await Promise.all([
      updateListingAction(listing.id, {
        title, description: description || undefined,
        propertyTypeId, propertySubtypeId: propertySubtypeId || undefined, purposeId,
        provinceId, districtId, municipalityId, wardId, localityId: localityId || undefined, landmark: landmark || undefined,
        currencyId, price: Number(price), isPriceNegotiable,
        landArea: toLandAreaPayload(landArea), builtUpArea: builtUpArea ? Number(builtUpArea) : undefined,
        hasRoadAccess, roadWidthFeet: roadWidthFeet ? Number(roadWidthFeet) : undefined, roadType: undefined,
        facing: undefined,
        bedrooms: bedrooms ? Number(bedrooms) : undefined,
        bathrooms: bathrooms ? Number(bathrooms) : undefined,
        floors: floors ? Number(floors) : undefined,
        parkingSpaces: parkingSpaces ? Number(parkingSpaces) : undefined,
        furnishing: furnishing || undefined,
        internalNotes: internalNotes || undefined,
        expiresAt: expiresAt || undefined,
      }),
      updateAmenitiesAction(listing.id, amenityIds),
    ]);

    setBusy(false);
    if (!listingResult.ok) {
      setError(listingResult.error ?? "Couldn't save changes.");
      return;
    }

    router.push(`/portal/properties/${listing.id}`);
  }

  return (
    <div className="space-y-6">
      {error ? <Alert variant="destructive">{error}</Alert> : null}

      <Section title="Type">
        <select className={selectClass} value={propertyTypeId} onChange={(e) => { setPropertyTypeId(e.target.value); setPropertySubtypeId(""); }}>
          {propertyTypes.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
        </select>
        {selectedType && selectedType.subtypes.length > 0 ? (
          <select className={`${selectClass} mt-2`} value={propertySubtypeId} onChange={(e) => setPropertySubtypeId(e.target.value)}>
            <option value="">No subtype</option>
            {selectedType.subtypes.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
        ) : null}
      </Section>

      <Section title="Basic information">
        <div className="space-y-3">
          <div>
            <Label>Title</Label>
            <input className={inputClass} value={title} onChange={(e) => setTitle(e.target.value)} />
          </div>
          <div>
            <Label>Purpose</Label>
            <select className={selectClass} value={purposeId} onChange={(e) => setPurposeId(e.target.value)}>
              {purposes.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
            </select>
          </div>
        </div>
      </Section>

      <Section title="Location">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <Label>Province</Label>
            <select className={selectClass} value={provinceId} onChange={(e) => { setProvinceId(e.target.value); setDistrictId(""); setMunicipalityId(""); setWardId(""); }}>
              {provinces.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
            </select>
          </div>
          <div>
            <Label>District</Label>
            <select className={selectClass} value={districtId} onChange={(e) => { setDistrictId(e.target.value); setMunicipalityId(""); setWardId(""); }}>
              <option value={districtId}>{listing.districtName}</option>
              {districts.filter((d) => d.id !== districtId).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select>
          </div>
          <div>
            <Label>Municipality</Label>
            <select className={selectClass} value={municipalityId} onChange={(e) => { setMunicipalityId(e.target.value); setWardId(""); }}>
              <option value={municipalityId}>{listing.municipalityName}</option>
              {municipalities.filter((m) => m.id !== municipalityId).map((m) => <option key={m.id} value={m.id}>{m.name}</option>)}
            </select>
          </div>
          <div>
            <Label>Ward</Label>
            <select className={selectClass} value={wardId} onChange={(e) => setWardId(e.target.value)}>
              <option value={wardId}>Ward {listing.wardNumber}</option>
              {wards.filter((w) => w.id !== wardId).map((w) => <option key={w.id} value={w.id}>Ward {w.number}</option>)}
            </select>
          </div>
          <div>
            <Label>Locality</Label>
            <select className={selectClass} value={localityId} onChange={(e) => setLocalityId(e.target.value)}>
              <option value="">None</option>
              {localities.map((l) => <option key={l.id} value={l.id}>{l.name}</option>)}
            </select>
          </div>
          <div>
            <Label>Landmark</Label>
            <input className={inputClass} value={landmark} onChange={(e) => setLandmark(e.target.value)} />
          </div>
        </div>
      </Section>

      <Section title="Specifications">
        <div className="mb-4">
          <LandAreaFields value={landArea} onChange={(patch) => setLandArea((s) => ({ ...s, ...patch }))} />
        </div>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div><Label>Built-up area</Label><input className={inputClass} type="number" value={builtUpArea} onChange={(e) => setBuiltUpArea(e.target.value)} /></div>
          <div><Label>Bedrooms</Label><input className={inputClass} type="number" value={bedrooms} onChange={(e) => setBedrooms(e.target.value)} /></div>
          <div><Label>Bathrooms</Label><input className={inputClass} type="number" value={bathrooms} onChange={(e) => setBathrooms(e.target.value)} /></div>
          <div><Label>Floors</Label><input className={inputClass} type="number" value={floors} onChange={(e) => setFloors(e.target.value)} /></div>
          <div><Label>Parking</Label><input className={inputClass} type="number" value={parkingSpaces} onChange={(e) => setParkingSpaces(e.target.value)} /></div>
          <div>
            <Label>Furnishing</Label>
            <select className={selectClass} value={furnishing} onChange={(e) => setFurnishing(e.target.value)}>
              <option value="">Unspecified</option>
              <option value="Unfurnished">Unfurnished</option>
              <option value="SemiFurnished">Semi-furnished</option>
              <option value="Furnished">Furnished</option>
            </select>
          </div>
          <div><Label>Road width (ft)</Label><input className={inputClass} type="number" value={roadWidthFeet} onChange={(e) => setRoadWidthFeet(e.target.value)} /></div>
          <label className="mt-6 flex items-center gap-2 text-sm text-foreground">
            <input type="checkbox" checked={hasRoadAccess} onChange={(e) => setHasRoadAccess(e.target.checked)} /> Has road access
          </label>
        </div>
      </Section>

      <Section title="Price">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div><Label>Currency</Label><select className={selectClass} value={currencyId} onChange={(e) => setCurrencyId(e.target.value)}>{currencies.map((c) => <option key={c.id} value={c.id}>{c.code}</option>)}</select></div>
          <div><Label>Price</Label><input className={inputClass} type="number" value={price} onChange={(e) => setPrice(e.target.value)} /></div>
          <label className="flex items-center gap-2 text-sm text-foreground">
            <input type="checkbox" checked={isPriceNegotiable} onChange={(e) => setIsPriceNegotiable(e.target.checked)} /> Negotiable
          </label>
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
        <Button href={`/portal/properties/${listing.id}`} variant="secondary">Cancel</Button>
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
