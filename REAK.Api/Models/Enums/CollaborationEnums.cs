namespace REAK.Api.Models.Enums;

public enum CollaborationRequestStatus
{
    Pending = 1,
    Accepted = 2,
    Declined = 3,
    Cancelled = 4,
}

/// <summary>What kind of private data a collaboration_contact_disclosures grant unlocks.</summary>
public enum ContactDataType
{
    ListingContact = 1,
    DemandContact = 2,
    Phone = 3,
    Email = 4,
}

public enum CollaborationTaskStatus
{
    Open = 1,
    Done = 2,
}
