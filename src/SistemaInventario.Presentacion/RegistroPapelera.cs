namespace SistemaInventario.Presentacion;
/// <summary>Fila común para los tres tipos de registros eliminados.</summary>
public sealed record RegistroPapelera(int Id, string Tipo, string Nombre, DateTime FechaCreacion, DateTime FechaModificacion);
