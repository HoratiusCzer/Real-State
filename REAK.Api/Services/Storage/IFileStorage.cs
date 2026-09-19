namespace REAK.Api.Services.Storage;

public record StoredFile(string StoragePath, long SizeBytes);

/// <summary>Storage areas must mirror application authorization and private paths must never be
/// publicly guessable/reachable (spec §18). Two containers: "listing-media" is served statically
/// (its own root, never overlapping the private one) — a listing's images are only ever handed to
/// a client whose API request already passed the listing's read authorization, so path secrecy is
/// not the security boundary, application authorization is. "listing-documents" is never
/// statically served; every read goes through ListingsController's authenticated download action.
///
/// LocalDiskFileStorage is a dev-only placeholder — swap for real object storage (S3/Azure Blob)
/// before production (spec §34).</summary>
public interface IFileStorage
{
    Task<StoredFile> SaveAsync(string container, string fileName, Stream content, CancellationToken ct = default);
    Task<Stream?> OpenReadAsync(string storagePath, CancellationToken ct = default);
    Task DeleteAsync(string storagePath, CancellationToken ct = default);

    /// <summary>Only valid for the public "listing-media" container.</summary>
    string GetPublicUrl(string storagePath);
}
