using System.Text.Json;
using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Contribuyentes;

public sealed class ObtenerModel(ApiClient apiClient) : PageModel
{
    [BindProperty] public string Nit { get; set; } = string.Empty;
    public Contribuyente? Contribuyente { get; private set; }
    public RespuestaApi Respuesta { get; private set; } = new();

    public async Task OnGetAsync(string? nit, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(nit))
        {
            Nit = nit;
            await ConsultarAsync(cancellationToken);
        }
    }

    public async Task OnPostAsync(CancellationToken cancellationToken) =>
        await ConsultarAsync(cancellationToken);

    private async Task ConsultarAsync(CancellationToken cancellationToken)
    {
        Respuesta = await apiClient.ObtenerAsync(Nit, cancellationToken);
        if (!Respuesta.Exito)
        {
            return;
        }

        try
        {
            Contribuyente = JsonSerializer.Deserialize<Contribuyente>(Respuesta.Cuerpo);
            if (Contribuyente is null)
            {
                Respuesta.Exito = false;
                Respuesta.Mensaje = "La API no devolvió un contribuyente válido.";
            }
        }
        catch (JsonException)
        {
            Respuesta.Exito = false;
            Respuesta.Mensaje = "La API devolvió datos de contribuyente no válidos.";
        }
    }
}
