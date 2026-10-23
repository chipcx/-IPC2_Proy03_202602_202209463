using System.Globalization;
using System.Text.Json;
using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Regex;

public sealed class ExtraerFechaModel(ApiClient apiClient) : PageModel
{
    [BindProperty] public string Texto { get; set; } = string.Empty;
    public string Fecha { get; private set; } = string.Empty;
    public RespuestaApi Respuesta { get; private set; } = new();

    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        Respuesta = await apiClient.ExtraerFechaAsync(Texto, cancellationToken);
        if (!Respuesta.Exito)
        {
            return;
        }

        try
        {
            var fecha = JsonSerializer.Deserialize<DateTime>(Respuesta.Cuerpo);
            Fecha = fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            Respuesta.Mensaje = $"Fecha extraída: {Fecha}";
        }
        catch (JsonException)
        {
            Respuesta.Exito = false;
            Respuesta.Mensaje = "La API devolvió una fecha no válida.";
        }
    }
}
