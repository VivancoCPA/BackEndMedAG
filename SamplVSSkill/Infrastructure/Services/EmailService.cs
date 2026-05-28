namespace SamplVSSkill.Infrastructure.Services;

// ── Interface ───────────────────────────────────────────────────────────────
public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, CancellationToken ct = default);
    Task SendTemporaryPasswordEmailAsync(string toEmail, string userName, string temporaryPassword, CancellationToken ct = default);
    Task SendForgotPasswordTemporaryPasswordEmailAsync(string toEmail, string userName, string temporaryPassword, CancellationToken ct = default);
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

    public Task SendTemporaryPasswordEmailAsync(string toEmail, string userName, string temporaryPassword, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "========== TEMPORARY PASSWORD EMAIL ==========\n" +
            "  To:       {Email}\n" +
            "  User:     {UserName}\n" +
            "  TempPass: {TemporaryPassword}\n" +
            "  Message:  Su cuenta ha sido creada. Por favor inicie sesión y cambie su contraseña.\n" +
            "==============================================",
            toEmail, userName, temporaryPassword);

        return Task.CompletedTask;
    }

    public Task SendForgotPasswordTemporaryPasswordEmailAsync(string toEmail, string userName, string temporaryPassword, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "========== FORGOT PASSWORD TEMPORARY PASSWORD EMAIL ==========\n" +
            "  To:       {Email}\n" +
            "  User:     {UserName}\n" +
            "  TempPass: {TemporaryPassword}\n" +
            "  Message:  Se ha restablecido su contraseña con un valor temporal. Por favor inicie sesión y cámbiela.\n" +
            "==============================================================",
            toEmail, userName, temporaryPassword);

        return Task.CompletedTask;
    }
}
