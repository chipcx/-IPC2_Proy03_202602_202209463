using System.Text.Json;
using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Regex;

public sealed class ImportarTextoModel(ApiClient apiClient) : PageModel
{
    [BindProperty] public string Texto { get; set; } = string.Empty;
    public ResultadoImportacion? Resultado { get; private set; }
    public RespuestaApi Respuesta { get; private set; } = new();

    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        Respuesta = await apiClient.ImportarTextoAsync(Texto, cancellationToken);
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
