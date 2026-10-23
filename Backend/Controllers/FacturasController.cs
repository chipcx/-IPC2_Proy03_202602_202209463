using System.Globalization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api")]
public sealed class FacturasController : ControllerBase
{
    private static readonly string[] FormatosFecha = ["dd/MM/yyyy", "yyyy-MM-dd"];
    private readonly FacturacionService _servicio;

    public FacturasController(FacturacionService servicio)
    {
        _servicio = servicio;
    }

    /// <summary>Valida, autoriza y persiste un lote de facturas.</summary>
    [HttpPost("Procesar")]
    [ProducesResponseType(typeof(ResultadoAutorizaciones), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Procesar([FromBody] List<Factura?>? facturas)
    {
        try
        {
            return Ok(_servicio.Procesar(facturas));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }

    /// <summary>Consulta facturas autorizadas emitidas o recibidas por un NIT en un rango.</summary>
    [HttpGet("ConsultaDatos")]
    [ProducesResponseType(typeof(ConsultaFacturasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Consultar(
        [FromQuery] string? nit,
        [FromQuery] string? tipo,
        [FromQuery] string? fechaInicio,
        [FromQuery] string? fechaFin)
    {
        if (string.IsNullOrWhiteSpace(nit) || !NitValidator.EsValido(NormalizarNit(nit)))
        {
            return BadRequest(new { mensaje = "El NIT consultado no es válido." });
        }

        if (!string.Equals(tipo, "emitidas", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(tipo, "recibidas", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { mensaje = "El tipo debe ser 'emitidas' o 'recibidas'." });
        }

        if (!IntentarRango(fechaInicio, fechaFin, out var inicio, out var fin, out var error))
        {
            return BadRequest(new { mensaje = error });
        }

        var tipoConsulta = string.Equals(tipo, "emitidas", StringComparison.OrdinalIgnoreCase)
            ? "emitidas"
            : "recibidas";
        return Ok(_servicio.Consultar(nit, tipoConsulta, inicio, fin));
    }

    /// <summary>Resume el IVA emitido y recibido por NIT en una fecha.</summary>
    [HttpGet("ResumenIva")]
    [ProducesResponseType(typeof(ResumenIvaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ResumenIva([FromQuery] string? fecha)
    {
        if (!IntentarFecha(fecha, out var fechaConsulta))
        {
            return BadRequest(new { mensaje = "La fecha debe tener formato dd/MM/yyyy o yyyy-MM-dd." });
        }

        return Ok(_servicio.ResumirIva(fechaConsulta));
    }

    /// <summary>Resume los montos autorizados por día dentro de un rango.</summary>
    [HttpGet("ResumenRango")]
    [ProducesResponseType(typeof(ResumenRangoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ResumenRango(
        [FromQuery] string? fechaInicio,
        [FromQuery] string? fechaFin,
        [FromQuery] bool incluirIva = true)
    {
        if (!IntentarRango(fechaInicio, fechaFin, out var inicio, out var fin, out var error))
        {
            return BadRequest(new { mensaje = error });
        }

        return Ok(_servicio.ResumirRango(inicio, fin, incluirIva));
    }

    /// <summary>Devuelve las series de datos usadas por las gráficas de IVA y montos.</summary>
    [HttpGet("Grafica")]
    [ProducesResponseType(typeof(ResultadoGraficasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Grafica(
        [FromQuery] string? fecha,
        [FromQuery] string? fechaInicio,
        [FromQuery] string? fechaFin,
        [FromQuery] bool incluirIva = true)
    {
        if (!IntentarFecha(fecha, out var fechaConsulta))
        {
            return BadRequest(new { mensaje = "La fecha debe tener formato dd/MM/yyyy o yyyy-MM-dd." });
        }

        if (!IntentarRango(fechaInicio, fechaFin, out var inicio, out var fin, out var error))
        {
            return BadRequest(new { mensaje = error });
        }

        return Ok(_servicio.ObtenerDatosGraficas(fechaConsulta, inicio, fin, incluirIva));
    }

    private static bool IntentarRango(
        string? fechaInicio,
        string? fechaFin,
        out DateTime inicio,
        out DateTime fin,
        out string error)
    {
        inicio = default;
        fin = default;
        if (!IntentarFecha(fechaInicio, out inicio) || !IntentarFecha(fechaFin, out fin))
        {
            error = "Las fechas deben tener formato dd/MM/yyyy o yyyy-MM-dd.";
            return false;
        }

        if (inicio.Date > fin.Date)
        {
            error = "La fecha inicial no puede ser posterior a la fecha final.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool IntentarFecha(string? valor, out DateTime fecha) =>
        DateTime.TryParseExact(
            valor,
            FormatosFecha,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out fecha);

    private static string NormalizarNit(string nit) =>
        nit.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
}
