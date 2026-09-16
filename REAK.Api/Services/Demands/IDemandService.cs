using REAK.Api.Models.Dto;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Demands;

public enum DemandOpResult { Success, NotFound, Forbidden, InvalidState }

public record DemandOp(DemandOpResult Result, string? Error = null);

public interface IDemandService
{
    Task<DemandSearchResult> SearchAsync(DemandSearchQuery query, CallerContext? caller, CancellationToken ct = default);
    Task<DemandDetailDto?> GetDetailAsync(Guid id, CallerContext? caller, CancellationToken ct = default);
    Task<(DemandOp Op, Guid? Id)> CreateAsync(CallerContext caller, CreateDemandRequest request, CancellationToken ct = default);
    Task<DemandOp> UpdateAsync(Guid id, CallerContext caller, UpdateDemandRequest request, CancellationToken ct = default);
    Task<DemandOp> PublishAsync(Guid id, CallerContext caller, CancellationToken ct = default);
    Task<DemandOp> FulfillAsync(Guid id, CallerContext caller, CancellationToken ct = default);
    Task<DemandOp> ArchiveAsync(Guid id, CallerContext caller, CancellationToken ct = default);
    Task<DemandOp> SoftDeleteAsync(Guid id, CallerContext caller, CancellationToken ct = default);
    Task<DemandOp> ReplacePropertyTypesAsync(Guid id, CallerContext caller, IReadOnlyList<Guid> propertyTypeIds, CancellationToken ct = default);
    Task<DemandOp> ReplaceLocationsAsync(Guid id, CallerContext caller, IReadOnlyList<DemandLocationInput> locations, CancellationToken ct = default);
    Task<DemandOp> ReplaceAmenitiesAsync(Guid id, CallerContext caller, IReadOnlyList<Guid> amenityIds, CancellationToken ct = default);
    Task<DemandOp> UpdateVisibilityAsync(Guid id, CallerContext caller, UpdateDemandVisibilityRequest request, CancellationToken ct = default);
    Task<(DemandOp Op, DemandContactInput? Contact)> GetContactAsync(Guid id, CallerContext caller, CancellationToken ct = default);
    Task<DemandOp> UpdateContactAsync(Guid id, CallerContext caller, DemandContactInput contact, CancellationToken ct = default);
}
