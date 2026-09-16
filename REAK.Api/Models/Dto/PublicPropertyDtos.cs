namespace REAK.Api.Models.Dto;

/// <summary>Explicit field allowlist for the anonymous public property projection (spec §16) —
/// deliberately a separate, narrower shape than ListingSummaryDto/ListingDetailDto so a future
/// edit to the authenticated DTOs can never silently widen what anonymous visitors see.</summary>
public record PublicPropertySummaryDto(
    Guid Id,
    string ReferenceCode,
    string Title,
    string MemberEntityName,
    string PropertyTypeName,
    string PurposeName,
    string ProvinceName,
    string DistrictName,
    string MunicipalityName,
    decimal Price,
    string CurrencyCode,
    decimal LandArea,
    string AreaUnitName,
    int? Bedrooms,
    int? Bathrooms,
    string? PrimaryImageUrl);

public record PublicPropertyDetailDto(
    Guid Id,
    string ReferenceCode,
    string Title,
    string? Description,
    string MemberEntityName,
    string PropertyTypeName,
    string? PropertySubtypeName,
    string PurposeName,
    string ProvinceName,
    string DistrictName,
    string MunicipalityName,
    int WardNumber,
    string? LocalityName,
    string? Landmark,
    decimal Price,
    string CurrencyCode,
    bool IsPriceNegotiable,
    decimal LandArea,
    decimal? BuiltUpArea,
    string AreaUnitName,
    bool HasRoadAccess,
    decimal? RoadWidthFeet,
    string? Facing,
    int? Bedrooms,
    int? Bathrooms,
    int? Floors,
    int? ParkingSpaces,
    string? Furnishing,
    IReadOnlyList<string> AmenityNames,
    IReadOnlyList<string> MediaUrls,
    DateTime CreatedAt);

public record PublicPropertySearchResult(bool Enabled, IReadOnlyList<PublicPropertySummaryDto> Items, int TotalCount, int Page, int PageSize);
