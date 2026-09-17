using System.ComponentModel.DataAnnotations;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Dto;

public record CreateMatchRuleSetRequest([Required, MaxLength(255)] string Name, [MaxLength(2000)] string? Description);

public record CreateMatchRuleRequest(
    [Required] MatchCriterion Criterion,
    [Required] decimal Weight,
    bool IsRequired,
    decimal? ToleranceValue,
    int SortOrder);

public record MatchRuleDto(Guid Id, MatchCriterion Criterion, decimal Weight, bool IsRequired, decimal? ToleranceValue, int SortOrder);

public record MatchRuleSetDto(
    Guid Id, string Name, int Version, string Status, string? Description,
    DateTime CreatedAt, DateTime? PublishedAt, IReadOnlyList<MatchRuleDto> Rules);

public record MatchComponentDto(MatchCriterion Criterion, MatchComponentResult Result, decimal? NumericDelta, string? DetailText);

public record MatchActionDto(Guid Id, MatchActionType ActionType, string? Notes, DateTime CreatedAt, string ByProfileName);

public record MatchSummaryDto(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    string ListingReferenceCode,
    string ListingMemberEntityName,
    Guid DemandId,
    string DemandTitle,
    string DemandReferenceCode,
    string DemandMemberEntityName,
    decimal Score,
    string Status,
    DateTime ComputedAt);

public record MatchSearchResult(IReadOnlyList<MatchSummaryDto> Items, int TotalCount, int Page, int PageSize);

public record MatchDetailDto(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    string ListingReferenceCode,
    Guid ListingMemberEntityId,
    string ListingMemberEntityName,
    Guid DemandId,
    string DemandTitle,
    string DemandReferenceCode,
    Guid DemandMemberEntityId,
    string DemandMemberEntityName,
    decimal Score,
    string Status,
    Guid MatchRuleSetId,
    string MatchRuleSetName,
    int MatchRuleSetVersion,
    DateTime ComputedAt,
    IReadOnlyList<MatchComponentDto> Components,
    IReadOnlyList<MatchActionDto> Actions);

public record RecordMatchActionRequest([Required] MatchActionType ActionType, [MaxLength(1000)] string? Notes);
