using System.Text.Json.Serialization;

namespace Frontend.Models;

public sealed class ResultadoImportacion
{
    [JsonPropertyName("CONTRIBUYENTES_NUEVOS")]
    public int ContribuyentesNuevos { get; set; }

    [JsonPropertyName("CONTRIBUYENTES_ACTUALIZADOS")]
    public int ContribuyentesActualizados { get; set; }

    [JsonPropertyName("NITS_INVALIDOS")]
    public int NitsInvalidos { get; set; }
}
