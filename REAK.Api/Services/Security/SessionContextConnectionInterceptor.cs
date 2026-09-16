using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace REAK.Api.Services.Security;

/// <summary>The bridge between JWT auth and the SQL Server RLS built in Stage 3
/// (Data/Security/RowLevelSecurity.sql), which reads SESSION_CONTEXT('app.profile_id') and
/// SESSION_CONTEXT('app.is_system_admin'). ASP.NET Core's authentication middleware runs before
/// controllers/EF ever touch the database, so by the time EF opens a connection for a request,
/// HttpContext.User already carries the caller's identity (or is anonymous) — this interceptor
/// just projects that identity onto every connection the moment it opens, via
/// sp_set_session_context, so the predicate functions see it regardless of which query runs.
///
/// Unauthenticated requests (no bearer token, or an invalid/expired one) leave both keys unset,
/// which is the correct fail-closed default the RLS predicates were written against — no session
/// context means no access to anything RLS-gated, not a bypass.</summary>
public class SessionContextConnectionInterceptor(IHttpContextAccessor httpContextAccessor) : DbConnectionInterceptor
{
    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await SetSessionContextAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SetSessionContextAsync(connection, CancellationToken.None).GetAwaiter().GetResult();
        base.ConnectionOpened(connection, eventData);
    }

    private async Task SetSessionContextAsync(DbConnection connection, CancellationToken ct)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var profileIdClaim = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(profileIdClaim, out var profileId))
        {
            return;
        }

        var isSystemAdmin = user.FindFirst(ClaimsNames.IsSystemAdmin)?.Value == "true";

        if (connection is not SqlConnection)
        {
            return;
        }

        await using var command = (SqlCommand)connection.CreateCommand();
        command.CommandText = "EXEC sp_set_session_context @key = N'app.profile_id', @value = @profileId; " +
                               "EXEC sp_set_session_context @key = N'app.is_system_admin', @value = @isSystemAdmin;";
        command.Parameters.Add(new SqlParameter("@profileId", System.Data.SqlDbType.UniqueIdentifier) { Value = profileId });
        command.Parameters.Add(new SqlParameter("@isSystemAdmin", System.Data.SqlDbType.Bit) { Value = isSystemAdmin });

        await command.ExecuteNonQueryAsync(ct);
    }
}
