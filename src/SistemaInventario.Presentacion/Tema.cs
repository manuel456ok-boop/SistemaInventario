namespace SistemaInventario.Presentacion;
/// <summary>Componentes visuales uniformes, creados por código para facilitar su lectura.</summary>
public static class Tema
{
    public static Color Fondo => Color.FromArgb(244, 247, 251);
    public static Color Tinta => Color.FromArgb(25, 43, 64);
    public static Color Acento => Color.FromArgb(24, 105, 175);
    /// <summary>Crea un botón con tamaño y contraste consistentes.</summary>
    public static Button Boton(string texto, bool primario = false)
    {
        var boton = new Button { Text = texto, AutoSize = true, MinimumSize = new Size(105, 36), Height = 36, FlatStyle = FlatStyle.Flat, BackColor = primario ? Acento : Color.White, ForeColor = primario ? Color.White : Tinta, Cursor = Cursors.Hand, Margin = new Padding(4) };
        boton.FlatAppearance.BorderColor = primario ? Acento : Color.FromArgb(215, 224, 235);
        return boton;
    }
    /// <summary>Configura una tabla de solo lectura con selección por fila.</summary>
    public static DataGridView Tabla()
    {
        var tabla = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false, AutoGenerateColumns = true, EnableHeadersVisualStyles = false };
        tabla.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(230, 238, 247), ForeColor = Tinta, Font = new Font("Segoe UI", 10, FontStyle.Bold), Padding = new Padding(5) };
        tabla.DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Tinta, SelectionBackColor = Color.FromArgb(214, 233, 250), SelectionForeColor = Tinta, Padding = new Padding(4) };
        tabla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
        tabla.RowTemplate.Height = 34;
        tabla.ColumnHeadersHeight = 42;
        return tabla;
    }
    /// <summary>Formatea moneda y auditoría, ocultando claves técnicas.</summary>
    public static void Formatear(DataGridView tabla)
    {
        foreach (DataGridViewColumn columna in tabla.Columns)
        {
            if (columna.Name is "CategoriaId" or "SuplidorId") columna.Visible = false;
            if (columna.Name is "FechaCreacion" or "FechaModificacion") columna.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (columna.Name is "PrecioUnitario" or "Promedio") columna.DefaultCellStyle.Format = "N2";
            columna.HeaderText = columna.Name switch { "FechaCreacion" => "Creación", "FechaModificacion" => "Modificación", "NombreEmpresa" => "Empresa", "NombreContacto" => "Contacto", "PrecioUnitario" => "Precio (RD$)", "Descripcion" => "Descripción", "Telefono" => "Teléfono", "SitioWeb" => "Sitio web", "Promedio" => "Promedio (RD$)", _ => columna.Name };
            if (columna.Name == "Id") { columna.FillWeight = 35; columna.MinimumWidth = 45; }
            else columna.MinimumWidth = 85;
        }
    }
    /// <summary>Muestra una causa comprensible y registra las excepciones inesperadas.</summary>
    public static void MostrarError(Exception ex, SistemaInventario.Entidades.ILogger logger)
    {
        if (ex is not (SistemaInventario.Entidades.PersistenciaException or SistemaInventario.LogicaNegocio.ValidacionException or InvalidOperationException or ArgumentException)) logger.Error("Formulario", ex);
        MessageBox.Show(ex.Message, "No se pudo completar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
