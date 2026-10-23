using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public sealed class InicializarModel(ApiClient apiClient) : PageModel
{
    public RespuestaApi Respuesta { get; private set; } = new();

    public async Task OnPostAsync(CancellationToken cancellationToken) =>
        Respuesta = await apiClient.InicializarAsync(cancellationToken);
}
