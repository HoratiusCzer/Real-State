using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Audit;

namespace REAK.Api.Services.Auth;

/// <summary>Flow A steps 1-3 (spec §2.4): a visitor applies, an admin approves or rejects, and
/// approval creates the MemberEntity plus a MemberAdmin invitation for the applicant in one step —
/// the rest of Flow A (invitation acceptance) is IInvitationService's job.</summary>
public class MembershipApplicationService(ReakDbContext db, IInvitationService invitationService, IAuditLogService auditLogService) : IMembershipApplicationService
{
    public async Task<(bool Success, string? Error, MembershipApplication? Application)> SubmitAsync(
        string companyName, string contactName, string email, string phone, string? message, CancellationToken ct = default)
    {
        var enabled = await db.FeatureFlags
            .Where(f => f.Key == "membership_application_enabled")
            .Select(f => f.IsEnabled)
            .FirstOrDefaultAsync(ct);

        if (!enabled)
        {
            return (false, "REAK is not currently accepting membership applications.", null);
        }

        var application = new MembershipApplication
        {
            Id = Guid.NewGuid(),
            CompanyName = companyName,
            ContactName = contactName,
            Email = email,
            Phone = phone,
            Message = message,
            Status = MembershipApplicationStatus.Pending,
        };

        db.MembershipApplications.Add(application);
        await db.SaveChangesAsync(ct);
        return (true, null, application);
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(Guid applicationId, Guid reviewedByProfileId, CancellationToken ct = default)
    {
        var application = await db.MembershipApplications.FirstOrDefaultAsync(a => a.Id == applicationId, ct);
        if (application is null)
        {
            return (false, "Application not found.");
        }

        if (application.Status != MembershipApplicationStatus.Pending)
        {
            return (false, "This application has already been reviewed.");
        }

        var memberAdminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "MemberAdmin", ct)
            ?? throw new InvalidOperationException("MemberAdmin role is not seeded.");

        var memberEntity = new MemberEntity
        {
            Id = Guid.NewGuid(),
            Name = application.CompanyName,
            Email = application.Email,
            Phone = application.Phone,
            IsActive = true,
        };
        db.MemberEntities.Add(memberEntity);

        application.Status = MembershipApplicationStatus.Approved;
        application.ReviewedByProfileId = reviewedByProfileId;
        application.ReviewedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        await invitationService.CreateAsync(application.Email, memberEntity.Id, memberAdminRole.Id, reviewedByProfileId, ct);
        await auditLogService.LogAsync(reviewedByProfileId, "MembershipApplicationApproved", "MembershipApplication", applicationId, $"{application.CompanyName}'s application was approved; {memberEntity.Name} created.", ct);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RejectAsync(Guid applicationId, Guid reviewedByProfileId, string reason, CancellationToken ct = default)
    {
        var application = await db.MembershipApplications.FirstOrDefaultAsync(a => a.Id == applicationId, ct);
        if (application is null)
        {
            return (false, "Application not found.");
        }

        if (application.Status != MembershipApplicationStatus.Pending)
        {
            return (false, "This application has already been reviewed.");
        }

        application.Status = MembershipApplicationStatus.Rejected;
        application.ReviewedByProfileId = reviewedByProfileId;
        application.ReviewedAt = DateTime.UtcNow;
        application.RejectionReason = reason;

        await db.SaveChangesAsync(ct);
        await auditLogService.LogAsync(reviewedByProfileId, "MembershipApplicationRejected", "MembershipApplication", applicationId, $"{application.CompanyName}'s application was rejected: {reason}", ct);
        return (true, null);
    }
}
