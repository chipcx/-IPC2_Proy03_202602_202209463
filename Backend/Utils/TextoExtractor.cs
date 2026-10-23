using System.Text.Json;
using System.Text.RegularExpressions;

public static partial class TextoExtractor
{
    [GeneratedRegex(
        @"NIT:\s*(?<nit>[0-9]{1,20}[0-9Kk])\s+Nombre:\s*(?<nombre>[^;\r\n]+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PatronContribuyente();

    public static List<Contribuyente?> ExtraerContribuyentes(string texto)
    {
        if (!string.IsNullOrWhiteSpace(texto) && texto.TrimStart().StartsWith('['))
        {
            return JsonSerializer.Deserialize<List<Contribuyente?>>(texto)
                ?? throw new JsonException("El contenido JSON debe ser un arreglo de contribuyentes.");
        }

        return PatronContribuyente().Matches(texto ?? string.Empty)
            .Select(coincidencia => (Contribuyente?)new Contribuyente
            {
                Nit = coincidencia.Groups["nit"].Value.Trim().ToUpperInvariant(),
                Nombre = coincidencia.Groups["nombre"].Value.Trim()
            })
            .ToList();
    }
}