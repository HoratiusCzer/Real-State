namespace REAK.Api.Services.Security;

/// <summary>A scoped (per-request/per-DbContext) flag SessionContextConnectionInterceptor checks
/// before falling back to the ambient HttpContext-derived identity. Exists because EF Core opens
/// and closes its connection around each individual operation rather than once per request — a
/// single `sp_set_session_context` call made before a multi-query operation gets silently
/// overwritten by the interceptor's own re-stamp on the very next query's ConnectionOpened, back
/// to the calling user's real (non-elevated) identity. Setting IsSystemLevel here instead makes
/// every connection-open within the scope re-apply the elevation, not just the first one — see
/// MatchingEngine, which is this override's only caller (spec §20's "security-sensitive database
/// function" pattern: narrow, explicit, and only for the one operation that genuinely needs to
/// see across every organization).</summary>
public class SessionContextOverride
{
    public bool IsSystemLevel { get; set; }
}
