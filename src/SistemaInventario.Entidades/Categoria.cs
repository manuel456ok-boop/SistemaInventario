namespace SistemaInventario.Entidades;
/// <summary>Clasifica productos y conserva hasta cinco imágenes.</summary>
public sealed class Categoria : EntidadBase
{
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public List<FotoCategoria> Fotos { get; set; } = new();
}
