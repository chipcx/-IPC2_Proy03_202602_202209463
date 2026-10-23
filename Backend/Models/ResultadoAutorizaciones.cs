using System.Text.Json.Serialization;

/// <summary>Respuesta completa del procesamiento de un lote de facturas.</summary>
public sealed class ResultadoAutorizaciones
{
    [JsonPropertyName("resultadoAutorizaciones")]
    public DatosResultadoAutorizaciones Resultado { get; init; } = new();

    [JsonPropertyName("estadisticasProcesamiento")]
    public EstadisticasProcesamiento Estadisticas { get; init; } = new();
}

/// <summary>Conteos de autorización y códigos generados para las facturas aprobadas.</summary>
public sealed class DatosResultadoAutorizaciones
{
    [JsonPropertyName("cantidadAutorizacionesAprobadas")]
    public int CantidadAutorizacionesAprobadas { get; init; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitEmisorInexistente")]
    public int CantidadRechazadasPorNitEmisorInexistente { get; init; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitReceptorInexistente")]
    public int CantidadRechazadasPorNitReceptorInexistente { get; init; }

    [JsonPropertyName("detalleAprobaciones")]
    public List<DetalleAprobacion> DetalleAprobaciones { get; init; } = [];
}

/// <summary>Estadísticas y desglose de errores detectados en el lote.</summary>
public sealed class EstadisticasProcesamiento
{
    [JsonPropertyName("cantidadFacturasRecibidas")]
    public int CantidadFacturasRecibidas { get; init; }

    [JsonPropertyName("cantidadFacturasDescartadasPorFormato")]
    public int CantidadFacturasDescartadasPorFormato { get; init; }

    [JsonPropertyName("cantidadFacturasAprobadas")]
    public int CantidadFacturasAprobadas { get; init; }

    [JsonPropertyName("cantidadFacturasRechazadas")]
    public int CantidadFacturasRechazadas { get; init; }

    [JsonPropertyName("errores")]
    public Dictionary<string, int> Errores { get; init; } = new(StringComparer.Ordinal);

    [JsonPropertyName("cantidadEmisoresDistintos")]
    public int CantidadEmisoresDistintos { get; init; }

    [JsonPropertyName("cantidadReceptoresDistintos")]
    public int CantidadReceptoresDistintos { get; init; }
}