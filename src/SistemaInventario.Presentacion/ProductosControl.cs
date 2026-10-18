using System.Globalization;
using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
/// <summary>Filtra una instantánea local sin consultar SQL por cada tecla.</summary>
public sealed class ProductosControl : ListadoControl<Producto>
{
    private readonly ComboBox categoria = new() { Width = 175, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox suplidor = new() { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox minimo = new() { Width = 105, PlaceholderText = "Precio mín." };
    private readonly TextBox maximo = new() { Width = 105, PlaceholderText = "Precio máx." };
    private IReadOnlyList<Categoria> categorias = Array.Empty<Categoria>();
    private IReadOnlyList<Suplidor> suplidores = Array.Empty<Suplidor>();
    private IReadOnlyList<ProductoVista> filas = Array.Empty<ProductoVista>();
    /// <summary>Crea filtros combinables; precio vacío significa sin límite.</summary>
    public ProductosControl(ServiciosAplicacion servicios) : base(servicios)
    {
        filtros.Controls.AddRange(new Control[] { categoria, suplidor, minimo, maximo });
        var limpiar = Tema.Boton("Limpiar filtros"); filtros.Controls.Add(limpiar);
        categoria.SelectedIndexChanged += (_, _) => AplicarSeguro(); suplidor.SelectedIndexChanged += (_, _) => AplicarSeguro();
        minimo.TextChanged += (_, _) => AplicarSeguro(); maximo.TextChanged += (_, _) => AplicarSeguro();
        limpiar.Click += (_, _) => { buscar.Clear(); categoria.SelectedIndex = 0; suplidor.SelectedIndex = 0; minimo.Clear(); maximo.Clear(); };
    }
    protected override IServicio<Producto> Servicio => servicios.Productos;
    protected override void CargarDatos()
    {
        datos = Servicio.Listar(); categorias = servicios.Categorias.Listar(); suplidores = servicios.Suplidores.Listar();
        filas = ConsultasInventario.Proyectar(datos, categorias, suplidores);
    }
    private void CargarOpciones(ComboBox combo, IEnumerable<OpcionFiltro> opciones)
    {
        int actual = (combo.SelectedItem as OpcionFiltro)?.Id ?? 0;
        combo.DisplayMember = "Nombre"; combo.ValueMember = "Id";
        var lista = opciones.ToList(); combo.DataSource = lista; combo.SelectedValue = lista.Any(x => x.Id == actual) ? actual : 0;
    }
    protected override void AplicarFiltro()
    {
        // El enlace de combos se hace en el hilo de UI después de cargar la instantánea.
        if (!ReferenceEquals(categoria.Tag, categorias))
        {
            categoria.Tag = categorias; suplidor.Tag = suplidores;
            CargarOpciones(categoria, new[] { new OpcionFiltro(0, "Todas las categorías") }.Concat(categorias.Select(c => new OpcionFiltro(c.Id, c.Nombre))));
            CargarOpciones(suplidor, new[] { new OpcionFiltro(0, "Todos los suplidores") }.Concat(suplidores.Select(s => new OpcionFiltro(s.Id, s.NombreEmpresa))));
        }
        int c = (categoria.SelectedItem as OpcionFiltro)?.Id ?? 0; int s = (suplidor.SelectedItem as OpcionFiltro)?.Id ?? 0;
        MostrarFilas(ConsultasInventario.Filtrar(filas, buscar.Text, c == 0 ? null : c, s == 0 ? null : s, LeerPrecio(minimo.Text), LeerPrecio(maximo.Text)).Select(p => p with { FechaCreacion = p.FechaCreacion.ToLocalTime(), FechaModificacion = p.FechaModificacion.ToLocalTime() }).ToList());
    }
    private static decimal? LeerPrecio(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        if (!decimal.TryParse(texto, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out decimal valor)) throw new ArgumentException("Escriba precios sin separador de miles; ejemplo: 1500.50.");
        return valor;
    }
    protected override async Task EditarAsync(Producto? entidad)
    {
        var opciones = await Task.Run(() => (Categorias: servicios.Categorias.Listar(), Suplidores: servicios.Suplidores.Listar()));
        using var form = new ProductoForm(servicios, entidad, opciones.Categorias, opciones.Suplidores); form.ShowDialog(FindForm());
    }
}
