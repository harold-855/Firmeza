namespace Firmeza.Application.Validators;

public class ResultadoValidacionEdad
{
    public bool EsValido { get; set; }
    public int? EdadConvertida { get; set; }
    public string? MensajeError { get; set; }
}

public static class ValidadorEdad
{
    /// <summary>
    /// Valida que la edad ingresada sea un número entero utilizando try-catch y int.Parse,
    /// capturando excepciones de formato y desbordamiento, y retornando mensajes amigables.
    /// </summary>
    /// <param name="edadTexto">Texto ingresado por el usuario.</param>
    /// <param name="edadMinima">Edad mínima permitida (por defecto 18).</param>
    /// <param name="edadMaxima">Edad máxima permitida (por defecto 120).</param>
    /// <returns>Resultado con indicador de validez, valor numérico y mensaje de error amigable si aplica.</returns>
    public static ResultadoValidacionEdad Validar(string? edadTexto, int edadMinima = 18, int edadMaxima = 120)
    {
        if (string.IsNullOrWhiteSpace(edadTexto))
        {
            return new ResultadoValidacionEdad
            {
                EsValido = false,
                MensajeError = "El campo 'Edad' es obligatorio. Por favor ingresa tu edad."
            };
        }

        try
        {
            // Intentar convertir la cadena de texto a un entero usando int.Parse
            int edad = int.Parse(edadTexto.Trim());

            // Validar rango de edad lógica para el negocio
            if (edad < edadMinima)
            {
                return new ResultadoValidacionEdad
                {
                    EsValido = false,
                    MensajeError = $"Debes ser mayor de edad para continuar (edad mínima permitida: {edadMinima} años)."
                };
            }

            if (edad > edadMaxima)
            {
                return new ResultadoValidacionEdad
                {
                    EsValido = false,
                    MensajeError = $"La edad ingresada no es válida (edad máxima permitida: {edadMaxima} años)."
                };
            }

            return new ResultadoValidacionEdad
            {
                EsValido = true,
                EdadConvertida = edad
            };
        }
        catch (FormatException)
        {
            // Excepción producida cuando la cadena contiene caracteres no numéricos (letras, símbolos, decimales)
            return new ResultadoValidacionEdad
            {
                EsValido = false,
                MensajeError = "La edad ingresada debe ser un número entero válido (por ejemplo: 25). No se permiten letras, espacios intermedios ni caracteres especiales."
            };
        }
        catch (OverflowException)
        {
            // Excepción producida cuando el número ingresado excede el límite de un entero de 32 bits
            return new ResultadoValidacionEdad
            {
                EsValido = false,
                MensajeError = "El número de edad ingresado es excesivamente grande. Por favor ingresa un valor real."
            };
        }
        catch (Exception ex)
        {
            // Captura de cualquier otra excepción imprevista durante la conversión
            return new ResultadoValidacionEdad
            {
                EsValido = false,
                MensajeError = $"Ocurrió un error inesperado al procesar la edad: {ex.Message}"
            };
        }
    }
}
