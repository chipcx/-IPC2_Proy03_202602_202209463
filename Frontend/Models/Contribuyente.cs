using System.Text.Json.Serialization;

namespace Frontend.Models;

public sealed class Contribuyente
{
    [JsonPropertyName("NIT")]
    public string Nit { get; set; } = string.Empty;

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
}
