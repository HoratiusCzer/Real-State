using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Entities.Collaboration;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Notifications;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Collaboration;

/// <summary>Flow D steps 1-3 (spec §2.4): a request, typically originating from a Match, that
/// either side can send; acceptance is what creates the CollaborationWorkspace — never automatic,
/// and never discloses contact info by itself (that's ICollaborationWorkspaceService's job, spec
/// §13.2). CollaborationRequest has no RLS of its own (it has no CollaborationWorkspaceId to hang
/// a participant-predicate off before a workspace exists), so every method here does its own
/// explicit from/to-org ownership check — same pattern as Notifications/MemberEntities.</summary>
public class CollaborationRequestService(ReakDbContext db, SessionContextOverride sessionContextOverride, INotificationService notificationService) : ICollaborationRequestService
{
    private record MatchOrgs(Guid ListingOrgId, string ListingOrgName, Guid DemandOrgId, string DemandOrgName);


    public async Task<(CollabOp Op, Guid? RequestId)> CreateFromMatchAsync(CallerContext caller, Guid matchId, string? message, CancellationToken ct = default)
    {
        if (caller.MemberEntityIds.Count == 0)
        {
            return (new CollabOp(CollabOpResult.Forbidden, "You must belong to a member organization to request collaboration."), null);
        }

        // Match joins PropertyListings + Demands (both RLS-protected) — same cross-tenant JOIN
        // shape Stage 8 hit, so this one read needs the same elevation. The actual authorization
        // decision (is the caller genuinely party to this match?) happens after, in plain C#.
        sessionContextOverride.IsSystemLevel = true;
        MatchOrgs? matchOrgs;
        try
        {
            matchOrgs = await db.Matches.Where(m => m.Id == matchId)
                .Select(m => new MatchOrgs(m.Listing.MemberEntityId, m.Listing.MemberEntity.Name, m.Demand.MemberEntityId, m.Demand.MemberEntity.Name))
                .FirstOrDefaultAsync(ct);
        }
        finally
        {
            sessionContextOverride.IsSystemLevel = false;
        }

        if (matchOrgs is null)
        {
            return (new CollabOp(CollabOpResult.NotFound), null);
        }

        Guid fromOrg, toOrg;
        string fromOrgName;
        if (caller.MemberEntityIds.Contains(matchOrgs.ListingOrgId))
        {
            fromOrg = matchOrgs.ListingOrgId;
            fromOrgName = matchOrgs.ListingOrgName;
            toOrg = matchOrgs.DemandOrgId;
        }
        else if (caller.MemberEntityIds.Contains(matchOrgs.DemandOrgId))
        {
            fromOrg = matchOrgs.DemandOrgId;
            fromOrgName = matchOrgs.DemandOrgName;
            toOrg = matchOrgs.ListingOrgId;
        }
        else
        {
            return (new CollabOp(CollabOpResult.Forbidden, "Your organization isn't party to this match."), null);
        }

        var existingPending = await db.CollaborationRequests
            .FirstOrDefaultAsync(r => r.MatchId == matchId && r.FromMemberEntityId == fromOrg && r.Status == CollaborationRequestStatus.Pending, ct);
        if (existingPending is not null)
        {
            return (new CollabOp(CollabOpResult.Success), existingPending.Id);
        }

        var request = new CollaborationRequest
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            FromMemberEntityId = fromOrg,
            ToMemberEntityId = toOrg,
            RequestedByProfileId = caller.ProfileId,
            Message = message,
        };
        db.CollaborationRequests.Add(request);
        await db.SaveChangesAsync(ct);

        await notificationService.NotifyOrgAsync(toOrg, NotificationType.CollaborationRequest, $"{fromOrgName} requested to collaborate", message, "/portal/collaborations", ct);

