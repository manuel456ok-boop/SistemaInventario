using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
/// <summary>Catálogo de empresas con búsqueda en todos los datos de contacto.</summary>
public sealed class SuplidoresControl : ListadoControl<Suplidor>
{
    /// <summary>Inyecta servicios.</summary>
    public SuplidoresControl(ServiciosAplicacion servicios) : base(servicios) { filtros.Controls.Add(new Label { Text = "Teléfono dominicano: 809/829/849-XXX-XXXX", AutoSize = true, Padding = new Padding(4, 10, 0, 0) }); }
    protected override IServicio<Suplidor> Servicio => servicios.Suplidores;
    protected override void AplicarFiltro() => MostrarFilas(Buscador.Filtrar(datos, buscar.Text, s => $"{s.NombreEmpresa} {s.NombreContacto} {s.Telefono} {s.Correo} {s.SitioWeb}").Select(s => new { s.Id, s.NombreEmpresa, s.NombreContacto, s.Telefono, s.Correo, s.SitioWeb, FechaCreacion = s.FechaCreacion.ToLocalTime(), FechaModificacion = s.FechaModificacion.ToLocalTime() }).ToList());
    protected override Task EditarAsync(Suplidor? entidad)
    { using var form = new SuplidorForm(servicios, entidad); form.ShowDialog(FindForm()); return Task.CompletedTask; }
}
