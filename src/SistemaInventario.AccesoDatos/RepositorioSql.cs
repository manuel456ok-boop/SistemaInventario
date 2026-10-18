using System.Data;
using Microsoft.Data.SqlClient;
using SistemaInventario.Entidades;
namespace SistemaInventario.AccesoDatos;
/// <summary>Implementa el ciclo CRUD común y delega el mapeo a repositorios concretos.</summary>
public abstract class RepositorioSql<T> : IRepositorio<T> where T : EntidadBase
{
    protected readonly EjecutorSql ejecutor;
    private readonly string tabla; // Solo proviene de constantes internas, nunca de la UI.
    /// <summary>Recibe ejecutor y tabla fija.</summary>
    protected RepositorioSql(EjecutorSql ejecutor, string tabla) { this.ejecutor = ejecutor; this.tabla = tabla; }
    protected abstract T Mapear(SqlDataReader lector);
    protected abstract string Campos { get; }
    protected abstract string Valores { get; }
    protected abstract string Asignaciones { get; }
    protected abstract void Parametros(SqlCommand cmd, T entidad);
    protected virtual void CargarRelaciones(SqlConnection cn, List<T> entidades) { }
    protected virtual void GuardarRelaciones(SqlConnection cn, SqlTransaction tx, int id, T entidad) { }
    /// <summary>Lee una lista filtrada por Activo.</summary>
    public IReadOnlyList<T> Listar(bool activos = true) => ejecutor.Ejecutar("Listar " + tabla, false, (cn, tx) =>
    {
        var lista = new List<T>();
        using (var cmd = new SqlCommand($"SELECT * FROM dbo.{tabla} WHERE Activo=@activo ORDER BY Id", cn))
        {
            cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activos;
            using var lector = cmd.ExecuteReader();
            while (lector.Read()) lista.Add(Mapear(lector));
        }
        CargarRelaciones(cn, lista);
        return lista;
    });
    /// <summary>Obtiene un registro de cualquier estado.</summary>
    public T? Obtener(int id) => ejecutor.Ejecutar("Obtener " + tabla, false, (cn, tx) =>
    {
        var lista = new List<T>();
        using (var cmd = new SqlCommand($"SELECT * FROM dbo.{tabla} WHERE Id=@id", cn))
        {
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            using var lector = cmd.ExecuteReader();
            if (lector.Read()) lista.Add(Mapear(lector));
        }
        CargarRelaciones(cn, lista);
        return lista.FirstOrDefault();
    });
    /// <summary>Inserta entidad y relaciones atómicamente.</summary>
    public int Crear(T entidad) => ejecutor.Ejecutar("Crear " + tabla, true, (cn, tx) =>
    {
        using var cmd = new SqlCommand($"DECLARE @nueva TABLE(Id INT); INSERT dbo.{tabla} ({Campos}) OUTPUT INSERTED.Id INTO @nueva VALUES ({Valores}); SELECT Id FROM @nueva;", cn, tx);
        Parametros(cmd, entidad);
        int id = Convert.ToInt32(cmd.ExecuteScalar());
        GuardarRelaciones(cn, tx!, id, entidad);
        return id;
    });
    /// <summary>Actualiza campos y relaciones de un registro activo.</summary>
    public void Actualizar(T entidad) => ejecutor.Ejecutar("Actualizar " + tabla, true, (cn, tx) =>
    {
        using var cmd = new SqlCommand($"UPDATE dbo.{tabla} SET {Asignaciones}, FechaModificacion=SYSUTCDATETIME() WHERE Id=@id AND Activo=1", cn, tx);
        Parametros(cmd, entidad);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = entidad.Id;
        ExigirFila(cmd.ExecuteNonQuery());
        GuardarRelaciones(cn, tx!, entidad.Id, entidad);
        return true;
    });
    /// <summary>Aplica borrado lógico; los triggers protegen relaciones activas.</summary>
    public void Eliminar(int id) => CambiarEstado(id, false);
    /// <summary>Restaura; los triggers validan nuevamente las relaciones.</summary>
    public void Restaurar(int id) => CambiarEstado(id, true);
    private void CambiarEstado(int id, bool activo) => ejecutor.Ejecutar("Cambiar estado " + tabla, true, (cn, tx) =>
    {
        using var cmd = new SqlCommand($"UPDATE dbo.{tabla} SET Activo=@activo, FechaModificacion=SYSUTCDATETIME() WHERE Id=@id AND Activo<>@activo", cn, tx);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo;
        ExigirFila(cmd.ExecuteNonQuery());
        return true;
    });
    private static void ExigirFila(int filas)
    {
        if (filas != 1) throw new InvalidOperationException("El registro cambió o ya no existe. Actualice el listado.");
    }
    protected static TEntidad Auditoria<TEntidad>(TEntidad entidad, SqlDataReader lector) where TEntidad : EntidadBase
    {
        entidad.Id = lector.GetInt32(lector.GetOrdinal("Id"));
        entidad.Activo = lector.GetBoolean(lector.GetOrdinal("Activo"));
        entidad.FechaCreacion = DateTime.SpecifyKind(lector.GetDateTime(lector.GetOrdinal("FechaCreacion")), DateTimeKind.Utc);
        entidad.FechaModificacion = DateTime.SpecifyKind(lector.GetDateTime(lector.GetOrdinal("FechaModificacion")), DateTimeKind.Utc);
        return entidad;
    }
}
