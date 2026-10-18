using System.Diagnostics;
using SistemaInventario.Entidades;
namespace SistemaInventario.AccesoDatos;
/// <summary>Escribe errores en una carpeta con permiso del usuario.</summary>
public sealed class Logger : ILogger
{
    private static readonly object candado = new();
    public static string Ruta => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SistemaInventario", "errores.log");
    /// <summary>Serializa escrituras; un fallo del log nunca reemplaza el error original.</summary>
    public void Error(string operacion, Exception excepcion)
    {
        try
        {
            lock (candado)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Ruta)!);
                File.AppendAllText(Ruta, $"{DateTime.Now:O} | {operacion} | {excepcion}{Environment.NewLine}");
            }
        }
        catch (Exception errorLog) when (errorLog is IOException or UnauthorizedAccessException)
        {
            Debug.WriteLine(errorLog.Message);
        }
    }
}
