using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/contribuyentes")]
public sealed class ContribuyentesController : ControllerBase
{
    private readonly ContribuyenteService _servicio;
    private readonly FacturacionService _facturacion;

    public ContribuyentesController(ContribuyenteService servicio, FacturacionService facturacion)
    {
        _servicio = servicio;
        _facturacion = facturacion;
    }

    [HttpGet]
    public ActionResult<List<Contribuyente>> Listar() => Ok(_servicio.Listar());

    [HttpGet("fecha")]
    public IActionResult ExtraerFecha([FromQuery] string? texto)
    {
        var fecha = FechaParse.ExtraerFecha(texto);
        return fecha.HasValue ? Ok(fecha.Value) : BadRequest("No se encontró una fecha válida.");
    }

    [HttpGet("{nit}")]
    public IActionResult Obtener(string nit)
    {
        var contribuyente = _servicio.Obtener(nit);
        return contribuyente is not null
            ? Ok(contribuyente)
            : NotFound(new ResultadoOperacion { Mensaje = ContribuyenteService.MensajeNoExiste });
    }

    [HttpPost]
    public IActionResult Crear([FromBody] Contribuyente? contribuyente)
    {
        var resultado = _servicio.Crear(contribuyente);
        if (resultado.Exito)
        {
            return CreatedAtAction(
                nameof(Obtener),
                new { nit = resultado.Contribuyente!.Nit },
                resultado.Contribuyente);
        }

        return resultado.Mensaje == ContribuyenteService.MensajeYaExiste
            ? Conflict(resultado)
            : BadRequest(resultado);
    }

    [HttpPut("{nit}")]
    public IActionResult Reemplazar(string nit, [FromBody] Contribuyente? contribuyente)
    {
        var resultado = _servicio.Reemplazar(nit, contribuyente);
        return resultado.Exito ? Ok(resultado.Contribuyente) : ResponderError(resultado);
    }

    [HttpPatch("{nit}")]
    public IActionResult ActualizarNombre(string nit, [FromBody] Contribuyente? contribuyente)
    {
        var resultado = _servicio.ActualizarNombre(nit, contribuyente);
        return resultado.Exito ? Ok(resultado.Contribuyente) : ResponderError(resultado);
    }

    [HttpDelete("inicializar")]
    public IActionResult Inicializar()
    {
        _servicio.Inicializar();
        _facturacion.Inicializar();
        return NoContent();
    }

    [HttpDelete("{nit}")]
    public IActionResult Eliminar(string nit)
    {
        var resultado = _servicio.Eliminar(nit);
        return resultado.Exito ? NoContent() : NotFound(resultado);
    }

    [HttpPost("importar-texto")]
    [Consumes("text/plain")]
    public async Task<IActionResult> ImportarTexto(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var texto = await reader.ReadToEndAsync(cancellationToken);

        try
        {
            return Ok(_servicio.ImportarDesdeTexto(texto));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ResultadoOperacion { Mensaje = exception.Message });
        }
        catch (JsonException)
        {
            return BadRequest(new ResultadoOperacion
            {
                Mensaje = "El contenido debe ser un arreglo JSON de contribuyentes."
            });
        }
    }

    private IActionResult ResponderError(ResultadoOperacion resultado) =>
        resultado.Mensaje == ContribuyenteService.MensajeNoExiste
            ? NotFound(resultado)
            : BadRequest(resultado);
}
