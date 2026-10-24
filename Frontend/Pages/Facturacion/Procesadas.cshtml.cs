using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Facturacion;

public sealed class ProcesadasModel(ApiClient apiClient) : PageModel
{
    private static readonly (string Codigo, string Descripcion)[] MotivosRechazo =
    [
        ("nitEmisorInvalido", "NIT del emisor inválido"),
        ("nitEmisorInexistente", "NIT del emisor no registrado"),
        ("nitReceptorInvalido", "NIT del receptor inválido"),
        ("nitReceptorInexistente", "NIT del receptor no registrado"),
        ("referenciaInvalida", "Referencia inválida"),
        ("referenciaDuplicada", "Referencia duplicada"),
        ("valorInvalido", "Valor inválido"),
        ("ivaMalCalculado", "IVA mal calculado"),
        ("totalMalCalculado", "Total mal calculado")
    ];

    public RespuestaApi Respuesta { get; private set; } = new();
    public IReadOnlyList<FacturaRegistradaResponse> Facturas { get; private set; } = [];
    public int Aprobadas => Facturas.Count(factura => factura.Aprobada);
    public int Rechazadas => Facturas.Count - Aprobadas;
    public IReadOnlyList<KeyValuePair<string, int>> ConteoRechazos =>
        MotivosRechazo
            .Select(motivo => new KeyValuePair<string, int>(
                motivo.Descripcion,
                Facturas.Sum(factura => factura.Errores.Count(error =>
                    string.Equals(error, motivo.Codigo, StringComparison.Ordinal)))))
            .ToList();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var resultado = await apiClient.ListarFacturasProcesadasAsync(cancellationToken);
        Respuesta = resultado.Respuesta;
        Facturas = resultado.Facturas;
    }
}
