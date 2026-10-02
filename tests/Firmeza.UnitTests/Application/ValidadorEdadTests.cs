using Firmeza.Application.Validators;
using Xunit;

namespace Firmeza.UnitTests.Application;

public class ValidadorEdadTests
{
    [Theory]
    [InlineData("18", 18)]
    [InlineData("25", 25)]
    [InlineData("65", 65)]
    public void ValidarEdad_CadenaNumericaValida_DebeRetornarResultadoExitoso(string input, int edadEsperada)
    {
        // Act
        var resultado = ValidadorEdad.Validar(input);

        // Assert
        Assert.True(resultado.EsValido);
        Assert.Equal(edadEsperada, resultado.EdadConvertida);
        Assert.Null(resultado.MensajeError);
    }

    [Theory]
    [InlineData("veinticinco")]
    [InlineData("abc")]
    [InlineData("12.5")]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidarEdad_TextoNoNumerico_DebeCapturarFormatExceptionYRetornarError(string input)
    {
        // Act
        var resultado = ValidadorEdad.Validar(input);

        // Assert
        Assert.False(resultado.EsValido);
        Assert.Null(resultado.EdadConvertida);
        Assert.NotNull(resultado.MensajeError);
    }

    [Theory]
    [InlineData("99999999999999999999999")]
    [InlineData("-99999999999999999999999")]
    public void ValidarEdad_NumeroFueraDeRangoInt32_DebeCapturarOverflowException(string input)
    {
        // Act
        var resultado = ValidadorEdad.Validar(input);

        // Assert
        Assert.False(resultado.EsValido);
        Assert.Null(resultado.EdadConvertida);
        Assert.Contains("excesivamente", resultado.MensajeError, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("17")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("125")]
    public void ValidarEdad_EdadFueraDeRangoLaboral_DebeRetornarErrorDeReglaDeNegocio(string input)
    {
        // Act
        var resultado = ValidadorEdad.Validar(input);

        // Assert
        Assert.False(resultado.EsValido);
        Assert.NotNull(resultado.MensajeError);
    }
}
