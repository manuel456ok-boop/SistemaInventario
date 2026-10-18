using System.Data;
using Microsoft.Data.SqlClient;
using SistemaInventario.Entidades;
namespace SistemaInventario.AccesoDatos;
/// <summary>Mapea la información de contacto del suplidor.</summary>
public sealed class SuplidorRepositorio : RepositorioSql<Suplidor>
{
    /// <summary>Inyecta el ejecutor.</summary>
    public SuplidorRepositorio(EjecutorSql ejecutor) : base(ejecutor, "Suplidores") { }
    protected override string Campos => "NombreEmpresa,NombreContacto,Telefono,Correo,SitioWeb";
    protected override string Valores => "@empresa,@contacto,@telefono,@correo,@web";
    protected override string Asignaciones => "NombreEmpresa=@empresa,NombreContacto=@contacto,Telefono=@telefono,Correo=@correo,SitioWeb=@web";
    protected override Suplidor Mapear(SqlDataReader lector) => Auditoria(new Suplidor { NombreEmpresa = (string)lector["NombreEmpresa"], NombreContacto = (string)lector["NombreContacto"], Telefono = (string)lector["Telefono"], Correo = (string)lector["Correo"], SitioWeb = lector["SitioWeb"] as string ?? "" }, lector);
    protected override void Parametros(SqlCommand cmd, Suplidor entidad)
    {
        cmd.Parameters.Add("@empresa", SqlDbType.NVarChar, 100).Value = entidad.NombreEmpresa;
        cmd.Parameters.Add("@contacto", SqlDbType.NVarChar, 100).Value = entidad.NombreContacto;
        cmd.Parameters.Add("@telefono", SqlDbType.VarChar, 12).Value = entidad.Telefono;
        cmd.Parameters.Add("@correo", SqlDbType.NVarChar, 254).Value = entidad.Correo;
        cmd.Parameters.Add("@web", SqlDbType.NVarChar, 500).Value = string.IsNullOrEmpty(entidad.SitioWeb) ? DBNull.Value : entidad.SitioWeb;
    }
}
