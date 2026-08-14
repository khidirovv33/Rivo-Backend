using Microsoft.Extensions.Logging;
using Rivo.Application.Common.Interfaces;

namespace Rivo.Infrastructure.ExternalServices;

/// <summary>
/// Logs the email instead of sending it. No SMTP/provider has been chosen yet (out of Dev1's scope) —
/// swap this implementation for a real provider (SendGrid/SMTP/etc.) behind the same IEmailService contract.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(string toEmail, string fullName, string verificationToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[Email:Verification] To={ToEmail} Name={FullName} Token={Token}", toEmail, fullName, verificationToken);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string fullName, string resetToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[Email:PasswordReset] To={ToEmail} Name={FullName} Token={Token}", toEmail, fullName, resetToken);
        return Task.CompletedTask;
    }

    public Task SendReceiptAsync(string toEmail, string customerName, byte[] pdfReceipt, string orderNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[Email:Receipt] To={ToEmail} Customer={CustomerName} Order={OrderNumber} SizeBytes={Size}",
            toEmail, customerName, orderNumber, pdfReceipt.Length);
        return Task.CompletedTask;
    }
}
