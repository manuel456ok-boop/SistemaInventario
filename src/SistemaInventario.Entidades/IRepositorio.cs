namespace SistemaInventario.Entidades;
/// <summary>Contrato genérico: el negocio solicita operaciones sin conocer SQL.</summary>
public interface IRepositorio<T> where T : EntidadBase
{
    /// <summary>Lista activos o eliminados según el argumento.</summary>
    IReadOnlyList<T> Listar(bool activos = true);
    /// <summary>Busca por identidad, incluyendo eliminados.</summary>
    T? Obtener(int id);
    /// <summary>Inserta y devuelve la identidad asignada.</summary>
    int Crear(T entidad);
    /// <summary>Actualiza un registro activo.</summary>
    void Actualizar(T entidad);
    /// <summary>Cambia Activo a falso, sin borrar físicamente.</summary>
    void Eliminar(int id);
    /// <summary>Recupera un registro eliminado.</summary>
    void Restaurar(int id);
}
