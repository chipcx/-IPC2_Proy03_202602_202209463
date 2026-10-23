
public sealed class ResultadoOperacion
{
    public bool Exito { get; init; }
    public string Mensaje { get; init; } = string.Empty;
    public Contribuyente? Contribuyente { get; init; }
}