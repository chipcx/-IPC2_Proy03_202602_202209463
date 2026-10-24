using System.Text.Json;
using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Regex;

public sealed class ImportarTextoModel(ApiClient apiClient) : PageModel
{
    [BindProperty] public IFormFile? Archivo { get; set; }
    public ResultadoImportacion? Resultado { get; private set; }
    public RespuestaApi Respuesta { get; private set; } = new();

    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        if (Archivo is null)
        {
            Respuesta = new RespuestaApi { Mensaje = "Selecciona un archivo JSON de RTU para importar." };
            return;
        }

        if (!string.Equals(Path.GetExtension(Archivo.FileName), ".json", StringComparison.OrdinalIgnoreCase))
        {
            Respuesta = new RespuestaApi { Mensaje = "Selecciona un archivo con extensión .json." };
            return;
        }

        if (Archivo.Length == 0)
        {
            Respuesta = new RespuestaApi { Mensaje = "El archivo seleccionado está vacío." };
            return;
        }

        using var lector = new StreamReader(Archivo.OpenReadStream(), detectEncodingFromByteOrderMarks: true);
        var contenido = await lector.ReadToEndAsync(cancellationToken);

        Respuesta = await apiClient.ImportarTextoAsync(contenido, cancellationToken);
        if (!Respuesta.Exito)
        {
            return;
        }

        try
        {
            Resultado = JsonSerializer.Deserialize<ResultadoImportacion>(Respuesta.Cuerpo);
            if (Resultado is null)
            {
                Respuesta.Exito = false;
                Respuesta.Mensaje = "La API no devolvió un resumen de importación válido.";
            }
        }
        catch (JsonException)
        {
            Respuesta.Exito = false;
            Respuesta.Mensaje = "La API devolvió un resumen de importación no válido.";
        }
    }
}
