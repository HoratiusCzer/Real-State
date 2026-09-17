using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Entities.Listings;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Matching;
using REAK.Api.Services.Notifications;
using REAK.Api.Services.Reference;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Listings;

/// <summary>Flow B (spec §2.4): Draft -> (submit) -> PendingReview -> Approved -> Expired/Archived,
/// with Rejected -> (edit) -> Draft. Every mutating method here relies on RLS
/// (Data/Security/RowLevelSecurity.sql) as the actual write authority — a caller who can read a
/// listing (e.g. via network-visibility sharing) but doesn't own it will have their UPDATE/DELETE
/// blocked at the database engine level, surfacing here as a SqlException we translate to
/// Forbidden rather than letting it become an unhandled 500. Also triggers Stage 8's matching
/// engine (Flow C, spec §2.4/§12: "a listing or demand is created or updated") at every point
/// that could newly qualify a listing for matching or change data that affects an existing
/// match's score — the engine itself no-ops when the listing isn't Approved or no rule set is
/// published, so it's safe to call unconditionally rather than duplicating that logic here.</summary>
public class ListingService(ReakDbContext db, IReferenceCodeGenerator referenceCodeGenerator, IMatchingEngine matchingEngine, INotificationService notificationService) : IListingService
{
    public async Task<ListingSearchResult> SearchAsync(ListingSearchQuery query, CallerContext? caller, CancellationToken ct = default)
    {
        // RLS's filter predicate already restricts which rows come back for this connection
        // (owner org, network-shared, or genuinely public) — everything below is refining that
        // set further, never widening it.
        var q = db.PropertyListings.Where(l => !l.IsDeleted);

        if (query.Status is not null) q = q.Where(l => l.Status == query.Status);
        if (query.PropertyTypeId is not null) q = q.Where(l => l.PropertyTypeId == query.PropertyTypeId);
        if (query.PropertySubtypeId is not null) q = q.Where(l => l.PropertySubtypeId == query.PropertySubtypeId);
        if (query.PurposeId is not null) q = q.Where(l => l.PurposeId == query.PurposeId);
        if (query.ProvinceId is not null) q = q.Where(l => l.ProvinceId == query.ProvinceId);
        if (query.DistrictId is not null) q = q.Where(l => l.DistrictId == query.DistrictId);
        if (query.MunicipalityId is not null) q = q.Where(l => l.MunicipalityId == query.MunicipalityId);
        if (query.WardId is not null) q = q.Where(l => l.WardId == query.WardId);
        if (query.LocalityId is not null) q = q.Where(l => l.LocalityId == query.LocalityId);
        if (query.MinPrice is not null) q = q.Where(l => l.Price >= query.MinPrice);
        if (query.MaxPrice is not null) q = q.Where(l => l.Price <= query.MaxPrice);
        if (query.MinArea is not null) q = q.Where(l => l.LandArea >= query.MinArea);
        if (query.MaxArea is not null) q = q.Where(l => l.LandArea <= query.MaxArea);
        if (query.MinRoadWidth is not null) q = q.Where(l => l.RoadWidthFeet != null && l.RoadWidthFeet >= query.MinRoadWidth);
        if (query.MinBedrooms is not null) q = q.Where(l => l.Bedrooms != null && l.Bedrooms >= query.MinBedrooms);
        if (query.MinBathrooms is not null) q = q.Where(l => l.Bathrooms != null && l.Bathrooms >= query.MinBathrooms);
        if (query.MinParking is not null) q = q.Where(l => l.ParkingSpaces != null && l.ParkingSpaces >= query.MinParking);
        if (query.Furnishing is not null) q = q.Where(l => l.Furnishing == query.Furnishing);
        if (query.Facing is not null) q = q.Where(l => l.Facing == query.Facing);
        if (query.MemberEntityId is not null) q = q.Where(l => l.MemberEntityId == query.MemberEntityId);
        if (query.AmenityIds is { Length: > 0 })
        {
            foreach (var amenityId in query.AmenityIds)
            {
                var id = amenityId;
                q = q.Where(l => l.ListingAmenities.Any(a => a.AmenityId == id));
            }
        }

        q = query.Sort switch
        {
            "price-asc" => q.OrderBy(l => l.Price),
            "price-desc" => q.OrderByDescending(l => l.Price),
            "area-asc" => q.OrderBy(l => l.LandArea),
            "area-desc" => q.OrderByDescending(l => l.LandArea),
            _ => q.OrderByDescending(l => l.CreatedAt),
        };

        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);

