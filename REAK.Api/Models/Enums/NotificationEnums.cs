namespace REAK.Api.Models.Enums;

/// <summary>Spec §14 — the fixed list of notification categories the platform must support.</summary>
public enum NotificationType
{
    Match = 1,
    CollaborationRequest = 2,
    CollaborationAccepted = 3,
    CollaborationDeclined = 4,
    Message = 5,
    ListingApproved = 6,
    ListingRejected = 7,
    Expiry = 8,
    Invitation = 9,
    AccountEvent = 10,
    AssociationNotice = 11,
}
