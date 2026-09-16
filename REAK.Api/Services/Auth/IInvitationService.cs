using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Services.Auth;

public interface IInvitationService
{
    Task<Invitation> CreateAsync(string email, Guid? memberEntityId, Guid roleId, Guid invitedByProfileId, CancellationToken ct = default);
    Task<InvitationLookupResult> LookupAsync(string token, CancellationToken ct = default);
    Task<(bool Success, string? Error)> AcceptAsync(string token, string? password, CancellationToken ct = default);
    Task<bool> RevokeAsync(Guid invitationId, CancellationToken ct = default);
}
