"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Image from "next/image";
import { Label } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Alert } from "@/components/ui/alert";
import {
  usePropertyTypes, usePurposes, useAmenities, useCurrencies,
  useProvinces, useDistricts, useMunicipalities, useWards, useLocalities,
} from "./use-reference-data";
import { uploadWithProgress } from "./upload-with-progress";
import {
  createListingAction, submitListingAction, updateAmenitiesAction,
  updateContactAction, updateListingAction, updateVisibilityAction,
} from "@/lib/listings/actions";
import type { ListingMedia, ListingDocument } from "@/lib/listings/types";
import { LandAreaFields } from "@/components/listings/land-area-fields";
import { initialLandAreaState, toLandAreaPayload, formatLandArea, type LandAreaState } from "@/lib/listings/land-area";

const selectClass =
  "flex h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";
const inputClass = selectClass;
const textareaClass =
  "flex w-full rounded-md border border-border bg-card px-3 py-2 text-sm text-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";

const STEPS = [
  "Type", "Basic Information", "Location", "Specifications", "Price",
  "Amenities", "Photos", "Documents", "Description", "Contact", "Visibility", "Expiry", "Review",
] as const;

type State = {
  propertyTypeId: string; propertySubtypeId: string;
  title: string; purposeId: string;
  provinceId: string; districtId: string; municipalityId: string; wardId: string; localityId: string; landmark: string;
  landArea: LandAreaState; builtUpArea: string;
  hasRoadAccess: boolean; roadWidthFeet: string; roadType: string; facing: string;
  bedrooms: string; bathrooms: string; floors: string; parkingSpaces: string; furnishing: string;
  currencyId: string; price: string; isPriceNegotiable: boolean;
  amenityIds: string[];
  description: string; internalNotes: string;
  contactName: string; contactPhone: string; contactEmail: string;
  networkVisibility: number; isPublicVisible: boolean;
  expiresAt: string;
};

const initialState: State = {
  propertyTypeId: "", propertySubtypeId: "",
  title: "", purposeId: "",
  provinceId: "", districtId: "", municipalityId: "", wardId: "", localityId: "", landmark: "",
  landArea: initialLandAreaState, builtUpArea: "",
  hasRoadAccess: false, roadWidthFeet: "", roadType: "", facing: "",
  bedrooms: "", bathrooms: "", floors: "", parkingSpaces: "", furnishing: "",
  currencyId: "", price: "", isPriceNegotiable: false,
  amenityIds: [],
  description: "", internalNotes: "",
  contactName: "", contactPhone: "", contactEmail: "",
  networkVisibility: 1, isPublicVisible: false,
  expiresAt: "",
};

function toUpdatePayload(s: State) {
  return {
    title: s.title,
    description: s.description || undefined,
    propertyTypeId: s.propertyTypeId,
    propertySubtypeId: s.propertySubtypeId || undefined,
    purposeId: s.purposeId,
    provinceId: s.provinceId,
    districtId: s.districtId,
    municipalityId: s.municipalityId,
    wardId: s.wardId,
    localityId: s.localityId || undefined,
    landmark: s.landmark || undefined,
    currencyId: s.currencyId,
    price: Number(s.price),
    isPriceNegotiable: s.isPriceNegotiable,
    landArea: toLandAreaPayload(s.landArea),
    builtUpArea: s.builtUpArea ? Number(s.builtUpArea) : undefined,
    hasRoadAccess: s.hasRoadAccess,
    roadWidthFeet: s.roadWidthFeet ? Number(s.roadWidthFeet) : undefined,
    roadType: s.roadType || undefined,
    facing: s.facing || undefined,
    bedrooms: s.bedrooms ? Number(s.bedrooms) : undefined,
    bathrooms: s.bathrooms ? Number(s.bathrooms) : undefined,
    floors: s.floors ? Number(s.floors) : undefined,
    parkingSpaces: s.parkingSpaces ? Number(s.parkingSpaces) : undefined,
    furnishing: s.furnishing || undefined,
    internalNotes: s.internalNotes || undefined,
    expiresAt: s.expiresAt || undefined,
  };
}

