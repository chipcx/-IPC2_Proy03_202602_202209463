using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Contribuyentes;

public sealed class CrearModel(ApiClient apiClient) : PageModel
{
    [BindProperty] public string Nit { get; set; } = string.Empty;
    [BindProperty] public string Nombre { get; set; } = string.Empty;
    public RespuestaApi Respuesta { get; private set; } = new();

    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        Respuesta = await apiClient.CrearAsync(
            new Contribuyente { Nit = Nit, Nombre = Nombre },
            cancellationToken);
    }
}
