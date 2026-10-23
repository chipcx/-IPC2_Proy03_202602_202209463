using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Contribuyentes;

public sealed class EliminarModel(ApiClient apiClient) : PageModel
{
    [BindProperty(SupportsGet = true)] public string Nit { get; set; } = string.Empty;
    public RespuestaApi Respuesta { get; private set; } = new();

    public async Task OnPostAsync(CancellationToken cancellationToken) =>
        Respuesta = await apiClient.EliminarAsync(Nit, cancellationToken);
}
