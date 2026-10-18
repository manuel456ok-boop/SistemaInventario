using System.Data;
using Microsoft.Data.SqlClient;
using SistemaInventario.Entidades;
namespace SistemaInventario.AccesoDatos;
/// <summary>Centraliza apertura, transacción y manejo específico de SqlException.</summary>
public sealed class EjecutorSql
{
    private readonly ConexionSql conexion;
    private readonly ILogger logger;
    /// <summary>Recibe dependencias por constructor.</summary>
    public EjecutorSql(ConexionSql conexion, ILogger logger) { this.conexion = conexion; this.logger = logger; }
    /// <summary>Ejecuta lecturas o escrituras; toda interacción SQL queda dentro del try.</summary>
    public T Ejecutar<T>(string operacion, bool escritura, Func<SqlConnection, SqlTransaction?, T> accion)
    {
        try
        {
            using var cn = conexion.CrearConexion();
            cn.Open();
            using var tx = escritura ? cn.BeginTransaction(IsolationLevel.Serializable) : null;
            // Todas las escrituras de la aplicación comparten el mismo bloqueo transaccional.
            if (tx != null)
            {
                using var bloqueo = new SqlCommand("DECLARE @r int; EXEC @r = sys.sp_getapplock @Resource=N'SistemaInventario.Escritura', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; IF @r < 0 THROW 51005, 'Otra operación está en curso. Intente nuevamente.', 1;", cn, tx);
                bloqueo.ExecuteNonQuery();
            }
            var resultado = accion(cn, tx);
            tx?.Commit(); // Dispose hace rollback si no se alcanzó este punto.
            return resultado;
        }
        catch (SqlException ex)
        {
            logger.Error(operacion, ex);
            var mensaje = ex.Number switch
            {
                2601 or 2627 => "Ese nombre ya existe, incluso en la papelera. Edite o restaure el registro original.",
                547 => "La operación incumple una relación o restricción de la base de datos.",
                1205 => "Hubo un conflicto simultáneo. Actualice la lista e intente nuevamente.",
                -2 => "SQL Server tardó demasiado en responder.",
                >= 51000 and <= 51010 => ex.Message,
                _ => "No fue posible acceder a SQL Server. Revise el servidor, los scripts y la conexión. Consulte errores.log."
            };
            throw new PersistenciaException(mensaje, ex);
        }
    }
}
