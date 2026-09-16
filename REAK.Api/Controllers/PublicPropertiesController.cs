using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Enums;

namespace REAK.Api.Controllers;

/// <summary>The sanitized public projection spec §16 requires — never a direct query against the
/// private listings table for anonymous callers. Deliberately re-checks every gate explicitly
/// (public_properties_enabled AND Approved AND IsPublicVisible AND active member AND not deleted
/// AND not expired) even though RLS's fn_ListingReadPredicate public branch already enforces the
/// same thing at the database engine level for a connection with no session context — this is
/// intentional defense-in-depth, not redundancy to trim, and the explicit field-by-field Select()
/// below is the "field allowlist" half of §16 (RLS is the row half).</summary>
[ApiController]
[Route("api/public/properties")]
[AllowAnonymous]
public class PublicPropertiesController(ReakDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? propertyTypeId, [FromQuery] Guid? purposeId,
        [FromQuery] Guid? provinceId, [FromQuery] Guid? districtId, [FromQuery] Guid? municipalityId,
        [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice,
        [FromQuery] int? minBedrooms, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var enabled = await IsPublicPropertiesEnabledAsync(ct);
        if (!enabled)
        {
            return Ok(new PublicPropertySearchResult(false, [], 0, page, pageSize));
        }

        var now = DateTime.UtcNow;
        var q = PublicListingsBase(now);

        if (propertyTypeId is not null) q = q.Where(l => l.PropertyTypeId == propertyTypeId);
        if (purposeId is not null) q = q.Where(l => l.PurposeId == purposeId);
        if (provinceId is not null) q = q.Where(l => l.ProvinceId == provinceId);
        if (districtId is not null) q = q.Where(l => l.DistrictId == districtId);
        if (municipalityId is not null) q = q.Where(l => l.MunicipalityId == municipalityId);
        if (minPrice is not null) q = q.Where(l => l.Price >= minPrice);
        if (maxPrice is not null) q = q.Where(l => l.Price <= maxPrice);
        if (minBedrooms is not null) q = q.Where(l => l.Bedrooms != null && l.Bedrooms >= minBedrooms);

        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(page, 1);

        var totalCount = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new PublicPropertySummaryDto(
                l.Id, l.ReferenceCode, l.Title, l.MemberEntity.Name,
                l.PropertyType.Name, l.Purpose.Name,
                l.Province.Name, l.District.Name, l.Municipality.Name,
                l.Price, l.Currency.Code, l.LandArea, l.AreaUnit.Name,
                l.Bedrooms, l.Bathrooms,
                l.Media.Where(m => m.IsPrimary).Select(m => m.Url).FirstOrDefault()))
            .ToListAsync(ct);

        return Ok(new PublicPropertySearchResult(true, items, totalCount, page, pageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        if (!await IsPublicPropertiesEnabledAsync(ct))
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        var detail = await PublicListingsBase(now)
            .Where(l => l.Id == id)
            .Select(l => new PublicPropertyDetailDto(
                l.Id, l.ReferenceCode, l.Title, l.Description, l.MemberEntity.Name,
                l.PropertyType.Name, l.PropertySubtype != null ? l.PropertySubtype.Name : null, l.Purpose.Name,
                l.Province.Name, l.District.Name, l.Municipality.Name, l.Ward.Number,
                l.Locality != null ? l.Locality.Name : null, l.Landmark,
                l.Price, l.Currency.Code, l.IsPriceNegotiable,
                l.LandArea, l.BuiltUpArea, l.AreaUnit.Name,
                l.HasRoadAccess, l.RoadWidthFeet, l.Facing != null ? l.Facing.ToString() : null,
                l.Bedrooms, l.Bathrooms, l.Floors, l.ParkingSpaces, l.Furnishing != null ? l.Furnishing.ToString() : null,
                l.ListingAmenities.Select(a => a.Amenity.Name).ToList(),
                l.Media.OrderBy(m => m.SortOrder).Select(m => m.Url).ToList(),
                l.CreatedAt))
            .FirstOrDefaultAsync(ct);

        return detail is null ? NotFound() : Ok(detail);
    }

    private IQueryable<Models.Entities.Listings.PropertyListing> PublicListingsBase(DateTime now) =>
        db.PropertyListings.Where(l =>
            !l.IsDeleted &&
            l.Status == ListingStatus.Approved &&
            l.IsPublicVisible &&
            l.MemberEntity.IsActive &&
            (l.ExpiresAt == null || l.ExpiresAt > now));

    private async Task<bool> IsPublicPropertiesEnabledAsync(CancellationToken ct) =>
        await db.FeatureFlags.Where(f => f.Key == "public_properties_enabled").Select(f => f.IsEnabled).FirstOrDefaultAsync(ct);
}
