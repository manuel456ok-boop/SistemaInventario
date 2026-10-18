using SistemaInventario.Entidades;
namespace SistemaInventario.LogicaNegocio;
/// <summary>Reglas del catálogo y protección de productos activos.</summary>
public sealed class CategoriaServicio : ServicioBase<Categoria>
{
    private readonly IRepositorio<Producto> productos;
    /// <summary>Recibe ambos repositorios por constructor.</summary>
    public CategoriaServicio(IRepositorio<Categoria> repositorio, IRepositorio<Producto> productos) : base(repositorio) { this.productos = productos; }
    protected override void Validar(Categoria entidad, Dictionary<string, string> errores)
    {
        entidad.Nombre = entidad.Nombre.Trim(); entidad.Descripcion = entidad.Descripcion.Trim();
        Validador.Texto(errores, nameof(entidad.Nombre), entidad.Nombre, 3, 100);
        Validador.Texto(errores, nameof(entidad.Descripcion), entidad.Descripcion, 1, 500);
        if (Todos().Any(c => c.Id != entidad.Id && string.Equals(c.Nombre, entidad.Nombre, StringComparison.OrdinalIgnoreCase))) errores[nameof(entidad.Nombre)] = "Ya existe una categoría con ese nombre (incluida la papelera).";
        if (entidad.Fotos.Count > 5) errores[nameof(entidad.Fotos)] = "Puede agregar de 0 a 5 fotos.";
        foreach (var foto in entidad.Fotos)
        {
            string? error = Validador.Foto(foto);
            if (error != null) errores[nameof(entidad.Fotos)] = error;
        }
    }
    protected override void AntesEliminar(Categoria entidad)
    {
        if (productos.Listar().Any(p => p.CategoriaId == entidad.Id)) throw new InvalidOperationException("No puede eliminar una categoría con productos activos. Reasigne o elimine esos productos primero.");
    }
}
