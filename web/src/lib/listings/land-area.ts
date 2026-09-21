// Mirrors REAK.Api/Services/Reference/LandAreaConverter.cs's approved coefficients (spec §11) —
// used ONLY for a live on-screen preview as the agent types. The server independently recomputes
// AreaInSquareFeet from the submitted compound values and is the sole source of truth; this never
// gets submitted.
export const ROPANI_TO_SQFT = 5476;
export const AANA_TO_SQFT = 342.25;
export const PAISA_TO_SQFT = 85.5625;
export const DAM_TO_SQFT = 21.390625;
export const BIGHA_TO_SQFT = 72900;
export const KATTHA_TO_SQFT = 3645;
export const DHUR_TO_SQFT = 182.25;
export const SQM_TO_SQFT = 10.7639;

export const AANA_MAX = 15;
export const PAISA_MAX = 3;
export const DAM_MAX = 3;
export const KATTHA_MAX = 19;
export const DHUR_MAX = 19;

export type MeasurementSystem = "1" | "2" | "3" | "4"; // RopaniSystem | BighaSystem | SquareFeet | SquareMetres

export type LandAreaState = {
  measurementSystem: MeasurementSystem;
  ropaniValue: string;
  aanaValue: string;
  paisaValue: string;
  damValue: string;
  bighaValue: string;
  katthaValue: string;
  dhurValue: string;
  landAreaDirect: string;
};

export const initialLandAreaState: LandAreaState = {
  measurementSystem: "3",
  ropaniValue: "",
  aanaValue: "",
  paisaValue: "",
  damValue: "",
  bighaValue: "",
  katthaValue: "",
  dhurValue: "",
  landAreaDirect: "",
};

export function previewSquareFeet(s: LandAreaState): number | null {
  const n = (v: string) => (v ? Number(v) : 0);
  switch (s.measurementSystem) {
    case "1":
      return n(s.ropaniValue) * ROPANI_TO_SQFT + n(s.aanaValue) * AANA_TO_SQFT + n(s.paisaValue) * PAISA_TO_SQFT + n(s.damValue) * DAM_TO_SQFT;
    case "2":
      return n(s.bighaValue) * BIGHA_TO_SQFT + n(s.katthaValue) * KATTHA_TO_SQFT + n(s.dhurValue) * DHUR_TO_SQFT;
    case "3":
      return s.landAreaDirect ? Number(s.landAreaDirect) : null;
    case "4":
      return s.landAreaDirect ? Number(s.landAreaDirect) * SQM_TO_SQFT : null;
    default:
      return null;
  }
}

/** Builds the LandAreaInput payload subset the API expects for the current system, leaving every
 * other system's fields undefined rather than empty strings/zeros — the server rejects a mix of
 * fields from more than one system. */
export function toLandAreaPayload(s: LandAreaState) {
  const n = (v: string) => (v === "" ? undefined : Number(v));
  const base = { measurementSystem: Number(s.measurementSystem) };
  switch (s.measurementSystem) {
    case "1":
      return { ...base, ropaniValue: n(s.ropaniValue) ?? 0, aanaValue: n(s.aanaValue) ?? 0, paisaValue: n(s.paisaValue) ?? 0, damValue: n(s.damValue) ?? 0 };
    case "2":
      return { ...base, bighaValue: n(s.bighaValue) ?? 0, katthaValue: n(s.katthaValue) ?? 0, dhurValue: n(s.dhurValue) ?? 0 };
    default:
      return { ...base, landArea: n(s.landAreaDirect) };
  }
}

/** Human-readable compound notation for the review step and detail pages — "4 Ropani 0 Aana 2
 * Paisa 0 Dam", not a lossy reconstructed decimal. */
export function formatLandArea(s: LandAreaState): string {
  switch (s.measurementSystem) {
    case "1":
      return `${s.ropaniValue || 0} Ropani ${s.aanaValue || 0} Aana ${s.paisaValue || 0} Paisa ${s.damValue || 0} Dam`;
    case "2":
      return `${s.bighaValue || 0} Bigha ${s.katthaValue || 0} Kattha ${s.dhurValue || 0} Dhur`;
    case "3":
      return `${s.landAreaDirect || 0} sq ft`;
    case "4":
      return `${s.landAreaDirect || 0} sq m`;
    default:
      return "";
  }
}

/** Display formatting for a listing already fetched from the API (summary or detail response) —
 * compound notation when the exact split values are available, otherwise a graceful fallback
 * (legacy pre-migration listings have MeasurementSystem classified but no compound values, since
 * there was nothing accurate to backfill them with). */
export function formatListingArea(l: {
  measurementSystem: string;
  ropaniValue?: number | null;
  aanaValue?: number | null;
  paisaValue?: number | null;
  damValue?: number | null;
  bighaValue?: number | null;
  katthaValue?: number | null;
  dhurValue?: number | null;
  landArea: number;
  areaUnitName: string;
}): string {
  if (l.measurementSystem === "RopaniSystem" && l.ropaniValue != null) {
    return `${l.ropaniValue} Ropani ${l.aanaValue ?? 0} Aana ${l.paisaValue ?? 0} Paisa ${l.damValue ?? 0} Dam`;
  }
  if (l.measurementSystem === "BighaSystem" && l.bighaValue != null) {
    return `${l.bighaValue} Bigha ${l.katthaValue ?? 0} Kattha ${l.dhurValue ?? 0} Dhur`;
  }
  return `${l.landArea} ${l.areaUnitName}`;
}

/** Repopulates edit-form state from a listing's exact stored values (never a re-derived decimal) —
 * measurementSystem comes back from the API as a string name ("RopaniSystem", etc.). */
export function fromListingDetail(l: {
  measurementSystem: string;
  ropaniValue: number | null;
  aanaValue: number | null;
  paisaValue: number | null;
  damValue: number | null;
  bighaValue: number | null;
  katthaValue: number | null;
  dhurValue: number | null;
  landArea: number;
}): LandAreaState {
  const systemCode: Record<string, MeasurementSystem> = { RopaniSystem: "1", BighaSystem: "2", SquareFeet: "3", SquareMetres: "4" };
  const measurementSystem = systemCode[l.measurementSystem] ?? "3";
  return {
    measurementSystem,
    ropaniValue: l.ropaniValue?.toString() ?? "",
    aanaValue: l.aanaValue?.toString() ?? "",
    paisaValue: l.paisaValue?.toString() ?? "",
    damValue: l.damValue?.toString() ?? "",
    bighaValue: l.bighaValue?.toString() ?? "",
    katthaValue: l.katthaValue?.toString() ?? "",
    dhurValue: l.dhurValue?.toString() ?? "",
    landAreaDirect: measurementSystem === "3" || measurementSystem === "4" ? l.landArea.toString() : "",
  };
}
