using System.Globalization;
using System.Text.Json;
using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Facturacion;

public sealed class ConsultasModel(ApiClient apiClient) : PageModel
{
    private const string FormatoFechaRegistro = "dd/MM/yyyy HH:mm";
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public List<KeyValuePair<string, string>> FechasConFacturasAutorizadas { get; private set; } = [];

    [BindProperty] public string Operacion { get; set; } = "facturas";
    [BindProperty] public string? Nit { get; set; } = string.Empty;
    [BindProperty] public string Tipo { get; set; } = "emitidas";
    [BindProperty] public string Fecha { get; set; } = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    [BindProperty] public string FechaInicio { get; set; } = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    [BindProperty] public string FechaFin { get; set; } = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    [BindProperty] public bool IncluirIva { get; set; } = true;

    public RespuestaApi Respuesta { get; private set; } = new();
    public ConsultaFacturasResponse? Consulta { get; private set; }
    public ResumenIvaResponse? ResumenIva { get; private set; }
    public ResumenRangoResponse? ResumenRango { get; private set; }
    public ResultadoGraficasResponse? Graficas { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await CargarFechasDisponiblesAsync(cancellationToken);
        if (FechasConFacturasAutorizadas.Count == 0)
        {
            return;
        }

        Fecha = FechasConFacturasAutorizadas[0].Key;
        FechaInicio = FechasConFacturasAutorizadas[0].Key;
        FechaFin = FechasConFacturasAutorizadas[^1].Key;
    }

    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        Respuesta = Operacion switch
        {
            "facturas" => await apiClient.ConsultarFacturasAsync(
                Nit ?? string.Empty, Tipo, FechaInicio, FechaFin, cancellationToken),
            "iva" => await apiClient.ResumirIvaAsync(Fecha, cancellationToken),
            "rango" => await apiClient.ResumirRangoAsync(
                FechaInicio, FechaFin, IncluirIva, cancellationToken),
            "graficas" => await apiClient.ObtenerGraficasAsync(
                Fecha, FechaInicio, FechaFin, IncluirIva, cancellationToken),
            _ => new RespuestaApi { Mensaje = "Selecciona un tipo de consulta válido." }
        };

        await CargarFechasDisponiblesAsync(cancellationToken);
        if (!Respuesta.Exito)
        {
            return;
        }

        try
        {
            switch (Operacion)
            {
                case "facturas":
                    Consulta = Deserializar<ConsultaFacturasResponse>();
                    break;
                case "iva":
                    ResumenIva = Deserializar<ResumenIvaResponse>();
                    break;
                case "rango":
                    ResumenRango = Deserializar<ResumenRangoResponse>();
                    break;
                case "graficas":
                    Graficas = Deserializar<ResultadoGraficasResponse>();
                    break;
            }

            if (Consulta is null && ResumenIva is null && ResumenRango is null && Graficas is null)
            {
                Respuesta.Exito = false;
                Respuesta.Mensaje = "La API no devolvió el formato esperado para la consulta.";
            }
        }
        catch (JsonException)
        {
            Respuesta.Exito = false;
            Respuesta.Mensaje = "La API devolvió datos no válidos para la consulta.";
        }
    }

    public double Porcentaje(decimal valor, decimal maximo) =>
        maximo <= 0 ? 0 : Math.Clamp((double)(valor / maximo * 100), 0, 100);

    private T? Deserializar<T>() =>
        JsonSerializer.Deserialize<T>(Respuesta.Cuerpo, OpcionesJson);

    private async Task CargarFechasDisponiblesAsync(CancellationToken cancellationToken)
    {
        var (respuesta, facturas) = await apiClient.ListarFacturasProcesadasAsync(cancellationToken);
        if (!respuesta.Exito)
        {
            FechasConFacturasAutorizadas = [];
            return;
        }

        FechasConFacturasAutorizadas = facturas
            .Where(factura => factura.Aprobada)
            .Select(factura =>
                DateTime.TryParseExact(
                    factura.Fecha,
                    FormatoFechaRegistro,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var fecha)
                    ? fecha.Date
                    : (DateTime?)null)
            .Where(fecha => fecha.HasValue)
            .Select(fecha => fecha!.Value)
            .Distinct()
            .Order()
            .Select(fecha => new KeyValuePair<string, string>(
                fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)))
            .ToList();
    }
}
