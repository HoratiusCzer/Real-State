using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Dto;

public record UpdateProfileRequest([Required, MaxLength(255)] string FullName, [MaxLength(50)] string? Phone);

public record UpdateMemberEntityRequest(
    [Required, MaxLength(255)] string Name,
    [MaxLength(2000)] string? Description,
    [MaxLength(255)] string? Website,
    [MaxLength(50)] string? Phone,
    [MaxLength(255)] string? Email,
    [MaxLength(500)] string? Address);

public record DashboardSummary(
    int ActivePropertiesCount,
    int DraftPropertiesCount,
    int ActiveRequirementsCount,
    int PotentialMatchesCount,
    int PendingCollaborationRequestsCount,
    int? SavedPropertiesCount,
    int ExpiringItemsCount,
    IReadOnlyList<RecentPropertyDto> RecentProperties);

public record RecentPropertyDto(Guid Id, string ReferenceCode, string Title, string Status, DateTime CreatedAt);
