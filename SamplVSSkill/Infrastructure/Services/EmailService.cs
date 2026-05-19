namespace SamplVSSkill.Infrastructure.Services;

// ── Interface ───────────────────────────────────────────────────────────────
public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, CancellationToken ct = default);
}

// ── Development/Logger Implementation ──────────────────────────────────────
/// <summary>
/// Development email service that logs the reset link to the console.
/// Replace with SmtpEmailService or a provider (SendGrid, Mailjet, etc.) in production.
/// </summary>
public class LoggerEmailService : IEmailService
{
    private readonly ILogger<LoggerEmailService> _logger;

    public LoggerEmailService(ILogger<LoggerEmailService> logger) => _logger = logger;

    public Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "========== PASSWORD RESET EMAIL ==========\n" +
            "  To:   {Email}\n" +
            "  User: {UserName}\n" +
            "  Link: {Link}\n" +
            "==========================================",
            toEmail, userName, resetLink);

        return Task.CompletedTask;
    }
}
