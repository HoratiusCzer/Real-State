using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Entities.Demands;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Audit;
using REAK.Api.Services.Matching;
using REAK.Api.Services.Reference;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Demands;

/// <summary>Spec §9 — mirrors ListingService's structure and RLS-reliance pattern deliberately
/// (spec §2.3: listings and demands are two sides of one relationship). Two real differences from
/// listings: no moderation (DemandStatus has no PendingReview/Rejected — spec never asks for one,
/// and no property_moderation_required-equivalent flag exists for demands), and PropertyType/
/// Location are proper many-to-many join tables here rather than single required FKs, since a
/// demand can accept several property types and several acceptable locations at whatever
/// hierarchy depth the client cares about (spec §9: "do not model relationships as arrays — use
/// proper join tables"). Also mirrors ListingService's Stage 8 matching-engine trigger points —
/// see SaveGuardedAndRecomputeAsync.</summary>
public class DemandService(ReakDbContext db, IReferenceCodeGenerator referenceCodeGenerator, IMatchingEngine matchingEngine, IAuditLogService auditLogService) : IDemandService
{
    public async Task<DemandSearchResult> SearchAsync(DemandSearchQuery query, CallerContext? caller, CancellationToken ct = default)
    {
        var q = db.Demands.Where(d => !d.IsDeleted);

        if (query.Status is not null) q = q.Where(d => d.Status == query.Status);
        if (query.PurposeId is not null) q = q.Where(d => d.PurposeId == query.PurposeId);
        if (query.MinBudget is not null) q = q.Where(d => d.MaxBudget == null || d.MaxBudget >= query.MinBudget);
        if (query.MaxBudget is not null) q = q.Where(d => d.MinBudget == null || d.MinBudget <= query.MaxBudget);
        if (query.MinBedrooms is not null) q = q.Where(d => d.MinBedrooms == null || d.MinBedrooms <= query.MinBedrooms);
        if (query.MemberEntityId is not null) q = q.Where(d => d.MemberEntityId == query.MemberEntityId);
        if (query.PropertyTypeId is not null)
        {
            var typeId = query.PropertyTypeId.Value;
            q = q.Where(d => d.PropertyTypes.Any(pt => pt.PropertyTypeId == typeId));
        }
        if (query.ProvinceId is not null)
        {
            var provinceId = query.ProvinceId.Value;
            q = q.Where(d => d.Locations.Any(l => l.ProvinceId == provinceId));
        }
        if (query.DistrictId is not null)
        {
            var districtId = query.DistrictId.Value;
            q = q.Where(d => d.Locations.Any(l => l.DistrictId == districtId));
        }

        q = q.OrderByDescending(d => d.CreatedAt);

        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);

