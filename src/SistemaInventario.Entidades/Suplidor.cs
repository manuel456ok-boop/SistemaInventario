namespace SistemaInventario.Entidades;
/// <summary>Empresa que suministra productos.</summary>
public sealed class Suplidor : EntidadBase
{
    public string NombreEmpresa { get; set; } = "";
    public string NombreContacto { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Correo { get; set; } = "";
    public string SitioWeb { get; set; } = "";
}
