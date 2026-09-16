using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REAK.Api.Models.Dto;
using REAK.Api.Services.Demands;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

/// <summary>Authenticated demand/requirement surface (spec §9). No public projection — spec §9.1
/// scopes demand search to members only, unlike listings' public branch. Search/detail rely on
/// RLS (fn_DemandReadPredicate) to scope visibility; mutations rely on the write predicate,
/// translated to 403 by DemandService.SaveGuardedAsync the same way ListingService does.</summary>
[ApiController]
[Route("api/demands")]
[Authorize]
public class DemandsController(IDemandService demandService) : ControllerBase
{
    [HttpGet]
    [RequirePermission("demands.read")]
    public async Task<IActionResult> Search([FromQuery] DemandSearchQuery query, CancellationToken ct)
    {
        var result = await demandService.SearchAsync(query, CallerContextFactory.From(User), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("demands.read")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var detail = await demandService.GetDetailAsync(id, CallerContextFactory.From(User), ct);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost]
    [RequirePermission("demands.create")]
    public async Task<IActionResult> Create(CreateDemandRequest request, CancellationToken ct)
    {
        var (op, id) = await demandService.CreateAsync(CallerContextFactory.From(User), request, ct);
        return op.Result == DemandOpResult.Success
            ? CreatedAtAction(nameof(Get), new { id }, new { id })
            : ToActionResult(op);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("demands.update")]
    public async Task<IActionResult> Update(Guid id, UpdateDemandRequest request, CancellationToken ct)
    {
        var op = await demandService.UpdateAsync(id, CallerContextFactory.From(User), request, ct);
        return op.Result == DemandOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPost("{id:guid}/publish")]
    [RequirePermission("demands.update")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var op = await demandService.PublishAsync(id, CallerContextFactory.From(User), ct);
        return op.Result == DemandOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPost("{id:guid}/fulfill")]
    [RequirePermission("demands.update")]
    public async Task<IActionResult> Fulfill(Guid id, CancellationToken ct)
    {
        var op = await demandService.FulfillAsync(id, CallerContextFactory.From(User), ct);
        return op.Result == DemandOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPost("{id:guid}/archive")]
    [RequirePermission("demands.update")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var op = await demandService.ArchiveAsync(id, CallerContextFactory.From(User), ct);
        return op.Result == DemandOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("demands.update")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var op = await demandService.SoftDeleteAsync(id, CallerContextFactory.From(User), ct);
        return op.Result == DemandOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPut("{id:guid}/property-types")]
    [RequirePermission("demands.update")]
    public async Task<IActionResult> ReplacePropertyTypes(Guid id, UpdateDemandPropertyTypesRequest request, CancellationToken ct)
    {
        var op = await demandService.ReplacePropertyTypesAsync(id, CallerContextFactory.From(User), request.PropertyTypeIds, ct);
        return op.Result == DemandOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPut("{id:guid}/locations")]
    [RequirePermission("demands.update")]
    public async Task<IActionResult> ReplaceLocations(Guid id, UpdateDemandLocationsRequest request, CancellationToken ct)
    {
        var op = await demandService.ReplaceLocationsAsync(id, CallerContextFactory.From(User), request.Locations, ct);
        return op.Result == DemandOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPut("{id:guid}/amenities")]
    [RequirePermission("demands.update")]
    public async Task<IActionResult> ReplaceAmenities(Guid id, UpdateDemandAmenitiesRequest request, CancellationToken ct)
    {
        var op = await demandService.ReplaceAmenitiesAsync(id, CallerContextFactory.From(User), request.AmenityIds, ct);
        return op.Result == DemandOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPut("{id:guid}/visibility")]
    [RequirePermission("demands.update")]
    public async Task<IActionResult> UpdateVisibility(Guid id, UpdateDemandVisibilityRequest request, CancellationToken ct)
    {
        var op = await demandService.UpdateVisibilityAsync(id, CallerContextFactory.From(User), request, ct);
        return op.Result == DemandOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpGet("{id:guid}/contact")]
    [RequirePermission("demands.read")]
    public async Task<IActionResult> GetContact(Guid id, CancellationToken ct)
    {
        var (op, contact) = await demandService.GetContactAsync(id, CallerContextFactory.From(User), ct);
        if (op.Result != DemandOpResult.Success) return ToActionResult(op);
        return contact is null ? Ok(new DemandContactInput(null, null, null, null)) : Ok(contact);
    }

    [HttpPut("{id:guid}/contact")]
    [RequirePermission("demands.update")]
    public async Task<IActionResult> UpdateContact(Guid id, DemandContactInput request, CancellationToken ct)
    {
        var op = await demandService.UpdateContactAsync(id, CallerContextFactory.From(User), request, ct);
        return op.Result == DemandOpResult.Success ? NoContent() : ToActionResult(op);
    }

    private IActionResult ToActionResult(DemandOp op) => op.Result switch
    {
        DemandOpResult.NotFound => NotFound(),
        DemandOpResult.Forbidden => Forbid(),
        DemandOpResult.InvalidState => BadRequest(new { error = op.Error }),
        _ => BadRequest(new { error = op.Error ?? "Request failed." }),
    };
}
