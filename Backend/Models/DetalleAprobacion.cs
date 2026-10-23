using System.Text.Json.Serialization;

/// <summary>Referencia de factura aprobada y su código de autorización.</summary>
public sealed class DetalleAprobacion
{
    [JsonPropertyName("referencia")]
    public string Referencia { get; init; } = string.Empty;

    [JsonPropertyName("codigoAprobacion")]
    public long CodigoAprobacion { get; init; }
}