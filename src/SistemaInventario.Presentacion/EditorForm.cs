using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
/// <summary>Editor común que conecta errores por propiedad con ErrorProvider.</summary>
public abstract class EditorForm : Form
{
    protected readonly ServiciosAplicacion servicios;
    protected readonly TableLayoutPanel campos = new() { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true, Padding = new Padding(18) };
    protected readonly Dictionary<string, Control> controles = new();
    protected readonly ErrorProvider errores;
    private readonly Button guardar = Tema.Boton("Guardar", true);
    private readonly Button cancelar = Tema.Boton("Cancelar");
    private bool guardando;
    /// <summary>Prepara campos, comandos y ciclo de guardado asíncrono.</summary>
    protected EditorForm(ServiciosAplicacion servicios, string titulo)
    {
        this.servicios = servicios; Text = titulo; Width = 740; Height = 640; MinimumSize = new Size(660, 560); StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10); BackColor = Tema.Fondo; MinimizeBox = false; ShowInTaskbar = false;
        errores = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165)); campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var marco = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        marco.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); marco.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); marco.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
        marco.Controls.Add(new Label { Text = titulo, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 19, FontStyle.Bold), ForeColor = Tema.Tinta, Padding = new Padding(20, 16, 0, 0) }, 0, 0);
        marco.Controls.Add(campos, 0, 1);
        var botones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(15, 6, 15, 0) };
        botones.Controls.AddRange(new Control[] { cancelar, guardar }); marco.Controls.Add(botones, 0, 2); Controls.Add(marco);
        cancelar.Click += (_, _) => Close(); CancelButton = cancelar; AcceptButton = guardar;
        guardar.Click += async (_, _) =>
        {
            if (guardando) return;
            errores.Clear(); guardando = true; guardar.Enabled = false; cancelar.Enabled = false; campos.Enabled = false; UseWaitCursor = true;
            try { await GuardarAsync(); DialogResult = DialogResult.OK; Close(); }
            catch (ValidacionException ex)
            {
                foreach (var error in ex.Errores) if (controles.TryGetValue(error.Key, out var control)) errores.SetError(control, error.Value);
                MessageBox.Show(ex.Message, "Revise los campos señalados", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex) { Tema.MostrarError(ex, servicios.Logger); }
            finally { guardando = false; guardar.Enabled = cancelar.Enabled = campos.Enabled = true; UseWaitCursor = false; }
        };
        FormClosing += (_, e) => { if (guardando && DialogResult != DialogResult.OK) e.Cancel = true; };
    }
    protected TextBox Texto(string propiedad, string etiqueta, string valor, int alto = 38)
    {
        var control = new TextBox { Text = valor, Multiline = alto > 45, ScrollBars = alto > 45 ? ScrollBars.Vertical : ScrollBars.None, Dock = DockStyle.Top };
        if (alto > 45) control.Height = alto - 12;
        Agregar(propiedad, etiqueta, control, alto); return control;
    }
    protected void Agregar(string propiedad, string etiqueta, Control control, int alto)
    {
        int fila = campos.RowCount++; campos.RowStyles.Add(new RowStyle(SizeType.Absolute, alto));
        campos.Controls.Add(new Label { Text = etiqueta, AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, fila);
        control.Margin = new Padding(3, 3, 28, 3); campos.Controls.Add(control, 1, fila); controles[propiedad] = control;
    }
    protected abstract Task GuardarAsync();
    /// <summary>Libera el componente de validación y los controles.</summary>
    protected override void Dispose(bool disposing) { if (disposing) errores.Dispose(); base.Dispose(disposing); }
}
