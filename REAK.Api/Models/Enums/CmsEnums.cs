namespace REAK.Api.Models.Enums;

/// <summary>Flow E (spec): Draft -> Review -> Published -> Archived. Shared by every CMS content type.</summary>
public enum ContentStatus
{
    Draft = 1,
    Review = 2,
    Published = 3,
    Archived = 4,
}
