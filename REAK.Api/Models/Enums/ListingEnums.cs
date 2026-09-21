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

/// <summary>How a listing's LandArea was entered (spec §11) — Nepal's two traditional
/// geography-based compound unit systems (Hill: Ropani-Aana-Paisa-Dam; Terai:
/// Bigha-Kattha-Dhur), or a direct modern single-value entry. Stored explicitly rather than
/// inferred from which compound columns are populated, so every reader (edit-form
/// repopulation, display formatting) doesn't need to re-derive it.</summary>
public enum LandAreaMeasurementSystem
{
    RopaniSystem = 1,
    BighaSystem = 2,
    SquareFeet = 3,
    SquareMetres = 4,
}
