using System.Data;
using Microsoft.Data.SqlClient;
using SistemaInventario.Entidades;

namespace SistemaInventario.AccesoDatos;

/// <summary>Persiste categorías y fotografías en una sola transacción de escritura.</summary>
public sealed class CategoriaRepositorio : RepositorioSql<Categoria>
{
    /// <summary>Inyecta el ejecutor compartido.</summary>
    public CategoriaRepositorio(EjecutorSql ejecutor) : base(ejecutor, "Categorias")
    {
    }

    protected override string Campos => "Nombre,Descripcion";
    protected override string Valores => "@nombre,@descripcion";
    protected override string Asignaciones => "Nombre=@nombre,Descripcion=@descripcion";

    protected override Categoria Mapear(SqlDataReader lector)
    {
        var categoria = new Categoria
        {
            Nombre = (string)lector["Nombre"],
            Descripcion = (string)lector["Descripcion"]
        };

        return Auditoria(categoria, lector);
    }

    protected override void Parametros(SqlCommand cmd, Categoria entidad)
    {
        cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = entidad.Nombre;
        cmd.Parameters.Add("@descripcion", SqlDbType.NVarChar, 500).Value = entidad.Descripcion;
    }

    protected override void CargarRelaciones(SqlConnection cn, List<Categoria> entidades)
    {
        if (entidades.Count == 0)
        {
            return;
        }

        var porId = entidades.ToDictionary(c => c.Id);

        using var cmd = new SqlCommand("SELECT * FROM dbo.FotosCategoria WHERE Activo=1", cn);
        using var lector = cmd.ExecuteReader();

        while (lector.Read())
        {
            int categoriaId = (int)lector["CategoriaId"];

            if (porId.TryGetValue(categoriaId, out var categoria))
            {
                var foto = new FotoCategoria
                {
                    CategoriaId = categoriaId,
                    NombreArchivo = (string)lector["NombreArchivo"],
                    Contenido = (byte[])lector["Contenido"]
                };

                categoria.Fotos.Add(Auditoria(foto, lector));
            }
        }
    }

    protected override void GuardarRelaciones(SqlConnection cn, SqlTransaction tx, int id, Categoria entidad)
    {
        // Se reemplaza la colección visible; las versiones anteriores quedan inactivas.
        using (var cmd = new SqlCommand("UPDATE dbo.FotosCategoria SET Activo=0, FechaModificacion=SYSUTCDATETIME() WHERE CategoriaId=@id AND Activo=1", cn, tx))
        {
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.ExecuteNonQuery();
        }

        foreach (var foto in entidad.Fotos)
        {
            using var cmd = new SqlCommand("INSERT dbo.FotosCategoria(CategoriaId,NombreArchivo,Contenido) VALUES(@id,@nombre,@contenido)", cn, tx);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 255).Value = foto.NombreArchivo;
            cmd.Parameters.Add("@contenido", SqlDbType.VarBinary, -1).Value = foto.Contenido;

            cmd.ExecuteNonQuery();
        }
    }
}