using System.Globalization;

public static class CodigoAutorizacionGenerator
{
    public static long Generar(DateTime fecha, int correlativo)
    {
        if (correlativo < 1 || correlativo > 99_999_999)
        {
            throw new ArgumentOutOfRangeException(nameof(correlativo));
        }

        var codigo = fecha.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
            + correlativo.ToString("D8", CultureInfo.InvariantCulture);
        return long.Parse(codigo, CultureInfo.InvariantCulture);
    }
}