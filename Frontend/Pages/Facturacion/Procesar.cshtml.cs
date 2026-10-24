using System.Text.Json;
using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Facturacion;

public sealed class ProcesarModel(ApiClient apiClient) : PageModel
{
    [BindProperty]
    public IFormFile? Archivo { get; set; }

    public RespuestaApi Respuesta { get; private set; } = new();

    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        if (Archivo is null)
        {
            Respuesta = new RespuestaApi { Mensaje = "Selecciona un archivo JSON de facturas para procesar." };
            return;
        }

        if (!string.Equals(Path.GetExtension(Archivo.FileName), ".json", StringComparison.OrdinalIgnoreCase))
        {
            Respuesta = new RespuestaApi { Mensaje = "Selecciona un archivo de facturas con extensión .json." };
            return;
        }

        if (Archivo.Length == 0)
        {
            Respuesta = new RespuestaApi { Mensaje = "El archivo seleccionado está vacío." };
            return;
        }

        using var lector = new StreamReader(Archivo.OpenReadStream(), detectEncodingFromByteOrderMarks: true);
        var contenido = await lector.ReadToEndAsync(cancellationToken);

        try
        {
            using var documento = JsonDocument.Parse(contenido);
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

        Respuesta = await apiClient.ProcesarFacturasAsync(contenido, cancellationToken);
        if (Respuesta.Exito)
        {
            Respuesta.Mensaje = "Archivo procesado. Las facturas y su resultado quedaron guardados; consúltalos en Facturas procesadas.";
        }
    }
}
