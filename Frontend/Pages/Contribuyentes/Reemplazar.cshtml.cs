using System.Text.Json;
using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Contribuyentes;

public sealed class ReemplazarModel(ApiClient apiClient) : PageModel
{
    [BindProperty(SupportsGet = true)] public string Nit { get; set; } = string.Empty;
    [BindProperty] public string Nombre { get; set; } = string.Empty;
    public RespuestaApi Respuesta { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(Nit))
        {
            var respuesta = await apiClient.ObtenerAsync(Nit, cancellationToken);
            if (respuesta.Exito)
            {
                try
                {
                    var contribuyente = JsonSerializer.Deserialize<Contribuyente>(respuesta.Cuerpo);
                    Nombre = contribuyente?.Nombre ?? string.Empty;
                }
                catch (JsonException)
                {
                    Respuesta = new RespuestaApi
                    {
                        Exito = false,
                        Mensaje = "La API devolvió datos de contribuyente no válidos."
                    };
                }
            }
            else
            {
                Respuesta = respuesta;
            }
        }
    }

    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        Respuesta = await apiClient.ReemplazarAsync(
            Nit,
            new Contribuyente { Nit = Nit, Nombre = Nombre },
            cancellationToken);
    }
}
