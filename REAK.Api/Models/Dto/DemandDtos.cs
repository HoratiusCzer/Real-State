using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Dto;

public record DemandContactInput(
    [MaxLength(255)] string? ClientName,
    [MaxLength(50)] string? Phone,
    [MaxLength(255)] string? Email,
    [MaxLength(1000)] string? ConfidentialNotes);

public record DemandLocationInput(
    [Required] Guid ProvinceId,
    Guid? DistrictId,
    Guid? MunicipalityId,
    Guid? WardId,
    Guid? LocalityId);

public record CreateDemandRequest(
    [Required, MaxLength(500)] string Title,
    [MaxLength(4000)] string? Description,
    [Required] Guid PurposeId,
    Guid? CurrencyId,
    decimal? MinBudget,
    decimal? MaxBudget,
    Guid? AreaUnitId,
    decimal? MinArea,
    decimal? MaxArea,
    int? MinBedrooms,
    int? MinBathrooms,
    [MaxLength(2000)] string? InternalNotes,
    IReadOnlyList<Guid>? PropertyTypeIds,
    IReadOnlyList<DemandLocationInput>? Locations,
    IReadOnlyList<Guid>? AmenityIds,
    DemandContactInput? Contact,
    DateTime? ExpiresAt);

public record UpdateDemandRequest(
    [Required, MaxLength(500)] string Title,
    [MaxLength(4000)] string? Description,
    [Required] Guid PurposeId,
    Guid? CurrencyId,
    decimal? MinBudget,
    decimal? MaxBudget,
    Guid? AreaUnitId,
    decimal? MinArea,
    decimal? MaxArea,
    int? MinBedrooms,
    int? MinBathrooms,
    [MaxLength(2000)] string? InternalNotes,
    DateTime? ExpiresAt);

public record UpdateDemandPropertyTypesRequest(IReadOnlyList<Guid> PropertyTypeIds);
public record UpdateDemandLocationsRequest(IReadOnlyList<DemandLocationInput> Locations);
public record UpdateDemandAmenitiesRequest(IReadOnlyList<Guid> AmenityIds);
public record UpdateDemandVisibilityRequest(Enums.NetworkVisibility NetworkVisibility, IReadOnlyList<Guid>? SelectedMemberEntityIds);

public record DemandSearchQuery
{
    public Guid? PropertyTypeId { get; init; }
    public Guid? PurposeId { get; init; }
    public Guid? ProvinceId { get; init; }
    public Guid? DistrictId { get; init; }
    public decimal? MinBudget { get; init; }
    public decimal? MaxBudget { get; init; }
    public int? MinBedrooms { get; init; }
    public Guid? MemberEntityId { get; init; }
    public Enums.DemandStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record DemandSummaryDto(
    Guid Id,
    string ReferenceCode,
    string Title,
    string MemberEntityName,
    string PurposeName,
    decimal? MinBudget,
    decimal? MaxBudget,
    string? CurrencyCode,
    string Status,
    IReadOnlyList<string> PropertyTypeNames,
    IReadOnlyList<string> LocationSummaries,
    DateTime CreatedAt);

public record DemandSearchResult(IReadOnlyList<DemandSummaryDto> Items, int TotalCount, int Page, int PageSize);

public record DemandLocationDto(Guid Id, Guid ProvinceId, string ProvinceName, string? DistrictName, string? MunicipalityName, int? WardNumber, string? LocalityName);

public record DemandDetailDto(
    Guid Id,
    string ReferenceCode,
    Guid MemberEntityId,
    string MemberEntityName,
    string Title,
    string? Description,
    string? InternalNotes,
    Guid PurposeId,
    string PurposeName,
    Guid? CurrencyId,
    string? CurrencyCode,
    decimal? MinBudget,
    decimal? MaxBudget,
    Guid? AreaUnitId,
    string? AreaUnitName,
    decimal? MinArea,
    decimal? MaxArea,
    int? MinBedrooms,
    int? MinBathrooms,
    string NetworkVisibility,
    string Status,
    DateTime? ExpiresAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsOwner,
    IReadOnlyList<Guid> PropertyTypeIds,
    IReadOnlyList<string> PropertyTypeNames,
    IReadOnlyList<DemandLocationDto> Locations,
    IReadOnlyList<Guid> AmenityIds,
    IReadOnlyList<string> AmenityNames);
