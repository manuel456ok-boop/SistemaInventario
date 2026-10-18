using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
/// <summary>Editor de empresa y datos de contacto.</summary>
public sealed class SuplidorForm : EditorForm
{
    private readonly int id;
    private readonly TextBox empresa, contacto, telefono, correo, web;
    /// <summary>Muestra una copia de los valores originales.</summary>
    public SuplidorForm(ServiciosAplicacion servicios, Suplidor? entidad) : base(servicios, entidad == null ? "Nuevo suplidor" : "Editar suplidor")
    {
        id = entidad?.Id ?? 0;
        empresa = Texto("NombreEmpresa", "Empresa *", entidad?.NombreEmpresa ?? "");
        contacto = Texto("NombreContacto", "Contacto *", entidad?.NombreContacto ?? "");
        telefono = Texto("Telefono", "Teléfono *", entidad?.Telefono ?? ""); telefono.PlaceholderText = "809-555-0101";
        correo = Texto("Correo", "Correo *", entidad?.Correo ?? ""); correo.PlaceholderText = "ventas@empresa.com";
        web = Texto("SitioWeb", "Sitio web", entidad?.SitioWeb ?? ""); web.PlaceholderText = "https://empresa.com (opcional)";
    }
    protected override Task GuardarAsync()
    {
        var entidad = new Suplidor { Id = id, NombreEmpresa = empresa.Text, NombreContacto = contacto.Text, Telefono = telefono.Text, Correo = correo.Text, SitioWeb = web.Text };
        return Task.Run(() => servicios.Suplidores.Guardar(entidad));
    }
}
