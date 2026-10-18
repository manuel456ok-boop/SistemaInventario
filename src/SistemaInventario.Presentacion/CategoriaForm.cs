using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
/// <summary>Edita una copia; cancelar nunca altera el objeto del listado.</summary>
public sealed class CategoriaForm : EditorForm
{
    private readonly int id;
    private readonly TextBox nombre;
    private readonly TextBox descripcion;
    private readonly List<FotoCategoria> fotos;
    private readonly ListBox lista = new() { Dock = DockStyle.Fill, DisplayMember = "NombreArchivo" };
    private readonly PictureBox vista = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White };
    /// <summary>Inicializa campos y previsualizador de fotos.</summary>
    public CategoriaForm(ServiciosAplicacion servicios, Categoria? entidad) : base(servicios, entidad == null ? "Nueva categoría" : "Editar categoría")
    {
        id = entidad?.Id ?? 0;
        fotos = entidad?.Fotos.Select(f => new FotoCategoria { NombreArchivo = f.NombreArchivo, Contenido = f.Contenido.ToArray() }).ToList() ?? new();
        nombre = Texto("Nombre", "Nombre *", entidad?.Nombre ?? "");
        descripcion = Texto("Descripcion", "Descripción *", entidad?.Descripcion ?? "", 105);
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55)); panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        panel.Controls.Add(lista, 0, 0); panel.Controls.Add(vista, 1, 0);
        var botones = new FlowLayoutPanel { Dock = DockStyle.Fill }; var agregar = Tema.Boton("Agregar"); var quitar = Tema.Boton("Quitar");
        botones.Controls.AddRange(new Control[] { agregar, quitar }); panel.Controls.Add(botones, 0, 1); panel.SetColumnSpan(botones, 2);
        Agregar("Fotos", "Fotos (0–5)", panel, 255);
        agregar.Click += (_, _) => AgregarFotos(); quitar.Click += (_, _) => { if (lista.SelectedIndex >= 0) { fotos.RemoveAt(lista.SelectedIndex); RefrescarFotos(); } };
        lista.SelectedIndexChanged += (_, _) => Previsualizar(); RefrescarFotos();
    }
    private void AgregarFotos()
    {
        using var dialogo = new OpenFileDialog { Filter = "Imágenes JPG y PNG|*.jpg;*.jpeg;*.png", Multiselect = true };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var nuevas = new List<FotoCategoria>();
            foreach (string ruta in dialogo.FileNames)
            {
                // Lee como máximo 2 MB + 1; evita cargar archivos grandes antes de validar.
                using var archivo = File.OpenRead(ruta);
                byte[] contenido = new byte[(int)Math.Min(archivo.Length, 2L * 1024 * 1024 + 1)]; archivo.ReadExactly(contenido);
                var foto = new FotoCategoria { NombreArchivo = Path.GetFileName(ruta), Contenido = contenido };
                string? error = Validador.Foto(foto);
                if (error != null) throw new ValidacionException(new Dictionary<string, string> { ["Fotos"] = error });
                nuevas.Add(foto);
            }
            if (fotos.Count + nuevas.Count > 5) throw new ValidacionException(new Dictionary<string, string> { ["Fotos"] = "Máximo cinco fotos por categoría." });
            fotos.AddRange(nuevas); errores.SetError(controles["Fotos"], ""); RefrescarFotos();
        }
        catch (ValidacionException ex) { errores.SetError(controles["Fotos"], ex.Message); Tema.MostrarError(ex, servicios.Logger); }
        catch (Exception ex) { Tema.MostrarError(ex, servicios.Logger); }
    }
    private void RefrescarFotos() { lista.DataSource = null; lista.DataSource = fotos.ToList(); lista.DisplayMember = "NombreArchivo"; Previsualizar(); }
    private void Previsualizar()
    {
        vista.Image?.Dispose(); vista.Image = null;
        if (lista.SelectedItem is not FotoCategoria foto) return;
        try { using var stream = new MemoryStream(foto.Contenido); using var imagen = Image.FromStream(stream); vista.Image = new Bitmap(imagen); }
        catch (ArgumentException) { errores.SetError(controles["Fotos"], "La imagen no se puede previsualizar. Quite ese archivo."); }
    }
    protected override Task GuardarAsync()
    {
        var entidad = new Categoria { Id = id, Nombre = nombre.Text, Descripcion = descripcion.Text, Fotos = fotos.ToList() };
        return Task.Run(() => servicios.Categorias.Guardar(entidad));
    }
    /// <summary>Libera la imagen GDI creada para la vista previa.</summary>
    protected override void Dispose(bool disposing) { if (disposing) vista.Image?.Dispose(); base.Dispose(disposing); }
}
