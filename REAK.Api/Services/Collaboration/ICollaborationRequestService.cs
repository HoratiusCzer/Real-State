using REAK.Api.Models.Dto;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Collaboration;

public enum CollabOpResult { Success, NotFound, Forbidden, InvalidState }
public record CollabOp(CollabOpResult Result, string? Error = null);

public interface ICollaborationRequestService
{
    Task<(CollabOp Op, Guid? RequestId)> CreateFromMatchAsync(CallerContext caller, Guid matchId, string? message, CancellationToken ct = default);
    Task<(CollabOp Op, Guid? RequestId)> CreateToOrgAsync(CallerContext caller, Guid toMemberEntityId, string? message, CancellationToken ct = default);
    Task<List<CollaborationRequestDto>> ListMineAsync(CallerContext caller, CancellationToken ct = default);
    Task<(CollabOp Op, Guid? WorkspaceId)> AcceptAsync(CallerContext caller, Guid requestId, CancellationToken ct = default);
    Task<CollabOp> DeclineAsync(CallerContext caller, Guid requestId, CancellationToken ct = default);
    Task<CollabOp> CancelAsync(CallerContext caller, Guid requestId, CancellationToken ct = default);
}
