using System.Text.Json.Serialization;

public sealed class ResultadoImportacion
{
    [JsonPropertyName("CONTRIBUYENTES_NUEVOS")]
    public int ContribuyentesNuevos { get; init; }

    [JsonPropertyName("CONTRIBUYENTES_ACTUALIZADOS")]
    public int ContribuyentesActualizados { get; init; }

    [JsonPropertyName("NITS_INVALIDOS")]
    public int NitsInvalidos { get; init; }
}