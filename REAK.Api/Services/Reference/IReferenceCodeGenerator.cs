namespace REAK.Api.Services.Reference;

/// <summary>Server-side reference code generation (spec §21: "never client-side"). Backed by SQL
/// Server SEQUENCE objects — atomic, gap-tolerant-but-never-duplicate, and safe under concurrent
/// inserts without needing an application-level lock. Closes the gap Stage 3 deliberately left
/// open ("no service exists yet that creates a listing/demand").</summary>
public interface IReferenceCodeGenerator
{
    Task<string> NextListingCodeAsync(CancellationToken ct = default);
    Task<string> NextDemandCodeAsync(CancellationToken ct = default);
}
