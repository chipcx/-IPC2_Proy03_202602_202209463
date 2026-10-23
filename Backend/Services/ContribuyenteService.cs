using System.Text.Json;

public sealed class ContribuyenteService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public const string MensajeCuerpoObligatorio = "El cuerpo de la petición es obligatorio.";
    public const string MensajeNitObligatorio = "El NIT es obligatorio.";
    public const string MensajeNitInvalido = "El NIT no es válido.";
    public const string MensajeNombreObligatorio = "El nombre es obligatorio.";
    public const string MensajeYaExiste = "El contribuyente ya existe.";
    public const string MensajeNoExiste = "El contribuyente no existe.";

    private readonly string _filePath;
    private readonly object _sync = new();
    private List<Contribuyente> _contribuyentes;

    public ContribuyenteService(IWebHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "contribuyentes.json");

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }

        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            _contribuyentes = [];
            GuardarDatos(_contribuyentes);
            return;
        }

        _contribuyentes = JsonSerializer.Deserialize<List<Contribuyente>>(json, JsonOptions)
            ?? throw new InvalidDataException($"El archivo '{_filePath}' no contiene una lista válida.");
    }

    public List<Contribuyente> Listar()
    {
        lock (_sync)
        {
            return _contribuyentes.Select(Copiar).ToList();
        }
    }

    public Contribuyente? Obtener(string? nit)
    {
        var nitNormalizado = Normalizar(nit);
        if (nitNormalizado.Length == 0)
        {
            return null;
        }

        lock (_sync)
        {
            var contribuyente = _contribuyentes.FirstOrDefault(item =>
                string.Equals(Normalizar(item.Nit), nitNormalizado, StringComparison.Ordinal));

            return contribuyente is null ? null : Copiar(contribuyente);
        }
    }

    public ResultadoOperacion Crear(Contribuyente? contribuyente)
    {
        if (contribuyente is null)
        {
            return Fallo(MensajeCuerpoObligatorio);
        }

        var nitNormalizado = Normalizar(contribuyente.Nit);
        if (nitNormalizado.Length == 0)
        {
            return Fallo(MensajeNitObligatorio);
        }

        if (!NitValidator.EsValido(nitNormalizado))
        {
            return Fallo(MensajeNitInvalido);
        }

        if (string.IsNullOrWhiteSpace(contribuyente.Nombre))
        {
            return Fallo(MensajeNombreObligatorio);
        }

        lock (_sync)
        {
            if (Existe(nitNormalizado))
            {
                return Fallo(MensajeYaExiste);
            }

            var creado = new Contribuyente
            {
                Nit = nitNormalizado,
                Nombre = contribuyente.Nombre.Trim()
            };
            List<Contribuyente> actualizados = [.. _contribuyentes, creado];
            GuardarDatos(actualizados);
            _contribuyentes = actualizados;

            return Exito("Contribuyente creado.", creado);
        }
    }

    public ResultadoOperacion Reemplazar(string? nit, Contribuyente? contribuyente)
    {
        if (contribuyente is null)
        {
            return Fallo(MensajeCuerpoObligatorio);
        }

        var nitNormalizado = Normalizar(nit);
        if (string.IsNullOrWhiteSpace(contribuyente.Nombre))
        {
            return Fallo(MensajeNombreObligatorio);
        }

        lock (_sync)
        {
            var indice = IndiceDe(nitNormalizado);
            if (indice < 0)
            {
                return Fallo(MensajeNoExiste);
            }

            if (!NitValidator.EsValido(nitNormalizado))
            {
                return Fallo(MensajeNitInvalido);
            }

            var reemplazado = new Contribuyente
            {
                Nit = nitNormalizado,
                Nombre = contribuyente.Nombre.Trim()
            };
            List<Contribuyente> actualizados = [.. _contribuyentes];
            actualizados[indice] = reemplazado;
            GuardarDatos(actualizados);
            _contribuyentes = actualizados;

            return Exito("Contribuyente reemplazado.", reemplazado);
        }
    }

    public ResultadoOperacion ActualizarNombre(string? nit, Contribuyente? contribuyente)
    {
        if (contribuyente is null)
        {
            return Fallo(MensajeCuerpoObligatorio);
        }

        if (string.IsNullOrWhiteSpace(contribuyente.Nombre))
        {
            return Fallo(MensajeNombreObligatorio);
        }

        var nitNormalizado = Normalizar(nit);
        lock (_sync)
        {
            var indice = IndiceDe(nitNormalizado);
            if (indice < 0)
            {
                return Fallo(MensajeNoExiste);
            }

            if (!NitValidator.EsValido(nitNormalizado))
            {
                return Fallo(MensajeNitInvalido);
            }

            var actualizado = Copiar(_contribuyentes[indice]);
            actualizado.Nombre = contribuyente.Nombre.Trim();
            List<Contribuyente> actualizados = [.. _contribuyentes];
            actualizados[indice] = actualizado;
            GuardarDatos(actualizados);
            _contribuyentes = actualizados;

            return Exito("Nombre actualizado.", actualizado);
        }
    }

    public ResultadoOperacion Eliminar(string? nit)
    {
        var nitNormalizado = Normalizar(nit);
        lock (_sync)
        {
            var indice = IndiceDe(nitNormalizado);
            if (indice < 0)
            {
                return Fallo(MensajeNoExiste);
            }

            if (!NitValidator.EsValido(nitNormalizado))
            {
                return Fallo(MensajeNitInvalido);
            }

            var eliminado = _contribuyentes[indice];
            var actualizados = _contribuyentes.Where((_, posicion) => posicion != indice).ToList();
            GuardarDatos(actualizados);
            _contribuyentes = actualizados;

            return Exito("Contribuyente eliminado.", Copiar(eliminado));
        }
    }

    public void Inicializar()
    {
        lock (_sync)
        {
            GuardarDatos([]);
            _contribuyentes = [];
        }
    }

    public ResultadoImportacion ImportarDesdeTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new ArgumentException("El contenido a importar es obligatorio.", nameof(texto));
        }

        var importados = TextoExtractor.ExtraerContribuyentes(texto);
        var nuevos = 0;
        var actualizadosCount = 0;
        var invalidos = 0;

        lock (_sync)
        {
            List<Contribuyente> actualizados = [.. _contribuyentes];
            foreach (var contribuyente in importados)
            {
                if (contribuyente is null)
                {
                    invalidos++;
                    continue;
                }

                var nitNormalizado = Normalizar(contribuyente.Nit);
                if (!NitValidator.EsValido(nitNormalizado) ||
                    string.IsNullOrWhiteSpace(contribuyente.Nombre))
                {
                    invalidos++;
                    continue;
                }

                var existente = actualizados.FindIndex(item =>
                    string.Equals(Normalizar(item.Nit), nitNormalizado, StringComparison.Ordinal));
                var normalizado = new Contribuyente
                {
                    Nit = nitNormalizado,
                    Nombre = contribuyente.Nombre.Trim()
                };

                if (existente >= 0)
                {
                    actualizados[existente] = normalizado;
                    actualizadosCount++;
                }
                else
                {
                    actualizados.Add(normalizado);
                    nuevos++;
                }
            }

            if (nuevos + actualizadosCount > 0)
            {
                GuardarDatos(actualizados);
                _contribuyentes = actualizados;
            }
        }

        return new ResultadoImportacion
        {
            ContribuyentesNuevos = nuevos,
            ContribuyentesActualizados = actualizadosCount,
            NitsInvalidos = invalidos
        };
    }

    private bool Existe(string nit) => IndiceDe(nit) >= 0;

    private int IndiceDe(string nit) => _contribuyentes.FindIndex(contribuyente =>
        string.Equals(Normalizar(contribuyente.Nit), nit, StringComparison.Ordinal));

    private void GuardarDatos(List<Contribuyente> contribuyentes)
    {
        var json = JsonSerializer.Serialize(contribuyentes, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private static string Normalizar(string? nit) =>
        (nit ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    private static Contribuyente Copiar(Contribuyente contribuyente) => new()
    {
        Nit = contribuyente.Nit,
        Nombre = contribuyente.Nombre
    };

    private static ResultadoOperacion Fallo(string mensaje) => new() { Mensaje = mensaje };

    private static ResultadoOperacion Exito(string mensaje, Contribuyente contribuyente) =>
        new() { Exito = true, Mensaje = mensaje, Contribuyente = Copiar(contribuyente) };
}
