namespace Rivo.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string fullName, string verificationToken, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string toEmail, string fullName, string resetToken, CancellationToken cancellationToken = default);
    Task SendReceiptAsync(string toEmail, string customerName, byte[] pdfReceipt, string orderNumber, CancellationToken cancellationToken = default);
}
