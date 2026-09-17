using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Audit;
using REAK.Api.Services.Notifications;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Auth;

/// <summary>Flow A steps 3-5 (spec §2.4): admin creates an invitation, invitee accepts (setting a
/// password if they're a brand-new profile), and an EntityUser row links them to their member
/// organization with the granted role.</summary>
public class InvitationService(ReakDbContext db, IPasswordHasher passwordHasher, IEmailSender emailSender, INotificationService notificationService, IAuditLogService auditLogService) : IInvitationService
{
    public async Task<Invitation> CreateAsync(string email, Guid? memberEntityId, Guid roleId, Guid invitedByProfileId, CancellationToken ct = default)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == roleId, ct)
            ?? throw new InvalidOperationException("Role not found.");

        if (role.Scope == RoleScope.Organization && memberEntityId is null)
        {
            throw new InvalidOperationException("An organization-scoped role requires a member entity.");
        }

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            Email = email,
            MemberEntityId = memberEntityId,
            RoleId = roleId,
            Token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-').Replace('/', '_').TrimEnd('='),
            Status = InvitationStatus.Pending,
            InvitedByProfileId = invitedByProfileId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        db.Invitations.Add(invitation);
        await db.SaveChangesAsync(ct);

        var memberEntityName = memberEntityId is null
            ? null
            : await db.MemberEntities.Where(m => m.Id == memberEntityId).Select(m => m.Name).FirstOrDefaultAsync(ct);

        await emailSender.SendAsync(
            email,
            "You've been invited to join REAK",
            $"You've been invited to join{(memberEntityName is null ? "" : $" {memberEntityName} on")} REAK as {role.Name}. " +
            $"Invitation token: {invitation.Token} (expires {invitation.ExpiresAt:u}).",
            ct);

        return invitation;
    }

    public async Task<InvitationLookupResult> LookupAsync(string token, CancellationToken ct = default)
    {
        var invitation = await db.Invitations
            .Include(i => i.Role)
            .Include(i => i.MemberEntity)
            .FirstOrDefaultAsync(i => i.Token == token, ct);

        if (invitation is null)
        {
            return new InvitationLookupResult(false, null, null, null, false, false);
        }

        var expiredOrUsed = invitation.Status != InvitationStatus.Pending || invitation.ExpiresAt <= DateTime.UtcNow;
        var existingProfile = await db.Profiles.FirstOrDefaultAsync(p => p.Email == invitation.Email, ct);

        return new InvitationLookupResult(
            Found: true,
            Email: invitation.Email,
            RoleName: invitation.Role.Name,
            MemberEntityName: invitation.MemberEntity?.Name,
            IsNewAccount: existingProfile is null,
            IsExpiredOrUsed: expiredOrUsed);
    }

    public async Task<(bool Success, string? Error)> AcceptAsync(string token, string? password, CancellationToken ct = default)
    {
        var invitation = await db.Invitations.Include(i => i.Role).FirstOrDefaultAsync(i => i.Token == token, ct);
        if (invitation is null)
        {
            return (false, "Invitation not found.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return (false, "This invitation has already been used or revoked.");
        }

        if (invitation.ExpiresAt <= DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await db.SaveChangesAsync(ct);
            return (false, "This invitation has expired.");
        }

        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Email == invitation.Email, ct);
        if (profile is null)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                return (false, "A password of at least 8 characters is required to activate a new account.");
            }

            profile = new Profile
            {
                Id = Guid.NewGuid(),
                Email = invitation.Email,
                PasswordHash = passwordHasher.Hash(password),
                FullName = invitation.Email,
                IsActive = true,
            };
            db.Profiles.Add(profile);
        }

        if (invitation.MemberEntityId is not null)
        {
            var alreadyMember = await db.EntityUsers.AnyAsync(
                eu => eu.ProfileId == profile.Id && eu.MemberEntityId == invitation.MemberEntityId, ct);
            if (!alreadyMember)
            {
                db.EntityUsers.Add(new EntityUser
                {
                    Id = Guid.NewGuid(),
                    ProfileId = profile.Id,
                    MemberEntityId = invitation.MemberEntityId.Value,
                    IsActive = true,
                });
            }
        }

        var alreadyAssigned = await db.ProfileRoleAssignments.AnyAsync(
            a => a.ProfileId == profile.Id && a.RoleId == invitation.RoleId && a.MemberEntityId == invitation.MemberEntityId, ct);
        if (!alreadyAssigned)
        {
            db.ProfileRoleAssignments.Add(new ProfileRoleAssignment
            {
                Id = Guid.NewGuid(),
                ProfileId = profile.Id,
                RoleId = invitation.RoleId,
                MemberEntityId = invitation.MemberEntityId,
            });
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        await notificationService.NotifyAsync(invitation.InvitedByProfileId, NotificationType.Invitation, $"{invitation.Email} accepted your invitation", null, null, ct);
        await auditLogService.LogAsync(profile.Id, "RoleGranted", "ProfileRoleAssignment", profile.Id, $"{invitation.Email} was granted {invitation.Role.Name} via invitation.", ct);

        return (true, null);
    }

    public async Task<bool> RevokeAsync(Guid invitationId, CancellationToken ct = default)
    {
        var invitation = await db.Invitations.FirstOrDefaultAsync(i => i.Id == invitationId, ct);
        if (invitation is null || invitation.Status != InvitationStatus.Pending)
        {
            return false;
        }

        invitation.Status = InvitationStatus.Revoked;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
