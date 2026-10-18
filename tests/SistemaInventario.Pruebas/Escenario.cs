using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Pruebas;
/// <summary>Construye un escenario independiente para cada prueba.</summary>
public sealed class Escenario
{
    public RepositorioMemoria<Categoria> Categorias { get; } = new();
    public RepositorioMemoria<Suplidor> Suplidores { get; } = new();
    public RepositorioMemoria<Producto> Productos { get; } = new();
    public CategoriaServicio CategoriaServicio { get; }
    public SuplidorServicio SuplidorServicio { get; }
    public ProductoServicio ProductoServicio { get; }
    /// <summary>Inicializa el mismo grafo de reglas que producción usando contratos.</summary>
    public Escenario()
    {
        CategoriaServicio = new(Categorias, Productos); SuplidorServicio = new(Suplidores, Productos); ProductoServicio = new(Productos, Categorias, Suplidores);
        CategoriaServicio.Guardar(new Categoria { Nombre = "Equipos", Descripcion = "Equipos de prueba" });
        SuplidorServicio.Guardar(SuplidorValido());
    }
    /// <summary>Devuelve un suplidor válido nuevo.</summary>
    public static Suplidor SuplidorValido() => new() { NombreEmpresa = "Empresa Caribe", NombreContacto = "Ana Pérez", Telefono = "809-555-0101", Correo = "ana@empresa.example" };
    /// <summary>Devuelve un producto válido asociado al escenario.</summary>
    public static Producto ProductoValido(decimal precio = 100) => new() { Nombre = "Producto prueba", CategoriaId = 1, SuplidorId = 1, PrecioUnitario = precio };
}
