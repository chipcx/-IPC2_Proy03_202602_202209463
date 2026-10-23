using System.Text.Json.Serialization;

public sealed class FacturaPersistida
{
    [JsonPropertyName("factura")]
    public Factura Factura { get; init; } = new();

    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; init; }

    [JsonPropertyName("codigoAprobacion")]
    public long? CodigoAprobacion { get; init; }

    [JsonPropertyName("errores")]
    public List<string> Errores { get; init; } = [];
}
