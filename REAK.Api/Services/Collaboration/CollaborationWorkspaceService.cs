using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Entities.Collaboration;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Notifications;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Collaboration;

/// <summary>Spec §13.1-§13.2. Every table here (Workspace, Participants, Messages, Files, Notes,
/// Tasks, Viewings, Activities, ContactDisclosures) has real RLS — a single shared predicate
/// (fn_CollaborationParticipantPredicate in RowLevelSecurity.sql) since, unlike listings/demands,
/// "can read this workspace" and "can post into it" are the same audience here (any participant).
/// That means most methods below can just run an ordinary query and trust RLS to scope it —
/// no rows for a non-participant means an empty list or a null result, not an error, which is
/// exactly the fail-closed behavior we want. Writes are guarded the same way ListingService
/// guards its writes: catch the RLS block-predicate SqlException and translate it to Forbidden.</summary>
public class CollaborationWorkspaceService(ReakDbContext db, SessionContextOverride sessionContextOverride, INotificationService notificationService) : ICollaborationWorkspaceService
{
    private record ListingDemandOrgs(Guid? ListingId, string? ListingTitle, Guid? ListingOrgId, Guid? DemandId, string? DemandTitle, Guid? DemandOrgId);

    public async Task<CollaborationWorkspaceDto?> GetAsync(Guid workspaceId, CancellationToken ct = default)
    {
        var workspace = await db.CollaborationWorkspaces
            .Include(w => w.Participants).ThenInclude(p => p.Profile)
            .Include(w => w.Participants).ThenInclude(p => p.MemberEntity)
            .Include(w => w.ContactDisclosures).ThenInclude(d => d.GrantingMemberEntity)
            .Include(w => w.ContactDisclosures).ThenInclude(d => d.ReceivingMemberEntity)
            .Include(w => w.ContactDisclosures).ThenInclude(d => d.GrantingProfile)
            .Include(w => w.Request)
            .FirstOrDefaultAsync(w => w.Id == workspaceId, ct);
        if (workspace is null) return null;

        ListingDemandOrgs? origin = null;
        if (workspace.Request.MatchId is { } matchId)
        {
            sessionContextOverride.IsSystemLevel = true;
            try
            {
                origin = await db.Matches.Where(m => m.Id == matchId)
                    .Select(m => new ListingDemandOrgs(
                        m.ListingId, m.Listing.Title, m.Listing.MemberEntityId,
                        m.DemandId, m.Demand.Title, m.Demand.MemberEntityId))
                    .FirstOrDefaultAsync(ct);
            }
            finally
            {
                sessionContextOverride.IsSystemLevel = false;
            }
        }

        return new CollaborationWorkspaceDto(
            workspace.Id, workspace.CollaborationRequestId, workspace.Request.MatchId,
            origin?.ListingId, origin?.ListingTitle, origin?.DemandId, origin?.DemandTitle,
            workspace.CreatedAt,
            workspace.Participants.Select(p => new CollaborationParticipantDto(p.ProfileId, p.Profile.FullName, p.MemberEntityId, p.MemberEntity.Name, p.JoinedAt)).ToList(),
            workspace.ContactDisclosures.Select(d => new CollaborationContactDisclosureDto(
                d.Id, d.DataType, d.GrantingMemberEntityId, d.GrantingMemberEntity.Name,
                d.ReceivingMemberEntityId, d.ReceivingMemberEntity.Name, d.GrantingProfile.FullName,
                d.GrantedAt, d.RevokedAt, d.PolicyVersion)).ToList());
    }

    public async Task<List<CollaborationMessageDto>> ListMessagesAsync(Guid workspaceId, CancellationToken ct = default) =>
        await db.CollaborationMessages.Where(m => m.CollaborationWorkspaceId == workspaceId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new CollaborationMessageDto(m.Id, m.SenderProfile.FullName, m.Body, m.CreatedAt))
            .ToListAsync(ct);

