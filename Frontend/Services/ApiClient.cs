using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Frontend.Models;

namespace Frontend.Services;

public sealed class ApiClient(HttpClient cliente, HistorialPeticionesService historial)
{
    private const string RutaContribuyentes = "api/contribuyentes";
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<(RespuestaApi Respuesta, List<Contribuyente> Contribuyentes)> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var respuesta = await EjecutarAsync(
            () => cliente.GetAsync(RutaContribuyentes, cancellationToken),
            cancellationToken,
            HttpMethod.Get,
            RutaContribuyentes);
        if (!respuesta.Exito)
        {
            return (respuesta, []);
        }

        try
        {
            var contribuyentes = JsonSerializer.Deserialize<List<Contribuyente>>(respuesta.Cuerpo, OpcionesJson);
            return contribuyentes is null
                ? (ErrorRespuesta(respuesta.Codigo, "La API devolvió una lista vacía no válida.", respuesta.Cuerpo), [])
                : (respuesta, contribuyentes);
        }
        catch (JsonException)
        {
            return (ErrorRespuesta(respuesta.Codigo, "La API devolvió una lista de contribuyentes no válida.", respuesta.Cuerpo), []);
        }
    }

    public Task<RespuestaApi> ObtenerAsync(string? nit, CancellationToken cancellationToken = default) =>
        TieneNit(nit)
            ? EjecutarAsync(
                () => cliente.GetAsync($"{RutaContribuyentes}/{Uri.EscapeDataString(Normalizar(nit))}", cancellationToken),
                cancellationToken,
                HttpMethod.Get,
                $"{RutaContribuyentes}/{Uri.EscapeDataString(Normalizar(nit))}")
            : Task.FromResult(ErrorLocal("El NIT es obligatorio."));

    public Task<RespuestaApi> CrearAsync(
        Contribuyente contribuyente,
        CancellationToken cancellationToken = default) =>
        EjecutarAsync(
            () => cliente.PostAsJsonAsync(RutaContribuyentes, contribuyente, cancellationToken),
            cancellationToken,
            HttpMethod.Post,
            RutaContribuyentes,
            JsonSerializer.Serialize(contribuyente));

    public Task<RespuestaApi> ReemplazarAsync(
        string? nit,
        Contribuyente contribuyente,
        CancellationToken cancellationToken = default) =>
        TieneNit(nit)
            ? EjecutarAsync(
                () => cliente.PutAsJsonAsync(
                    $"{RutaContribuyentes}/{Uri.EscapeDataString(Normalizar(nit))}",
                    contribuyente,
                    cancellationToken),
                cancellationToken,
                HttpMethod.Put,
                $"{RutaContribuyentes}/{Uri.EscapeDataString(Normalizar(nit))}",
                JsonSerializer.Serialize(contribuyente))
            : Task.FromResult(ErrorLocal("El NIT es obligatorio."));

    public Task<RespuestaApi> ActualizarNombreAsync(
        string? nit,
        string nombre,
        CancellationToken cancellationToken = default)
    {
        if (!TieneNit(nit))
        {
            return Task.FromResult(ErrorLocal("El NIT es obligatorio."));
        }

        return EnviarPatchAsync(nit!, nombre, cancellationToken);
    }

    public Task<RespuestaApi> EliminarAsync(string? nit, CancellationToken cancellationToken = default) =>
        TieneNit(nit)
            ? EjecutarAsync(
                () => cliente.DeleteAsync(
                    $"{RutaContribuyentes}/{Uri.EscapeDataString(Normalizar(nit))}",
                    cancellationToken),
                cancellationToken,
                HttpMethod.Delete,
                $"{RutaContribuyentes}/{Uri.EscapeDataString(Normalizar(nit))}")
            : Task.FromResult(ErrorLocal("El NIT es obligatorio."));

    public Task<RespuestaApi> InicializarAsync(CancellationToken cancellationToken = default) =>
        EjecutarAsync(
            () => cliente.DeleteAsync($"{RutaContribuyentes}/inicializar", cancellationToken),
            cancellationToken,
            HttpMethod.Delete,
            $"{RutaContribuyentes}/inicializar");

    public Task<RespuestaApi> ImportarTextoAsync(
        string texto,
        CancellationToken cancellationToken = default)
    {
        var contenido = new StringContent(texto ?? string.Empty, Encoding.UTF8, "text/plain");
        return EjecutarAsync(
            () => cliente.PostAsync($"{RutaContribuyentes}/importar-texto", contenido, cancellationToken),
            cancellationToken,
            HttpMethod.Post,
            $"{RutaContribuyentes}/importar-texto",
            texto ?? string.Empty);
    }

    public Task<RespuestaApi> ExtraerFechaAsync(
        string texto,
        CancellationToken cancellationToken = default) =>
        EjecutarAsync(
            () => cliente.GetAsync(
                $"{RutaContribuyentes}/fecha?texto={Uri.EscapeDataString(texto ?? string.Empty)}",
                cancellationToken),
            cancellationToken,
            HttpMethod.Get,
            $"{RutaContribuyentes}/fecha?texto={Uri.EscapeDataString(texto ?? string.Empty)}");

    public Task<RespuestaApi> ProcesarFacturasAsync(
        string json,
        CancellationToken cancellationToken = default)
    {
        var contenido = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json");
        return EjecutarAsync(
            () => cliente.PostAsync("api/Procesar", contenido, cancellationToken),
            cancellationToken,
            HttpMethod.Post,
            "api/Procesar",
            json ?? string.Empty);
    }

    public async Task<(RespuestaApi Respuesta, List<FacturaRegistradaResponse> Facturas)> ListarFacturasProcesadasAsync(
        CancellationToken cancellationToken = default)
    {
        var respuesta = await EjecutarAsync(
            () => cliente.GetAsync("api/facturas", cancellationToken),
            cancellationToken,
            HttpMethod.Get,
            "api/facturas");
        if (!respuesta.Exito)
        {
            return (respuesta, []);
        }

        try
        {
            var facturas = JsonSerializer.Deserialize<List<FacturaRegistradaResponse>>(respuesta.Cuerpo, OpcionesJson);
            return facturas is null
                ? (ErrorRespuesta(respuesta.Codigo, "La API devolvió una lista de facturas no válida.", respuesta.Cuerpo), [])
                : (respuesta, facturas);
        }
        catch (JsonException)
        {
            return (ErrorRespuesta(respuesta.Codigo, "La API devolvió una lista de facturas no válida.", respuesta.Cuerpo), []);
        }
    }

    public Task<RespuestaApi> ConsultarFacturasAsync(
        string nit,
        string tipo,
        string fechaInicio,
        string fechaFin,
        CancellationToken cancellationToken = default) =>
        ObtenerRutaAsync(
            "api/ConsultaDatos",
            new Dictionary<string, string>
            {
                ["nit"] = Normalizar(nit),
                ["tipo"] = tipo,
                ["fechaInicio"] = fechaInicio,
                ["fechaFin"] = fechaFin
            },
            cancellationToken);

    public Task<RespuestaApi> ResumirIvaAsync(
        string fecha,
        CancellationToken cancellationToken = default) =>
        ObtenerRutaAsync(
            "api/ResumenIva",
            new Dictionary<string, string> { ["fecha"] = fecha },
            cancellationToken);

    public Task<RespuestaApi> ResumirRangoAsync(
        string fechaInicio,
        string fechaFin,
        bool incluirIva,
        CancellationToken cancellationToken = default) =>
        ObtenerRutaAsync(
            "api/ResumenRango",
            new Dictionary<string, string>
            {
                ["fechaInicio"] = fechaInicio,
                ["fechaFin"] = fechaFin,
                ["incluirIva"] = incluirIva.ToString().ToLowerInvariant()
            },
            cancellationToken);

    public Task<RespuestaApi> ObtenerGraficasAsync(
        string fecha,
        string fechaInicio,
        string fechaFin,
        bool incluirIva,
        CancellationToken cancellationToken = default) =>
        ObtenerRutaAsync(
            "api/Grafica",
            new Dictionary<string, string>
            {
                ["fecha"] = fecha,
                ["fechaInicio"] = fechaInicio,
                ["fechaFin"] = fechaFin,
                ["incluirIva"] = incluirIva.ToString().ToLowerInvariant()
            },
            cancellationToken);

    private Task<RespuestaApi> ObtenerRutaAsync(
        string ruta,
        IReadOnlyDictionary<string, string> parametros,
        CancellationToken cancellationToken)
    {
        var query = string.Join(
            "&",
            parametros.Select(parametro =>
                $"{Uri.EscapeDataString(parametro.Key)}={Uri.EscapeDataString(parametro.Value)}"));
        return EjecutarAsync(
            () => cliente.GetAsync($"{ruta}?{query}", cancellationToken),
            cancellationToken,
            HttpMethod.Get,
            $"{ruta}?{query}");
    }

    private async Task<RespuestaApi> EnviarPatchAsync(
        string nit,
        string nombre,
        CancellationToken cancellationToken)
    {
        using var solicitud = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{RutaContribuyentes}/{Uri.EscapeDataString(Normalizar(nit))}")
        {
            Content = JsonContent.Create(new Contribuyente { Nombre = nombre ?? string.Empty })
        };

        return await EjecutarAsync(
            () => cliente.SendAsync(solicitud, cancellationToken),
            cancellationToken,
            HttpMethod.Patch,
            solicitud.RequestUri?.ToString() ?? $"{RutaContribuyentes}/{Normalizar(nit)}",
            JsonSerializer.Serialize(new Contribuyente { Nombre = nombre ?? string.Empty }));
    }

    private async Task<RespuestaApi> EjecutarAsync(
        Func<Task<HttpResponseMessage>> solicitud,
        CancellationToken cancellationToken,
        HttpMethod metodo,
        string ruta,
        string cuerpoSolicitud = "")
    {
        try
        {
            using var respuesta = await solicitud();
            var resultado = await CrearRespuestaAsync(respuesta, cancellationToken);
            historial.Registrar(new PeticionHistorial(
                DateTimeOffset.UtcNow,
                metodo.Method,
                ruta,
                resultado.Codigo,
                resultado.Exito,
                cuerpoSolicitud,
                resultado.Cuerpo));
            return resultado;
        }
        catch (HttpRequestException)
        {
            historial.Registrar(new PeticionHistorial(
                DateTimeOffset.UtcNow,
                metodo.Method,
                ruta,
                null,
                false,
                cuerpoSolicitud,
                "No se pudo conectar con la API."));
            return ErrorConexion();
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            historial.Registrar(new PeticionHistorial(
                DateTimeOffset.UtcNow,
                metodo.Method,
                ruta,
                null,
                false,
                cuerpoSolicitud,
                "La solicitud agotó el tiempo de espera."));
            return ErrorConexion();
        }
    }

    private static async Task<RespuestaApi> CrearRespuestaAsync(
        HttpResponseMessage respuesta,
        CancellationToken cancellationToken)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync(cancellationToken);
        var codigo = (int)respuesta.StatusCode;
        return new RespuestaApi
        {
            Codigo = codigo,
            Exito = respuesta.IsSuccessStatusCode,
            Cuerpo = cuerpo,
            Mensaje = ObtenerMensaje(codigo, respuesta.IsSuccessStatusCode, cuerpo)
        };
    }

    private static string ObtenerMensaje(int codigo, bool exito, string cuerpo)
    {
        if (!string.IsNullOrWhiteSpace(cuerpo))
        {
            try
            {
                using var documento = JsonDocument.Parse(cuerpo);
                if (documento.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var nombre in new[] { "mensaje", "detail", "title" })
                    {
                        var propiedad = documento.RootElement.EnumerateObject()
                            .FirstOrDefault(item => string.Equals(
                                item.Name, nombre, StringComparison.OrdinalIgnoreCase));
                        if (propiedad.Value.ValueKind == JsonValueKind.String &&
                            !string.IsNullOrWhiteSpace(propiedad.Value.GetString()))
                        {
                            return propiedad.Value.GetString()!;
                        }
                    }
                }
            }
            catch (JsonException)
            {
                if (!exito)
                {
                    return cuerpo;
                }
            }
        }

        if (!exito)
        {
            return $"La API respondió con el código HTTP {codigo}.";
        }

        return codigo == StatusCodes.Status201Created
            ? "Contribuyente creado correctamente."
            : "Operación realizada correctamente.";
    }

    private static string Normalizar(string? texto) => (texto ?? string.Empty).Trim();
    private static bool TieneNit(string? nit) => !string.IsNullOrWhiteSpace(nit);

    private static RespuestaApi ErrorLocal(string mensaje) => new()
    {
        Codigo = StatusCodes.Status400BadRequest,
        Exito = false,
        Mensaje = mensaje
    };

    private static RespuestaApi ErrorConexion() => new()
    {
        Exito = false,
        Mensaje = "No se pudo conectar con la API. Verifica que el backend esté ejecutándose."
    };

    private static RespuestaApi ErrorRespuesta(int codigo, string mensaje, string cuerpo) => new()
    {
        Codigo = codigo,
        Exito = false,
        Cuerpo = cuerpo,
        Mensaje = mensaje
    };
}
