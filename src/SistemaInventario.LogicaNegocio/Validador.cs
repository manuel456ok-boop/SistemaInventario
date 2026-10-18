using System.Text.RegularExpressions;
using SistemaInventario.Entidades;
namespace SistemaInventario.LogicaNegocio;
/// <summary>Reglas reutilizadas por los servicios; la UI solo presenta errores.</summary>
public static class Validador
{
    /// <summary>Valida texto obligatorio, después de quitar espacios extremos.</summary>
    public static void Texto(IDictionary<string, string> errores, string campo, string? valor, int minimo, int maximo)
    {
        int longitud = valor?.Trim().Length ?? 0;
        if (longitud < minimo || longitud > maximo) errores[campo] = $"Debe tener entre {minimo} y {maximo} caracteres.";
    }
    /// <summary>Valida correo mediante regex con tiempo máximo.</summary>
    public static bool Correo(string valor) => valor.Length <= 254 && Regex.IsMatch(valor, @"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(200));
    /// <summary>Exige un teléfono dominicano con guiones.</summary>
    public static bool Telefono(string valor) => Regex.IsMatch(valor, @"^(809|829|849)-\d{3}-\d{4}$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(200));
    /// <summary>Acepta exclusivamente URL absolutas HTTP o HTTPS.</summary>
    public static bool Web(string valor) => valor.Length <= 500 && Uri.TryCreate(valor, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) && !string.IsNullOrWhiteSpace(uri.Host);
    /// <summary>Comprueba extensión, tamaño y firma para no aceptar texto renombrado.</summary>
    public static string? Foto(FotoCategoria foto)
    {
        string extension = Path.GetExtension(foto.NombreArchivo).ToLowerInvariant();
        if (foto.NombreArchivo.Length == 0 || foto.NombreArchivo.Length > 255) return "El nombre de la imagen debe tener de 1 a 255 caracteres.";
        if (extension is not (".jpg" or ".jpeg" or ".png")) return "Solo se permiten imágenes JPG, JPEG o PNG.";
        if (foto.Contenido.Length == 0 || foto.Contenido.Length > 2 * 1024 * 1024) return "Cada foto debe ocupar entre 1 byte y 2 MB.";
        byte[] b = foto.Contenido;
        bool png = b.Length >= 8 && b.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        bool jpg = b.Length >= 4 && b[0] == 255 && b[1] == 216 && b[2] == 255 && b[^2] == 255 && b[^1] == 217;
        if (extension == ".png" ? !png : !jpg) return "El contenido de la foto no coincide con su extensión.";
        return null;
    }
}
