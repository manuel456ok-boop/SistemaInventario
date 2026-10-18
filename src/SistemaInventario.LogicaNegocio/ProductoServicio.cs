using SistemaInventario.Entidades;
namespace SistemaInventario.LogicaNegocio;
/// <summary>Exige precio válido y relaciones activas al guardar y restaurar.</summary>
public sealed class ProductoServicio : ServicioBase<Producto>
{
    private readonly IRepositorio<Categoria> categorias;
    private readonly IRepositorio<Suplidor> suplidores;
    /// <summary>Recibe los contratos requeridos para validar relaciones.</summary>
    public ProductoServicio(IRepositorio<Producto> repositorio, IRepositorio<Categoria> categorias, IRepositorio<Suplidor> suplidores) : base(repositorio) { this.categorias = categorias; this.suplidores = suplidores; }
    protected override void Validar(Producto entidad, Dictionary<string, string> errores)
    {
        entidad.Nombre = entidad.Nombre.Trim();
        Validador.Texto(errores, nameof(entidad.Nombre), entidad.Nombre, 3, 150);
        if (entidad.PrecioUnitario <= 0 || entidad.PrecioUnitario > 9999999999999999.99m || decimal.Round(entidad.PrecioUnitario, 2) != entidad.PrecioUnitario)
            errores[nameof(entidad.PrecioUnitario)] = "El precio debe ser mayor a cero, tener máximo 2 decimales y caber en DECIMAL(18,2).";
        Relaciones(entidad, errores);
    }
    private void Relaciones(Producto entidad, Dictionary<string, string> errores)
    {
        if (categorias.Obtener(entidad.CategoriaId)?.Activo != true) errores[nameof(entidad.CategoriaId)] = "Seleccione una categoría activa.";
        if (suplidores.Obtener(entidad.SuplidorId)?.Activo != true) errores[nameof(entidad.SuplidorId)] = "Seleccione un suplidor activo.";
    }
    protected override void AntesRestaurar(Producto entidad)
    {
        var errores = new Dictionary<string, string>();
        Relaciones(entidad, errores);
        if (errores.Count > 0) throw new ValidacionException(errores);
    }
}
