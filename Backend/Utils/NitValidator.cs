public static class NitValidator
{
    public static bool EsValido(string? nit)
    {
        if (string.IsNullOrWhiteSpace(nit) || nit.Length < 2 || nit.Length > 21)
        {
            return false;
        }

        var digitoVerificador = char.ToUpperInvariant(nit[^1]);
        if (!char.IsAsciiDigit(digitoVerificador) && digitoVerificador != 'K')
        {
            return false;
        }

        var suma = 0;
        var posicion = 2;

        for (var indice = nit.Length - 2; indice >= 0; indice--)
        {
            if (!char.IsAsciiDigit(nit[indice]))
            {
                return false;
            }

            suma += (nit[indice] - '0') * posicion;
            posicion++;
        }

        var resultado = (11 - (suma % 11)) % 11;
        var verificadorEsperado = resultado == 10
            ? 'K'
            : (char)('0' + resultado);

        return digitoVerificador == verificadorEsperado;
    }
}