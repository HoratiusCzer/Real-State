namespace REAK.Api.Models.Enums;

/// <summary>System-level roles are not tied to a member organization; organization-level roles are scoped to one.</summary>
public enum RoleScope
{
    System = 1,
    Organization = 2,
}

public enum InvitationStatus
{
    Pending = 1,
    Accepted = 2,
    Expired = 3,
    Revoked = 4,
}

public enum MembershipApplicationStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
}
