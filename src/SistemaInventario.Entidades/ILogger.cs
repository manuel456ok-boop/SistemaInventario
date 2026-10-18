namespace SistemaInventario.Entidades;
/// <summary>Destino intercambiable de errores.</summary>
public interface ILogger
{
    /// <summary>Registra detalles técnicos sin incluir la cadena de conexión.</summary>
    void Error(string operacion, Exception excepcion);
}
