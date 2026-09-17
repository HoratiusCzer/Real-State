using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REAK.Api.Models.Dto;
using REAK.Api.Services.Collaboration;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

/// <summary>Flow D steps 1-3 (spec §2.4): initiate, list, and respond to collaboration requests.
/// See CollaborationRequestService for why this stage has no RLS of its own. "collaboration.create"
/// gates every mutation (mirrors the listings/demands MemberAdmin-vs-MemberStaff .create/.update
/// asymmetry); "collaboration.read" alone only lets a caller list their own.</summary>
[ApiController]
[Route("api/collaboration-requests")]
[Authorize]
public class CollaborationRequestsController(ICollaborationRequestService requestService) : ControllerBase
{
    [HttpGet]
    [RequirePermission("collaboration.read")]
    public async Task<IActionResult> ListMine(CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        return Ok(await requestService.ListMineAsync(caller, ct));
    }

    [HttpPost("from-match")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> CreateFromMatch(CreateCollaborationRequestFromMatchRequest request, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var (op, requestId) = await requestService.CreateFromMatchAsync(caller, request.MatchId, request.Message, ct);
        return op.Result == CollabOpResult.Success ? Ok(new { id = requestId }) : ToActionResult(op);
    }

    [HttpPost("to-org")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> CreateToOrg(CreateCollaborationRequestToOrgRequest request, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var (op, requestId) = await requestService.CreateToOrgAsync(caller, request.ToMemberEntityId, request.Message, ct);
        return op.Result == CollabOpResult.Success ? Ok(new { id = requestId }) : ToActionResult(op);
    }

    [HttpPost("{id:guid}/accept")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var (op, workspaceId) = await requestService.AcceptAsync(caller, id, ct);
        return op.Result == CollabOpResult.Success ? Ok(new { workspaceId }) : ToActionResult(op);
    }

    [HttpPost("{id:guid}/decline")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> Decline(Guid id, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var op = await requestService.DeclineAsync(caller, id, ct);
        return op.Result == CollabOpResult.Success ? NoContent() : ToActionResult(op);
    }

    [HttpPost("{id:guid}/cancel")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var op = await requestService.CancelAsync(caller, id, ct);
        return op.Result == CollabOpResult.Success ? NoContent() : ToActionResult(op);
    }

    private IActionResult ToActionResult(CollabOp op) => op.Result switch
    {
        CollabOpResult.NotFound => NotFound(),
        CollabOpResult.Forbidden => Forbid(),
        CollabOpResult.InvalidState => BadRequest(new { error = op.Error }),
        _ => BadRequest(new { error = op.Error ?? "Request failed." }),
    };
}
