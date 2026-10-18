using SistemaInventario.AccesoDatos;
namespace SistemaInventario.LogicaNegocio;
/// <summary>Único lugar que conoce las implementaciones concretas de datos.</summary>
public static class Composicion
{
    /// <summary>Construye el grafo y devuelve solo servicios a la presentación.</summary>
    public static ServiciosAplicacion Crear(string cadenaConexion)
    {
        var conexion = ConexionSql.Instancia;
        conexion.Configurar(cadenaConexion);
        var logger = new Logger();
        var ejecutor = new EjecutorSql(conexion, logger);
        var categorias = new CategoriaRepositorio(ejecutor);
        var suplidores = new SuplidorRepositorio(ejecutor);
        var productos = new ProductoRepositorio(ejecutor);
        return new ServiciosAplicacion(new CategoriaServicio(categorias, productos), new SuplidorServicio(suplidores, productos), new ProductoServicio(productos, categorias, suplidores), logger);
    }
}
