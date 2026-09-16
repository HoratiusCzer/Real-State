using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Services.Auth;

public interface IMembershipApplicationService
{
    Task<(bool Success, string? Error, MembershipApplication? Application)> SubmitAsync(
        string companyName, string contactName, string email, string phone, string? message, CancellationToken ct = default);

    Task<(bool Success, string? Error)> ApproveAsync(Guid applicationId, Guid reviewedByProfileId, CancellationToken ct = default);

    Task<(bool Success, string? Error)> RejectAsync(Guid applicationId, Guid reviewedByProfileId, string reason, CancellationToken ct = default);
}
