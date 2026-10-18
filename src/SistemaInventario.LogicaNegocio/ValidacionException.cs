namespace SistemaInventario.LogicaNegocio;
/// <summary>Transporta errores por propiedad para ErrorProvider.</summary>
public sealed class ValidacionException : Exception
{
    public IReadOnlyDictionary<string, string> Errores { get; }
    /// <summary>Construye un único mensaje y conserva el detalle por campo.</summary>
    public ValidacionException(IReadOnlyDictionary<string, string> errores) : base(string.Join(Environment.NewLine, errores.Values)) { Errores = errores; }
}