        return (new CollabOp(CollabOpResult.Success), request.Id);
    }

    public async Task<(CollabOp Op, Guid? RequestId)> CreateToOrgAsync(CallerContext caller, Guid toMemberEntityId, string? message, CancellationToken ct = default)
    {
        if (caller.MemberEntityIds.Count == 0)
        {
            return (new CollabOp(CollabOpResult.Forbidden, "You must belong to a member organization to request collaboration."), null);
        }

        var fromOrg = caller.MemberEntityIds[0];
        if (fromOrg == toMemberEntityId)
        {
            return (new CollabOp(CollabOpResult.InvalidState, "You can't request collaboration with your own organization."), null);
        }

        if (!await db.MemberEntities.AnyAsync(m => m.Id == toMemberEntityId && m.IsActive, ct))
        {
            return (new CollabOp(CollabOpResult.NotFound), null);
        }

        var request = new CollaborationRequest
        {
            Id = Guid.NewGuid(),
            MatchId = null,
            FromMemberEntityId = fromOrg,
            ToMemberEntityId = toMemberEntityId,
            RequestedByProfileId = caller.ProfileId,
            Message = message,
        };
        db.CollaborationRequests.Add(request);
        await db.SaveChangesAsync(ct);

        var fromOrgName = await db.MemberEntities.Where(m => m.Id == fromOrg).Select(m => m.Name).FirstAsync(ct);
        await notificationService.NotifyOrgAsync(toMemberEntityId, NotificationType.CollaborationRequest, $"{fromOrgName} requested to collaborate", message, "/portal/collaborations", ct);

        return (new CollabOp(CollabOpResult.Success), request.Id);
    }

    public async Task<List<CollaborationRequestDto>> ListMineAsync(CallerContext caller, CancellationToken ct = default)
    {
        if (caller.MemberEntityIds.Count == 0 && !caller.IsSystemAdmin)
        {
            return [];
        }

        var q = db.CollaborationRequests.AsQueryable();
        if (!caller.IsSystemAdmin)
        {
            q = q.Where(r => caller.MemberEntityIds.Contains(r.FromMemberEntityId) || caller.MemberEntityIds.Contains(r.ToMemberEntityId));
        }

        return await q
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new CollaborationRequestDto(
                r.Id, r.MatchId, r.FromMemberEntityId, r.FromMemberEntity.Name, r.ToMemberEntityId, r.ToMemberEntity.Name,
                r.RequestedByProfile.FullName, r.Status.ToString(), r.Message, r.RespondedAt, r.CreatedAt,
                r.Workspace != null ? r.Workspace.Id : (Guid?)null))
            .ToListAsync(ct);
    }

    public async Task<(CollabOp Op, Guid? WorkspaceId)> AcceptAsync(CallerContext caller, Guid requestId, CancellationToken ct = default)
    {
        var request = await db.CollaborationRequests.FirstOrDefaultAsync(r => r.Id == requestId, ct);
        if (request is null) return (new CollabOp(CollabOpResult.NotFound), null);
        if (!caller.IsSystemAdmin && !caller.MemberEntityIds.Contains(request.ToMemberEntityId))
        {
            return (new CollabOp(CollabOpResult.Forbidden, "Only the recipient organization can accept this request."), null);
        }
        if (request.Status != CollaborationRequestStatus.Pending)
        {
            return (new CollabOp(CollabOpResult.InvalidState, "This request has already been responded to."), null);
        }

        request.Status = CollaborationRequestStatus.Accepted;
        request.RespondedByProfileId = caller.ProfileId;
        request.RespondedAt = DateTime.UtcNow;

        var workspace = new CollaborationWorkspace { Id = Guid.NewGuid(), CollaborationRequestId = request.Id };
        db.CollaborationWorkspaces.Add(workspace);
        db.CollaborationParticipants.Add(new CollaborationParticipant
        {
            Id = Guid.NewGuid(), CollaborationWorkspaceId = workspace.Id,
            ProfileId = request.RequestedByProfileId, MemberEntityId = request.FromMemberEntityId,
        });
        db.CollaborationParticipants.Add(new CollaborationParticipant
        {
            Id = Guid.NewGuid(), CollaborationWorkspaceId = workspace.Id,
            ProfileId = caller.ProfileId, MemberEntityId = request.ToMemberEntityId,
        });
        db.CollaborationActivities.Add(new CollaborationActivity
        {
            Id = Guid.NewGuid(), CollaborationWorkspaceId = workspace.Id, ProfileId = caller.ProfileId,
            Action = "Collaboration workspace created — request accepted.",
        });

        // Bootstrapping problem, same shape as the matching engine's: CollaborationWorkspaces/
        // CollaborationParticipants' own INSERT block predicates check "is the session's profile
        // already a participant of this workspace?" — which is impossible to be true for the
        // very first participant rows being inserted in the same transaction that creates the
        // workspace itself. Elevate only for this one bootstrap write; every later action in an
        // existing workspace goes through as the caller's real, already-a-participant identity.
        sessionContextOverride.IsSystemLevel = true;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        finally
        {
            sessionContextOverride.IsSystemLevel = false;
        }

        await notificationService.NotifyAsync(request.RequestedByProfileId, NotificationType.CollaborationAccepted, "Your collaboration request was accepted", null, $"/portal/collaborations/{workspace.Id}", ct);

        return (new CollabOp(CollabOpResult.Success), workspace.Id);
    }

    public async Task<CollabOp> DeclineAsync(CallerContext caller, Guid requestId, CancellationToken ct = default)
    {
        var request = await db.CollaborationRequests.FirstOrDefaultAsync(r => r.Id == requestId, ct);
        if (request is null) return new CollabOp(CollabOpResult.NotFound);
        if (!caller.IsSystemAdmin && !caller.MemberEntityIds.Contains(request.ToMemberEntityId))
        {
            return new CollabOp(CollabOpResult.Forbidden, "Only the recipient organization can decline this request.");
        }
        if (request.Status != CollaborationRequestStatus.Pending)
        {
            return new CollabOp(CollabOpResult.InvalidState, "This request has already been responded to.");
        }

        request.Status = CollaborationRequestStatus.Declined;
        request.RespondedByProfileId = caller.ProfileId;
        request.RespondedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await notificationService.NotifyAsync(request.RequestedByProfileId, NotificationType.CollaborationDeclined, "Your collaboration request was declined", null, "/portal/collaborations", ct);

        return new CollabOp(CollabOpResult.Success);
    }

    public async Task<CollabOp> CancelAsync(CallerContext caller, Guid requestId, CancellationToken ct = default)
    {
        var request = await db.CollaborationRequests.FirstOrDefaultAsync(r => r.Id == requestId, ct);
        if (request is null) return new CollabOp(CollabOpResult.NotFound);
        if (!caller.IsSystemAdmin && !caller.MemberEntityIds.Contains(request.FromMemberEntityId))
        {
            return new CollabOp(CollabOpResult.Forbidden, "Only the requesting organization can cancel this request.");
        }
        if (request.Status != CollaborationRequestStatus.Pending)
        {
            return new CollabOp(CollabOpResult.InvalidState, "This request has already been responded to.");
        }

        request.Status = CollaborationRequestStatus.Cancelled;
        request.RespondedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return new CollabOp(CollabOpResult.Success);
    }
}
