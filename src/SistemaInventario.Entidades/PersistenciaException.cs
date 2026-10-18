namespace SistemaInventario.Entidades;
/// <summary>Expone un mensaje seguro al negocio sin filtrar tipos de SqlClient.</summary>
public sealed class PersistenciaException : Exception
{
    /// <summary>Conserva la causa técnica para diagnóstico.</summary>
    public PersistenciaException(string mensaje, Exception causa) : base(mensaje, causa) { }
}
