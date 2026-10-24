using System.Text.Json;
using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public sealed class HistorialModel(HistorialPeticionesService historial) : PageModel
{
    private static readonly JsonSerializerOptions OpcionesJson = new() { WriteIndented = true };

    public IReadOnlyList<PeticionHistorial> Peticiones { get; private set; } = [];
    public int Exitosas => Peticiones.Count(peticion => peticion.Exito);
    public int Fallidas => Peticiones.Count - Exitosas;

    public void OnGet()
    {
        Peticiones = historial.Listar();
    }

    public string FormatearCuerpo(string cuerpo)
    {
        if (string.IsNullOrWhiteSpace(cuerpo))
        {
            return "(sin contenido)";
        }

        try
        {
            using var documento = JsonDocument.Parse(cuerpo);
            return JsonSerializer.Serialize(documento.RootElement, OpcionesJson);
        }
        catch (JsonException)
        {
            return cuerpo;
        }
    }
}
