using SistemaInventario.Entidades;
namespace SistemaInventario.LogicaNegocio;
/// <summary>Operaciones autorizadas para los formularios.</summary>
public interface IServicio<T> where T : EntidadBase
{
    /// <summary>Obtiene registros activos o eliminados.</summary>
    IReadOnlyList<T> Listar(bool activos = true);
    /// <summary>Valida e inserta o actualiza según la identidad.</summary>
    void Guardar(T entidad);
    /// <summary>Valida relaciones antes de desactivar.</summary>
    void Eliminar(int id);
    /// <summary>Valida la recuperación del registro.</summary>
    void Restaurar(int id);
}