    public async Task<CollabOp> SendMessageAsync(Guid workspaceId, CallerContext caller, string body, CancellationToken ct = default)
    {
        db.CollaborationMessages.Add(new CollaborationMessage { Id = Guid.NewGuid(), CollaborationWorkspaceId = workspaceId, SenderProfileId = caller.ProfileId, Body = body });
        var op = await SaveGuardedAsync(ct);
        if (op.Result != CollabOpResult.Success) return op;

        // RLS already proved the caller is a participant of this workspace (the insert above only
        // succeeded because of that), so reading the other participant rows of the same workspace
        // needs no elevation — "any participant can see every row of their own workspace" is the
        // whole point of the shared predicate.
        var otherProfileIds = await db.CollaborationParticipants
            .Where(p => p.CollaborationWorkspaceId == workspaceId && p.ProfileId != caller.ProfileId)
            .Select(p => p.ProfileId)
            .ToListAsync(ct);
        var senderName = await db.Profiles.Where(p => p.Id == caller.ProfileId).Select(p => p.FullName).FirstAsync(ct);
        var preview = body.Length > 200 ? body[..200] + "…" : body;
        await notificationService.NotifyManyAsync(otherProfileIds, NotificationType.Message, $"New message from {senderName}", preview, $"/portal/collaborations/{workspaceId}", ct);

        return op;
    }

    public async Task<List<CollaborationNoteDto>> ListNotesAsync(Guid workspaceId, CancellationToken ct = default) =>
        await db.CollaborationNotes.Where(n => n.CollaborationWorkspaceId == workspaceId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new CollaborationNoteDto(n.Id, n.AuthorProfile.FullName, n.Body, n.CreatedAt))
            .ToListAsync(ct);

    public async Task<CollabOp> AddNoteAsync(Guid workspaceId, CallerContext caller, string body, CancellationToken ct = default)
    {
        db.CollaborationNotes.Add(new CollaborationNote { Id = Guid.NewGuid(), CollaborationWorkspaceId = workspaceId, AuthorProfileId = caller.ProfileId, Body = body });
        return await SaveGuardedAsync(ct);
    }

    public async Task<List<CollaborationTaskDto>> ListTasksAsync(Guid workspaceId, CancellationToken ct = default) =>
        await db.CollaborationTasks.Where(t => t.CollaborationWorkspaceId == workspaceId)
            .OrderBy(t => t.Status).ThenBy(t => t.DueDate)
            .Select(t => new CollaborationTaskDto(t.Id, t.Title, t.AssignedToProfile != null ? t.AssignedToProfile.FullName : null, t.DueDate, t.Status.ToString(), t.CreatedAt))
            .ToListAsync(ct);

    public async Task<CollabOp> CreateTaskAsync(Guid workspaceId, CallerContext caller, string title, Guid? assignedToProfileId, DateTime? dueDate, CancellationToken ct = default)
    {
        db.CollaborationTasks.Add(new CollaborationTask
        {
            Id = Guid.NewGuid(), CollaborationWorkspaceId = workspaceId, CreatedByProfileId = caller.ProfileId,
            Title = title, AssignedToProfileId = assignedToProfileId, DueDate = dueDate,
        });
        return await SaveGuardedAsync(ct);
    }

