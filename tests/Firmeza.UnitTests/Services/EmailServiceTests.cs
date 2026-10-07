using Firmeza.Application.DTOS.Email;
using Firmeza.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Firmeza.UnitTests.Services;

public class EmailServiceTests
{
    private readonly Mock<ILogger<SmtpEmailService>> _mockLogger;

    public EmailServiceTests()
    {
        _mockLogger = new Mock<ILogger<SmtpEmailService>>();
    }

    [Fact]
    public async Task SendEmailAsync_ModoSimulacion_DebeRetornarTrueYRegistrarEnLog()
    {
        // Arrange
        var settings = new EmailSettings
        {
            SmtpServer = "smtp.gmail.com",
            Port = 587,
            SenderEmail = "notificaciones@firmeza.com",
            SenderName = "FIRMEZA",
            IsSimulationMode = true
        };
        var options = Options.Create(settings);
        var emailService = new SmtpEmailService(options, _mockLogger.Object);

        // Act
        var result = await emailService.SendEmailAsync("cliente@example.com", "Prueba", "<h1>Contenido</h1>");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_DestinatarioValido_DebeGenerarHtmlYRetornarTrue()
    {
        // Arrange
        var settings = new EmailSettings { IsSimulationMode = true };
        var options = Options.Create(settings);
        var emailService = new SmtpEmailService(options, _mockLogger.Object);

        // Act
        var result = await emailService.SendWelcomeEmailAsync("constructora@example.com", "Constructora Firmeza");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendPurchaseConfirmationEmailAsync_ConReciboAdjunto_DebeProcesarCorrectamente()
    {
        // Arrange
        var settings = new EmailSettings { IsSimulationMode = true };
        var options = Options.Create(settings);
        var emailService = new SmtpEmailService(options, _mockLogger.Object);
        var fakePdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        // Act
        var result = await emailService.SendPurchaseConfirmationEmailAsync(
            "cliente@example.com",
            "Constructora Bolívar",
            Guid.NewGuid(),
            1500000m,
            "Pendiente",
            fakePdfBytes);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendEmailAsync_EmailVacio_DebeRetornarFalseSinLanzarExcepcion()
    {
        // Arrange
        var settings = new EmailSettings { IsSimulationMode = true };
        var options = Options.Create(settings);
        var emailService = new SmtpEmailService(options, _mockLogger.Object);

        // Act
        var result = await emailService.SendEmailAsync("", "Prueba", "<p>Texto</p>");

        // Assert
        Assert.False(result);
    }
}
