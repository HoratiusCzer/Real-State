export type PublicMemberSummary = { id: string; name: string; description: string | null; website: string | null; logoUrl: string | null };
export type PublicMemberListResult = { enabled: boolean; items: PublicMemberSummary[] };
