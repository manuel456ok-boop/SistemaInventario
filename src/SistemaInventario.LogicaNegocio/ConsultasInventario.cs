using SistemaInventario.Entidades;
namespace SistemaInventario.LogicaNegocio;
/// <summary>Consultas LINQ en memoria sobre una instantánea de los catálogos.</summary>
public static class ConsultasInventario
{
    /// <summary>LINQ 1: dos joins y proyección de productos a filas legibles.</summary>
    public static IReadOnlyList<ProductoVista> Proyectar(IEnumerable<Producto> productos, IEnumerable<Categoria> categorias, IEnumerable<Suplidor> suplidores) =>
        (from p in productos join c in categorias on p.CategoriaId equals c.Id join s in suplidores on p.SuplidorId equals s.Id
         orderby p.Nombre select new ProductoVista(p.Id, p.Nombre, p.PrecioUnitario, p.CategoriaId, c.Nombre, p.SuplidorId, s.NombreEmpresa, p.FechaCreacion, p.FechaModificacion)).ToList();
    /// <summary>LINQ 2: búsqueda textual, categoría, suplidor y rango inclusivo.</summary>
    public static IReadOnlyList<ProductoVista> Filtrar(IEnumerable<ProductoVista> origen, string texto, int? categoriaId, int? suplidorId, decimal? minimo, decimal? maximo)
    {
        if (minimo < 0 || maximo < 0 || minimo > maximo) throw new ArgumentException("El rango de precios es inválido: mínimo y máximo no negativos, mínimo menor o igual al máximo.");
        return origen.Where(p => (!categoriaId.HasValue || p.CategoriaId == categoriaId) && (!suplidorId.HasValue || p.SuplidorId == suplidorId)
            && (!minimo.HasValue || p.PrecioUnitario >= minimo) && (!maximo.HasValue || p.PrecioUnitario <= maximo)
            && (p.Nombre.Contains(texto.Trim(), StringComparison.OrdinalIgnoreCase) || p.Categoria.Contains(texto.Trim(), StringComparison.OrdinalIgnoreCase) || p.Suplidor.Contains(texto.Trim(), StringComparison.OrdinalIgnoreCase)))
            .OrderBy(p => p.Nombre).ToList();
    }
    /// <summary>LINQ 3: group join, conteo y promedio; conserva categorías sin productos.</summary>
    public static IReadOnlyList<ResumenCategoria> PorCategoria(IEnumerable<Producto> productos, IEnumerable<Categoria> categorias) =>
        categorias.GroupJoin(productos.Where(p => p.Activo), c => c.Id, p => p.CategoriaId, (c, grupo) => new ResumenCategoria(c.Nombre, grupo.Count(), grupo.Any() ? grupo.Average(p => p.PrecioUnitario) : 0m))
            .OrderByDescending(x => x.Cantidad).ThenBy(x => x.Categoria).ToList();
    /// <summary>LINQ 4: agrupa, une, ordena y toma los cinco suplidores con más productos.</summary>
    public static IReadOnlyList<ResumenSuplidor> TopSuplidores(IEnumerable<Producto> productos, IEnumerable<Suplidor> suplidores) =>
        productos.Where(p => p.Activo).GroupBy(p => p.SuplidorId).Join(suplidores, grupo => grupo.Key, s => s.Id, (grupo, s) => new ResumenSuplidor(s.NombreEmpresa, grupo.Count()))
            .OrderByDescending(x => x.Cantidad).ThenBy(x => x.Suplidor).Take(5).ToList();
    /// <summary>LINQ 5: productos sobre el promedio global, ordenados por precio.</summary>
    public static IReadOnlyList<ProductoVista> SobrePromedio(IEnumerable<ProductoVista> productos)
    {
        var lista = productos.ToList();
        decimal promedio = lista.Select(p => p.PrecioUnitario).DefaultIfEmpty(0m).Average();
        return lista.Where(p => p.PrecioUnitario > promedio).OrderByDescending(p => p.PrecioUnitario).ToList();
    }
    // LINQ 6: Buscador.Filtrar<T> realiza la búsqueda genérica de categorías y suplidores.
}
