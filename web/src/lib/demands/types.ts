export type DemandSummary = {
  id: string;
  referenceCode: string;
  title: string;
  memberEntityName: string;
  purposeName: string;
  minBudget: number | null;
  maxBudget: number | null;
  currencyCode: string | null;
  status: string;
  propertyTypeNames: string[];
  locationSummaries: string[];
  createdAt: string;
};

export type DemandSearchResult = { items: DemandSummary[]; totalCount: number; page: number; pageSize: number };

export type DemandLocation = {
  id: string;
  provinceId: string;
  provinceName: string;
  districtName: string | null;
  municipalityName: string | null;
  wardNumber: number | null;
  localityName: string | null;
};

export type DemandDetail = {
  id: string;
  referenceCode: string;
  memberEntityId: string;
  memberEntityName: string;
  title: string;
  description: string | null;
  internalNotes: string | null;
  purposeId: string;
  purposeName: string;
  currencyId: string | null;
  currencyCode: string | null;
  minBudget: number | null;
  maxBudget: number | null;
  areaUnitId: string | null;
  areaUnitName: string | null;
  minArea: number | null;
  maxArea: number | null;
  minBedrooms: number | null;
  minBathrooms: number | null;
  networkVisibility: string;
  status: string;
  expiresAt: string | null;
  createdAt: string;
  updatedAt: string | null;
  isOwner: boolean;
  propertyTypeIds: string[];
  propertyTypeNames: string[];
  locations: DemandLocation[];
  amenityIds: string[];
  amenityNames: string[];
};

export type DemandContact = { clientName: string | null; phone: string | null; email: string | null; confidentialNotes: string | null };

export type DemandLocationInput = { provinceId: string; districtId?: string; municipalityId?: string; wardId?: string; localityId?: string };

export type CreateDemandInput = {
  title: string;
  description?: string;
  purposeId: string;
  currencyId?: string;
  minBudget?: number;
  maxBudget?: number;
  areaUnitId?: string;
  minArea?: number;
  maxArea?: number;
  minBedrooms?: number;
  minBathrooms?: number;
  internalNotes?: string;
  propertyTypeIds?: string[];
  locations?: DemandLocationInput[];
  amenityIds?: string[];
  contact?: DemandContact;
  expiresAt?: string;
};

export type UpdateDemandInput = Omit<CreateDemandInput, "propertyTypeIds" | "locations" | "amenityIds" | "contact">;
