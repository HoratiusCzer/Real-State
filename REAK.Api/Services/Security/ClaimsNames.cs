namespace REAK.Api.Services.Security;

/// <summary>Custom JWT claim type names shared between token issuance (JwtTokenService), the
/// connection interceptor that projects them into SQL Server SESSION_CONTEXT
/// (SessionContextConnectionInterceptor), and the RequirePermission authorization filter.</summary>
public static class ClaimsNames
{
    public const string IsSystemAdmin = "is_system_admin";
    public const string Permission = "perm";
    public const string MemberEntityId = "member_entity_id";
}
