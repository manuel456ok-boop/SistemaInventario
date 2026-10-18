namespace SistemaInventario.LogicaNegocio;
/// <summary>Utilidad genérica propia adicional al repositorio.</summary>
public static class Buscador
{
    /// <summary>Filtra cualquier tipo por su texto, sin distinguir mayúsculas.</summary>
    public static IReadOnlyList<T> Filtrar<T>(IEnumerable<T> origen, string texto, Func<T, string> selector) =>
        origen.Where(item => selector(item).Contains(texto.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
}
