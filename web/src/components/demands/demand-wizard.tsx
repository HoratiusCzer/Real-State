"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Label } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Alert } from "@/components/ui/alert";
import {
  usePurposes, usePropertyTypes, useAmenities, useAreaUnits, useCurrencies,
  useProvinces, useDistricts, useMunicipalities, useWards, useLocalities,
} from "@/components/listings/wizard/use-reference-data";
import {
  createDemandAction, publishDemandAction, updateDemandAction, updateDemandAmenitiesAction,
  updateDemandContactAction, updateDemandVisibilityAction, updateLocationsAction, updatePropertyTypesAction,
} from "@/lib/demands/actions";

const selectClass =
  "flex h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";
const inputClass = selectClass;
const textareaClass =
  "flex w-full rounded-md border border-border bg-card px-3 py-2 text-sm text-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";

const STEPS = ["Basic Information", "Property Types", "Location", "Budget & Area", "Amenities", "Description", "Client Contact", "Visibility", "Expiry", "Review"] as const;

type State = {
  title: string; purposeId: string;
  propertyTypeIds: string[];
  provinceId: string; districtId: string; municipalityId: string; wardId: string; localityId: string;
  currencyId: string; minBudget: string; maxBudget: string;
  areaUnitId: string; minArea: string; maxArea: string;
  minBedrooms: string; minBathrooms: string;
  amenityIds: string[];
  description: string; internalNotes: string;
  clientName: string; clientPhone: string; clientEmail: string; confidentialNotes: string;
  networkVisibility: number;
  expiresAt: string;
};

const initialState: State = {
  title: "", purposeId: "",
  propertyTypeIds: [],
  provinceId: "", districtId: "", municipalityId: "", wardId: "", localityId: "",
  currencyId: "", minBudget: "", maxBudget: "",
  areaUnitId: "", minArea: "", maxArea: "",
  minBedrooms: "", minBathrooms: "",
  amenityIds: [],
  description: "", internalNotes: "",
  clientName: "", clientPhone: "", clientEmail: "", confidentialNotes: "",
  networkVisibility: 1,
  expiresAt: "",
};

function toUpdatePayload(s: State) {
  return {
    title: s.title,
    description: s.description || undefined,
    purposeId: s.purposeId,
    currencyId: s.currencyId || undefined,
    minBudget: s.minBudget ? Number(s.minBudget) : undefined,
    maxBudget: s.maxBudget ? Number(s.maxBudget) : undefined,
    areaUnitId: s.areaUnitId || undefined,
    minArea: s.minArea ? Number(s.minArea) : undefined,
    maxArea: s.maxArea ? Number(s.maxArea) : undefined,
    minBedrooms: s.minBedrooms ? Number(s.minBedrooms) : undefined,
    minBathrooms: s.minBathrooms ? Number(s.minBathrooms) : undefined,
    internalNotes: s.internalNotes || undefined,
    expiresAt: s.expiresAt || undefined,
  };
}

