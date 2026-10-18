using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
/// <summary>Catálogo de categorías con búsqueda por nombre y descripción.</summary>
public sealed class CategoriasControl : ListadoControl<Categoria>
{
    /// <summary>Inyecta los servicios.</summary>
    public CategoriasControl(ServiciosAplicacion servicios) : base(servicios) { filtros.Controls.Add(new Label { Text = "0–5 fotos por categoría · Máximo 2 MB por imagen · JPG / PNG", AutoSize = true, Padding = new Padding(4, 10, 0, 0) }); }
    protected override IServicio<Categoria> Servicio => servicios.Categorias;
    protected override void AplicarFiltro() => MostrarFilas(Buscador.Filtrar(datos, buscar.Text, c => c.Nombre + " " + c.Descripcion).Select(c => new { c.Id, c.Nombre, c.Descripcion, Fotos = c.Fotos.Count, FechaCreacion = c.FechaCreacion.ToLocalTime(), FechaModificacion = c.FechaModificacion.ToLocalTime() }).ToList());
    protected override Task EditarAsync(Categoria? entidad)
    {
        using var form = new CategoriaForm(servicios, entidad); form.ShowDialog(FindForm()); return Task.CompletedTask;
    }
}
