using System.Globalization;
using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
/// <summary>Editor con combos restringidos a asociaciones activas.</summary>
public sealed class ProductoForm : EditorForm
{
    private readonly int id;
    private readonly TextBox nombre, precio;
    private readonly ComboBox categoria = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox suplidor = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
    /// <summary>Recibe catálogos activos precargados.</summary>
    public ProductoForm(ServiciosAplicacion servicios, Producto? entidad, IReadOnlyList<Categoria> categorias, IReadOnlyList<Suplidor> suplidores) : base(servicios, entidad == null ? "Nuevo producto" : "Editar producto")
    {
        id = entidad?.Id ?? 0;
        nombre = Texto("Nombre", "Producto *", entidad?.Nombre ?? "");
        precio = Texto("PrecioUnitario", "Precio unitario (RD$) *", entidad?.PrecioUnitario.ToString("0.00", CultureInfo.CurrentCulture) ?? ""); precio.PlaceholderText = "Ejemplo: 1500.50";
        categoria.DisplayMember = "Nombre"; categoria.ValueMember = "Id";
        categoria.DataSource = new[] { new OpcionFiltro(0, "Seleccione una categoría") }.Concat(categorias.Select(c => new OpcionFiltro(c.Id, c.Nombre))).ToList();
        suplidor.DisplayMember = "Nombre"; suplidor.ValueMember = "Id";
        suplidor.DataSource = new[] { new OpcionFiltro(0, "Seleccione un suplidor") }.Concat(suplidores.Select(s => new OpcionFiltro(s.Id, s.NombreEmpresa))).ToList();
        categoria.SelectedValue = entidad?.CategoriaId ?? 0; suplidor.SelectedValue = entidad?.SuplidorId ?? 0;
        Agregar("CategoriaId", "Categoría *", categoria, 48); Agregar("SuplidorId", "Suplidor *", suplidor, 48);
    }
    protected override Task GuardarAsync()
    {
        decimal valor = PrecioEntrada.Leer(precio.Text, CultureInfo.CurrentCulture);
        var entidad = new Producto { Id = id, Nombre = nombre.Text, PrecioUnitario = valor, CategoriaId = (categoria.SelectedItem as OpcionFiltro)?.Id ?? 0, SuplidorId = (suplidor.SelectedItem as OpcionFiltro)?.Id ?? 0 };
        return Task.Run(() => servicios.Productos.Guardar(entidad));
    }
}
