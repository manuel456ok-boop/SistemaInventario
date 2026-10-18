using Microsoft.Data.SqlClient;
namespace SistemaInventario.AccesoDatos;
/// <summary>Singleton de configuración; crea una conexión corta por operación.</summary>
public sealed class ConexionSql
{
    private static readonly Lazy<ConexionSql> instancia = new(() => new ConexionSql());
    private string? cadena;
    private ConexionSql() { }
    public static ConexionSql Instancia => instancia.Value;
    /// <summary>Configura una vez al iniciar; valida la sintaxis sin abrir la conexión.</summary>
    public void Configurar(string cadenaConexion)
    {
        var builder = new SqlConnectionStringBuilder(cadenaConexion);
        if (string.IsNullOrWhiteSpace(builder.DataSource) || string.IsNullOrWhiteSpace(builder.InitialCatalog))
            throw new ArgumentException("La conexión debe indicar Server y Database.");
        cadena = builder.ConnectionString;
    }
    /// <summary>Crea una conexión nueva; el pool reutiliza conexiones físicas.</summary>
    public SqlConnection CrearConexion() => new(cadena ?? throw new InvalidOperationException("Configure la conexión primero."));
}
