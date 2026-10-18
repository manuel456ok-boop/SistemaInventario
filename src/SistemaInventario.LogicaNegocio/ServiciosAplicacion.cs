using SistemaInventario.Entidades;
namespace SistemaInventario.LogicaNegocio;
/// <summary>Dependencias que recibe la ventana principal.</summary>
public sealed class ServiciosAplicacion
{
    public IServicio<Categoria> Categorias { get; }
    public IServicio<Suplidor> Suplidores { get; }
    public IServicio<Producto> Productos { get; }
    public ILogger Logger { get; }
    /// <summary>Permite montar SQL o repositorios de prueba sin alterar formularios.</summary>
    public ServiciosAplicacion(IServicio<Categoria> categorias, IServicio<Suplidor> suplidores, IServicio<Producto> productos, ILogger logger)
    { Categorias = categorias; Suplidores = suplidores; Productos = productos; Logger = logger; }
}
