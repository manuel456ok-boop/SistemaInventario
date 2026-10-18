using System.Data;
using Microsoft.Data.SqlClient;
using SistemaInventario.Entidades;
namespace SistemaInventario.AccesoDatos;
/// <summary>Persiste precios y claves foráneas usando parámetros tipados.</summary>
public sealed class ProductoRepositorio : RepositorioSql<Producto>
{
    /// <summary>Inyecta el ejecutor.</summary>
    public ProductoRepositorio(EjecutorSql ejecutor) : base(ejecutor, "Productos") { }
    protected override string Campos => "Nombre,PrecioUnitario,CategoriaId,SuplidorId";
    protected override string Valores => "@nombre,@precio,@categoria,@suplidor";
    protected override string Asignaciones => "Nombre=@nombre,PrecioUnitario=@precio,CategoriaId=@categoria,SuplidorId=@suplidor";
    protected override Producto Mapear(SqlDataReader lector) => Auditoria(new Producto { Nombre = (string)lector["Nombre"], PrecioUnitario = (decimal)lector["PrecioUnitario"], CategoriaId = (int)lector["CategoriaId"], SuplidorId = (int)lector["SuplidorId"] }, lector);
    protected override void Parametros(SqlCommand cmd, Producto entidad)
    {
        cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 150).Value = entidad.Nombre;
        var precio = cmd.Parameters.Add("@precio", SqlDbType.Decimal);
        precio.Precision = 18; precio.Scale = 2; precio.Value = entidad.PrecioUnitario;
        cmd.Parameters.Add("@categoria", SqlDbType.Int).Value = entidad.CategoriaId;
        cmd.Parameters.Add("@suplidor", SqlDbType.Int).Value = entidad.SuplidorId;
    }
}
