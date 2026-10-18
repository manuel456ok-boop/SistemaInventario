using System.Globalization;
namespace SistemaInventario.LogicaNegocio;
/// <summary>Interpreta el precio sin redondear silenciosamente los decimales del usuario.</summary>
public static class PrecioEntrada
{
    /// <summary>Acepta el separador decimal de la cultura; rechaza miles y texto inválido.</summary>
    public static decimal Leer(string texto, CultureInfo cultura)
    {
        if (!decimal.TryParse(texto.Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, cultura, out decimal valor))
            throw new ValidacionException(new Dictionary<string, string> { ["PrecioUnitario"] = "Escriba un precio numérico sin separadores de miles." });
        return valor;
    }
}
