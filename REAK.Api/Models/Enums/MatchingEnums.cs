namespace REAK.Api.Models.Enums;

public enum MatchRuleSetStatus
{
    Draft = 1,
    Published = 2,
    Archived = 3,
}

/// <summary>What a single match_rules row scores. Weight/tolerance for each criterion is admin-configured data, never hardcoded in application code (spec §12).</summary>
public enum MatchCriterion
{
    Location = 1,
    Price = 2,
    Area = 3,
    PropertyType = 4,
    Purpose = 5,
    Bedrooms = 6,
    Bathrooms = 7,
    Furnishing = 8,
    Amenities = 9,
}

public enum MatchComponentResult
{
    Pass = 1,
    Partial = 2,
    Fail = 3,
    MissingData = 4,
}

public enum MatchStatus
{
    New = 1,
    Shortlisted = 2,
    Dismissed = 3,
    Reopened = 4,
}

public enum MatchActionType
{
    Shortlist = 1,
    Dismiss = 2,
    Reopen = 3,
    RequestCollaboration = 4,
    ReportIncorrectData = 5,
}
