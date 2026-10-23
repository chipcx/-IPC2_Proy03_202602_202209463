using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Contribuyentes;

public sealed class ListarModel(ApiClient apiClient) : PageModel
{
    public List<Contribuyente> Contribuyentes { get; private set; } = [];
    public RespuestaApi Respuesta { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var resultado = await apiClient.ListarAsync(cancellationToken);
        Respuesta = resultado.Respuesta;
        Contribuyentes = resultado.Contribuyentes;
    }
}
