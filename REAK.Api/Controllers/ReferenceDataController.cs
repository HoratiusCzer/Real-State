using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Entities.Reference;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

/// <summary>Admin-configurable taxonomy (spec §10, §11 note on area units already seeded in Stage
/// 3, property types/subtypes/amenities/locations deliberately left empty per §22 — inventing an
/// exhaustive list would be fabricated business data). Reads are anonymous (the public property
/// search page needs these for its filter dropdowns too); writes require settings.manage.
///
/// Only Create is implemented here — full manage (edit/disable/reorder) stays deferred past Stage
/// 11 too: this is eleven flat/hierarchical tables (property types, subtypes, purposes, amenities,
/// the five-level Nepal location hierarchy, area units, currencies) and none of them are things
/// a REAK admin would realistically edit day-to-day once seeded — Nepal's administrative
/// divisions in particular are fixed geography, not editorial content. Stage 11's Admin Portal
/// gives this an explicit "manage via API for now" placeholder rather than eleven near-identical
/// CRUD screens; Roles below is the one addition, since it's needed both for oversight and for the
/// invitation-creation role picker.</summary>
[ApiController]
[Route("api/reference")]
public class ReferenceDataController(ReakDbContext db) : ControllerBase
{
    /// <summary>Read-only (spec §4.3 lists "roles, permissions" as an Admin Portal screen, but
    /// actually editing the RBAC matrix is security-sensitive enough to defer deliberately — this
    /// gives admins visibility into what's granted, and gives the invitation UI a role picker,
    /// without a write path that could let someone grant themselves more access.</summary>
    [HttpGet("roles")]
    [Authorize]
    [RequirePermission("members.read")]
    public async Task<IActionResult> ListRoles(CancellationToken ct)
    {
        var roles = await db.Roles
            .OrderBy(r => r.Scope).ThenBy(r => r.Name)
            .Select(r => new
            {
                r.Id, r.Name, Scope = r.Scope.ToString(),
                Permissions = r.RolePermissions.Select(rp => rp.Permission.Slug).ToList(),
            })
            .ToListAsync(ct);
        foreach (var role in roles) role.Permissions.Sort();
        return Ok(roles);
    }

