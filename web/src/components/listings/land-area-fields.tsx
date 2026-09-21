"use client";

import { Label } from "@/components/ui/input";
import {
  AANA_MAX, PAISA_MAX, DAM_MAX, KATTHA_MAX, DHUR_MAX,
  previewSquareFeet, type LandAreaState, type MeasurementSystem,
} from "@/lib/listings/land-area";

const selectClass =
  "flex h-11 w-full rounded-md border border-border bg-card px-3 text-sm text-foreground transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";
const inputClass = selectClass;

/** Nepal land area entry (spec §11) — a "Measurement system" selector plus whichever split
 * fields that system needs: Ropani System (Hill) shows 4 side-by-side integer fields, Bigha
 * System (Terai) shows 3, Square Feet/Square Metres show a single decimal field. The server
 * recomputes and owns the canonical square-feet figure; the preview shown here is informational
 * only (see lib/listings/land-area.ts). */
export function LandAreaFields({ value, onChange }: { value: LandAreaState; onChange: (patch: Partial<LandAreaState>) => void }) {
  const set = <K extends keyof LandAreaState>(key: K, v: LandAreaState[K]) => onChange({ [key]: v } as Partial<LandAreaState>);
  const preview = previewSquareFeet(value);

  return (
    <div className="space-y-4">
      <div>
        <Label>Measurement system</Label>
        <select
          className={selectClass}
          value={value.measurementSystem}
          onChange={(e) => onChange({ measurementSystem: e.target.value as MeasurementSystem })}
        >
          <option value="1">Ropani System (Hill)</option>
          <option value="2">Bigha System (Terai)</option>
          <option value="3">Square Feet</option>
          <option value="4">Square Metres</option>
        </select>
      </div>

      {value.measurementSystem === "1" ? (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
          <div>
            <Label>Ropani</Label>
            <input className={inputClass} type="number" min={0} step={1} value={value.ropaniValue} onChange={(e) => set("ropaniValue", e.target.value)} />
          </div>
          <div>
            <Label>Aana (0–{AANA_MAX})</Label>
            <input className={inputClass} type="number" min={0} max={AANA_MAX} step={1} value={value.aanaValue} onChange={(e) => set("aanaValue", e.target.value)} />
          </div>
          <div>
            <Label>Paisa (0–{PAISA_MAX})</Label>
            <input className={inputClass} type="number" min={0} max={PAISA_MAX} step={1} value={value.paisaValue} onChange={(e) => set("paisaValue", e.target.value)} />
          </div>
          <div>
            <Label>Dam (0–{DAM_MAX})</Label>
            <input className={inputClass} type="number" min={0} max={DAM_MAX} step={1} value={value.damValue} onChange={(e) => set("damValue", e.target.value)} />
          </div>
        </div>
      ) : value.measurementSystem === "2" ? (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
          <div>
            <Label>Bigha</Label>
            <input className={inputClass} type="number" min={0} step={1} value={value.bighaValue} onChange={(e) => set("bighaValue", e.target.value)} />
          </div>
          <div>
            <Label>Kattha (0–{KATTHA_MAX})</Label>
            <input className={inputClass} type="number" min={0} max={KATTHA_MAX} step={1} value={value.katthaValue} onChange={(e) => set("katthaValue", e.target.value)} />
          </div>
          <div>
            <Label>Dhur (0–{DHUR_MAX})</Label>
            <input className={inputClass} type="number" min={0} max={DHUR_MAX} step={1} value={value.dhurValue} onChange={(e) => set("dhurValue", e.target.value)} />
          </div>
        </div>
      ) : (
        <div>
          <Label>{value.measurementSystem === "3" ? "Area (square feet)" : "Area (square metres)"}</Label>
          <input className={inputClass} type="number" min={0} step="0.01" value={value.landAreaDirect} onChange={(e) => set("landAreaDirect", e.target.value)} />
        </div>
      )}

      {preview !== null ? (
        <p className="text-xs text-muted-foreground">≈ {preview.toLocaleString(undefined, { maximumFractionDigits: 2 })} sq ft</p>
      ) : null}
    </div>
  );
}
