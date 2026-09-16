using System.ComponentModel.DataAnnotations;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Dto;

public record ListingContactInput([MaxLength(255)] string? ContactName, [MaxLength(50)] string? Phone, [MaxLength(255)] string? Email);

public record CreateListingRequest(
    [Required, MaxLength(500)] string Title,
    [MaxLength(4000)] string? Description,
    [Required] Guid PropertyTypeId,
    Guid? PropertySubtypeId,
    [Required] Guid PurposeId,
    [Required] Guid ProvinceId,
    [Required] Guid DistrictId,
    [Required] Guid MunicipalityId,
    [Required] Guid WardId,
    Guid? LocalityId,
    [MaxLength(255)] string? Landmark,
    decimal? Latitude,
    decimal? Longitude,
    [Required] Guid CurrencyId,
    [Required] decimal Price,
    bool IsPriceNegotiable,
    [Required] decimal LandArea,
    decimal? BuiltUpArea,
    [Required] Guid AreaUnitId,
    bool HasRoadAccess,
    decimal? RoadWidthFeet,
    [MaxLength(100)] string? RoadType,
    PropertyFacing? Facing,
    int? Bedrooms,
    int? Bathrooms,
    int? Floors,
    int? ParkingSpaces,
    Furnishing? Furnishing,
    [MaxLength(2000)] string? InternalNotes,
    IReadOnlyList<Guid>? AmenityIds,
    ListingContactInput? Contact,
    DateTime? ExpiresAt);

// Update reuses the same shape — a full replace of every editable field.
public record UpdateListingRequest(
    [Required, MaxLength(500)] string Title,
    [MaxLength(4000)] string? Description,
    [Required] Guid PropertyTypeId,
    Guid? PropertySubtypeId,
    [Required] Guid PurposeId,
    [Required] Guid ProvinceId,
    [Required] Guid DistrictId,
    [Required] Guid MunicipalityId,
    [Required] Guid WardId,
    Guid? LocalityId,
    [MaxLength(255)] string? Landmark,
    decimal? Latitude,
    decimal? Longitude,
    [Required] Guid CurrencyId,
    [Required] decimal Price,
    bool IsPriceNegotiable,
    [Required] decimal LandArea,
    decimal? BuiltUpArea,
    [Required] Guid AreaUnitId,
    bool HasRoadAccess,
    decimal? RoadWidthFeet,
    [MaxLength(100)] string? RoadType,
    PropertyFacing? Facing,
    int? Bedrooms,
    int? Bathrooms,
    int? Floors,
    int? ParkingSpaces,
    Furnishing? Furnishing,
    [MaxLength(2000)] string? InternalNotes,
    DateTime? ExpiresAt);

public record UpdateListingAmenitiesRequest(IReadOnlyList<Guid> AmenityIds);

public record UpdateListingVisibilityRequest(NetworkVisibility NetworkVisibility, bool IsPublicVisible, IReadOnlyList<Guid>? SelectedMemberEntityIds);

public record RejectListingRequest([Required, MaxLength(1000)] string Reason);

public record ListingSearchQuery
{
    public Guid? PropertyTypeId { get; init; }
    public Guid? PropertySubtypeId { get; init; }
    public Guid? PurposeId { get; init; }
    public Guid? ProvinceId { get; init; }
    public Guid? DistrictId { get; init; }
    public Guid? MunicipalityId { get; init; }
    public Guid? WardId { get; init; }
    public Guid? LocalityId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public decimal? MinArea { get; init; }
    public decimal? MaxArea { get; init; }
    public decimal? MinRoadWidth { get; init; }
    public int? MinBedrooms { get; init; }
    public int? MinBathrooms { get; init; }
    public int? MinParking { get; init; }
    public Furnishing? Furnishing { get; init; }
    public PropertyFacing? Facing { get; init; }
    public Guid[]? AmenityIds { get; init; }
    public Guid? MemberEntityId { get; init; }
    public ListingStatus? Status { get; init; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record ListingMediaDto(Guid Id, string Url, bool IsPrimary, int SortOrder, string? Caption);

public record ListingDocumentDto(Guid Id, string? DocumentType, DateTime CreatedAt);

public record ListingSummaryDto(
    Guid Id,
    string ReferenceCode,
    string Title,
    string MemberEntityName,
    string PropertyTypeName,
    string? PropertySubtypeName,
    string PurposeName,
    string ProvinceName,
    string DistrictName,
    string MunicipalityName,
    decimal Price,
    string CurrencyCode,
    bool IsPriceNegotiable,
    decimal LandArea,
    string AreaUnitName,
    int? Bedrooms,
    int? Bathrooms,
    string Status,
    string NetworkVisibility,
    bool IsPublicVisible,
    string? PrimaryImageUrl,
    DateTime CreatedAt);

public record ListingSearchResult(IReadOnlyList<ListingSummaryDto> Items, int TotalCount, int Page, int PageSize);

public record ListingDetailDto(
    Guid Id,
    string ReferenceCode,
    Guid MemberEntityId,
    string MemberEntityName,
    string Title,
    string? Description,
    string? InternalNotes,
    Guid PropertyTypeId,
    string PropertyTypeName,
    Guid? PropertySubtypeId,
    string? PropertySubtypeName,
    Guid PurposeId,
    string PurposeName,
    Guid ProvinceId,
    string ProvinceName,
    Guid DistrictId,
    string DistrictName,
    Guid MunicipalityId,
    string MunicipalityName,
    Guid WardId,
    int WardNumber,
    Guid? LocalityId,
    string? LocalityName,
    string? Landmark,
    decimal? Latitude,
    decimal? Longitude,
    Guid CurrencyId,
    string CurrencyCode,
    decimal Price,
    bool IsPriceNegotiable,
    decimal LandArea,
    decimal? BuiltUpArea,
    Guid AreaUnitId,
    string AreaUnitName,
    bool HasRoadAccess,
    decimal? RoadWidthFeet,
    string? RoadType,
    string? Facing,
    int? Bedrooms,
    int? Bathrooms,
    int? Floors,
    int? ParkingSpaces,
    string? Furnishing,
    string NetworkVisibility,
    bool IsPublicVisible,
    string Status,
    string? RejectionReason,
    DateTime? ExpiresAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsOwner,
    IReadOnlyList<string> AmenityNames,
    IReadOnlyList<Guid> AmenityIds,
    IReadOnlyList<ListingMediaDto> Media,
    IReadOnlyList<ListingDocumentDto> Documents);