export function PropertyWizard() {
  const router = useRouter();
  const [step, setStep] = useState(0);
  const [state, setState] = useState<State>(initialState);
  const [listingId, setListingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [media, setMedia] = useState<ListingMedia[]>([]);
  const [documents, setDocuments] = useState<ListingDocument[]>([]);
  const [uploadProgress, setUploadProgress] = useState<number | null>(null);

  const propertyTypes = usePropertyTypes();
  const purposes = usePurposes();
  const amenities = useAmenities();
  const currencies = useCurrencies();
  const provinces = useProvinces();
  const districts = useDistricts(state.provinceId);
  const municipalities = useMunicipalities(state.districtId);
  const wards = useWards(state.municipalityId);
  const localities = useLocalities(state.wardId);
  const selectedType = propertyTypes.find((t) => t.id === state.propertyTypeId);

  // Unsaved-changes protection (spec §8.2) — only matters before the draft exists server-side.
  useEffect(() => {
    if (listingId) return;
    const hasData = state.title || state.propertyTypeId || state.provinceId;
    if (!hasData) return;
    const handler = (e: BeforeUnloadEvent) => { e.preventDefault(); };
    window.addEventListener("beforeunload", handler);
    return () => window.removeEventListener("beforeunload", handler);
  }, [state, listingId]);

  function set<K extends keyof State>(key: K, value: State[K]) {
    setState((s) => ({ ...s, [key]: value }));
  }

  function canContinue(): boolean {
    switch (step) {
      case 0: return !!state.propertyTypeId;
      case 1: return !!state.title && !!state.purposeId;
      case 2: return !!state.provinceId && !!state.districtId && !!state.municipalityId && !!state.wardId;
      case 3: {
        const a = state.landArea;
        if (a.measurementSystem === "1") return !!a.ropaniValue || !!a.aanaValue || !!a.paisaValue || !!a.damValue;
        if (a.measurementSystem === "2") return !!a.bighaValue || !!a.katthaValue || !!a.dhurValue;
        return !!a.landAreaDirect;
      }
      case 4: return !!state.currencyId && !!state.price;
      default: return true;
    }
  }

  async function persist(): Promise<boolean> {
    if (!listingId) return true;
    const result = await updateListingAction(listingId, toUpdatePayload(state));
    if (!result.ok) {
      setError(result.error ?? "Couldn't save progress.");
      return false;
    }
    return true;
  }

  async function handleContinue() {
    setError(null);

    // Transition from Price (step 4) to Amenities (step 5): create the draft now — every
    // NOT NULL column REAK.Api requires is known by this point (spec §21).
    if (step === 4 && !listingId) {
      setBusy(true);
      const result = await createListingAction(toUpdatePayload(state));
      setBusy(false);
      if (!result.ok || !result.id) {
        setError(result.error ?? "Couldn't create the listing.");
        return;
      }
      setListingId(result.id);
      setStep(step + 1);
      return;
    }

    if (listingId) {
      setBusy(true);
      let ok = true;
      switch (step) {
        case 5:
          await updateAmenitiesAction(listingId, state.amenityIds);
          break;
        case 8:
        case 11:
          ok = await persist();
          break;
        case 9:
          await updateContactAction(listingId, {
            contactName: state.contactName || null,
            phone: state.contactPhone || null,
            email: state.contactEmail || null,
          });
          break;
        case 10:
          await updateVisibilityAction(listingId, { networkVisibility: state.networkVisibility, isPublicVisible: state.isPublicVisible });
          break;
        default:
          break;
      }
      setBusy(false);
      if (!ok) return;
    }

    setStep((s) => Math.min(s + 1, STEPS.length - 1));
  }

  async function handleUploadMedia(files: FileList | null) {
    if (!files || !listingId) return;
    setUploadProgress(0);
    for (const file of Array.from(files)) {
      const fd = new FormData();
      fd.append("file", file);
      const res = await uploadWithProgress(
        `/api/portal/listings/${listingId}/media?isPrimary=${media.length === 0}`,
        fd,
        setUploadProgress
      );
      if (res.ok) setMedia((m) => [...m, res.body as ListingMedia]);
    }
    setUploadProgress(null);
  }

  async function handleUploadDocument(files: FileList | null, documentType: string) {
    if (!files || !listingId) return;
    setUploadProgress(0);
    for (const file of Array.from(files)) {
      const fd = new FormData();
      fd.append("file", file);
      fd.append("documentType", documentType);
      const res = await uploadWithProgress(`/api/portal/listings/${listingId}/documents`, fd, setUploadProgress);
      if (res.ok) setDocuments((d) => [...d, res.body as ListingDocument]);
    }
    setUploadProgress(null);
  }

  async function handleFinish(submitForReview: boolean) {
    if (!listingId) return;
    setBusy(true);
    if (submitForReview) {
      await submitListingAction(listingId);
    }
    setBusy(false);
    router.push(`/portal/properties/${listingId}`);
  }

  return (
    <div className="space-y-6">
      <ol className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
        {STEPS.map((label, i) => (
          <li key={label} className={i === step ? "font-semibold text-foreground" : undefined}>
            {i + 1}. {label}
          </li>
        ))}
      </ol>

      {error ? <Alert variant="destructive">{error}</Alert> : null}

      <div className="rounded-lg border border-border bg-card p-6">
        {step === 0 && (
          <Field label="Property type">
            <select className={selectClass} value={state.propertyTypeId} onChange={(e) => { set("propertyTypeId", e.target.value); set("propertySubtypeId", ""); }}>
              <option value="">Select…</option>
              {propertyTypes.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
            </select>
            {selectedType && selectedType.subtypes.length > 0 ? (
              <div className="mt-4">
                <Label>Subtype (optional)</Label>
                <select className={selectClass} value={state.propertySubtypeId} onChange={(e) => set("propertySubtypeId", e.target.value)}>
                  <option value="">None</option>
                  {selectedType.subtypes.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
                </select>
              </div>
            ) : null}
          </Field>
        )}

        {step === 1 && (
          <div className="space-y-4">
            <Field label="Title">
              <input className={inputClass} value={state.title} onChange={(e) => set("title", e.target.value)} maxLength={500} />
            </Field>
            <Field label="Purpose">
              <select className={selectClass} value={state.purposeId} onChange={(e) => set("purposeId", e.target.value)}>
                <option value="">Select…</option>
                {purposes.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
              </select>
            </Field>
          </div>
        )}

        {step === 2 && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Province">
              <select className={selectClass} value={state.provinceId} onChange={(e) => setState((s) => ({ ...s, provinceId: e.target.value, districtId: "", municipalityId: "", wardId: "", localityId: "" }))}>
                <option value="">Select…</option>
                {provinces.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
              </select>
            </Field>
            <Field label="District">
              <select className={selectClass} value={state.districtId} disabled={!state.provinceId} onChange={(e) => setState((s) => ({ ...s, districtId: e.target.value, municipalityId: "", wardId: "", localityId: "" }))}>
                <option value="">Select…</option>
                {districts.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
              </select>
            </Field>
            <Field label="Municipality">
              <select className={selectClass} value={state.municipalityId} disabled={!state.districtId} onChange={(e) => setState((s) => ({ ...s, municipalityId: e.target.value, wardId: "", localityId: "" }))}>
                <option value="">Select…</option>
                {municipalities.map((m) => <option key={m.id} value={m.id}>{m.name}</option>)}
              </select>
            </Field>
            <Field label="Ward">
              <select className={selectClass} value={state.wardId} disabled={!state.municipalityId} onChange={(e) => setState((s) => ({ ...s, wardId: e.target.value, localityId: "" }))}>
                <option value="">Select…</option>
                {wards.map((w) => <option key={w.id} value={w.id}>Ward {w.number}</option>)}
              </select>
            </Field>
            {localities.length > 0 ? (
              <Field label="Locality (optional)">
                <select className={selectClass} value={state.localityId} onChange={(e) => set("localityId", e.target.value)}>
                  <option value="">None</option>
                  {localities.map((l) => <option key={l.id} value={l.id}>{l.name}</option>)}
                </select>
              </Field>
            ) : null}
            <Field label="Landmark (optional)">
              <input className={inputClass} value={state.landmark} onChange={(e) => set("landmark", e.target.value)} />
            </Field>
          </div>
        )}

        {step === 3 && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="sm:col-span-2">
              <LandAreaFields
                value={state.landArea}
                onChange={(patch) => setState((s) => ({ ...s, landArea: { ...s.landArea, ...patch } }))}
              />
            </div>
            <Field label="Built-up area (optional)">
              <input className={inputClass} type="number" min={0} step="0.01" value={state.builtUpArea} onChange={(e) => set("builtUpArea", e.target.value)} />
            </Field>
            <Field label="Bedrooms">
              <input className={inputClass} type="number" min={0} value={state.bedrooms} onChange={(e) => set("bedrooms", e.target.value)} />
            </Field>
            <Field label="Bathrooms">
              <input className={inputClass} type="number" min={0} value={state.bathrooms} onChange={(e) => set("bathrooms", e.target.value)} />
            </Field>
            <Field label="Floors">
              <input className={inputClass} type="number" min={0} value={state.floors} onChange={(e) => set("floors", e.target.value)} />
            </Field>
            <Field label="Parking spaces">
              <input className={inputClass} type="number" min={0} value={state.parkingSpaces} onChange={(e) => set("parkingSpaces", e.target.value)} />
            </Field>
            <Field label="Furnishing">
              <select className={selectClass} value={state.furnishing} onChange={(e) => set("furnishing", e.target.value)}>
                <option value="">Unspecified</option>
                <option value="Unfurnished">Unfurnished</option>
                <option value="SemiFurnished">Semi-furnished</option>
                <option value="Furnished">Furnished</option>
              </select>
            </Field>
            <Field label="Road width (feet, optional)">
              <input className={inputClass} type="number" min={0} value={state.roadWidthFeet} onChange={(e) => set("roadWidthFeet", e.target.value)} />
            </Field>
            <label className="mt-6 flex items-center gap-2 text-sm text-foreground">
              <input type="checkbox" checked={state.hasRoadAccess} onChange={(e) => set("hasRoadAccess", e.target.checked)} />
              Has road access
            </label>
          </div>
        )}

        {step === 4 && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Currency">
              <select className={selectClass} value={state.currencyId} onChange={(e) => set("currencyId", e.target.value)}>
                <option value="">Select…</option>
                {currencies.map((c) => <option key={c.id} value={c.id}>{c.code}</option>)}
              </select>
            </Field>
            <Field label="Price">
              <input className={inputClass} type="number" min={0} value={state.price} onChange={(e) => set("price", e.target.value)} />
            </Field>
            <label className="flex items-center gap-2 text-sm text-foreground">
              <input type="checkbox" checked={state.isPriceNegotiable} onChange={(e) => set("isPriceNegotiable", e.target.checked)} />
              Price is negotiable
            </label>
          </div>
        )}

        {step === 5 && (
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
            {amenities.length === 0 ? <p className="text-sm text-muted-foreground">No amenities configured yet.</p> : null}
            {amenities.map((a) => (
              <label key={a.id} className="flex items-center gap-2 text-sm text-foreground">
                <input
                  type="checkbox"
                  checked={state.amenityIds.includes(a.id)}
                  onChange={(e) =>
                    setState((s) => ({
                      ...s,
                      amenityIds: e.target.checked ? [...s.amenityIds, a.id] : s.amenityIds.filter((id) => id !== a.id),
                    }))
                  }
                />
                {a.name}
              </label>
            ))}
          </div>
        )}

        {step === 6 && (
          <div className="space-y-4">
            <input type="file" accept="image/jpeg,image/png,image/webp" multiple onChange={(e) => handleUploadMedia(e.target.files)} />
            {uploadProgress !== null ? <p className="text-sm text-muted-foreground">Uploading… {uploadProgress}%</p> : null}
            <div className="grid grid-cols-3 gap-2 sm:grid-cols-4">
              {media.map((m, i) => (
                <div key={m.id} className="relative aspect-square overflow-hidden rounded-md">
                  <Image src={m.url} alt={`Uploaded photo ${i + 1}`} fill sizes="(max-width: 640px) 33vw, 25vw" className="object-cover" />
                </div>
              ))}
            </div>
          </div>
        )}

        {step === 7 && (
          <div className="space-y-4">
            <input type="file" accept="application/pdf,image/jpeg,image/png" onChange={(e) => handleUploadDocument(e.target.files, "Document")} />
            {uploadProgress !== null ? <p className="text-sm text-muted-foreground">Uploading… {uploadProgress}%</p> : null}
            <ul className="text-sm text-foreground">
              {documents.map((d) => <li key={d.id}>{d.documentType ?? "Document"}</li>)}
            </ul>
            <p className="text-xs text-muted-foreground">Private — never shown outside your organization except via an explicit collaboration disclosure.</p>
          </div>
        )}

        {step === 8 && (
          <div className="space-y-4">
            <Field label="Description">
              <textarea className={textareaClass} rows={5} value={state.description} onChange={(e) => set("description", e.target.value)} maxLength={4000} />
            </Field>
            <Field label="Internal notes (optional — never shown outside your organization)">
              <textarea className={textareaClass} rows={3} value={state.internalNotes} onChange={(e) => set("internalNotes", e.target.value)} maxLength={2000} />
            </Field>
          </div>
        )}

        {step === 9 && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Contact name">
              <input className={inputClass} value={state.contactName} onChange={(e) => set("contactName", e.target.value)} />
            </Field>
            <Field label="Phone">
              <input className={inputClass} value={state.contactPhone} onChange={(e) => set("contactPhone", e.target.value)} />
            </Field>
            <Field label="Email">
              <input className={inputClass} type="email" value={state.contactEmail} onChange={(e) => set("contactEmail", e.target.value)} />
            </Field>
            <p className="col-span-full text-xs text-muted-foreground">
              Private — never shown to other organizations except through an explicit collaboration disclosure.
            </p>
          </div>
        )}

        {step === 10 && (
          <div className="space-y-4">
            <Field label="Network visibility">
              <select className={selectClass} value={state.networkVisibility} onChange={(e) => set("networkVisibility", Number(e.target.value))}>
                <option value={1}>Owner only</option>
                <option value={3}>All REAK members</option>
              </select>
            </Field>
            <label className="flex items-center gap-2 text-sm text-foreground">
              <input type="checkbox" checked={state.isPublicVisible} onChange={(e) => set("isPublicVisible", e.target.checked)} />
              Also show on the public website (once approved, if enabled by REAK)
            </label>
          </div>
        )}

        {step === 11 && (
          <Field label="Expiry date (optional)">
            <input
              className={inputClass}
              type="date"
              min={new Date().toISOString().slice(0, 10)}
              value={state.expiresAt}
              onChange={(e) => set("expiresAt", e.target.value)}
            />
          </Field>
        )}

        {step === 12 && (
          <div className="space-y-3 text-sm">
            <p><span className="text-muted-foreground">Title:</span> {state.title}</p>
            <p><span className="text-muted-foreground">Price:</span> {state.price}</p>
            <p><span className="text-muted-foreground">Land area:</span> {formatLandArea(state.landArea)}</p>
            <p><span className="text-muted-foreground">Photos:</span> {media.length}</p>
            <p><span className="text-muted-foreground">Documents:</span> {documents.length}</p>
            <div className="flex gap-3 pt-4">
              <Button onClick={() => handleFinish(false)} variant="secondary" disabled={busy}>Save as draft</Button>
              <Button onClick={() => handleFinish(true)} disabled={busy}>Submit for review</Button>
            </div>
          </div>
        )}
      </div>

      {step < 12 ? (
        <div className="flex justify-between">
          <Button variant="secondary" onClick={() => setStep((s) => Math.max(0, s - 1))} disabled={step === 0 || busy}>
            Back
          </Button>
          <Button onClick={handleContinue} disabled={!canContinue() || busy}>
            {busy ? "Saving…" : "Continue"}
          </Button>
        </div>
      ) : null}
    </div>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <Label>{label}</Label>
      {children}
    </div>
  );
}
