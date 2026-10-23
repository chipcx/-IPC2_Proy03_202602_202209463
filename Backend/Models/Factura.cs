using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>Factura recibida en un lote de autorización.</summary>
public sealed class Factura
{
    [Required]
    [JsonPropertyName("TIEMPO")]
    public string Tiempo { get; init; } = string.Empty;

    [Required]
    [JsonPropertyName("REFERENCIA")]
    public string Referencia { get; init; } = string.Empty;

    [Required]
    [JsonPropertyName("NIT_EMISOR")]
    public string NitEmisor { get; init; } = string.Empty;

    [Required]
    [JsonPropertyName("NIT_RECEPTOR")]
    public string NitReceptor { get; init; } = string.Empty;

    [JsonPropertyName("VALOR")]
    public decimal Valor { get; init; }

    [JsonPropertyName("IVA")]
    public decimal Iva { get; init; }

    [JsonPropertyName("TOTAL")]
    public decimal Total { get; init; }
}