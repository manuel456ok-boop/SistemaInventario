using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
/// <summary>Expone los resultados de las consultas LINQ en tablas separadas.</summary>
public sealed class InformesControl : UserControl
{
    private readonly ServiciosAplicacion servicios;
    private readonly DataGridView categorias = Tema.Tabla(), suplidores = Tema.Tabla(), destacados = Tema.Tabla();
    private readonly Label resumen = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Tema.Tinta, Padding = new Padding(5, 12, 0, 0) };
    private bool ocupado;
    /// <summary>Crea informe de categorías, ranking y productos sobre el promedio.</summary>
    public InformesControl(ServiciosAplicacion servicios)
    {
        this.servicios = servicios;
        var marco = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        marco.RowStyles.Add(new RowStyle(SizeType.Absolute, 55)); marco.RowStyles.Add(new RowStyle(SizeType.Absolute, 65)); marco.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var barra = new FlowLayoutPanel { Dock = DockStyle.Fill }; var actualizar = Tema.Boton("Actualizar", true); barra.Controls.Add(actualizar); marco.Controls.Add(barra, 0, 0); marco.Controls.Add(resumen, 0, 1);
        var pestanas = new TabControl { Dock = DockStyle.Fill };
        foreach (var par in new[] { ("Cantidad y promedio por categoría", categorias), ("Top 5 suplidores", suplidores), ("Productos sobre el promedio", destacados) })
        { var pagina = new TabPage(par.Item1); pagina.Controls.Add(par.Item2); pestanas.TabPages.Add(pagina); }
        marco.Controls.Add(pestanas, 0, 2); Controls.Add(marco);
        actualizar.Click += async (_, _) => await Cargar(); Load += async (_, _) => await Cargar();
    }
    private async Task Cargar()
    {
        if (ocupado) return; ocupado = true; Enabled = false; UseWaitCursor = true;
        try
        {
            var datos = await Task.Run(() => (Productos: servicios.Productos.Listar(), Categorias: servicios.Categorias.Listar(), Suplidores: servicios.Suplidores.Listar()));
            if (IsDisposed) return;
            categorias.DataSource = ConsultasInventario.PorCategoria(datos.Productos, datos.Categorias);
            suplidores.DataSource = ConsultasInventario.TopSuplidores(datos.Productos, datos.Suplidores);
            destacados.DataSource = ConsultasInventario.SobrePromedio(ConsultasInventario.Proyectar(datos.Productos, datos.Categorias, datos.Suplidores));
            Tema.Formatear(categorias); Tema.Formatear(suplidores); Tema.Formatear(destacados);
            decimal promedio = datos.Productos.Select(p => p.PrecioUnitario).DefaultIfEmpty(0m).Average();
            resumen.Text = $"{datos.Productos.Count} productos activos   ·   {datos.Categorias.Count} categorías   ·   {datos.Suplidores.Count} suplidores   ·   Precio promedio RD$ {promedio:N2}";
        }
        catch (Exception ex) { if (!IsDisposed) Tema.MostrarError(ex, servicios.Logger); }
        finally { if (!IsDisposed) { ocupado = false; Enabled = true; UseWaitCursor = false; } }
    }
}
