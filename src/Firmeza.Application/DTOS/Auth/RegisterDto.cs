using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Firmeza.Application.DTOS.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La edad es obligatoria.")]
    [Display(Name = "Edad")]
    [JsonConverter(typeof(StringOrNumberConverter))]
    public string Edad { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    [Compare("Password", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? Role { get; set; }
}

public class StringOrNumberConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out long l))
                return l.ToString();
            if (reader.TryGetDouble(out double d))
                return d.ToString();
        }
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        using var doc = JsonDocument.ParseValue(ref reader);
        return doc.RootElement.GetRawText();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

