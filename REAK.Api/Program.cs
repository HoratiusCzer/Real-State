using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using REAK.Api.Data;
using REAK.Api.Services.Audit;
using REAK.Api.Services.Auth;
using REAK.Api.Services.Collaboration;
using REAK.Api.Services.Demands;
using REAK.Api.Services.Listings;
using REAK.Api.Services.Matching;
using REAK.Api.Services.Notifications;
using REAK.Api.Services.Reference;
using REAK.Api.Services.Security;
using REAK.Api.Services.Storage;

var builder = WebApplication.CreateBuilder(args);

// Fail fast rather than silently signing tokens with a guessable/default key (spec §29 —
// no committed secrets, and no insecure bypass). Set via the REAK_JWT_KEY environment variable
// or `dotnet user-secrets set Jwt:Key <value>` for local development.
var jwtKey = builder.Configuration["REAK_JWT_KEY"]
    ?? throw new InvalidOperationException(
        "REAK_JWT_KEY is not configured. Set it as an environment variable (or a user-secret) " +
        "before starting the API — the JWT signing key must never be a committed default.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "REAK.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "REAK.Clients";

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddScoped<SessionContextOverride>();
builder.Services.AddScoped<SessionContextConnectionInterceptor>();
builder.Services.AddDbContext<ReakDbContext>((sp, options) =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .AddInterceptors(sp.GetRequiredService<SessionContextConnectionInterceptor>()));

builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserClaimsFactory, UserClaimsFactory>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<IMembershipApplicationService, MembershipApplicationService>();
// No email provider is configured for this project yet — logs instead of delivering. Swap for a
// real provider (SendGrid/SES/SMTP) here before production (spec §34).
builder.Services.AddScoped<IEmailSender, LoggingEmailSender>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddHostedService<ExpiryScanService>();

builder.Services.AddScoped<IReferenceCodeGenerator, ReferenceCodeGenerator>();
builder.Services.AddScoped<IMatchingEngine, MatchingEngine>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IDemandService, DemandService>();
builder.Services.AddScoped<ICollaborationRequestService, CollaborationRequestService>();
builder.Services.AddScoped<ICollaborationWorkspaceService, CollaborationWorkspaceService>();
// Dev-only local-disk placeholder — swap for real object storage (S3/Azure Blob) before
// production (spec §34), same pattern as LoggingEmailSender.
builder.Services.AddSingleton<IFileStorage, LocalDiskFileStorage>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Without this, the handler silently remaps short claim names ("sub", "email") to long
        // legacy XML-namespace URIs on the way in, which breaks every FindFirstValue(sub) lookup
        // across the API (AuthController, ActiveProfileMiddleware, the RLS session-context
        // interceptor) even though the token was issued with the short names.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

// Rate limiting on the endpoints most attractive to credential-stuffing/enumeration attacks
// (spec §13 hardening) — login, password reset, and the two public unauthenticated write paths
// (membership applications, invitation acceptance). Keyed by client IP, not by account, so it
// can't itself be used to lock a real user out by hammering their email. 429s are cheap and
// expected under abuse; a real user retrying a typo'd password a handful of times in a minute
// never comes close to the limit.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext => RateLimitPartition.GetSlidingWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 4,
            QueueLimit = 0,
        }));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ReakDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");
    await DatabaseSeeder.SeedAsync(context, builder.Configuration, logger);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    // Only meaningful once actually deployed behind HTTPS — HSTS on plain HTTP localhost would
    // just make local dev painful for zero benefit.
    app.UseHsts();
}

app.UseHttpsRedirection();

// A handful of defense-in-depth response headers (spec §13 hardening) — none of these replace a
// real security decision made elsewhere (RLS, permission checks, CORS-by-omission below), they
// just remove a few classes of attack a misconfigured client or an embedding page could otherwise
// exploit. CSP is deliberately narrow: this API serves JSON plus one static image directory, never
// HTML a browser would execute script from, so "default-src 'none'" is correct, not merely strict.
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'");
    await next();
});

// No CORS policy is registered — deliberately, not an oversight. Every browser-originated request
// in this architecture is same-origin: Next.js Server Components call this API server-to-server
// (no browser involved, CORS doesn't apply), and the only browser-JS calls go to Next.js's own
// same-origin proxy routes, never directly here (see e.g. the collaboration file-download proxy).
// Registering a CORS policy with no real cross-origin caller to serve would only widen the attack
// surface for no benefit — ASP.NET Core's default of rejecting cross-origin browser requests when
// no policy exists is the secure choice for this shape of app.

// Serves ONLY the "listing-media" storage subfolder — listing-documents (private) is never
// mounted here, so it has no static URL at all; it's only reachable through
// ListingsController's authenticated, ownership-checked download action (spec §18).
var publicMediaRoot = Path.Combine(LocalDiskFileStorage.ResolveRoot(builder.Configuration, app.Environment), "listing-media");
Directory.CreateDirectory(publicMediaRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(publicMediaRoot),
    RequestPath = "/media",
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<REAK.Api.Services.Security.ActiveProfileMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
