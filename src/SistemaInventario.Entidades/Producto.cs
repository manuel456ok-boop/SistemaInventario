namespace SistemaInventario.Entidades;
/// <summary>Producto con precio y referencias obligatorias a sus catálogos.</summary>
public sealed class Producto : EntidadBase
{
    public string Nombre { get; set; } = "";
    public decimal PrecioUnitario { get; set; }
    public int CategoriaId { get; set; }
    public int SuplidorId { get; set; }
}
