using SistemaInventario.Entidades;
namespace SistemaInventario.Pruebas;
/// <summary>Doble de prueba; no representa una conexión SQL ni se usa en la aplicación.</summary>
public sealed class RepositorioMemoria<T> : IRepositorio<T> where T : EntidadBase
{
    private readonly List<T> registros = new();
    /// <summary>Lista el estado solicitado.</summary>
    public IReadOnlyList<T> Listar(bool activos = true) => registros.Where(x => x.Activo == activos).ToList();
    /// <summary>Obtiene por identidad.</summary>
    public T? Obtener(int id) => registros.FirstOrDefault(x => x.Id == id);
    /// <summary>Asigna identidad y conserva el registro.</summary>
    public int Crear(T entidad) { entidad.Id = registros.Count + 1; registros.Add(entidad); return entidad.Id; }
    /// <summary>Reemplaza el registro.</summary>
    public void Actualizar(T entidad) { registros[registros.FindIndex(x => x.Id == entidad.Id)] = entidad; }
    /// <summary>Simula borrado lógico.</summary>
    public void Eliminar(int id) { Obtener(id)!.Activo = false; }
    /// <summary>Simula restauración.</summary>
    public void Restaurar(int id) { Obtener(id)!.Activo = true; }
}
