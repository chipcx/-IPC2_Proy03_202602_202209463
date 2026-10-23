using System.Text.Json;
using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Facturacion;

public sealed class ProcesarModel(ApiClient apiClient) : PageModel
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [BindProperty]
    public string JsonFacturas { get; set; } = string.Empty;

    public RespuestaApi Respuesta { get; private set; } = new();
    public ResultadoAutorizaciones? Resultado { get; private set; }

    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(JsonFacturas))
        {
            Respuesta = new RespuestaApi { Mensaje = "Pega un arreglo JSON de facturas para procesar." };
            return;
        }

        try
        {
            using var documento = JsonDocument.Parse(JsonFacturas);
            if (documento.RootElement.ValueKind != JsonValueKind.Array)
            {
                Respuesta = new RespuestaApi { Mensaje = "El contenido debe ser un arreglo JSON de facturas." };
                return;
            }
        }
        catch (JsonException)
        {
            Respuesta = new RespuestaApi { Mensaje = "El JSON no es válido. Revisa la sintaxis del arreglo de facturas." };
            return;
        }

        Respuesta = await apiClient.ProcesarFacturasAsync(JsonFacturas, cancellationToken);
        if (!Respuesta.Exito)
        {
            return;
        }

        try
        {
            Resultado = JsonSerializer.Deserialize<ResultadoAutorizaciones>(Respuesta.Cuerpo, OpcionesJson);
            if (Resultado is null)
            {
                Respuesta.Exito = false;
                Respuesta.Mensaje = "La API no devolvió un resultado de autorización válido.";
            }
        }
        catch (JsonException)
        {
            Respuesta.Exito = false;
            Respuesta.Mensaje = "La API devolvió un resultado de autorización no válido.";
        }
    }
}
