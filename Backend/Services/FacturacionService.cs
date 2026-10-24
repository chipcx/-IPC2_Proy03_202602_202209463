using System.Globalization;
using System.Text.Json;

public sealed class FacturacionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly ContribuyenteService _contribuyentes;
    private readonly object _sync = new();
    private List<FacturaPersistida> _facturas;

    public FacturacionService(IWebHostEnvironment environment, ContribuyenteService contribuyentes)
    {
        _contribuyentes = contribuyentes;
        var dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "facturas_procesadas.json");

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }

        var json = File.ReadAllText(_filePath);
        _facturas = string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<FacturaPersistida>>(json, JsonOptions)
                ?? throw new InvalidDataException($"El archivo '{_filePath}' no contiene una lista válida.");
    }

    public ResultadoAutorizaciones Procesar(List<Factura?>? facturas)
    {
        if (facturas is null)
        {
            throw new ArgumentException("El cuerpo debe ser un arreglo de facturas.", nameof(facturas));
        }

        var candidatos = new List<(Factura Factura, DateTime Fecha)>();
        var descartadas = 0;
        foreach (var factura in facturas)
        {
            if (factura is null || FechaParse.ExtraerFecha(factura.Tiempo) is not { } fecha)
            {
                descartadas++;
                continue;
            }

            candidatos.Add((factura, fecha));
        }

        var contribuyentes = _contribuyentes.Listar()
            .ToDictionary(contribuyente => NormalizarNit(contribuyente.Nit), StringComparer.Ordinal);
        var referenciasEnLote = candidatos
            .Select(candidato => NormalizarReferencia(candidato.Factura.Referencia))
            .Where(referencia => referencia.Length > 0)
            .GroupBy(referencia => referencia, StringComparer.Ordinal)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Count(), StringComparer.Ordinal);

        lock (_sync)
        {
            var referenciasPrevias = _facturas
                .Where(registro => registro.CodigoAprobacion.HasValue)
                .Select(registro => NormalizarReferencia(registro.Factura.Referencia))
                .Where(referencia => referencia.Length > 0)
                .ToHashSet(StringComparer.Ordinal);
            var codigosAprobacion = _facturas
                .Where(registro => registro.CodigoAprobacion.HasValue)
                .GroupBy(registro => registro.Fecha.Date)
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo => grupo.Max(registro => SecuenciaDelCodigo(registro.CodigoAprobacion!.Value)));

            var nuevosRegistros = new List<FacturaPersistida>(candidatos.Count);
            var aprobaciones = new List<DetalleAprobacion>();
            var errores = new Dictionary<string, int>(StringComparer.Ordinal);
            var emisores = new HashSet<string>(StringComparer.Ordinal);
            var receptores = new HashSet<string>(StringComparer.Ordinal);
            var rechazadasPorEmisorInexistente = 0;
            var rechazadasPorReceptorInexistente = 0;

            foreach (var (factura, fecha) in candidatos)
            {
                var erroresFactura = new List<string>();
                var nitEmisor = NormalizarNit(factura.NitEmisor);
                var nitReceptor = NormalizarNit(factura.NitReceptor);
                var referencia = NormalizarReferencia(factura.Referencia);

                if (nitEmisor.Length > 0)
                {
                    emisores.Add(nitEmisor);
                }

                if (nitReceptor.Length > 0)
                {
                    receptores.Add(nitReceptor);
                }

                ValidarNit(
                    nitEmisor,
                    "nitEmisorInvalido",
                    "nitEmisorInexistente",
                    contribuyentes,
                    erroresFactura,
                    ref rechazadasPorEmisorInexistente);
                ValidarNit(
                    nitReceptor,
                    "nitReceptorInvalido",
                    "nitReceptorInexistente",
                    contribuyentes,
                    erroresFactura,
                    ref rechazadasPorReceptorInexistente);

                if (referencia.Length == 0 || referencia.Length > 40)
                {
                    erroresFactura.Add("referenciaInvalida");
                }
                else if (referenciasPrevias.Contains(referencia) ||
                         referenciasEnLote[referencia] > 1)
                {
                    erroresFactura.Add("referenciaDuplicada");
                }

                var valorValido = factura.Valor > 0 &&
                    decimal.Round(factura.Valor, 2, MidpointRounding.AwayFromZero) == factura.Valor;
                if (!valorValido)
                {
                    erroresFactura.Add("valorInvalido");
                }
                else
                {
                    var ivaEsperado = decimal.Round(
                        factura.Valor * 0.12m,
                        2,
                        MidpointRounding.AwayFromZero);
                    if (factura.Iva != ivaEsperado)
                    {
                        erroresFactura.Add("ivaMalCalculado");
                    }
                }

                if (factura.Total != decimal.Round(factura.Valor + factura.Iva, 2, MidpointRounding.AwayFromZero))
                {
                    erroresFactura.Add("totalMalCalculado");
                }

                foreach (var error in erroresFactura)
                {
                    errores[error] = errores.GetValueOrDefault(error) + 1;
                }

                long? codigoAprobacion = null;
                if (erroresFactura.Count == 0)
                {
                    var secuencia = codigosAprobacion.GetValueOrDefault(fecha.Date) + 1;
                    codigoAprobacion = CodigoAutorizacionGenerator.Generar(fecha, secuencia);
                    codigosAprobacion[fecha.Date] = secuencia;
                    aprobaciones.Add(new DetalleAprobacion
                    {
                        Referencia = factura.Referencia.Trim(),
                        CodigoAprobacion = codigoAprobacion.Value
                    });
                }

                nuevosRegistros.Add(new FacturaPersistida
                {
                    Factura = CopiarFactura(factura),
                    Fecha = fecha,
                    CodigoAprobacion = codigoAprobacion,
                    Errores = erroresFactura
                });
            }

            if (nuevosRegistros.Count > 0)
            {
                List<FacturaPersistida> actualizadas = [.. _facturas, .. nuevosRegistros];
                Guardar(actualizadas);
                _facturas = actualizadas;
            }

            var aprobadas = aprobaciones.Count;
            var rechazadas = candidatos.Count - aprobadas;
            return new ResultadoAutorizaciones
            {
                Resultado = new DatosResultadoAutorizaciones
                {
                    CantidadAutorizacionesAprobadas = aprobadas,
                    CantidadRechazadasPorNitEmisorInexistente = rechazadasPorEmisorInexistente,
                    CantidadRechazadasPorNitReceptorInexistente = rechazadasPorReceptorInexistente,
                    DetalleAprobaciones = aprobaciones
                },
                Estadisticas = new EstadisticasProcesamiento
                {
                    CantidadFacturasRecibidas = facturas.Count,
                    CantidadFacturasDescartadasPorFormato = descartadas,
                    CantidadFacturasAprobadas = aprobadas,
                    CantidadFacturasRechazadas = rechazadas,
                    Errores = errores,
                    CantidadEmisoresDistintos = emisores.Count,
                    CantidadReceptoresDistintos = receptores.Count
                }
            };
        }
    }

    public IReadOnlyList<FacturaRegistradaResponse> ListarFacturasProcesadas()
    {
        lock (_sync)
        {
            return _facturas
                .OrderByDescending(registro => registro.Fecha)
                .Select(registro => new FacturaRegistradaResponse(
                    registro.Fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    CopiarFactura(registro.Factura),
                    registro.CodigoAprobacion.HasValue,
                    registro.CodigoAprobacion,
                    registro.Errores.ToList()))
                .ToList();
        }
    }

    public ConsultaFacturasResponse Consultar(string nit, string tipo, DateTime fechaInicio, DateTime fechaFin)
    {
        var nitNormalizado = NormalizarNit(nit);
        var buscarEmisor = string.Equals(tipo, "emitidas", StringComparison.OrdinalIgnoreCase);
        var registros = ObtenerAutorizadas()
            .Where(registro =>
                registro.Fecha.Date >= fechaInicio.Date &&
                registro.Fecha.Date <= fechaFin.Date &&
                string.Equals(
                    NormalizarNit(buscarEmisor ? registro.Factura.NitEmisor : registro.Factura.NitReceptor),
                    nitNormalizado,
                    StringComparison.Ordinal))
            .ToList();

        var resumen = registros
            .GroupBy(registro => registro.Fecha.Date)
            .OrderBy(grupo => grupo.Key)
            .Select(grupo => new DetalleConsultaFecha
            {
                Fecha = grupo.Key.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                CantidadFacturasAutorizadas = grupo.Count(),
                MontoTotalFacturado = grupo.Sum(registro => registro.Factura.Total)
            })
            .ToList();

        return new ConsultaFacturasResponse
        {
            Nit = nitNormalizado,
            Tipo = buscarEmisor ? "emitidas" : "recibidas",
            FechaInicio = fechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            FechaFin = fechaFin.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            ResumenPorFecha = resumen,
            CantidadTotalFacturas = registros.Count,
            MontoTotalFacturado = registros.Sum(registro => registro.Factura.Total)
        };
    }

    public ResumenIvaResponse ResumirIva(DateTime fecha)
    {
        var registros = ObtenerAutorizadas()
            .Where(registro => registro.Fecha.Date == fecha.Date)
            .ToList();
        var contribuyentes = _contribuyentes.Listar()
            .ToDictionary(contribuyente => NormalizarNit(contribuyente.Nit), StringComparer.Ordinal);
        var movimientos = new Dictionary<string, (decimal Emitido, decimal Recibido)>(StringComparer.Ordinal);

        foreach (var registro in registros)
        {
            var emisor = NormalizarNit(registro.Factura.NitEmisor);
            var receptor = NormalizarNit(registro.Factura.NitReceptor);
            var emisorTotales = movimientos.GetValueOrDefault(emisor);
            movimientos[emisor] = (emisorTotales.Emitido + registro.Factura.Iva, emisorTotales.Recibido);
            var receptorTotales = movimientos.GetValueOrDefault(receptor);
            movimientos[receptor] = (receptorTotales.Emitido, receptorTotales.Recibido + registro.Factura.Iva);
        }

        return new ResumenIvaResponse
        {
            Fecha = fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            ResumenPorNit = movimientos
                .OrderBy(movimiento => movimiento.Key, StringComparer.Ordinal)
                .Select(movimiento => new DetalleResumenIva
                {
                    Nit = movimiento.Key,
                    Nombre = contribuyentes.GetValueOrDefault(movimiento.Key)?.Nombre ?? string.Empty,
                    IvaEmitido = movimiento.Value.Emitido,
                    IvaRecibido = movimiento.Value.Recibido
                })
                .ToList()
        };
    }

    public ResumenRangoResponse ResumirRango(DateTime fechaInicio, DateTime fechaFin, bool incluirIva)
    {
        var registros = ObtenerAutorizadas()
            .Where(registro =>
                registro.Fecha.Date >= fechaInicio.Date &&
                registro.Fecha.Date <= fechaFin.Date)
            .ToList();
        var resumen = registros
            .GroupBy(registro => registro.Fecha.Date)
            .OrderBy(grupo => grupo.Key)
            .Select(grupo => new DetalleResumenRango
            {
                Fecha = grupo.Key.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                MontoAutorizado = grupo.Sum(registro =>
                    incluirIva ? registro.Factura.Total : registro.Factura.Valor)
            })
            .ToList();

        return new ResumenRangoResponse
        {
            FechaInicio = fechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            FechaFin = fechaFin.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            IncluirIva = incluirIva,
            ResumenPorFecha = resumen,
            MontoTotalRango = resumen.Sum(detalle => detalle.MontoAutorizado)
        };
    }

    public ResultadoGraficasResponse ObtenerDatosGraficas(
        DateTime fecha,
        DateTime fechaInicio,
        DateTime fechaFin,
        bool incluirIva)
    {
        var iva = ResumirIva(fecha);
        var rango = ResumirRango(fechaInicio, fechaFin, incluirIva);
        return new ResultadoGraficasResponse
        {
            GraficaIva = new DatosGraficaIva
            {
                Fecha = iva.Fecha,
                Labels = iva.ResumenPorNit.Select(detalle => detalle.Nit).ToList(),
                IvaEmitido = iva.ResumenPorNit.Select(detalle => detalle.IvaEmitido).ToList(),
                IvaRecibido = iva.ResumenPorNit.Select(detalle => detalle.IvaRecibido).ToList()
            },
            GraficaRango = new DatosGraficaRango
            {
                FechaInicio = rango.FechaInicio,
                FechaFin = rango.FechaFin,
                IncluirIva = rango.IncluirIva,
                Labels = rango.ResumenPorFecha.Select(detalle => detalle.Fecha).ToList(),
                Valores = rango.ResumenPorFecha.Select(detalle => detalle.MontoAutorizado).ToList()
            }
        };
    }

    public void Inicializar()
    {
        lock (_sync)
        {
            Guardar([]);
            _facturas = [];
        }
    }

    private List<FacturaPersistida> ObtenerAutorizadas()
    {
        lock (_sync)
        {
            return _facturas
                .Where(registro => registro.CodigoAprobacion.HasValue)
                .Select(CopiarRegistro)
                .ToList();
        }
    }

    private void Guardar(List<FacturaPersistida> facturas)
    {
        var json = JsonSerializer.Serialize(facturas, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private static void ValidarNit(
        string nit,
        string codigoInvalido,
        string codigoInexistente,
        IReadOnlyDictionary<string, Contribuyente> contribuyentes,
        ICollection<string> errores,
        ref int cantidadInexistente)
    {
        if (!NitValidator.EsValido(nit))
        {
            errores.Add(codigoInvalido);
        }
        else if (!contribuyentes.ContainsKey(nit))
        {
            errores.Add(codigoInexistente);
            cantidadInexistente++;
        }
    }

    private static int SecuenciaDelCodigo(long codigo)
    {
        var texto = codigo.ToString(CultureInfo.InvariantCulture);
        return int.Parse(texto[^8..], CultureInfo.InvariantCulture);
    }

    private static string NormalizarNit(string? nit) =>
        (nit ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    private static string NormalizarReferencia(string? referencia) => (referencia ?? string.Empty).Trim();

    private static Factura CopiarFactura(Factura factura) => new()
    {
        Tiempo = factura.Tiempo,
        Referencia = factura.Referencia,
        NitEmisor = factura.NitEmisor,
        NitReceptor = factura.NitReceptor,
        Valor = factura.Valor,
        Iva = factura.Iva,
        Total = factura.Total
    };

    private static FacturaPersistida CopiarRegistro(FacturaPersistida registro) => new()
    {
        Factura = CopiarFactura(registro.Factura),
        Fecha = registro.Fecha,
        CodigoAprobacion = registro.CodigoAprobacion,
        Errores = [.. registro.Errores]
    };
}
