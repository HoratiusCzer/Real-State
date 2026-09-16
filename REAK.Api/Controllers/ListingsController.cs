using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Entities.Listings;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Listings;
using REAK.Api.Services.Security;
using REAK.Api.Services.Storage;

namespace REAK.Api.Controllers;

/// <summary>Authenticated Property Exchange surface (spec §8). Search/detail rely on RLS
/// (Data/Security/RowLevelSecurity.sql) to scope visibility; lifecycle mutations rely on it to
/// enforce ownership (ListingService.SaveGuardedAsync translates an RLS block-predicate violation
/// into a 403). Media/documents are the documented exception — ListingMedia/ListingDocument have
/// no RLS yet (Stage 3/13 gap) — so upload/delete/download here does an explicit ownership check
/// instead.</summary>
[ApiController]
[Route("api/listings")]
[Authorize]
public class ListingsController(ReakDbContext db, IListingService listingService, IFileStorage fileStorage) : ControllerBase
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedDocumentExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private const long MaxUploadBytes = 10 * 1024 * 1024;

    [HttpGet]
    [RequirePermission("listings.read")]
    public async Task<IActionResult> Search([FromQuery] ListingSearchQuery query, CancellationToken ct)
    {
        var result = await listingService.SearchAsync(query, CallerContextFactory.From(User), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("listings.read")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var detail = await listingService.GetDetailAsync(id, CallerContextFactory.From(User), ct);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost]
    [RequirePermission("listings.create")]
    public async Task<IActionResult> Create(CreateListingRequest request, CancellationToken ct)
    {
        var (op, id) = await listingService.CreateAsync(CallerContextFactory.From(User), request, ct);
        return op.Result == ListingOpResult.Success
            ? CreatedAtAction(nameof(Get), new { id }, new { id })
            : ToActionResult(op);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("listings.update")]
    public async Task<IActionResult> Update(Guid id, UpdateListingRequest request, CancellationToken ct)
    {
        var op = await listingService.UpdateAsync(id, CallerContextFactory.From(User), request, ct);
        return op.Result == ListingOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPost("{id:guid}/submit")]
    [RequirePermission("listings.update")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var op = await listingService.SubmitAsync(id, CallerContextFactory.From(User), ct);
        return op.Result == ListingOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPost("{id:guid}/approve")]
    [RequirePermission("listings.moderate")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var op = await listingService.ApproveAsync(id, CallerContextFactory.From(User), ct);
        return op.Result == ListingOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPost("{id:guid}/reject")]
    [RequirePermission("listings.moderate")]
    public async Task<IActionResult> Reject(Guid id, RejectListingRequest request, CancellationToken ct)
    {
        var op = await listingService.RejectAsync(id, CallerContextFactory.From(User), request.Reason, ct);
        return op.Result == ListingOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPost("{id:guid}/archive")]
    [RequirePermission("listings.update")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var op = await listingService.ArchiveAsync(id, CallerContextFactory.From(User), ct);
        return op.Result == ListingOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("listings.update")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var op = await listingService.SoftDeleteAsync(id, CallerContextFactory.From(User), ct);
        return op.Result == ListingOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPut("{id:guid}/amenities")]
    [RequirePermission("listings.update")]
    public async Task<IActionResult> ReplaceAmenities(Guid id, UpdateListingAmenitiesRequest request, CancellationToken ct)
    {
        var op = await listingService.ReplaceAmenitiesAsync(id, CallerContextFactory.From(User), request.AmenityIds, ct);
        return op.Result == ListingOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPut("{id:guid}/visibility")]
    [RequirePermission("listings.update")]
    public async Task<IActionResult> UpdateVisibility(Guid id, UpdateListingVisibilityRequest request, CancellationToken ct)
    {
        var op = await listingService.UpdateVisibilityAsync(id, CallerContextFactory.From(User), request, ct);
        return op.Result == ListingOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpGet("{id:guid}/contact")]
    [RequirePermission("listings.read")]
    public async Task<IActionResult> GetContact(Guid id, CancellationToken ct)
    {
        var (op, contact) = await listingService.GetContactAsync(id, CallerContextFactory.From(User), ct);
        if (op.Result != ListingOpResult.Success) return ToActionResult(op);
        return contact is null ? Ok(new ListingContactInput(null, null, null)) : Ok(contact);
    }

    [HttpPut("{id:guid}/contact")]
    [RequirePermission("listings.update")]
    public async Task<IActionResult> UpdateContact(Guid id, ListingContactInput request, CancellationToken ct)
    {
        var op = await listingService.UpdateContactAsync(id, CallerContextFactory.From(User), request, ct);
        return op.Result == ListingOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpGet("saved")]
    [RequirePermission("listings.read")]
    public async Task<IActionResult> ListSaved(CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var saved = await db.SavedListings
            .Where(s => s.ProfileId == caller.ProfileId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.ListingId,
                s.CreatedAt,
                Listing = new
                {
                    s.Listing.Id,
                    s.Listing.ReferenceCode,
                    s.Listing.Title,
                    s.Listing.Price,
                    CurrencyCode = s.Listing.Currency.Code,
                    Status = s.Listing.Status.ToString(),
                },
            })
            .ToListAsync(ct);

        return Ok(saved);
    }

    [HttpPost("{id:guid}/save")]
    [RequirePermission("listings.read")]
    public async Task<IActionResult> Save(Guid id, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        if (!await db.PropertyListings.AnyAsync(l => l.Id == id && !l.IsDeleted, ct))
        {
            return NotFound();
        }

        if (!await db.SavedListings.AnyAsync(s => s.ProfileId == caller.ProfileId && s.ListingId == id, ct))
        {
            db.SavedListings.Add(new SavedListing { Id = Guid.NewGuid(), ProfileId = caller.ProfileId, ListingId = id });
            await db.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}/save")]
    [RequirePermission("listings.read")]
    public async Task<IActionResult> Unsave(Guid id, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var saved = await db.SavedListings.FirstOrDefaultAsync(s => s.ProfileId == caller.ProfileId && s.ListingId == id, ct);
        if (saved is not null)
        {
            db.SavedListings.Remove(saved);
            await db.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/media")]
    [RequirePermission("listings.update")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> UploadMedia(Guid id, IFormFile file, [FromQuery] bool isPrimary, CancellationToken ct)
    {
        var listing = await GetOwnedListingOrNullAsync(id, ct);
        if (listing is null) return Forbid();

        var extensionError = ValidateExtension(file, AllowedImageExtensions);
        if (extensionError is not null) return BadRequest(new { error = extensionError });

        var stored = await SaveUploadAsync("listing-media", file, ct);

        if (isPrimary)
        {
            var currentPrimary = await db.ListingMedia.Where(m => m.ListingId == id && m.IsPrimary).ToListAsync(ct);
            foreach (var m in currentPrimary) m.IsPrimary = false;
        }

        var sortOrder = await db.ListingMedia.Where(m => m.ListingId == id).CountAsync(ct);
        var media = new ListingMedia
        {
            Id = Guid.NewGuid(),
            ListingId = id,
            Url = fileStorage.GetPublicUrl(stored.StoragePath),
            IsPrimary = isPrimary,
            SortOrder = sortOrder,
        };
        db.ListingMedia.Add(media);
        await db.SaveChangesAsync(ct);

        return Ok(new ListingMediaDto(media.Id, media.Url, media.IsPrimary, media.SortOrder, media.Caption));
    }

    [HttpDelete("{id:guid}/media/{mediaId:guid}")]
    [RequirePermission("listings.update")]
    public async Task<IActionResult> DeleteMedia(Guid id, Guid mediaId, CancellationToken ct)
    {
        var listing = await GetOwnedListingOrNullAsync(id, ct);
        if (listing is null) return Forbid();

        var media = await db.ListingMedia.FirstOrDefaultAsync(m => m.Id == mediaId && m.ListingId == id, ct);
        if (media is null) return NotFound();

        db.ListingMedia.Remove(media);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/documents")]
    [RequirePermission("listings.update")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> UploadDocument(Guid id, IFormFile file, [FromForm] string? documentType, CancellationToken ct)
    {
        var listing = await GetOwnedListingOrNullAsync(id, ct);
        if (listing is null) return Forbid();

        var extensionError = ValidateExtension(file, AllowedDocumentExtensions);
        if (extensionError is not null) return BadRequest(new { error = extensionError });

        var stored = await SaveUploadAsync("listing-documents", file, ct);

        var caller = CallerContextFactory.From(User);
        var document = new ListingDocument
        {
            Id = Guid.NewGuid(),
            ListingId = id,
            DocumentType = documentType,
            StoragePath = stored.StoragePath,
            UploadedByProfileId = caller.ProfileId,
        };
        db.ListingDocuments.Add(document);
        await db.SaveChangesAsync(ct);

        return Ok(new ListingDocumentDto(document.Id, document.DocumentType, document.CreatedAt));
    }

    [HttpGet("{id:guid}/documents/{documentId:guid}/download")]
    [RequirePermission("listings.read")]
    public async Task<IActionResult> DownloadDocument(Guid id, Guid documentId, CancellationToken ct)
    {
        var listing = await GetOwnedListingOrNullAsync(id, ct);
        if (listing is null) return Forbid();

        var document = await db.ListingDocuments.FirstOrDefaultAsync(d => d.Id == documentId && d.ListingId == id, ct);
        if (document is null) return NotFound();

        var stream = await fileStorage.OpenReadAsync(document.StoragePath, ct);
        if (stream is null) return NotFound();

        return File(stream, "application/octet-stream", Path.GetFileName(document.StoragePath));
    }

    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    [RequirePermission("listings.update")]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId, CancellationToken ct)
    {
        var listing = await GetOwnedListingOrNullAsync(id, ct);
        if (listing is null) return Forbid();

        var document = await db.ListingDocuments.FirstOrDefaultAsync(d => d.Id == documentId && d.ListingId == id, ct);
        if (document is null) return NotFound();

        await fileStorage.DeleteAsync(document.StoragePath, ct);
        db.ListingDocuments.Remove(document);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Explicit ownership check for the media/document endpoints, since ListingMedia and
    /// ListingDocument have no RLS of their own to fall back on (unlike the main PropertyListings
    /// row, which ListingService's other mutations already trust RLS to guard).</summary>
    private async Task<PropertyListing?> GetOwnedListingOrNullAsync(Guid listingId, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var listing = await db.PropertyListings.FirstOrDefaultAsync(l => l.Id == listingId && !l.IsDeleted, ct);
        if (listing is null) return null;
        if (!caller.IsSystemAdmin && !caller.MemberEntityIds.Contains(listing.MemberEntityId)) return null;
        return listing;
    }

    private async Task<StoredFile> SaveUploadAsync(string container, IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return await fileStorage.SaveAsync(container, file.FileName, stream, ct);
    }

    private static string? ValidateExtension(IFormFile file, string[] allowed)
    {
        if (file.Length == 0) return "File is empty.";
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowed.Contains(ext) ? null : $"Unsupported file type. Allowed: {string.Join(", ", allowed)}.";
    }

    private IActionResult ToActionResult(ListingOp op) => op.Result switch
    {
        ListingOpResult.NotFound => NotFound(),
        ListingOpResult.Forbidden => Forbid(),
        ListingOpResult.InvalidState => BadRequest(new { error = op.Error }),
        _ => BadRequest(new { error = op.Error ?? "Request failed." }),
    };
}
