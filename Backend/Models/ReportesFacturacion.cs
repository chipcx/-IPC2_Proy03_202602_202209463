using System.Text.Json.Serialization;

/// <summary>Resumen de facturas autorizadas emitidas o recibidas por un NIT.</summary>
public sealed class ConsultaFacturasResponse
{
    [JsonPropertyName("nit")]
    public string Nit { get; init; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string Tipo { get; init; } = string.Empty;

    [JsonPropertyName("fechaInicio")]
    public string FechaInicio { get; init; } = string.Empty;

    [JsonPropertyName("fechaFin")]
    public string FechaFin { get; init; } = string.Empty;

    [JsonPropertyName("resumenPorFecha")]
    public List<DetalleConsultaFecha> ResumenPorFecha { get; init; } = [];

    [JsonPropertyName("cantidadTotalFacturas")]
    public int CantidadTotalFacturas { get; init; }

    [JsonPropertyName("montoTotalFacturado")]
    public decimal MontoTotalFacturado { get; init; }
}

/// <summary>Totales de facturas autorizadas en una fecha.</summary>
public sealed class DetalleConsultaFecha
{
    [JsonPropertyName("fecha")]
    public string Fecha { get; init; } = string.Empty;

    [JsonPropertyName("cantidadFacturasAutorizadas")]
    public int CantidadFacturasAutorizadas { get; init; }

    [JsonPropertyName("montoTotalFacturado")]
    public decimal MontoTotalFacturado { get; init; }
}

/// <summary>IVA emitido y recibido por contribuyente en una fecha.</summary>
public sealed class ResumenIvaResponse
{
    [JsonPropertyName("fecha")]
    public string Fecha { get; init; } = string.Empty;

    [JsonPropertyName("resumenPorNit")]
    public List<DetalleResumenIva> ResumenPorNit { get; init; } = [];
}

/// <summary>Totales de IVA asociados a un contribuyente.</summary>
public sealed class DetalleResumenIva
{
    [JsonPropertyName("nit")]
    public string Nit { get; init; } = string.Empty;

    [JsonPropertyName("nombre")]
    public string Nombre { get; init; } = string.Empty;

    [JsonPropertyName("ivaEmitido")]
    public decimal IvaEmitido { get; init; }

    [JsonPropertyName("ivaRecibido")]
    public decimal IvaRecibido { get; init; }
}

/// <summary>Monto autorizado resumido por día durante un rango.</summary>
public sealed class ResumenRangoResponse
{
    [JsonPropertyName("fechaInicio")]
    public string FechaInicio { get; init; } = string.Empty;

    [JsonPropertyName("fechaFin")]
    public string FechaFin { get; init; } = string.Empty;

    [JsonPropertyName("incluirIva")]
    public bool IncluirIva { get; init; }

    [JsonPropertyName("resumenPorFecha")]
    public List<DetalleResumenRango> ResumenPorFecha { get; init; } = [];

    [JsonPropertyName("montoTotalRango")]
    public decimal MontoTotalRango { get; init; }
}

/// <summary>Monto autorizado en una fecha del rango solicitado.</summary>
public sealed class DetalleResumenRango
{
    [JsonPropertyName("fecha")]
    public string Fecha { get; init; } = string.Empty;

    [JsonPropertyName("montoAutorizado")]
    public decimal MontoAutorizado { get; init; }
}

/// <summary>Datos agregados para las gráficas de IVA y de montos por rango.</summary>
public sealed class ResultadoGraficasResponse
{
    [JsonPropertyName("graficaIva")]
    public DatosGraficaIva GraficaIva { get; init; } = new();

    [JsonPropertyName("graficaRango")]
    public DatosGraficaRango GraficaRango { get; init; } = new();
}

/// <summary>Series de IVA emitido y recibido agrupadas por NIT.</summary>
public sealed class DatosGraficaIva
{
    [JsonPropertyName("fecha")]
    public string Fecha { get; init; } = string.Empty;

    [JsonPropertyName("labels")]
    public List<string> Labels { get; init; } = [];

    [JsonPropertyName("ivaEmitido")]
    public List<decimal> IvaEmitido { get; init; } = [];

    [JsonPropertyName("ivaRecibido")]
    public List<decimal> IvaRecibido { get; init; } = [];
}

/// <summary>Serie de montos autorizados agrupados por fecha.</summary>
public sealed class DatosGraficaRango
{
    [JsonPropertyName("fechaInicio")]
    public string FechaInicio { get; init; } = string.Empty;

    [JsonPropertyName("fechaFin")]
    public string FechaFin { get; init; } = string.Empty;

    [JsonPropertyName("incluirIva")]
    public bool IncluirIva { get; init; }

    [JsonPropertyName("labels")]
    public List<string> Labels { get; init; } = [];

    [JsonPropertyName("valores")]
    public List<decimal> Valores { get; init; } = [];
}