    public async Task<CollabOp> UpdateTaskStatusAsync(Guid workspaceId, Guid taskId, CollaborationTaskStatus status, CancellationToken ct = default)
    {
        var task = await db.CollaborationTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.CollaborationWorkspaceId == workspaceId, ct);
        if (task is null) return new CollabOp(CollabOpResult.NotFound);
        task.Status = status;
        return await SaveGuardedAsync(ct);
    }

    public async Task<List<CollaborationViewingDto>> ListViewingsAsync(Guid workspaceId, CancellationToken ct = default) =>
        await db.CollaborationViewings.Where(v => v.CollaborationWorkspaceId == workspaceId)
            .OrderBy(v => v.ScheduledAt)
            .Select(v => new CollaborationViewingDto(v.Id, v.ScheduledByProfile.FullName, v.ScheduledAt, v.Notes, v.CreatedAt))
            .ToListAsync(ct);

    public async Task<CollabOp> ScheduleViewingAsync(Guid workspaceId, CallerContext caller, DateTime scheduledAt, string? notes, CancellationToken ct = default)
    {
        db.CollaborationViewings.Add(new CollaborationViewing { Id = Guid.NewGuid(), CollaborationWorkspaceId = workspaceId, ScheduledByProfileId = caller.ProfileId, ScheduledAt = scheduledAt, Notes = notes });
        return await SaveGuardedAsync(ct);
    }

    public async Task<List<CollaborationFileDto>> ListFilesAsync(Guid workspaceId, CancellationToken ct = default) =>
        await db.CollaborationFiles.Where(f => f.CollaborationWorkspaceId == workspaceId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new CollaborationFileDto(f.Id, f.FileName, f.UploadedByProfile.FullName, f.CreatedAt))
            .ToListAsync(ct);

    public async Task<(CollabOp Op, Guid? FileId)> AddFileAsync(Guid workspaceId, CallerContext caller, string fileName, string storagePath, CancellationToken ct = default)
    {
        var file = new CollaborationFile { Id = Guid.NewGuid(), CollaborationWorkspaceId = workspaceId, UploadedByProfileId = caller.ProfileId, FileName = fileName, StoragePath = storagePath };
        db.CollaborationFiles.Add(file);
        var op = await SaveGuardedAsync(ct);
        return (op, op.Result == CollabOpResult.Success ? file.Id : null);
    }

    public async Task<(CollabOp Op, string? StoragePath)> GetFileForDownloadAsync(Guid workspaceId, Guid fileId, CancellationToken ct = default)
    {
        // RLS's filter predicate already scopes this to participants only — a non-participant's
        // query simply returns no row, which we report as NotFound (never a distinguishable
        // "exists but forbidden", matching the same fail-closed shape used elsewhere).
        var file = await db.CollaborationFiles.FirstOrDefaultAsync(f => f.Id == fileId && f.CollaborationWorkspaceId == workspaceId, ct);
        return file is null ? (new CollabOp(CollabOpResult.NotFound), null) : (new CollabOp(CollabOpResult.Success), file.StoragePath);
    }

    public async Task<(CollabOp Op, string? StoragePath)> DeleteFileAsync(Guid workspaceId, Guid fileId, CancellationToken ct = default)
    {
        var file = await db.CollaborationFiles.FirstOrDefaultAsync(f => f.Id == fileId && f.CollaborationWorkspaceId == workspaceId, ct);
        if (file is null) return (new CollabOp(CollabOpResult.NotFound), null);
        db.CollaborationFiles.Remove(file);
        var op = await SaveGuardedAsync(ct);
        return (op, op.Result == CollabOpResult.Success ? file.StoragePath : null);
    }

    public async Task<List<CollaborationActivityDto>> ListActivitiesAsync(Guid workspaceId, CancellationToken ct = default) =>
        await db.CollaborationActivities.Where(a => a.CollaborationWorkspaceId == workspaceId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new CollaborationActivityDto(a.Id, a.Profile != null ? a.Profile.FullName : null, a.Action, a.CreatedAt))
            .ToListAsync(ct);

    public async Task<CollabOp> GrantContactDisclosureAsync(Guid workspaceId, CallerContext caller, ContactDataType dataType, CancellationToken ct = default)
    {
        if (dataType is not (ContactDataType.ListingContact or ContactDataType.DemandContact))
        {
            // Phone/Email exist in the enum but RowLevelSecurity.sql's disclosure predicates only
            // ever check DataType 1 (ListingContact) and 2 (DemandContact) — granting any other
            // type would be recorded but have zero actual effect. Refuse rather than pretend it works.
            return new CollabOp(CollabOpResult.InvalidState, "Only ListingContact or DemandContact disclosures are currently enforced.");
        }

        var workspace = await db.CollaborationWorkspaces.Include(w => w.Request).FirstOrDefaultAsync(w => w.Id == workspaceId, ct);
        if (workspace is null) return new CollabOp(CollabOpResult.NotFound);
        if (workspace.Request.MatchId is null)
        {
            return new CollabOp(CollabOpResult.InvalidState, "This collaboration didn't originate from a match, so there's no specific listing/demand to scope a contact disclosure to.");
        }

        sessionContextOverride.IsSystemLevel = true;
        ListingDemandOrgs? origin;
        try
        {
            origin = await db.Matches.Where(m => m.Id == workspace.Request.MatchId)
                .Select(m => new ListingDemandOrgs(m.ListingId, m.Listing.Title, m.Listing.MemberEntityId, m.DemandId, m.Demand.Title, m.Demand.MemberEntityId))
                .FirstOrDefaultAsync(ct);
        }
        finally
        {
            sessionContextOverride.IsSystemLevel = false;
        }
        if (origin is null) return new CollabOp(CollabOpResult.NotFound);

        var grantingOrgId = dataType == ContactDataType.ListingContact ? origin.ListingOrgId!.Value : origin.DemandOrgId!.Value;
        var receivingOrgId = dataType == ContactDataType.ListingContact ? origin.DemandOrgId!.Value : origin.ListingOrgId!.Value;

        if (!caller.IsSystemAdmin && !caller.MemberEntityIds.Contains(grantingOrgId))
        {
            return new CollabOp(CollabOpResult.Forbidden, "Only the organization whose contact information this is can disclose it.");
        }

        var alreadyGranted = await db.CollaborationContactDisclosures.AnyAsync(
            d => d.CollaborationWorkspaceId == workspaceId && d.DataType == dataType && d.RevokedAt == null, ct);
        if (alreadyGranted) return new CollabOp(CollabOpResult.Success);

        db.CollaborationContactDisclosures.Add(new CollaborationContactDisclosure
        {
            Id = Guid.NewGuid(), CollaborationWorkspaceId = workspaceId, DataType = dataType,
            GrantingMemberEntityId = grantingOrgId, ReceivingMemberEntityId = receivingOrgId, GrantingProfileId = caller.ProfileId,
        });
        db.CollaborationActivities.Add(new CollaborationActivity
        {
            Id = Guid.NewGuid(), CollaborationWorkspaceId = workspaceId, ProfileId = caller.ProfileId,
            Action = $"{dataType} disclosed to the other organization.",
        });
        return await SaveGuardedAsync(ct);
    }

    public async Task<CollabOp> RevokeContactDisclosureAsync(Guid workspaceId, Guid disclosureId, CallerContext caller, CancellationToken ct = default)
    {
        var disclosure = await db.CollaborationContactDisclosures.FirstOrDefaultAsync(d => d.Id == disclosureId && d.CollaborationWorkspaceId == workspaceId, ct);
        if (disclosure is null) return new CollabOp(CollabOpResult.NotFound);
        if (!caller.IsSystemAdmin && !caller.MemberEntityIds.Contains(disclosure.GrantingMemberEntityId))
        {
            return new CollabOp(CollabOpResult.Forbidden, "Only the organization that granted this disclosure can revoke it.");
        }
        if (disclosure.RevokedAt is not null) return new CollabOp(CollabOpResult.Success);

        disclosure.RevokedAt = DateTime.UtcNow;
        db.CollaborationActivities.Add(new CollaborationActivity
        {
            Id = Guid.NewGuid(), CollaborationWorkspaceId = workspaceId, ProfileId = caller.ProfileId,
            Action = $"{disclosure.DataType} disclosure revoked.",
        });
        return await SaveGuardedAsync(ct);
    }

    private async Task<CollabOp> SaveGuardedAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
            return new CollabOp(CollabOpResult.Success);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Message.Contains("block predicate", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var entry in db.ChangeTracker.Entries())
            {
                entry.State = EntityState.Unchanged;
            }
            return new CollabOp(CollabOpResult.Forbidden, "You aren't a participant in this collaboration.");
        }
    }
}