        var totalCount = await q.CountAsync(ct);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DemandSummaryDto(
                d.Id, d.ReferenceCode, d.Title, d.MemberEntity.Name, d.Purpose.Name,
                d.MinBudget, d.MaxBudget, d.Currency != null ? d.Currency.Code : null,
                d.Status.ToString(),
                d.PropertyTypes.Select(pt => pt.PropertyType.Name).ToList(),
                d.Locations.Select(l =>
                    (l.District != null ? l.District.Name : l.Province.Name)).ToList(),
                d.CreatedAt))
            .ToListAsync(ct);

        return new DemandSearchResult(items, totalCount, page, pageSize);
    }

    public async Task<DemandDetailDto?> GetDetailAsync(Guid id, CallerContext? caller, CancellationToken ct = default)
    {
        var demand = await db.Demands.Where(d => d.Id == id && !d.IsDeleted).FirstOrDefaultAsync(ct);
        if (demand is null) return null;

        var isOwner = caller is not null && (caller.IsSystemAdmin || caller.MemberEntityIds.Contains(demand.MemberEntityId));
        return await ProjectDetailAsync(id, isOwner, ct);
    }

    public async Task<(DemandOp Op, Guid? Id)> CreateAsync(CallerContext caller, CreateDemandRequest request, CancellationToken ct = default)
    {
        if (caller.MemberEntityIds.Count == 0)
        {
            return (new DemandOp(DemandOpResult.Forbidden, "You must belong to a member organization to register a requirement."), null);
        }

        if (ValidateExpiresAt(request.ExpiresAt) is { } expiryError)
        {
            return (new DemandOp(DemandOpResult.InvalidState, expiryError), null);
        }

        var memberEntityId = caller.MemberEntityIds[0];
        var referenceCode = await referenceCodeGenerator.NextDemandCodeAsync(ct);

        var demand = new Demand
        {
            Id = Guid.NewGuid(),
            ReferenceCode = referenceCode,
            MemberEntityId = memberEntityId,
            CreatedByProfileId = caller.ProfileId,
            Title = request.Title,
            Description = request.Description,
            InternalNotes = request.InternalNotes,
            PurposeId = request.PurposeId,
            CurrencyId = request.CurrencyId,
            MinBudget = request.MinBudget,
            MaxBudget = request.MaxBudget,
            AreaUnitId = request.AreaUnitId,
            MinArea = request.MinArea,
            MaxArea = request.MaxArea,
            MinBedrooms = request.MinBedrooms,
            MinBathrooms = request.MinBathrooms,
            Status = DemandStatus.Draft,
            NetworkVisibility = NetworkVisibility.OwnerOnly,
            ExpiresAt = request.ExpiresAt,
        };
        db.Demands.Add(demand);

        foreach (var propertyTypeId in (request.PropertyTypeIds ?? []).Distinct())
        {
            db.DemandPropertyTypes.Add(new DemandPropertyType { Id = Guid.NewGuid(), DemandId = demand.Id, PropertyTypeId = propertyTypeId });
        }

        foreach (var loc in request.Locations ?? [])
        {
            db.DemandLocations.Add(new DemandLocation
            {
                Id = Guid.NewGuid(), DemandId = demand.Id,
                ProvinceId = loc.ProvinceId, DistrictId = loc.DistrictId, MunicipalityId = loc.MunicipalityId,
                WardId = loc.WardId, LocalityId = loc.LocalityId,
            });
        }

        foreach (var amenityId in (request.AmenityIds ?? []).Distinct())
        {
            db.DemandAmenities.Add(new DemandAmenity { Id = Guid.NewGuid(), DemandId = demand.Id, AmenityId = amenityId });
        }

        if (request.Contact is { } contact && (contact.ClientName is not null || contact.Phone is not null || contact.Email is not null))
        {
            db.DemandContacts.Add(new DemandContact
            {
                DemandId = demand.Id,
                ClientName = contact.ClientName,
                Phone = contact.Phone,
                Email = contact.Email,
                ConfidentialNotes = contact.ConfidentialNotes,
            });
        }

        await db.SaveChangesAsync(ct);
        await auditLogService.LogAsync(caller.ProfileId, "DemandCreated", "Demand", demand.Id, $"\"{demand.Title}\" ({referenceCode}) was created.", ct);
        return (new DemandOp(DemandOpResult.Success), demand.Id);
    }

    public async Task<DemandOp> UpdateAsync(Guid id, CallerContext caller, UpdateDemandRequest request, CancellationToken ct = default)
    {
        var demand = await db.Demands.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);
        if (demand is null) return new DemandOp(DemandOpResult.NotFound);

        if (ValidateExpiresAt(request.ExpiresAt) is { } expiryError)
        {
            return new DemandOp(DemandOpResult.InvalidState, expiryError);
        }

        demand.Title = request.Title;
        demand.Description = request.Description;
        demand.InternalNotes = request.InternalNotes;
        demand.PurposeId = request.PurposeId;
        demand.CurrencyId = request.CurrencyId;
        demand.MinBudget = request.MinBudget;
        demand.MaxBudget = request.MaxBudget;
        demand.AreaUnitId = request.AreaUnitId;
        demand.MinArea = request.MinArea;
        demand.MaxArea = request.MaxArea;
        demand.MinBedrooms = request.MinBedrooms;
        demand.MinBathrooms = request.MinBathrooms;
        demand.ExpiresAt = request.ExpiresAt;
        demand.UpdatedByProfileId = caller.ProfileId;
        demand.UpdatedAt = DateTime.UtcNow;

        return await SaveGuardedAndRecomputeAsync(id, ct);
    }

    public async Task<DemandOp> PublishAsync(Guid id, CallerContext caller, CancellationToken ct = default)
    {
        var demand = await db.Demands.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);
        if (demand is null) return new DemandOp(DemandOpResult.NotFound);
        if (demand.Status is not (DemandStatus.Draft or DemandStatus.Archived))
        {
            return new DemandOp(DemandOpResult.InvalidState, "Only a draft or archived requirement can be published.");
        }

        demand.Status = DemandStatus.Active;
        demand.UpdatedByProfileId = caller.ProfileId;
        demand.UpdatedAt = DateTime.UtcNow;
        var publishOp = await SaveGuardedAndRecomputeAsync(id, ct);
        if (publishOp.Result == DemandOpResult.Success)
        {
            await auditLogService.LogAsync(caller.ProfileId, "DemandPublished", "Demand", id, $"\"{demand.Title}\" was published.", ct);
        }
        return publishOp;
    }

    public async Task<DemandOp> FulfillAsync(Guid id, CallerContext caller, CancellationToken ct = default)
    {
        var demand = await db.Demands.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);
        if (demand is null) return new DemandOp(DemandOpResult.NotFound);
        if (demand.Status != DemandStatus.Active)
        {
            return new DemandOp(DemandOpResult.InvalidState, "Only an active requirement can be marked fulfilled.");
        }

        demand.Status = DemandStatus.Fulfilled;
        demand.UpdatedByProfileId = caller.ProfileId;
        demand.UpdatedAt = DateTime.UtcNow;
        var op = await SaveGuardedAsync(ct);
        if (op.Result == DemandOpResult.Success)
        {
            await auditLogService.LogAsync(caller.ProfileId, "DemandFulfilled", "Demand", id, $"\"{demand.Title}\" was marked fulfilled.", ct);
        }
        return op;
    }

    public async Task<DemandOp> ArchiveAsync(Guid id, CallerContext caller, CancellationToken ct = default)
    {
        var demand = await db.Demands.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);
        if (demand is null) return new DemandOp(DemandOpResult.NotFound);

        demand.Status = DemandStatus.Archived;
        demand.UpdatedByProfileId = caller.ProfileId;
        demand.UpdatedAt = DateTime.UtcNow;
        var op = await SaveGuardedAsync(ct);
        if (op.Result == DemandOpResult.Success)
        {
            await auditLogService.LogAsync(caller.ProfileId, "DemandArchived", "Demand", id, $"\"{demand.Title}\" was archived.", ct);
        }
        return op;
    }

    public async Task<DemandOp> SoftDeleteAsync(Guid id, CallerContext caller, CancellationToken ct = default)
    {
        var demand = await db.Demands.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);
        if (demand is null) return new DemandOp(DemandOpResult.NotFound);

        demand.IsDeleted = true;
        demand.DeletedAt = DateTime.UtcNow;
        demand.UpdatedByProfileId = caller.ProfileId;
        demand.UpdatedAt = DateTime.UtcNow;
        var op = await SaveGuardedAsync(ct);
        if (op.Result == DemandOpResult.Success)
        {
            await auditLogService.LogAsync(caller.ProfileId, "DemandDeleted", "Demand", id, $"\"{demand.Title}\" was deleted.", ct);
        }
        return op;
    }

    public async Task<DemandOp> ReplacePropertyTypesAsync(Guid id, CallerContext caller, IReadOnlyList<Guid> propertyTypeIds, CancellationToken ct = default)
    {
        var demand = await db.Demands.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);
        if (demand is null) return new DemandOp(DemandOpResult.NotFound);

        db.DemandPropertyTypes.RemoveRange(await db.DemandPropertyTypes.Where(pt => pt.DemandId == id).ToListAsync(ct));
        foreach (var propertyTypeId in propertyTypeIds.Distinct())
        {
            db.DemandPropertyTypes.Add(new DemandPropertyType { Id = Guid.NewGuid(), DemandId = id, PropertyTypeId = propertyTypeId });
        }
        demand.UpdatedByProfileId = caller.ProfileId;
        demand.UpdatedAt = DateTime.UtcNow;
        return await SaveGuardedAndRecomputeAsync(id, ct);
    }

    public async Task<DemandOp> ReplaceLocationsAsync(Guid id, CallerContext caller, IReadOnlyList<DemandLocationInput> locations, CancellationToken ct = default)
    {
        var demand = await db.Demands.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);
        if (demand is null) return new DemandOp(DemandOpResult.NotFound);

        db.DemandLocations.RemoveRange(await db.DemandLocations.Where(l => l.DemandId == id).ToListAsync(ct));
        foreach (var loc in locations)
        {
            db.DemandLocations.Add(new DemandLocation
            {
                Id = Guid.NewGuid(), DemandId = id,
                ProvinceId = loc.ProvinceId, DistrictId = loc.DistrictId, MunicipalityId = loc.MunicipalityId,
                WardId = loc.WardId, LocalityId = loc.LocalityId,
            });
        }
        demand.UpdatedByProfileId = caller.ProfileId;
        demand.UpdatedAt = DateTime.UtcNow;
        return await SaveGuardedAndRecomputeAsync(id, ct);
    }

    public async Task<DemandOp> ReplaceAmenitiesAsync(Guid id, CallerContext caller, IReadOnlyList<Guid> amenityIds, CancellationToken ct = default)
    {
        var demand = await db.Demands.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);
        if (demand is null) return new DemandOp(DemandOpResult.NotFound);

        db.DemandAmenities.RemoveRange(await db.DemandAmenities.Where(a => a.DemandId == id).ToListAsync(ct));
        foreach (var amenityId in amenityIds.Distinct())
        {
            db.DemandAmenities.Add(new DemandAmenity { Id = Guid.NewGuid(), DemandId = id, AmenityId = amenityId });
        }
        demand.UpdatedByProfileId = caller.ProfileId;
        demand.UpdatedAt = DateTime.UtcNow;
        return await SaveGuardedAndRecomputeAsync(id, ct);
    }

    public async Task<DemandOp> UpdateVisibilityAsync(Guid id, CallerContext caller, UpdateDemandVisibilityRequest request, CancellationToken ct = default)
    {
        var demand = await db.Demands.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);
        if (demand is null) return new DemandOp(DemandOpResult.NotFound);

        demand.NetworkVisibility = request.NetworkVisibility;
        demand.UpdatedByProfileId = caller.ProfileId;
        demand.UpdatedAt = DateTime.UtcNow;

        db.DemandVisibilityMembers.RemoveRange(await db.DemandVisibilityMembers.Where(m => m.DemandId == id).ToListAsync(ct));
        if (request.NetworkVisibility == NetworkVisibility.SelectedMembers && request.SelectedMemberEntityIds is { Count: > 0 })
        {
            foreach (var memberEntityId in request.SelectedMemberEntityIds.Distinct())
            {
                db.DemandVisibilityMembers.Add(new DemandVisibilityMember { Id = Guid.NewGuid(), DemandId = id, MemberEntityId = memberEntityId });
            }
        }

        return await SaveGuardedAsync(ct);
    }

    public async Task<(DemandOp Op, DemandContactInput? Contact)> GetContactAsync(Guid id, CallerContext caller, CancellationToken ct = default)
    {
        var contact = await db.DemandContacts.FirstOrDefaultAsync(c => c.DemandId == id, ct);
        if (contact is null)
        {
            var exists = await db.Demands.AnyAsync(d => d.Id == id && !d.IsDeleted, ct);
            return (new DemandOp(exists ? DemandOpResult.Success : DemandOpResult.NotFound), null);
        }

        return (new DemandOp(DemandOpResult.Success), new DemandContactInput(contact.ClientName, contact.Phone, contact.Email, contact.ConfidentialNotes));
    }

    public async Task<DemandOp> UpdateContactAsync(Guid id, CallerContext caller, DemandContactInput contact, CancellationToken ct = default)
    {
        if (!await db.Demands.AnyAsync(d => d.Id == id && !d.IsDeleted, ct))
        {
            return new DemandOp(DemandOpResult.NotFound);
        }

        var existing = await db.DemandContacts.FirstOrDefaultAsync(c => c.DemandId == id, ct);
        if (existing is null)
        {
            db.DemandContacts.Add(new DemandContact
            {
                DemandId = id,
                ClientName = contact.ClientName,
                Phone = contact.Phone,
                Email = contact.Email,
                ConfidentialNotes = contact.ConfidentialNotes,
            });
        }
        else
        {
            existing.ClientName = contact.ClientName;
            existing.Phone = contact.Phone;
            existing.Email = contact.Email;
            existing.ConfidentialNotes = contact.ConfidentialNotes;
        }

        return await SaveGuardedAsync(ct);
    }

    private async Task<DemandDetailDto> ProjectDetailAsync(Guid id, bool isOwner, CancellationToken ct)
    {
        var dto = await db.Demands
            .Where(d => d.Id == id)
            .Select(d => new DemandDetailDto(
                d.Id, d.ReferenceCode, d.MemberEntityId, d.MemberEntity.Name,
                d.Title, d.Description, isOwner ? d.InternalNotes : null,
                d.PurposeId, d.Purpose.Name,
                d.CurrencyId, d.Currency != null ? d.Currency.Code : null,
                d.MinBudget, d.MaxBudget,
                d.AreaUnitId, d.AreaUnit != null ? d.AreaUnit.Name : null, d.MinArea, d.MaxArea,
                d.MinBedrooms, d.MinBathrooms,
                d.NetworkVisibility.ToString(), d.Status.ToString(),
                d.ExpiresAt, d.CreatedAt, d.UpdatedAt, isOwner,
                d.PropertyTypes.Select(pt => pt.PropertyTypeId).ToList(),
                d.PropertyTypes.Select(pt => pt.PropertyType.Name).ToList(),
                d.Locations.Select(l => new DemandLocationDto(
                    l.Id, l.ProvinceId, l.Province.Name,
                    l.District != null ? l.District.Name : null,
                    l.Municipality != null ? l.Municipality.Name : null,
                    l.Ward != null ? l.Ward.Number : (int?)null,
                    l.Locality != null ? l.Locality.Name : null)).ToList(),
                d.DemandAmenities.Select(a => a.AmenityId).ToList(),
                d.DemandAmenities.Select(a => a.Amenity.Name).ToList()))
            .FirstAsync(ct);

        return dto;
    }

    private async Task<DemandOp> SaveGuardedAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
            return new DemandOp(DemandOpResult.Success);
        }
        catch (DbUpdateException ex) when (IsRlsBlockPredicateViolation(ex))
        {
            foreach (var entry in db.ChangeTracker.Entries())
            {
                entry.State = EntityState.Unchanged;
            }
            return new DemandOp(DemandOpResult.Forbidden, "You don't have permission to modify this requirement.");
        }
    }

    private async Task<DemandOp> SaveGuardedAndRecomputeAsync(Guid demandId, CancellationToken ct)
    {
        var op = await SaveGuardedAsync(ct);
        if (op.Result == DemandOpResult.Success)
        {
            await matchingEngine.RecomputeForDemandAsync(demandId, ct);
        }
        return op;
    }

    private static bool IsRlsBlockPredicateViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException sqlEx && sqlEx.Message.Contains("block predicate", StringComparison.OrdinalIgnoreCase);

    /// <summary>A past expiry date would create a requirement that's already stale the moment
    /// it's saved. Needs "today" (DateTime.UtcNow), not a compile-time constant, so this can't be
    /// expressed as a DataAnnotations attribute on CreateDemandRequest/UpdateDemandRequest.</summary>
    private static string? ValidateExpiresAt(DateTime? expiresAt) =>
        expiresAt is { } exp && exp.Date < DateTime.UtcNow.Date
            ? "Expiry date can't be in the past."
            : null;
}
