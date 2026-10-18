using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
/// <summary>Permite buscar y restaurar categorías, suplidores y productos.</summary>
public sealed class PapeleraControl : UserControl
{
    private readonly ServiciosAplicacion servicios;
    private readonly DataGridView tabla = Tema.Tabla();
    private readonly TextBox buscar = new() { Width = 280, PlaceholderText = "Buscar eliminados…", Margin = new Padding(4, 8, 10, 4) };
    private readonly ComboBox tipo = new() { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(4, 8, 10, 4) };
    private readonly Button restaurar = Tema.Boton("Restaurar", true);
    private readonly Label estado = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
    private List<RegistroPapelera> registros = new();
    private bool ocupado;
    /// <summary>Recibe servicios y arma búsqueda, selección y restauración.</summary>
    public PapeleraControl(ServiciosAplicacion servicios)
    {
        this.servicios = servicios; tipo.Items.AddRange(new[] { "Todos", "Categoría", "Suplidor", "Producto" }); tipo.SelectedIndex = 0;
        var marco = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        marco.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); marco.RowStyles.Add(new RowStyle(SizeType.Absolute, 54)); marco.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); marco.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        var barra = new FlowLayoutPanel { Dock = DockStyle.Fill }; var actualizar = Tema.Boton("Actualizar");
        barra.Controls.AddRange(new Control[] { buscar, tipo, restaurar, actualizar }); marco.Controls.Add(barra, 0, 0);
        marco.Controls.Add(new Label { Text = "Para restaurar un producto, restaure primero su categoría y su suplidor. Las fotos se conservan con su categoría.", Dock = DockStyle.Fill, ForeColor = Tema.Tinta }, 0, 1);
        marco.Controls.Add(tabla, 0, 2); marco.Controls.Add(estado, 0, 3); Controls.Add(marco);
        buscar.TextChanged += (_, _) => Filtrar(); tipo.SelectedIndexChanged += (_, _) => Filtrar();
        tabla.SelectionChanged += (_, _) => restaurar.Enabled = !ocupado && tabla.CurrentRow != null;
        actualizar.Click += async (_, _) => await Ejecutar(Cargar);
        restaurar.Click += async (_, _) => await Ejecutar(async () =>
        {
            if (tabla.CurrentRow?.DataBoundItem is not RegistroPapelera registro) return;
            if (MessageBox.Show($"¿Restaurar '{registro.Nombre}'?", "Restauración", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            await Task.Run(() =>
            {
                switch (registro.Tipo)
                {
                    case "Categoría": servicios.Categorias.Restaurar(registro.Id); break;
                    case "Suplidor": servicios.Suplidores.Restaurar(registro.Id); break;
                    case "Producto": servicios.Productos.Restaurar(registro.Id); break;
                }
            });
            await Cargar();
        });
        Load += async (_, _) => await Ejecutar(Cargar);
    }
    private async Task Cargar()
    {
        registros = await Task.Run(() => servicios.Categorias.Listar(false).Select(c => new RegistroPapelera(c.Id, "Categoría", c.Nombre, c.FechaCreacion.ToLocalTime(), c.FechaModificacion.ToLocalTime()))
            .Concat(servicios.Suplidores.Listar(false).Select(s => new RegistroPapelera(s.Id, "Suplidor", s.NombreEmpresa, s.FechaCreacion.ToLocalTime(), s.FechaModificacion.ToLocalTime())))
            .Concat(servicios.Productos.Listar(false).Select(p => new RegistroPapelera(p.Id, "Producto", p.Nombre, p.FechaCreacion.ToLocalTime(), p.FechaModificacion.ToLocalTime()))).ToList());
        if (!IsDisposed) Filtrar();
    }
    private void Filtrar()
    {
        tabla.DataSource = Buscador.Filtrar(registros.Where(r => tipo.SelectedIndex == 0 || r.Tipo == tipo.Text), buscar.Text, r => r.Nombre + " " + r.Tipo).ToList();
        Tema.Formatear(tabla); estado.Text = $"{tabla.Rows.Count} eliminados · Seleccione una fila para restaurar";
    }
    private async Task Ejecutar(Func<Task> accion)
    {
        if (ocupado) return; ocupado = true; Enabled = false; UseWaitCursor = true;
        try { await accion(); }
        catch (Exception ex) { if (!IsDisposed) Tema.MostrarError(ex, servicios.Logger); }
        finally { if (!IsDisposed) { ocupado = false; Enabled = true; UseWaitCursor = false; restaurar.Enabled = tabla.CurrentRow != null; } }
    }
}
