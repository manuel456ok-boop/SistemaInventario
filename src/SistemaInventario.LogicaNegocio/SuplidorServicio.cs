using SistemaInventario.Entidades;
namespace SistemaInventario.LogicaNegocio;
/// <summary>Valida los datos de contacto y la unicidad de empresa.</summary>
public sealed class SuplidorServicio : ServicioBase<Suplidor>
{
    private readonly IRepositorio<Producto> productos;
    /// <summary>Recibe repositorios por constructor.</summary>
    public SuplidorServicio(IRepositorio<Suplidor> repositorio, IRepositorio<Producto> productos) : base(repositorio) { this.productos = productos; }
    protected override void Validar(Suplidor entidad, Dictionary<string, string> errores)
    {
        entidad.NombreEmpresa = entidad.NombreEmpresa.Trim(); entidad.NombreContacto = entidad.NombreContacto.Trim();
        entidad.Telefono = entidad.Telefono.Trim(); entidad.Correo = entidad.Correo.Trim(); entidad.SitioWeb = entidad.SitioWeb.Trim();
        Validador.Texto(errores, nameof(entidad.NombreEmpresa), entidad.NombreEmpresa, 3, 100);
        Validador.Texto(errores, nameof(entidad.NombreContacto), entidad.NombreContacto, 3, 100);
        if (Todos().Any(s => s.Id != entidad.Id && string.Equals(s.NombreEmpresa, entidad.NombreEmpresa, StringComparison.OrdinalIgnoreCase))) errores[nameof(entidad.NombreEmpresa)] = "Esa empresa ya existe (incluida la papelera).";
        if (!Validador.Telefono(entidad.Telefono)) errores[nameof(entidad.Telefono)] = "Use 809/829/849-XXX-XXXX; ejemplo: 809-555-0101.";
        if (!Validador.Correo(entidad.Correo)) errores[nameof(entidad.Correo)] = "Escriba un correo válido; ejemplo: ventas@empresa.com.";
        if (entidad.SitioWeb.Length > 0 && !Validador.Web(entidad.SitioWeb)) errores[nameof(entidad.SitioWeb)] = "Escriba una URL completa: https://empresa.com.";
    }
    protected override void AntesEliminar(Suplidor entidad)
    {
        if (productos.Listar().Any(p => p.SuplidorId == entidad.Id)) throw new InvalidOperationException("No puede eliminar un suplidor con productos activos. Reasigne o elimine esos productos primero.");
    }
}
