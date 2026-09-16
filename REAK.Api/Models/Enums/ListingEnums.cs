namespace REAK.Api.Models.Enums;

/// <summary>Flow B (spec): Draft -> (submit) -> PendingReview -> Approved -> Expired/Archived, with Rejected -> (edit) -> Draft.</summary>
public enum ListingStatus
{
    Draft = 1,
    PendingReview = 2,
    Approved = 3,
    Rejected = 4,
    Expired = 5,
    Archived = 6,
}

/// <summary>Network visibility and public-site visibility are separate gates (spec §2.4 Flow B) — this enum is the network gate only; public visibility is a separate bool plus the public_properties_enabled feature flag.</summary>
public enum NetworkVisibility
{
    OwnerOnly = 1,
    SelectedMembers = 2,
    AllMembers = 3,
}

public enum Furnishing
{
    Unfurnished = 1,
    SemiFurnished = 2,
    Furnished = 3,
}

public enum PropertyFacing
{
    North = 1,
    South = 2,
    East = 3,
    West = 4,
    NorthEast = 5,
    NorthWest = 6,
    SouthEast = 7,
    SouthWest = 8,
}