export function DemandWizard() {
  const router = useRouter();
  const [step, setStep] = useState(0);
  const [state, setState] = useState<State>(initialState);
  const [demandId, setDemandId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const purposes = usePurposes();
  const propertyTypes = usePropertyTypes();
  const amenities = useAmenities();
  const areaUnits = useAreaUnits();
  const currencies = useCurrencies();
  const provinces = useProvinces();
  const districts = useDistricts(state.provinceId);
  const municipalities = useMunicipalities(state.districtId);
  const wards = useWards(state.municipalityId);
  const localities = useLocalities(state.wardId);

  function set<K extends keyof State>(key: K, value: State[K]) {
    setState((s) => ({ ...s, [key]: value }));
  }

  function canContinue(): boolean {
    if (step === 0) return !!state.title && !!state.purposeId;
    return true;
  }

  async function handleContinue() {
    setError(null);

    if (step === 0 && !demandId) {
      setBusy(true);
      const result = await createDemandAction(toUpdatePayload(state));
      setBusy(false);
      if (!result.ok || !result.id) {
        setError(result.error ?? "Couldn't create the requirement.");
        return;
      }
      setDemandId(result.id);
      setStep(1);
      return;
    }

    if (demandId) {
      setBusy(true);
      switch (step) {
        case 1:
          await updatePropertyTypesAction(demandId, state.propertyTypeIds);
          break;
        case 2:
          if (state.provinceId) {
            await updateLocationsAction(demandId, [{
              provinceId: state.provinceId,
              districtId: state.districtId || undefined,
              municipalityId: state.municipalityId || undefined,
              wardId: state.wardId || undefined,
              localityId: state.localityId || undefined,
            }]);
          }
          break;
        case 3:
          await updateDemandAction(demandId, toUpdatePayload(state));
          break;
        case 4:
          await updateDemandAmenitiesAction(demandId, state.amenityIds);
          break;
        case 5:
          await updateDemandAction(demandId, toUpdatePayload(state));
          break;
        case 6:
          await updateDemandContactAction(demandId, {
            clientName: state.clientName || null,
            phone: state.clientPhone || null,
            email: state.clientEmail || null,
            confidentialNotes: state.confidentialNotes || null,
          });
          break;
        case 7:
          await updateDemandVisibilityAction(demandId, { networkVisibility: state.networkVisibility });
          break;
        case 8:
          await updateDemandAction(demandId, toUpdatePayload(state));
          break;
        default:
          break;
      }
      setBusy(false);
    }

    setStep((s) => Math.min(s + 1, STEPS.length - 1));
  }

  async function handleFinish(publish: boolean) {
    if (!demandId) return;
    setBusy(true);
    if (publish) await publishDemandAction(demandId);
    setBusy(false);
    router.push(`/portal/demands/${demandId}`);
  }

  return (
    <div className="space-y-6">
      <ol className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
        {STEPS.map((label, i) => (
          <li key={label} className={i === step ? "font-semibold text-foreground" : undefined}>{i + 1}. {label}</li>
        ))}
      </ol>

      {error ? <Alert variant="destructive">{error}</Alert> : null}

      <div className="rounded-lg border border-border bg-card p-6">
        {step === 0 && (
          <div className="space-y-4">
            <div>
              <Label>Title</Label>
              <input className={inputClass} value={state.title} onChange={(e) => set("title", e.target.value)} maxLength={500} />
            </div>
            <div>
              <Label>Purpose</Label>
              <select className={selectClass} value={state.purposeId} onChange={(e) => set("purposeId", e.target.value)}>
                <option value="">Select…</option>
                {purposes.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
              </select>
            </div>
          </div>
        )}

        {step === 1 && (
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
            {propertyTypes.length === 0 ? <p className="text-sm text-muted-foreground">No property types configured yet.</p> : null}
            {propertyTypes.map((t) => (
              <label key={t.id} className="flex items-center gap-2 text-sm text-foreground">
                <input
                  type="checkbox"
                  checked={state.propertyTypeIds.includes(t.id)}
                  onChange={(e) =>
                    setState((s) => ({
                      ...s,
                      propertyTypeIds: e.target.checked ? [...s.propertyTypeIds, t.id] : s.propertyTypeIds.filter((id) => id !== t.id),
                    }))
                  }
                />
                {t.name}
              </label>
            ))}
          </div>
        )}

        {step === 2 && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <Label>Province</Label>
              <select className={selectClass} value={state.provinceId} onChange={(e) => setState((s) => ({ ...s, provinceId: e.target.value, districtId: "", municipalityId: "", wardId: "", localityId: "" }))}>
                <option value="">Any / not specified</option>
                {provinces.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
              </select>
            </div>
            {state.provinceId ? (
              <div>
                <Label>District (optional)</Label>
                <select className={selectClass} value={state.districtId} onChange={(e) => setState((s) => ({ ...s, districtId: e.target.value, municipalityId: "", wardId: "", localityId: "" }))}>
                  <option value="">Any district</option>
                  {districts.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
                </select>
              </div>
            ) : null}
            {state.districtId ? (
              <div>
                <Label>Municipality (optional)</Label>
                <select className={selectClass} value={state.municipalityId} onChange={(e) => setState((s) => ({ ...s, municipalityId: e.target.value, wardId: "", localityId: "" }))}>
                  <option value="">Any municipality</option>
                  {municipalities.map((m) => <option key={m.id} value={m.id}>{m.name}</option>)}
                </select>
              </div>
            ) : null}
            {state.municipalityId ? (
              <div>
                <Label>Ward (optional)</Label>
                <select className={selectClass} value={state.wardId} onChange={(e) => setState((s) => ({ ...s, wardId: e.target.value, localityId: "" }))}>
                  <option value="">Any ward</option>
                  {wards.map((w) => <option key={w.id} value={w.id}>Ward {w.number}</option>)}
                </select>
              </div>
            ) : null}
            {state.wardId && localities.length > 0 ? (
              <div>
                <Label>Locality (optional)</Label>
                <select className={selectClass} value={state.localityId} onChange={(e) => set("localityId", e.target.value)}>
                  <option value="">Any locality</option>
                  {localities.map((l) => <option key={l.id} value={l.id}>{l.name}</option>)}
                </select>
              </div>
            ) : null}
            <p className="col-span-full text-xs text-muted-foreground">This wizard collects one acceptable location; add more later from the requirement&apos;s detail page if needed.</p>
          </div>
        )}

        {step === 3 && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div><Label>Currency</Label><select className={selectClass} value={state.currencyId} onChange={(e) => set("currencyId", e.target.value)}><option value="">Not specified</option>{currencies.map((c) => <option key={c.id} value={c.id}>{c.code}</option>)}</select></div>
            <div><Label>Min budget</Label><input className={inputClass} type="number" min={0} value={state.minBudget} onChange={(e) => set("minBudget", e.target.value)} /></div>
            <div><Label>Max budget</Label><input className={inputClass} type="number" min={0} value={state.maxBudget} onChange={(e) => set("maxBudget", e.target.value)} /></div>
            <div><Label>Area unit</Label><select className={selectClass} value={state.areaUnitId} onChange={(e) => set("areaUnitId", e.target.value)}><option value="">Not specified</option>{areaUnits.map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}</select></div>
            <div><Label>Min area</Label><input className={inputClass} type="number" min={0} value={state.minArea} onChange={(e) => set("minArea", e.target.value)} /></div>
            <div><Label>Max area</Label><input className={inputClass} type="number" min={0} value={state.maxArea} onChange={(e) => set("maxArea", e.target.value)} /></div>
            <div><Label>Min bedrooms</Label><input className={inputClass} type="number" min={0} value={state.minBedrooms} onChange={(e) => set("minBedrooms", e.target.value)} /></div>
            <div><Label>Min bathrooms</Label><input className={inputClass} type="number" min={0} value={state.minBathrooms} onChange={(e) => set("minBathrooms", e.target.value)} /></div>
          </div>
        )}

        {step === 4 && (
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
            {amenities.map((a) => (
              <label key={a.id} className="flex items-center gap-2 text-sm text-foreground">
                <input
                  type="checkbox"
                  checked={state.amenityIds.includes(a.id)}
                  onChange={(e) =>
                    setState((s) => ({ ...s, amenityIds: e.target.checked ? [...s.amenityIds, a.id] : s.amenityIds.filter((id) => id !== a.id) }))
                  }
                />
                {a.name}
              </label>
            ))}
          </div>
        )}

        {step === 5 && (
          <div className="space-y-4">
            <div><Label>Description</Label><textarea className={textareaClass} rows={5} value={state.description} onChange={(e) => set("description", e.target.value)} maxLength={4000} /></div>
            <div><Label>Internal notes (optional — never shown outside your organization)</Label><textarea className={textareaClass} rows={3} value={state.internalNotes} onChange={(e) => set("internalNotes", e.target.value)} maxLength={2000} /></div>
          </div>
        )}

        {step === 6 && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div><Label>Client name</Label><input className={inputClass} value={state.clientName} onChange={(e) => set("clientName", e.target.value)} /></div>
            <div><Label>Phone</Label><input className={inputClass} value={state.clientPhone} onChange={(e) => set("clientPhone", e.target.value)} /></div>
            <div><Label>Email</Label><input className={inputClass} type="email" value={state.clientEmail} onChange={(e) => set("clientEmail", e.target.value)} /></div>
            <div><Label>Confidential notes</Label><textarea className={textareaClass} rows={2} value={state.confidentialNotes} onChange={(e) => set("confidentialNotes", e.target.value)} /></div>
            <p className="col-span-full text-xs text-muted-foreground">
              The client&apos;s identity is kept private (spec §9) — never shown to other organizations except through an explicit collaboration disclosure.
            </p>
          </div>
        )}

        {step === 7 && (
          <div>
            <Label>Network visibility</Label>
            <select className={selectClass} value={state.networkVisibility} onChange={(e) => set("networkVisibility", Number(e.target.value))}>
              <option value={1}>Owner only</option>
              <option value={3}>All REAK members</option>
            </select>
          </div>
        )}

        {step === 8 && (
          <div>
            <Label>Expiry date (optional)</Label>
            <input
              className={inputClass}
              type="date"
              min={new Date().toISOString().slice(0, 10)}
              value={state.expiresAt}
              onChange={(e) => set("expiresAt", e.target.value)}
            />
          </div>
        )}

        {step === 9 && (
          <div className="space-y-3 text-sm">
            <p><span className="text-muted-foreground">Title:</span> {state.title}</p>
            <p><span className="text-muted-foreground">Budget:</span> {state.minBudget || "?"}–{state.maxBudget || "?"}</p>
            <p><span className="text-muted-foreground">Property types:</span> {state.propertyTypeIds.length}</p>
            <div className="flex gap-3 pt-4">
              <Button onClick={() => handleFinish(false)} variant="secondary" disabled={busy}>Save as draft</Button>
              <Button onClick={() => handleFinish(true)} disabled={busy}>Publish</Button>
            </div>
          </div>
        )}
      </div>

      {step < 9 ? (
        <div className="flex justify-between">
          <Button variant="secondary" onClick={() => setStep((s) => Math.max(0, s - 1))} disabled={step === 0 || busy}>Back</Button>
          <Button onClick={handleContinue} disabled={!canContinue() || busy}>{busy ? "Saving…" : "Continue"}</Button>
        </div>
      ) : null}
    </div>
  );
}
