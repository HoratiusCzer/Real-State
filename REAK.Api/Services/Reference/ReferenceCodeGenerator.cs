using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;

namespace REAK.Api.Services.Reference;

public class ReferenceCodeGenerator(ReakDbContext db) : IReferenceCodeGenerator
{
    public async Task<string> NextListingCodeAsync(CancellationToken ct = default)
    {
        var results = await db.Database
            .SqlQueryRaw<int>("SELECT NEXT VALUE FOR dbo.ListingReferenceCodeSeq AS [Value]")
            .ToListAsync(ct);
        return $"RK-L-{DateTime.UtcNow.Year}-{results[0]:D6}";
    }

    public async Task<string> NextDemandCodeAsync(CancellationToken ct = default)
    {
        var results = await db.Database
            .SqlQueryRaw<int>("SELECT NEXT VALUE FOR dbo.DemandReferenceCodeSeq AS [Value]")
            .ToListAsync(ct);
        return $"RK-D-{DateTime.UtcNow.Year}-{results[0]:D6}";
    }
}
