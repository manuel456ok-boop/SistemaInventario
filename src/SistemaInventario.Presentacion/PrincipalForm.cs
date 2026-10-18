using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
/// <summary>Contenedor principal con navegación a los cinco módulos.</summary>
public sealed class PrincipalForm : Form
{
    private readonly Panel contenido = new() { Dock = DockStyle.Fill, Padding = new Padding(22), BackColor = Tema.Fondo };
    private readonly Label titulo = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Tema.Tinta, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(22, 0, 0, 0) };
    private readonly ServiciosAplicacion servicios;
    /// <summary>Recibe todos los servicios sin referenciar AccesoDatos.</summary>
    public PrincipalForm(ServiciosAplicacion servicios)
    {
        this.servicios = servicios;
        Text = "Sistema de Inventario | ISW-124"; Width = 1350; Height = 820; MinimumSize = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen; Font = new Font("Segoe UI", 10); BackColor = Tema.Fondo;
        var estructura = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 215)); estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 90)); estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var menu = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Tema.Tinta, Padding = new Padding(16), Margin = Padding.Empty };
        menu.Controls.Add(new Label { Text = "INVENTARIO\nISW-124 · C#", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), Width = 180, Height = 105 });
        AgregarMenu(menu, "Productos", () => new ProductosControl(servicios));
        AgregarMenu(menu, "Categorías", () => new CategoriasControl(servicios));
        AgregarMenu(menu, "Suplidores", () => new SuplidoresControl(servicios));
        AgregarMenu(menu, "Papelera", () => new PapeleraControl(servicios));
        AgregarMenu(menu, "Informes LINQ", () => new InformesControl(servicios));
        estructura.Controls.Add(menu, 0, 0); estructura.SetRowSpan(menu, 2);
        estructura.Controls.Add(titulo, 1, 0); estructura.Controls.Add(contenido, 1, 1); Controls.Add(estructura);
        Shown += (_, _) => Mostrar("Productos", new ProductosControl(servicios));
    }
    private void AgregarMenu(FlowLayoutPanel menu, string texto, Func<Control> crear)
    {
        var boton = Tema.Boton(texto); boton.Width = 175; boton.Height = 43; boton.AutoSize = false; boton.TextAlign = ContentAlignment.MiddleLeft;
        boton.Click += (_, _) => Mostrar(texto, crear()); menu.Controls.Add(boton);
    }
    private void Mostrar(string nombre, Control control)
    {
        foreach (Control anterior in contenido.Controls.Cast<Control>().ToArray()) { contenido.Controls.Remove(anterior); anterior.Dispose(); }
        titulo.Text = nombre; control.Dock = DockStyle.Fill; contenido.Controls.Add(control);
    }
}
