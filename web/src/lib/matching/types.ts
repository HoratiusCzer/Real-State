export type MatchSummary = {
  id: string;
  listingId: string;
  listingTitle: string;
  listingReferenceCode: string;
  listingMemberEntityName: string;
  demandId: string;
  demandTitle: string;
  demandReferenceCode: string;
  demandMemberEntityName: string;
  score: number;
  status: string;
  computedAt: string;
};

export type MatchSearchResult = { items: MatchSummary[]; totalCount: number; page: number; pageSize: number };
export type MatchSearchResponse = { configured: boolean; result: MatchSearchResult };

// Mirrors REAK.Api.Models.Enums.MatchCriterion
export const MATCH_CRITERION_LABELS: Record<number, string> = {
  1: "Location",
  2: "Budget",
  3: "Area",
  4: "Property type",
  5: "Purpose",
  6: "Bedrooms",
  7: "Bathrooms",
  8: "Furnishing",
  9: "Amenities",
};

// Mirrors REAK.Api.Models.Enums.MatchComponentResult
export const MATCH_RESULT_LABELS: Record<number, string> = {
  1: "Pass",
  2: "Partial",
  3: "Fail",
  4: "No data",
};

export type MatchComponent = { criterion: number; result: number; numericDelta: number | null; detailText: string | null };
export type MatchAction = { id: string; actionType: number; notes: string | null; createdAt: string; byProfileName: string };

export type MatchDetail = {
  id: string;
  listingId: string;
  listingTitle: string;
  listingReferenceCode: string;
  listingMemberEntityId: string;
  listingMemberEntityName: string;
  demandId: string;
  demandTitle: string;
  demandReferenceCode: string;
  demandMemberEntityId: string;
  demandMemberEntityName: string;
  score: number;
  status: string;
  matchRuleSetId: string;
  matchRuleSetName: string;
  matchRuleSetVersion: number;
  computedAt: string;
  components: MatchComponent[];
  actions: MatchAction[];
};
