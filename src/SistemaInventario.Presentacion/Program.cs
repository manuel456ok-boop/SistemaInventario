using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Presentacion;
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            string ruta = Path.Combine(AppContext.BaseDirectory, "conexion.txt");
            string cadena = Environment.GetEnvironmentVariable("INVENTARIO_SQL") ?? File.ReadAllText(ruta).Trim();
            var servicios = Composicion.Crear(cadena);
            Application.ThreadException += (_, e) =>
            {
                servicios.Logger.Error("Interfaz", e.Exception);
                MessageBox.Show("Ocurrió un error inesperado. Consulte errores.log.", "Sistema de inventario", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            Application.Run(new PrincipalForm(servicios));
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo iniciar. Revise conexion.txt junto al ejecutable.\n" + ex.Message, "Inicio", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
