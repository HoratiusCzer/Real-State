export type CollaborationRequest = {
  id: string;
  matchId: string | null;
  fromMemberEntityId: string;
  fromMemberEntityName: string;
  toMemberEntityId: string;
  toMemberEntityName: string;
  requestedByProfileName: string;
  status: string;
  message: string | null;
  respondedAt: string | null;
  createdAt: string;
  workspaceId: string | null;
};

export type CollaborationParticipant = {
  profileId: string;
  profileName: string;
  memberEntityId: string;
  memberEntityName: string;
  joinedAt: string;
};

// Mirrors REAK.Api.Models.Enums.ContactDataType — only ListingContact/DemandContact are
// currently enforced by RowLevelSecurity.sql's disclosure predicates (see
// CollaborationWorkspaceService.GrantContactDisclosureAsync).
export const CONTACT_DATA_TYPE = { ListingContact: 1, DemandContact: 2 } as const;

export type ContactDisclosure = {
  id: string;
  dataType: number;
  grantingMemberEntityId: string;
  grantingMemberEntityName: string;
  receivingMemberEntityId: string;
  receivingMemberEntityName: string;
  grantingProfileName: string;
  grantedAt: string;
  revokedAt: string | null;
  policyVersion: string;
};

export type CollaborationWorkspace = {
  id: string;
  collaborationRequestId: string;
  matchId: string | null;
  listingId: string | null;
  listingTitle: string | null;
  demandId: string | null;
  demandTitle: string | null;
  createdAt: string;
  participants: CollaborationParticipant[];
  contactDisclosures: ContactDisclosure[];
};

export type CollaborationMessage = { id: string; senderProfileName: string; body: string; createdAt: string };
export type CollaborationNote = { id: string; authorProfileName: string; body: string; createdAt: string };
export type CollaborationTask = {
  id: string;
  title: string;
  assignedToProfileName: string | null;
  dueDate: string | null;
  status: string;
  createdAt: string;
};
export type CollaborationViewing = { id: string; scheduledByProfileName: string; scheduledAt: string; notes: string | null; createdAt: string };
export type CollaborationFile = { id: string; fileName: string; uploadedByProfileName: string; createdAt: string };
export type CollaborationActivity = { id: string; profileName: string | null; action: string; createdAt: string };

// Mirrors REAK.Api.Models.Enums.CollaborationTaskStatus (only Open/Done exist — no in-between state)
export const TASK_STATUS_VALUES = { Open: 1, Done: 2 } as const;