        var totalCount = await q.CountAsync(ct);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ListingSummaryDto(
                l.Id, l.ReferenceCode, l.Title, l.MemberEntity.Name,
                l.PropertyType.Name, l.PropertySubtype != null ? l.PropertySubtype.Name : null, l.Purpose.Name,
                l.Province.Name, l.District.Name, l.Municipality.Name,
                l.Price, l.Currency.Code, l.IsPriceNegotiable,
                l.LandArea, l.AreaUnit.Name, l.Bedrooms, l.Bathrooms,
                l.Status.ToString(), l.NetworkVisibility.ToString(), l.IsPublicVisible,
                l.Media.Where(m => m.IsPrimary).Select(m => m.Url).FirstOrDefault(),
                l.CreatedAt))
            .ToListAsync(ct);

        return new ListingSearchResult(items, totalCount, page, pageSize);
    }

    public async Task<ListingDetailDto?> GetDetailAsync(Guid id, CallerContext? caller, CancellationToken ct = default)
    {
        var listing = await db.PropertyListings
            .Where(l => l.Id == id && !l.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (listing is null)
        {
            return null;
        }

        var isOwner = caller is not null && (caller.IsSystemAdmin || caller.MemberEntityIds.Contains(listing.MemberEntityId));
        return await ProjectDetailAsync(listing.Id, isOwner, ct);
    }

    public async Task<(ListingOp Op, Guid? Id)> CreateAsync(CallerContext caller, CreateListingRequest request, CancellationToken ct = default)
    {
        if (caller.MemberEntityIds.Count == 0)
        {
            return (new ListingOp(ListingOpResult.Forbidden, "You must belong to a member organization to create a listing."), null);
        }

        var memberEntityId = caller.MemberEntityIds[0];
        var referenceCode = await referenceCodeGenerator.NextListingCodeAsync(ct);

        var listing = new PropertyListing
        {
            Id = Guid.NewGuid(),
            ReferenceCode = referenceCode,
            MemberEntityId = memberEntityId,
            CreatedByProfileId = caller.ProfileId,
            Title = request.Title,
            Description = request.Description,
            InternalNotes = request.InternalNotes,
            PropertyTypeId = request.PropertyTypeId,
            PropertySubtypeId = request.PropertySubtypeId,
            PurposeId = request.PurposeId,
            ProvinceId = request.ProvinceId,
            DistrictId = request.DistrictId,
            MunicipalityId = request.MunicipalityId,
            WardId = request.WardId,
            LocalityId = request.LocalityId,
            Landmark = request.Landmark,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CurrencyId = request.CurrencyId,
            Price = request.Price,
            IsPriceNegotiable = request.IsPriceNegotiable,
            LandArea = request.LandArea,
            BuiltUpArea = request.BuiltUpArea,
            AreaUnitId = request.AreaUnitId,
            HasRoadAccess = request.HasRoadAccess,
            RoadWidthFeet = request.RoadWidthFeet,
            RoadType = request.RoadType,
            Facing = request.Facing,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            Floors = request.Floors,
            ParkingSpaces = request.ParkingSpaces,
            Furnishing = request.Furnishing,
            Status = ListingStatus.Draft,
            NetworkVisibility = NetworkVisibility.OwnerOnly,
            ExpiresAt = request.ExpiresAt,
        };

        db.PropertyListings.Add(listing);

        if (request.AmenityIds is { Count: > 0 })
        {
            foreach (var amenityId in request.AmenityIds.Distinct())
            {
                db.ListingAmenities.Add(new ListingAmenity { Id = Guid.NewGuid(), ListingId = listing.Id, AmenityId = amenityId });
            }
        }

        if (request.Contact is { } contact && (contact.ContactName is not null || contact.Phone is not null || contact.Email is not null))
        {
            db.ListingContacts.Add(new ListingContact
            {
                ListingId = listing.Id,
                ContactName = contact.ContactName,
                Phone = contact.Phone,
                Email = contact.Email,
            });
        }

        await db.SaveChangesAsync(ct);
        return (new ListingOp(ListingOpResult.Success), listing.Id);
    }

    public async Task<ListingOp> UpdateAsync(Guid id, CallerContext caller, UpdateListingRequest request, CancellationToken ct = default)
    {
        var listing = await db.PropertyListings.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);
        if (listing is null)
        {
            return new ListingOp(ListingOpResult.NotFound);
        }

        listing.Title = request.Title;
        listing.Description = request.Description;
        listing.InternalNotes = request.InternalNotes;
        listing.PropertyTypeId = request.PropertyTypeId;
        listing.PropertySubtypeId = request.PropertySubtypeId;
        listing.PurposeId = request.PurposeId;
        listing.ProvinceId = request.ProvinceId;
        listing.DistrictId = request.DistrictId;
        listing.MunicipalityId = request.MunicipalityId;
        listing.WardId = request.WardId;
        listing.LocalityId = request.LocalityId;
        listing.Landmark = request.Landmark;
        listing.Latitude = request.Latitude;
        listing.Longitude = request.Longitude;
        listing.CurrencyId = request.CurrencyId;
        listing.Price = request.Price;
        listing.IsPriceNegotiable = request.IsPriceNegotiable;
        listing.LandArea = request.LandArea;
        listing.BuiltUpArea = request.BuiltUpArea;
        listing.AreaUnitId = request.AreaUnitId;
        listing.HasRoadAccess = request.HasRoadAccess;
        listing.RoadWidthFeet = request.RoadWidthFeet;
        listing.RoadType = request.RoadType;
        listing.Facing = request.Facing;
        listing.Bedrooms = request.Bedrooms;
        listing.Bathrooms = request.Bathrooms;
        listing.Floors = request.Floors;
        listing.ParkingSpaces = request.ParkingSpaces;
        listing.Furnishing = request.Furnishing;
        listing.ExpiresAt = request.ExpiresAt;
        listing.UpdatedByProfileId = caller.ProfileId;
        listing.UpdatedAt = DateTime.UtcNow;

        return await SaveGuardedAndRecomputeAsync(id, ct);
    }

    public async Task<ListingOp> SubmitAsync(Guid id, CallerContext caller, CancellationToken ct = default)
    {
        var listing = await db.PropertyListings.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);
        if (listing is null) return new ListingOp(ListingOpResult.NotFound);
        if (listing.Status is not (ListingStatus.Draft or ListingStatus.Rejected))
        {
            return new ListingOp(ListingOpResult.InvalidState, "Only a draft or rejected listing can be submitted.");
        }

        var moderationRequired = await db.FeatureFlags
            .Where(f => f.Key == "property_moderation_required")
            .Select(f => f.IsEnabled)
            .FirstOrDefaultAsync(ct);

        listing.Status = moderationRequired ? ListingStatus.PendingReview : ListingStatus.Approved;
        if (!moderationRequired)
        {
            listing.ApprovedAt = DateTime.UtcNow;
        }
        listing.RejectionReason = null;
        listing.UpdatedByProfileId = caller.ProfileId;
        listing.UpdatedAt = DateTime.UtcNow;

        return await SaveGuardedAndRecomputeAsync(id, ct);
    }

    public async Task<ListingOp> ApproveAsync(Guid id, CallerContext caller, CancellationToken ct = default)
    {
        var listing = await db.PropertyListings.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);
        if (listing is null) return new ListingOp(ListingOpResult.NotFound);
        if (listing.Status != ListingStatus.PendingReview)
        {
            return new ListingOp(ListingOpResult.InvalidState, "Only a listing pending review can be approved.");
        }

        listing.Status = ListingStatus.Approved;
        listing.ApprovedByProfileId = caller.ProfileId;
        listing.ApprovedAt = DateTime.UtcNow;
        listing.RejectionReason = null;

        var op = await SaveGuardedAndRecomputeAsync(id, ct);
        if (op.Result == ListingOpResult.Success)
        {
            await notificationService.NotifyAsync(listing.CreatedByProfileId, NotificationType.ListingApproved, $"\"{listing.Title}\" was approved", null, $"/portal/properties/{id}", ct);
        }
        return op;
    }

    public async Task<ListingOp> RejectAsync(Guid id, CallerContext caller, string reason, CancellationToken ct = default)
    {
        var listing = await db.PropertyListings.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);
        if (listing is null) return new ListingOp(ListingOpResult.NotFound);
        if (listing.Status != ListingStatus.PendingReview)
        {
            return new ListingOp(ListingOpResult.InvalidState, "Only a listing pending review can be rejected.");
        }

        listing.Status = ListingStatus.Rejected;
        listing.RejectionReason = reason;
        listing.ApprovedByProfileId = caller.ProfileId;
        listing.ApprovedAt = null;

        var op = await SaveGuardedAsync(ct);
        if (op.Result == ListingOpResult.Success)
        {
            await notificationService.NotifyAsync(listing.CreatedByProfileId, NotificationType.ListingRejected, $"\"{listing.Title}\" was rejected", reason, $"/portal/properties/{id}", ct);
        }
        return op;
    }

    public async Task<ListingOp> ArchiveAsync(Guid id, CallerContext caller, CancellationToken ct = default)
    {
        var listing = await db.PropertyListings.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);
        if (listing is null) return new ListingOp(ListingOpResult.NotFound);

        listing.Status = ListingStatus.Archived;
        listing.UpdatedByProfileId = caller.ProfileId;
        listing.UpdatedAt = DateTime.UtcNow;

        return await SaveGuardedAsync(ct);
    }

    public async Task<ListingOp> SoftDeleteAsync(Guid id, CallerContext caller, CancellationToken ct = default)
    {
        var listing = await db.PropertyListings.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);
        if (listing is null) return new ListingOp(ListingOpResult.NotFound);

        listing.IsDeleted = true;
        listing.DeletedAt = DateTime.UtcNow;
        listing.UpdatedByProfileId = caller.ProfileId;
        listing.UpdatedAt = DateTime.UtcNow;

        return await SaveGuardedAsync(ct);
    }

    public async Task<ListingOp> ReplaceAmenitiesAsync(Guid id, CallerContext caller, IReadOnlyList<Guid> amenityIds, CancellationToken ct = default)
    {
        var listing = await db.PropertyListings.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);
        if (listing is null) return new ListingOp(ListingOpResult.NotFound);

        var existing = await db.ListingAmenities.Where(a => a.ListingId == id).ToListAsync(ct);
        db.ListingAmenities.RemoveRange(existing);
        foreach (var amenityId in amenityIds.Distinct())
        {
            db.ListingAmenities.Add(new ListingAmenity { Id = Guid.NewGuid(), ListingId = id, AmenityId = amenityId });
        }
        listing.UpdatedByProfileId = caller.ProfileId;
        listing.UpdatedAt = DateTime.UtcNow;

        return await SaveGuardedAndRecomputeAsync(id, ct);
    }

    public async Task<ListingOp> UpdateVisibilityAsync(Guid id, CallerContext caller, UpdateListingVisibilityRequest request, CancellationToken ct = default)
    {
        var listing = await db.PropertyListings.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);
        if (listing is null) return new ListingOp(ListingOpResult.NotFound);

        listing.NetworkVisibility = request.NetworkVisibility;
        listing.IsPublicVisible = request.IsPublicVisible;
        listing.UpdatedByProfileId = caller.ProfileId;
        listing.UpdatedAt = DateTime.UtcNow;

        var existing = await db.ListingVisibilityMembers.Where(m => m.ListingId == id).ToListAsync(ct);
        db.ListingVisibilityMembers.RemoveRange(existing);
        if (request.NetworkVisibility == NetworkVisibility.SelectedMembers && request.SelectedMemberEntityIds is { Count: > 0 })
        {
            foreach (var memberEntityId in request.SelectedMemberEntityIds.Distinct())
            {
                db.ListingVisibilityMembers.Add(new ListingVisibilityMember { Id = Guid.NewGuid(), ListingId = id, MemberEntityId = memberEntityId });
            }
        }

        return await SaveGuardedAsync(ct);
    }

    public async Task<(ListingOp Op, ListingContactInput? Contact)> GetContactAsync(Guid id, CallerContext caller, CancellationToken ct = default)
    {
        // ListingContacts is RLS-filtered separately from PropertyListings — this query only
        // returns a row if the caller passes fn_ListingContactReadPredicate (owner org, or an
        // explicit unrevoked disclosure grant once Stage 9 exists).
        var contact = await db.ListingContacts.FirstOrDefaultAsync(c => c.ListingId == id, ct);
        if (contact is null)
        {
            var exists = await db.PropertyListings.AnyAsync(l => l.Id == id && !l.IsDeleted, ct);
            return (new ListingOp(exists ? ListingOpResult.Success : ListingOpResult.NotFound), null);
        }

        return (new ListingOp(ListingOpResult.Success), new ListingContactInput(contact.ContactName, contact.Phone, contact.Email));
    }

    public async Task<ListingOp> UpdateContactAsync(Guid id, CallerContext caller, ListingContactInput contact, CancellationToken ct = default)
    {
        if (!await db.PropertyListings.AnyAsync(l => l.Id == id && !l.IsDeleted, ct))
        {
            return new ListingOp(ListingOpResult.NotFound);
        }

        var existing = await db.ListingContacts.FirstOrDefaultAsync(c => c.ListingId == id, ct);
        if (existing is null)
        {
            db.ListingContacts.Add(new ListingContact
            {
                ListingId = id,
                ContactName = contact.ContactName,
                Phone = contact.Phone,
                Email = contact.Email,
            });
        }
        else
        {
            existing.ContactName = contact.ContactName;
            existing.Phone = contact.Phone;
            existing.Email = contact.Email;
        }

        return await SaveGuardedAsync(ct);
    }

    private async Task<ListingDetailDto> ProjectDetailAsync(Guid id, bool isOwner, CancellationToken ct)
    {
        var dto = await db.PropertyListings
            .Where(l => l.Id == id)
            .Select(l => new ListingDetailDto(
                l.Id, l.ReferenceCode, l.MemberEntityId, l.MemberEntity.Name,
                l.Title, l.Description, isOwner ? l.InternalNotes : null,
                l.PropertyTypeId, l.PropertyType.Name, l.PropertySubtypeId, l.PropertySubtype != null ? l.PropertySubtype.Name : null,
                l.PurposeId, l.Purpose.Name,
                l.ProvinceId, l.Province.Name, l.DistrictId, l.District.Name, l.MunicipalityId, l.Municipality.Name,
                l.WardId, l.Ward.Number, l.LocalityId, l.Locality != null ? l.Locality.Name : null,
                l.Landmark, l.Latitude, l.Longitude,
                l.CurrencyId, l.Currency.Code, l.Price, l.IsPriceNegotiable,
                l.LandArea, l.BuiltUpArea, l.AreaUnitId, l.AreaUnit.Name,
                l.HasRoadAccess, l.RoadWidthFeet, l.RoadType, l.Facing != null ? l.Facing.ToString() : null,
                l.Bedrooms, l.Bathrooms, l.Floors, l.ParkingSpaces, l.Furnishing != null ? l.Furnishing.ToString() : null,
                l.NetworkVisibility.ToString(), l.IsPublicVisible, l.Status.ToString(), l.RejectionReason,
                l.ExpiresAt, l.CreatedAt, l.UpdatedAt, isOwner,
                l.ListingAmenities.Select(a => a.Amenity.Name).ToList(),
                l.ListingAmenities.Select(a => a.AmenityId).ToList(),
                l.Media.OrderBy(m => m.SortOrder).Select(m => new ListingMediaDto(m.Id, m.Url, m.IsPrimary, m.SortOrder, m.Caption)).ToList(),
                isOwner
                    ? l.Documents.Select(d => new ListingDocumentDto(d.Id, d.DocumentType, d.CreatedAt)).ToList()
                    : new List<ListingDocumentDto>()))
            .FirstAsync(ct);

        return dto;
    }

    private async Task<ListingOp> SaveGuardedAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
            return new ListingOp(ListingOpResult.Success);
        }
        catch (DbUpdateException ex) when (IsRlsBlockPredicateViolation(ex))
        {
            foreach (var entry in db.ChangeTracker.Entries())
            {
                entry.State = EntityState.Unchanged;
            }
            return new ListingOp(ListingOpResult.Forbidden, "You don't have permission to modify this listing.");
        }
    }

    /// <summary>Saves, then — only on success — asks the matching engine to recompute this
    /// listing's matches. The engine itself no-ops if the listing isn't Approved or no rule set
    /// is published, so every call site that could plausibly affect matching eligibility or score
    /// just calls this instead of SaveGuardedAsync, rather than each one re-deciding whether a
    /// recompute is warranted.</summary>
    private async Task<ListingOp> SaveGuardedAndRecomputeAsync(Guid listingId, CancellationToken ct)
    {
        var op = await SaveGuardedAsync(ct);
        if (op.Result == ListingOpResult.Success)
        {
            await matchingEngine.RecomputeForListingAsync(listingId, ct);
        }
        return op;
    }

    private static bool IsRlsBlockPredicateViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException sqlEx && sqlEx.Message.Contains("block predicate", StringComparison.OrdinalIgnoreCase);
}
