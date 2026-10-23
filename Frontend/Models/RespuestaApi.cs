namespace Frontend.Models;

public sealed class RespuestaApi
{
    public int Codigo { get; set; }
    public bool Exito { get; set; }
    public string Cuerpo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}
