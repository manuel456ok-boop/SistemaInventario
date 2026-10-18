namespace SistemaInventario.LogicaNegocio;
/// <summary>Proyección para listados, sin consultas SQL desde la presentación.</summary>
public sealed record ProductoVista(int Id, string Nombre, decimal PrecioUnitario, int CategoriaId, string Categoria, int SuplidorId, string Suplidor, DateTime FechaCreacion, DateTime FechaModificacion);
