using System.Globalization;
using System.Text.Json;
using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Facturacion;

public sealed class ConsultasModel(ApiClient apiClient) : PageModel
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [BindProperty] public string Operacion { get; set; } = "facturas";
    [BindProperty] public string Nit { get; set; } = string.Empty;
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

    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        Respuesta = Operacion switch
        {
            "facturas" => await apiClient.ConsultarFacturasAsync(
                Nit, Tipo, FechaInicio, FechaFin, cancellationToken),
            "iva" => await apiClient.ResumirIvaAsync(Fecha, cancellationToken),
            "rango" => await apiClient.ResumirRangoAsync(
                FechaInicio, FechaFin, IncluirIva, cancellationToken),
            "graficas" => await apiClient.ObtenerGraficasAsync(
                Fecha, FechaInicio, FechaFin, IncluirIva, cancellationToken),
            _ => new RespuestaApi { Mensaje = "Selecciona un tipo de consulta válido." }
        };

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
}
