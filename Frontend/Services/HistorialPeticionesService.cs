using System.Text.Json;
using Frontend.Models;

namespace Frontend.Services;

public sealed class HistorialPeticionesService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly object _sync = new();
    private List<PeticionHistorial> _peticiones;

    public HistorialPeticionesService(IWebHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "historial_peticiones.json");

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }

        var json = File.ReadAllText(_filePath);
        _peticiones = string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<PeticionHistorial>>(json, JsonOptions)
                ?? throw new InvalidDataException($"El archivo '{_filePath}' no contiene un historial válido.");
    }

    public void Registrar(PeticionHistorial peticion)
    {
        lock (_sync)
        {
            List<PeticionHistorial> actualizadas = [.. _peticiones, peticion];
            var json = JsonSerializer.Serialize(actualizadas, JsonOptions);
            var temporal = $"{_filePath}.tmp";
            File.WriteAllText(temporal, json);
            File.Move(temporal, _filePath, overwrite: true);
            _peticiones = actualizadas;
        }
    }

    public IReadOnlyList<PeticionHistorial> Listar()
    {
        lock (_sync)
        {
            return _peticiones.OrderByDescending(peticion => peticion.Fecha).ToList();
        }
    }
}
