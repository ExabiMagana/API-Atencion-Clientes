namespace AtencionClientesAPI.Services;

public class ReglaNegocioException : Exception
{
    public int CodigoHttp { get; }

    public ReglaNegocioException(int codigoHttp, string mensaje)
        : base(mensaje)
    {
        CodigoHttp = codigoHttp;
    }
}