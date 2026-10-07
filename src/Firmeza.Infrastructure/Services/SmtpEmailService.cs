using System.Globalization;
using System.Net;
using System.Net.Mail;
using Firmeza.Application.DTOS.Email;
using Firmeza.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Firmeza.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> options, ILogger<SmtpEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string bodyHtml,
        byte[]? attachmentBytes = null,
        string? attachmentFileName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Intento de envío de correo omitido: La dirección de destino está vacía.");
            return false;
        }

        // Si estamos en modo simulación o no se han configurado credenciales reales, registramos en log de auditoría
        if (_settings.IsSimulationMode || string.IsNullOrWhiteSpace(_settings.Username) || string.IsNullOrWhiteSpace(_settings.Password))
        {
            _logger.LogInformation(
                "[SIMULACIÓN SMTP] Correo procesado. Para: {ToEmail} | Asunto: {Subject} | Adjunto: {HasAttachment}",
                toEmail, subject, attachmentBytes != null);
            return true;
        }

        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_settings.SenderEmail, _settings.SenderName);
            message.To.Add(new MailAddress(toEmail.Trim()));
            message.Subject = subject;
            message.Body = bodyHtml;
            message.IsBodyHtml = true;

            if (attachmentBytes != null && attachmentBytes.Length > 0 && !string.IsNullOrWhiteSpace(attachmentFileName))
            {
                var stream = new MemoryStream(attachmentBytes);
                message.Attachments.Add(new Attachment(stream, attachmentFileName, "application/pdf"));
            }

            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Username.Trim(), _settings.Password.Trim()),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Correo enviado exitosamente a {ToEmail} con asunto: '{Subject}'", toEmail, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo electrónico vía SMTP a {ToEmail}. Asunto: '{Subject}'", toEmail, subject);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(
        string toEmail,
        string nombreCliente,
        CancellationToken cancellationToken = default)
    {
        var subject = "¡Bienvenido a FIRMEZA - Sistema de Gestión de Materiales!";
        var body = $$"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
                <meta charset="UTF-8">
                <style>
                    body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f1f5f9; margin: 0; padding: 20px; }
                    .container { max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); }
                    .header { background-color: #0f172a; padding: 30px 20px; text-align: center; color: #ffffff; }
                    .header h1 { margin: 0; font-size: 24px; color: #3b82f6; }
                    .content { padding: 30px; color: #334155; line-height: 1.6; }
                    .badge { display: inline-block; background-color: #dbeafe; color: #1e40af; font-weight: 600; padding: 4px 12px; border-radius: 9999px; font-size: 13px; margin-bottom: 15px; }
                    .button { display: inline-block; background-color: #2563eb; color: #ffffff; text-decoration: none; padding: 12px 24px; border-radius: 8px; font-weight: 600; margin-top: 20px; }
                    .footer { background-color: #f8fafc; padding: 20px; text-align: center; color: #64748b; font-size: 12px; border-top: 1px solid #e2e8f0; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>🏗️ FIRMEZA</h1>
                        <p style="margin: 5px 0 0 0; color: #94a3b8; font-size: 14px;">Materiales de Construcción y Soluciones Industriales</p>
                    </div>
                    <div class="content">
                        <span class="badge">Registro Exitoso</span>
                        <h2 style="color: #0f172a; margin-top: 0;">¡Hola, {{WebUtility.HtmlEncode(nombreCliente)}}!</h2>
                        <p>Nos complace darte la bienvenida a <strong>FIRMEZA</strong>. Tu cuenta ha sido registrada satisfactoriamente en nuestra plataforma.</p>
                        <p>A partir de este momento puedes:</p>
                        <ul>
                            <li>Consultar nuestro catálogo actualizado de materiales pesados y de construcción.</li>
                            <li>Generar y cotizar órdenes de compra en tiempo real.</li>
                            <li>Hacer seguimiento en tiempo real al estado de entrega y despacho de tus pedidos.</li>
                            <li>Descargar comprobantes y recibos oficiales en formato PDF.</li>
                        </ul>
                        <p style="text-align: center;">
                            <a href="http://localhost:5281" class="button" style="color: #ffffff;">Ingresar a la Plataforma</a>
                        </p>
                    </div>
                    <div class="footer">
                        <p>FIRMEZA S.A.S. - NIT: 900.123.456-7<br>PBX: +57 (601) 745-9000 | soporte@firmeza.com</p>
                        <p>Este es un correo automático generado por el sistema. Por favor no responda a este mensaje.</p>
                    </div>
                </div>
            </body>
            </html>
            """;

        return await SendEmailAsync(toEmail, subject, body, null, null, cancellationToken);
    }

    public async Task<bool> SendPurchaseConfirmationEmailAsync(
        string toEmail,
        string nombreCliente,
        Guid ventaId,
        decimal total,
        string estadoDespacho,
        byte[]? reciboPdf = null,
        CancellationToken cancellationToken = default)
    {
        var colCulture = new CultureInfo("es-CO");
        var totalFormateado = total.ToString("C0", colCulture);
        var subject = $"Confirmación de Compra - Orden #{ventaId.ToString()[..8].ToUpper()} - FIRMEZA";

        var body = $$"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
                <meta charset="UTF-8">
                <style>
                    body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f1f5f9; margin: 0; padding: 20px; }
                    .container { max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); }
                    .header { background-color: #0f172a; padding: 25px 20px; text-align: center; color: #ffffff; }
                    .header h1 { margin: 0; font-size: 24px; color: #3b82f6; }
                    .content { padding: 30px; color: #334155; line-height: 1.6; }
                    .card { background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; margin: 20px 0; }
                    .total-box { background-color: #eff6ff; border-left: 4px solid #2563eb; padding: 15px; margin: 15px 0; }
                    .status-badge { display: inline-block; background-color: #dcfce7; color: #15803d; font-weight: bold; padding: 4px 10px; border-radius: 6px; font-size: 13px; }
                    .footer { background-color: #f8fafc; padding: 20px; text-align: center; color: #64748b; font-size: 12px; border-top: 1px solid #e2e8f0; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>🏗️ FIRMEZA</h1>
                        <p style="margin: 5px 0 0 0; color: #94a3b8; font-size: 14px;">Confirmación Oficial de Orden de Despacho</p>
                    </div>
                    <div class="content">
                        <h2 style="color: #0f172a; margin-top: 0;">¡Gracias por tu compra, {{WebUtility.HtmlEncode(nombreCliente)}}!</h2>
                        <p>Hemos procesado tu orden de compra satisfactoriamente en el sistema.</p>
                        
                        <div class="card">
                            <h3 style="margin-top: 0; color: #1e293b; font-size: 16px;">Resumen de la Transacción</h3>
                            <p style="margin: 6px 0;"><strong>Identificador de Orden:</strong> <code>{{ventaId}}</code></p>
                            <p style="margin: 6px 0;"><strong>Fecha y Hora:</strong> {{DateTime.UtcNow:dd/MM/yyyy HH:mm}} UTC</p>
                            <p style="margin: 6px 0;"><strong>Estado de Entrega:</strong> <span class="status-badge">{{WebUtility.HtmlEncode(estadoDespacho)}}</span></p>
                            <div class="total-box">
                                <span style="font-size: 13px; color: #64748b;">Monto Total Facturado:</span>
                                <h3 style="margin: 4px 0 0 0; color: #1e40af; font-size: 22px;">{{totalFormateado}} COP</h3>
                            </div>
                        </div>

                        <p>Adjunto a este correo encontrarás el comprobante y recibo oficial de entrega en formato <strong>PDF</strong> con el desglose detallado de materiales y liquidación de impuestos.</p>
                    </div>
                    <div class="footer">
                        <p>FIRMEZA S.A.S. - NIT: 900.123.456-7<br>Despachos y Logística de Materiales | PBX: +57 (601) 745-9000</p>
                        <p>Este es un comprobante digital generado automáticamente.</p>
                    </div>
                </div>
            </body>
            </html>
            """;

        var fileName = $"recibo_venta_{ventaId.ToString()[..8]}.pdf";
        return await SendEmailAsync(toEmail, subject, body, reciboPdf, fileName, cancellationToken);
    }
}
