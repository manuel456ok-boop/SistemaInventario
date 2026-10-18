using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
/// <summary>Reutiliza carga asíncrona, búsqueda, selección y comandos del CRUD.</summary>
public abstract class ListadoControl<T> : UserControl where T : EntidadBase
{
    protected readonly ServiciosAplicacion servicios;
    protected readonly DataGridView tabla = Tema.Tabla();
    protected readonly TextBox buscar = new() { Width = 300, PlaceholderText = "Buscar en tiempo real…", Margin = new Padding(4, 8, 12, 4) };
    protected readonly FlowLayoutPanel filtros = new() { Dock = DockStyle.Fill, AutoScroll = true };
    protected IReadOnlyList<T> datos = Array.Empty<T>();
    private readonly Button editar = Tema.Boton("Editar");
    private readonly Button eliminar = Tema.Boton("Eliminar");
    private readonly Label estado = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Tema.Tinta };
    private bool ocupado;
    /// <summary>Arma el listado; las subclases aportan datos y formularios.</summary>
    protected ListadoControl(ServiciosAplicacion servicios)
    {
        this.servicios = servicios;
        var marco = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        marco.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); marco.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        marco.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); marco.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        var comandos = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var nuevo = Tema.Boton("+ Nuevo", true); var actualizar = Tema.Boton("Actualizar");
        comandos.Controls.AddRange(new Control[] { buscar, nuevo, editar, eliminar, actualizar });
        marco.Controls.Add(comandos, 0, 0); marco.Controls.Add(filtros, 0, 1); marco.Controls.Add(tabla, 0, 2); marco.Controls.Add(estado, 0, 3); Controls.Add(marco);
        buscar.TextChanged += (_, _) => AplicarSeguro();
        tabla.SelectionChanged += (_, _) => ActualizarBotones();
        tabla.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditarSeleccion(); };
        nuevo.Click += async (_, _) => await Ejecutar(async () => { await EditarAsync(null); await Recargar(); });
        editar.Click += async (_, _) => await EditarSeleccion();
        actualizar.Click += async (_, _) => await Ejecutar(Recargar);
        eliminar.Click += async (_, _) => await Ejecutar(async () =>
        {
            T? seleccionado = Seleccionado(); if (seleccionado == null) return;
            if (MessageBox.Show("¿Enviar el registro seleccionado a la papelera?", "Borrado lógico", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            await Task.Run(() => Servicio.Eliminar(seleccionado.Id)); await Recargar();
        });
        Load += async (_, _) => await Ejecutar(Recargar);
    }
    protected abstract IServicio<T> Servicio { get; }
    protected virtual void CargarDatos() { datos = Servicio.Listar(); }
    protected abstract void AplicarFiltro();
    protected abstract Task EditarAsync(T? entidad);
    protected void MostrarFilas(object filas)
    {
        tabla.DataSource = filas; Tema.Formatear(tabla); estado.Text = $"{tabla.Rows.Count} registros · Seleccione una fila para editar o eliminar · Fechas en hora local"; ActualizarBotones();
    }
    protected void AplicarSeguro()
    {
        if (IsDisposed) return;
        try { AplicarFiltro(); }
        catch (ArgumentException ex) { estado.Text = ex.Message; tabla.DataSource = null; }
    }
    private async Task Recargar()
    {
        await Task.Run(CargarDatos);
        if (!IsDisposed) AplicarSeguro();
    }
    private T? Seleccionado()
    {
        if (tabla.CurrentRow == null || !tabla.Columns.Contains("Id")) return null;
        int id = Convert.ToInt32(tabla.CurrentRow.Cells["Id"].Value);
        return datos.FirstOrDefault(x => x.Id == id);
    }
    private void ActualizarBotones() { editar.Enabled = eliminar.Enabled = !ocupado && tabla.CurrentRow != null; }
    private async Task EditarSeleccion() => await Ejecutar(async () => { var seleccionado = Seleccionado(); if (seleccionado != null) { await EditarAsync(seleccionado); await Recargar(); } });
    private async Task Ejecutar(Func<Task> accion)
    {
        if (ocupado) return;
        ocupado = true; Enabled = false; UseWaitCursor = true;
        try { await accion(); }
        catch (Exception ex) { if (!IsDisposed) { estado.Text = "No se completó la operación. Corrija la causa y pulse Actualizar."; Tema.MostrarError(ex, servicios.Logger); } }
        finally { if (!IsDisposed) { ocupado = false; Enabled = true; UseWaitCursor = false; ActualizarBotones(); } }
    }
}
