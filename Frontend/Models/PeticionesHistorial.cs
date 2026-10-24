namespace Frontend.Models;

public sealed record PeticionHistorial(
    DateTimeOffset Fecha,
    string Metodo,
    string Ruta,
    int? CodigoHttp,
    bool Exito,
    string CuerpoSolicitud,
    string CuerpoRespuesta);
