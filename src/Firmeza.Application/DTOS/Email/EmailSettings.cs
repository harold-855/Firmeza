namespace Firmeza.Application.DTOS.Email;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SenderName { get; set; } = "FIRMEZA - Materiales de Construcción";
    public string SenderEmail { get; set; } = "notificaciones@firmeza.com";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool IsSimulationMode { get; set; } = false;
}
