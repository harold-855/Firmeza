namespace Firmeza.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string bodyHtml,
        byte[]? attachmentBytes = null,
        string? attachmentFileName = null,
        CancellationToken cancellationToken = default);

    Task<bool> SendWelcomeEmailAsync(
        string toEmail,
        string nombreCliente,
        CancellationToken cancellationToken = default);

    Task<bool> SendPurchaseConfirmationEmailAsync(
        string toEmail,
        string nombreCliente,
        Guid ventaId,
        decimal total,
        string estadoDespacho,
        byte[]? reciboPdf = null,
        CancellationToken cancellationToken = default);
}
