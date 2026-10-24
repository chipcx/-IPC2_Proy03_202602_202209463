using System.Text.Json.Serialization;

/// <summary>Factura procesada y conservada en el historial de autorizaciones.</summary>
public sealed record FacturaRegistradaResponse(
    [property: JsonPropertyName("fecha")] string Fecha,
    [property: JsonPropertyName("factura")] Factura Factura,
    [property: JsonPropertyName("aprobada")] bool Aprobada,
    [property: JsonPropertyName("codigoAprobacion")] long? CodigoAprobacion,
    [property: JsonPropertyName("errores")] IReadOnlyList<string> Errores);
