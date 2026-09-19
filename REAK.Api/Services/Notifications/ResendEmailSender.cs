using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace REAK.Api.Services.Notifications;

/// <summary>Real IEmailSender via Resend's HTTP API (https://resend.com/docs/api-reference/emails/send-email),
/// replacing LoggingEmailSender (spec §34 production-readiness item). Configuration:
/// REAK_RESEND_API_KEY (required — startup fails fast without it, same pattern as REAK_JWT_KEY)
/// and REAK_EMAIL_FROM (optional, defaults to Resend's own unverified-sender testing address,
/// which sends real mail with no domain setup — replace with a real "from" address on a verified
/// domain before sending to real users at any volume, not just for a one-off demo).
///
/// Deliberately does not throw on a failed send (bad key, Resend outage, etc.) — it logs and
/// returns, the same "never let email delivery break the underlying operation" behavior
/// LoggingEmailSender already had. The invitation/reset-token row is already committed by the
/// time this runs, so a failed send doesn't lose anything; it just means the token needs handing
/// to the recipient another way. Bounce/delivery-failure handling beyond this log line is a
/// separate follow-up, not part of this fix.</summary>
public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly string _from;
    private readonly ILogger<ResendEmailSender> _logger;

    public ResendEmailSender(HttpClient http, IConfiguration configuration, ILogger<ResendEmailSender> logger)
    {
        var apiKey = configuration["REAK_RESEND_API_KEY"]
            ?? throw new InvalidOperationException(
                "REAK_RESEND_API_KEY is not configured. Set it as an environment variable or a user-secret before starting the API.");

        http.BaseAddress = new Uri("https://api.resend.com/");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _http = http;
        _from = configuration["REAK_EMAIL_FROM"] ?? "REAK <onboarding@resend.dev>";
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("emails", new
        {
            from = _from,
            to = new[] { toEmail },
            subject,
            text = body,
        }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Resend email send failed ({Status}) to {To}: {Error}", response.StatusCode, toEmail, error);
        }
    }
}
