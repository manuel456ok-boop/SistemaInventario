using SistemaInventario.Entidades;
namespace SistemaInventario.LogicaNegocio;
/// <summary>Plantilla del CRUD: validar siempre antes de persistir.</summary>
public abstract class ServicioBase<T> : IServicio<T> where T : EntidadBase
{
    protected readonly IRepositorio<T> repositorio;
    /// <summary>Inyecta un contrato, sin crear un repositorio concreto.</summary>
    protected ServicioBase(IRepositorio<T> repositorio) { this.repositorio = repositorio; }
    protected abstract void Validar(T entidad, Dictionary<string, string> errores);
    protected virtual void AntesEliminar(T entidad) { }
    protected virtual void AntesRestaurar(T entidad) { }
    /// <summary>Lista únicamente el estado solicitado.</summary>
    public IReadOnlyList<T> Listar(bool activos = true) => repositorio.Listar(activos);
    /// <summary>Impide editar eliminados y aplica todas las validaciones.</summary>
    public void Guardar(T entidad)
    {
        if (entidad.Id != 0 && !Exigir(entidad.Id).Activo) throw new InvalidOperationException("Restaure el registro antes de editarlo.");
        var errores = new Dictionary<string, string>();
        Validar(entidad, errores);
        if (errores.Count > 0) throw new ValidacionException(errores);
        if (entidad.Id == 0) entidad.Id = repositorio.Crear(entidad);
        else repositorio.Actualizar(entidad);
    }
    /// <summary>Rechaza borrados repetidos y verifica dependencias activas.</summary>
    public void Eliminar(int id)
    {
        T entidad = Exigir(id);
        if (!entidad.Activo) throw new InvalidOperationException("El registro ya está en la papelera.");
        AntesEliminar(entidad);
        repositorio.Eliminar(id);
    }
    /// <summary>Restaura solamente si las reglas actuales lo permiten.</summary>
    public void Restaurar(int id)
    {
        T entidad = Exigir(id);
        if (entidad.Activo) throw new InvalidOperationException("El registro ya está activo.");
        AntesRestaurar(entidad);
        repositorio.Restaurar(id);
    }
    protected T Exigir(int id) => repositorio.Obtener(id) ?? throw new InvalidOperationException("El registro ya no existe.");
    protected IEnumerable<T> Todos() => repositorio.Listar().Concat(repositorio.Listar(false));
}
