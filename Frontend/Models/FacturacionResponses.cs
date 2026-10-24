using System.Text.Json.Serialization;

namespace Frontend.Models;

public sealed class ResultadoAutorizaciones
{
    [JsonPropertyName("resultadoAutorizaciones")]
    public DatosResultadoAutorizaciones Resultado { get; init; } = new();

    [JsonPropertyName("estadisticasProcesamiento")]
    public EstadisticasProcesamiento Estadisticas { get; init; } = new();
}

public sealed class DatosResultadoAutorizaciones
{
    [JsonPropertyName("cantidadAutorizacionesAprobadas")]
    public int CantidadAprobadas { get; init; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitEmisorInexistente")]
    public int RechazadasPorEmisorInexistente { get; init; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitReceptorInexistente")]
    public int RechazadasPorReceptorInexistente { get; init; }

    [JsonPropertyName("detalleAprobaciones")]
    public List<DetalleAprobacion> Detalles { get; init; } = [];
}

public sealed class DetalleAprobacion
{
    [JsonPropertyName("referencia")]
    public string Referencia { get; init; } = string.Empty;

    [JsonPropertyName("codigoAprobacion")]
    public long CodigoAprobacion { get; init; }
}

public sealed class FacturaRegistradaResponse
{
    [JsonPropertyName("fecha")]
    public string Fecha { get; init; } = string.Empty;

    [JsonPropertyName("factura")]
    public FacturaRegistrada Factura { get; init; } = new();

    [JsonPropertyName("aprobada")]
    public bool Aprobada { get; init; }

    [JsonPropertyName("codigoAprobacion")]
    public long? CodigoAprobacion { get; init; }

    [JsonPropertyName("errores")]
    public List<string> Errores { get; init; } = [];
}

public sealed class FacturaRegistrada
{
    [JsonPropertyName("TIEMPO")]
    public string Tiempo { get; init; } = string.Empty;

    [JsonPropertyName("REFERENCIA")]
    public string Referencia { get; init; } = string.Empty;

    [JsonPropertyName("NIT_EMISOR")]
    public string NitEmisor { get; init; } = string.Empty;

    [JsonPropertyName("NIT_RECEPTOR")]
    public string NitReceptor { get; init; } = string.Empty;

    [JsonPropertyName("VALOR")]
    public decimal Valor { get; init; }

    [JsonPropertyName("IVA")]
    public decimal Iva { get; init; }

    [JsonPropertyName("TOTAL")]
    public decimal Total { get; init; }
}

public sealed class EstadisticasProcesamiento
{
    [JsonPropertyName("cantidadFacturasRecibidas")]
    public int Recibidas { get; init; }

    [JsonPropertyName("cantidadFacturasDescartadasPorFormato")]
    public int DescartadasPorFormato { get; init; }

    [JsonPropertyName("cantidadFacturasAprobadas")]
    public int Aprobadas { get; init; }

    [JsonPropertyName("cantidadFacturasRechazadas")]
    public int Rechazadas { get; init; }

    [JsonPropertyName("errores")]
    public Dictionary<string, int> Errores { get; init; } = new(StringComparer.Ordinal);

    [JsonPropertyName("cantidadEmisoresDistintos")]
    public int EmisoresDistintos { get; init; }

    [JsonPropertyName("cantidadReceptoresDistintos")]
    public int ReceptoresDistintos { get; init; }
}

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

public sealed class DetalleConsultaFecha
{
    [JsonPropertyName("fecha")]
    public string Fecha { get; init; } = string.Empty;

    [JsonPropertyName("cantidadFacturasAutorizadas")]
    public int CantidadFacturasAutorizadas { get; init; }

    [JsonPropertyName("montoTotalFacturado")]
    public decimal MontoTotalFacturado { get; init; }
}

public sealed class ResumenIvaResponse
{
    [JsonPropertyName("fecha")]
    public string Fecha { get; init; } = string.Empty;

    [JsonPropertyName("resumenPorNit")]
    public List<DetalleResumenIva> ResumenPorNit { get; init; } = [];
}

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

public sealed class DetalleResumenRango
{
    [JsonPropertyName("fecha")]
    public string Fecha { get; init; } = string.Empty;

    [JsonPropertyName("montoAutorizado")]
    public decimal MontoAutorizado { get; init; }
}

public sealed class ResultadoGraficasResponse
{
    [JsonPropertyName("graficaIva")]
    public DatosGraficaIva GraficaIva { get; init; } = new();

    [JsonPropertyName("graficaRango")]
    public DatosGraficaRango GraficaRango { get; init; } = new();
}

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
