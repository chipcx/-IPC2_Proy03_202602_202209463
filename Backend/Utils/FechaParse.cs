using System.Globalization;
using System.Text.RegularExpressions;

public static partial class FechaParse
{
    [GeneratedRegex(
        @"(?<!\d)(?<fecha>\d{2}/\d{2}/\d{4})(?:\s+(?<hora>\d{2}:\d{2}))?(?:\s*hrs?\.?)?(?!\d)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PatronFecha();

    public static DateTime? ExtraerFecha(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return null;
        }

        foreach (Match coincidencia in PatronFecha().Matches(texto))
        {
            var formato = coincidencia.Groups["hora"].Success
                ? "dd/MM/yyyy HH:mm"
                : "dd/MM/yyyy";
            var valor = coincidencia.Groups["hora"].Success
                ? $"{coincidencia.Groups["fecha"].Value} {coincidencia.Groups["hora"].Value}"
                : coincidencia.Groups["fecha"].Value;

            if (DateTime.TryParseExact(
                valor,
                formato,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha))
            {
                return fecha;
            }
        }

        return null;
    }
}