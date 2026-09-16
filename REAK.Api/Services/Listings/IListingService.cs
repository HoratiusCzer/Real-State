using REAK.Api.Models.Dto;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Listings;

public enum ListingOpResult { Success, NotFound, Forbidden, InvalidState }

public record ListingOp(ListingOpResult Result, string? Error = null);

public interface IListingService
{
    Task<ListingSearchResult> SearchAsync(ListingSearchQuery query, CallerContext? caller, CancellationToken ct = default);
    Task<ListingDetailDto?> GetDetailAsync(Guid id, CallerContext? caller, CancellationToken ct = default);
    Task<(ListingOp Op, Guid? Id)> CreateAsync(CallerContext caller, CreateListingRequest request, CancellationToken ct = default);
    Task<ListingOp> UpdateAsync(Guid id, CallerContext caller, UpdateListingRequest request, CancellationToken ct = default);
    Task<ListingOp> SubmitAsync(Guid id, CallerContext caller, CancellationToken ct = default);
    Task<ListingOp> ApproveAsync(Guid id, CallerContext caller, CancellationToken ct = default);
    Task<ListingOp> RejectAsync(Guid id, CallerContext caller, string reason, CancellationToken ct = default);
    Task<ListingOp> ArchiveAsync(Guid id, CallerContext caller, CancellationToken ct = default);
    Task<ListingOp> SoftDeleteAsync(Guid id, CallerContext caller, CancellationToken ct = default);
    Task<ListingOp> ReplaceAmenitiesAsync(Guid id, CallerContext caller, IReadOnlyList<Guid> amenityIds, CancellationToken ct = default);
    Task<ListingOp> UpdateVisibilityAsync(Guid id, CallerContext caller, UpdateListingVisibilityRequest request, CancellationToken ct = default);
    Task<(ListingOp Op, ListingContactInput? Contact)> GetContactAsync(Guid id, CallerContext caller, CancellationToken ct = default);
    Task<ListingOp> UpdateContactAsync(Guid id, CallerContext caller, ListingContactInput contact, CancellationToken ct = default);
}
