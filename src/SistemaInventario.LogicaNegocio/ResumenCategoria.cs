namespace SistemaInventario.LogicaNegocio;
/// <summary>Conteo y promedio de una categoría, incluyendo las vacías.</summary>
public sealed record ResumenCategoria(string Categoria, int Cantidad, decimal Promedio);