    [HttpGet("property-types")]
    [AllowAnonymous]
    public async Task<IActionResult> ListPropertyTypes(CancellationToken ct)
    {
        var types = await db.PropertyTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Name)
            .Select(t => new { t.Id, t.Name, Subtypes = t.Subtypes.Where(s => s.IsActive).Select(s => new { s.Id, s.Name }) })
            .ToListAsync(ct);
        return Ok(types);
    }

    [HttpPost("property-types")]
    [Authorize]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> CreatePropertyType(CreatePropertyTypeRequest request, CancellationToken ct)
    {
        if (await db.PropertyTypes.AnyAsync(t => t.Name == request.Name, ct))
        {
            return BadRequest(new { error = "A property type with this name already exists." });
        }

        var entity = new PropertyType { Id = Guid.NewGuid(), Name = request.Name };
        db.PropertyTypes.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new { entity.Id, entity.Name });
    }

    [HttpPost("property-subtypes")]
    [Authorize]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> CreatePropertySubtype(CreatePropertySubtypeRequest request, CancellationToken ct)
    {
        if (!await db.PropertyTypes.AnyAsync(t => t.Id == request.PropertyTypeId, ct))
        {
            return BadRequest(new { error = "Property type not found." });
        }

        if (await db.PropertySubtypes.AnyAsync(s => s.PropertyTypeId == request.PropertyTypeId && s.Name == request.Name, ct))
        {
            return BadRequest(new { error = "A subtype with this name already exists for this property type." });
        }

        var entity = new PropertySubtype { Id = Guid.NewGuid(), PropertyTypeId = request.PropertyTypeId, Name = request.Name };
        db.PropertySubtypes.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new { entity.Id, entity.Name });
    }

    [HttpGet("purposes")]
    [AllowAnonymous]
    public async Task<IActionResult> ListPurposes(CancellationToken ct)
    {
        var purposes = await db.Purposes
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
            .Select(p => new { p.Id, p.Name })
            .ToListAsync(ct);
        return Ok(purposes);
    }

    [HttpPost("purposes")]
    [Authorize]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> CreatePurpose(CreatePurposeRequest request, CancellationToken ct)
    {
        if (await db.Purposes.AnyAsync(p => p.Name == request.Name, ct))
        {
            return BadRequest(new { error = "A purpose with this name already exists." });
        }

        var entity = new Purpose { Id = Guid.NewGuid(), Name = request.Name };
        db.Purposes.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new { entity.Id, entity.Name });
    }

    [HttpGet("amenities")]
    [AllowAnonymous]
    public async Task<IActionResult> ListAmenities(CancellationToken ct)
    {
        var amenities = await db.Amenities
            .Where(a => a.IsActive)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name)
            .Select(a => new { a.Id, a.Name })
            .ToListAsync(ct);
        return Ok(amenities);
    }

    [HttpPost("amenities")]
    [Authorize]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> CreateAmenity(CreateAmenityRequest request, CancellationToken ct)
    {
        if (await db.Amenities.AnyAsync(a => a.Name == request.Name, ct))
        {
            return BadRequest(new { error = "An amenity with this name already exists." });
        }

        var entity = new Amenity { Id = Guid.NewGuid(), Name = request.Name };
        db.Amenities.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new { entity.Id, entity.Name });
    }

    [HttpGet("area-units")]
    [AllowAnonymous]
    public async Task<IActionResult> ListAreaUnits(CancellationToken ct)
    {
        var units = await db.AreaUnits
            .Where(u => u.IsActive)
            .OrderBy(u => u.SortOrder)
            .Select(u => new { u.Id, u.Name, u.Abbreviation })
            .ToListAsync(ct);
        return Ok(units);
    }

    [HttpGet("currencies")]
    [AllowAnonymous]
    public async Task<IActionResult> ListCurrencies(CancellationToken ct)
    {
        var currencies = await db.Currencies
            .Where(c => c.IsActive)
            .Select(c => new { c.Id, c.Code, c.Symbol })
            .ToListAsync(ct);
        return Ok(currencies);
    }

    [HttpGet("provinces")]
    [AllowAnonymous]
    public async Task<IActionResult> ListProvinces(CancellationToken ct)
    {
        var provinces = await db.Provinces
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
            .Select(p => new { p.Id, p.Name })
            .ToListAsync(ct);
        return Ok(provinces);
    }

    [HttpGet("districts")]
    [AllowAnonymous]
    public async Task<IActionResult> ListDistricts([FromQuery] Guid provinceId, CancellationToken ct)
    {
        var districts = await db.Districts
            .Where(d => d.IsActive && d.ProvinceId == provinceId)
            .OrderBy(d => d.SortOrder).ThenBy(d => d.Name)
            .Select(d => new { d.Id, d.Name })
            .ToListAsync(ct);
        return Ok(districts);
    }

    [HttpPost("districts")]
    [Authorize]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> CreateDistrict(CreateDistrictRequest request, CancellationToken ct)
    {
        if (!await db.Provinces.AnyAsync(p => p.Id == request.ProvinceId, ct))
        {
            return BadRequest(new { error = "Province not found." });
        }

        if (await db.Districts.AnyAsync(d => d.ProvinceId == request.ProvinceId && d.Name == request.Name, ct))
        {
            return BadRequest(new { error = "A district with this name already exists in this province." });
        }

        var entity = new District { Id = Guid.NewGuid(), ProvinceId = request.ProvinceId, Name = request.Name };
        db.Districts.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new { entity.Id, entity.Name });
    }

    [HttpGet("municipalities")]
    [AllowAnonymous]
    public async Task<IActionResult> ListMunicipalities([FromQuery] Guid districtId, CancellationToken ct)
    {
        var municipalities = await db.Municipalities
            .Where(m => m.IsActive && m.DistrictId == districtId)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Name)
            .Select(m => new { m.Id, m.Name })
            .ToListAsync(ct);
        return Ok(municipalities);
    }

    [HttpPost("municipalities")]
    [Authorize]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> CreateMunicipality(CreateMunicipalityRequest request, CancellationToken ct)
    {
        if (!await db.Districts.AnyAsync(d => d.Id == request.DistrictId, ct))
        {
            return BadRequest(new { error = "District not found." });
        }

        if (await db.Municipalities.AnyAsync(m => m.DistrictId == request.DistrictId && m.Name == request.Name, ct))
        {
            return BadRequest(new { error = "A municipality with this name already exists in this district." });
        }

        var entity = new Municipality { Id = Guid.NewGuid(), DistrictId = request.DistrictId, Name = request.Name };
        db.Municipalities.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new { entity.Id, entity.Name });
    }

    [HttpGet("wards")]
    [AllowAnonymous]
    public async Task<IActionResult> ListWards([FromQuery] Guid municipalityId, CancellationToken ct)
    {
        var wards = await db.Wards
            .Where(w => w.IsActive && w.MunicipalityId == municipalityId)
            .OrderBy(w => w.Number)
            .Select(w => new { w.Id, w.Number })
            .ToListAsync(ct);
        return Ok(wards);
    }

    [HttpPost("wards")]
    [Authorize]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> CreateWard(CreateWardRequest request, CancellationToken ct)
    {
        if (!await db.Municipalities.AnyAsync(m => m.Id == request.MunicipalityId, ct))
        {
            return BadRequest(new { error = "Municipality not found." });
        }

        if (await db.Wards.AnyAsync(w => w.MunicipalityId == request.MunicipalityId && w.Number == request.Number, ct))
        {
            return BadRequest(new { error = "This ward number already exists in this municipality." });
        }

        var entity = new Ward { Id = Guid.NewGuid(), MunicipalityId = request.MunicipalityId, Number = request.Number };
        db.Wards.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new { entity.Id, entity.Number });
    }

    [HttpGet("localities")]
    [AllowAnonymous]
    public async Task<IActionResult> ListLocalities([FromQuery] Guid wardId, CancellationToken ct)
    {
        var localities = await db.Localities
            .Where(l => l.IsActive && l.WardId == wardId)
            .OrderBy(l => l.SortOrder).ThenBy(l => l.Name)
            .Select(l => new { l.Id, l.Name })
            .ToListAsync(ct);
        return Ok(localities);
    }

    [HttpPost("localities")]
    [Authorize]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> CreateLocality(CreateLocalityRequest request, CancellationToken ct)
    {
        if (!await db.Wards.AnyAsync(w => w.Id == request.WardId, ct))
        {
            return BadRequest(new { error = "Ward not found." });
        }

        if (await db.Localities.AnyAsync(l => l.WardId == request.WardId && l.Name == request.Name, ct))
        {
            return BadRequest(new { error = "A locality with this name already exists in this ward." });
        }

        var entity = new Locality { Id = Guid.NewGuid(), WardId = request.WardId, Name = request.Name };
        db.Localities.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new { entity.Id, entity.Name });
    }
}
