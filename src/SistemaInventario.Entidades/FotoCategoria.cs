namespace SistemaInventario.Entidades;
/// <summary>Imagen almacenada en SQL Server; no depende de rutas del equipo.</summary>
public sealed class FotoCategoria : EntidadBase
{
    public int CategoriaId { get; set; }
    public string NombreArchivo { get; set; } = "";
    public byte[] Contenido { get; set; } = Array.Empty<byte>();
}
