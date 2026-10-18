using System.Globalization;
using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio;
namespace SistemaInventario.Pruebas;
internal static class Program
{
    private static int aprobadas, fallidas;
    private static void Prueba(string nombre, Action accion)
    {
        try { accion(); aprobadas++; Console.WriteLine("OK " + nombre); }
        catch (Exception ex) { fallidas++; Console.WriteLine("FALLO " + nombre + ": " + ex.Message); }
    }
    private static void Exigir(bool condicion) { if (!condicion) throw new Exception("No se cumplió la condición esperada."); }
    private static void ErrorCampo(Action accion, string campo)
    {
        try { accion(); } catch (ValidacionException ex) { Exigir(ex.Errores.ContainsKey(campo)); return; }
        throw new Exception("Debió rechazarse con ValidacionException: " + campo);
    }
    private static void ErrorOperacion(Action accion)
    {
        try { accion(); } catch (InvalidOperationException) { return; }
        throw new Exception("Debió rechazarse la operación.");
    }
    private static int Main()
    {
        foreach (string nombre in new[] { "", "  ", "ab", new string('x', 101) })
            Prueba("Categoría rechaza longitud " + nombre.Length, () => { var e = new Escenario(); ErrorCampo(() => e.CategoriaServicio.Guardar(new Categoria { Nombre = nombre, Descripcion = "Válida" }), "Nombre"); });
        foreach (int longitud in new[] { 3, 100 })
            Prueba("Categoría acepta longitud " + longitud, () => { var e = new Escenario(); e.CategoriaServicio.Guardar(new Categoria { Nombre = new string('x', longitud), Descripcion = "Válida" }); Exigir(e.Categorias.Listar().Count == 2); });
        foreach (string descripcion in new[] { "", "   ", new string('d', 501) })
            Prueba("Descripción rechaza longitud " + descripcion.Length, () => { var e = new Escenario(); ErrorCampo(() => e.CategoriaServicio.Guardar(new Categoria { Nombre = "Nueva", Descripcion = descripcion }), "Descripcion"); });
        Prueba("Descripción acepta 500", () => { var e = new Escenario(); e.CategoriaServicio.Guardar(new Categoria { Nombre = "Nueva", Descripcion = new string('x', 500) }); });
        Prueba("Nombre se recorta", () => { var e = new Escenario(); var c = new Categoria { Nombre = "  Nueva  ", Descripcion = " Texto " }; e.CategoriaServicio.Guardar(c); Exigir(c.Nombre == "Nueva" && c.Descripcion == "Texto"); });
        Prueba("Categoría duplicada ignora mayúsculas", () => { var e = new Escenario(); ErrorCampo(() => e.CategoriaServicio.Guardar(new Categoria { Nombre = "EQUIPOS", Descripcion = "Prueba" }), "Nombre"); });
        Prueba("Categoría duplicada incluye papelera", () => { var e = new Escenario(); e.CategoriaServicio.Eliminar(1); ErrorCampo(() => e.CategoriaServicio.Guardar(new Categoria { Nombre = "Equipos", Descripcion = "Prueba" }), "Nombre"); });
        foreach (string telefono in new[] { "809-123-4567", "829-123-4567", "849-123-4567" }) Prueba("Teléfono válido " + telefono, () => Exigir(Validador.Telefono(telefono)));
        foreach (string telefono in new[] { "8091234567", "800-123-4567", "809-12-4567", "809-123-45678", "" }) Prueba("Teléfono inválido " + telefono, () => Exigir(!Validador.Telefono(telefono)));
        foreach (string correo in new[] { "ana@empresa.com", "ana+ventas@sub.empresa.do" }) Prueba("Correo válido " + correo, () => Exigir(Validador.Correo(correo)));
        foreach (string correo in new[] { "ana@", "@empresa.com", "ana empresa.com", "ana@empresa", new string('a', 255) }) Prueba("Correo inválido " + correo.Length, () => Exigir(!Validador.Correo(correo)));
        foreach (string web in new[] { "https://empresa.com", "http://empresa.com/ruta" }) Prueba("Web válida " + web, () => Exigir(Validador.Web(web)));
        foreach (string web in new[] { "empresa.com", "ftp://empresa.com", "javascript:alert(1)", "https://" }) Prueba("Web inválida " + web, () => Exigir(!Validador.Web(web)));
        Prueba("Web opcional", () => { var e = new Escenario(); var s = Escenario.SuplidorValido(); s.Id = 1; s.SitioWeb = ""; e.SuplidorServicio.Guardar(s); });
        Prueba("Suplidor único", () => { var e = new Escenario(); ErrorCampo(() => e.SuplidorServicio.Guardar(Escenario.SuplidorValido()), "NombreEmpresa"); });
        Prueba("Contacto obligatorio", () => { var e = new Escenario(); var s = Escenario.SuplidorValido(); s.Id = 1; s.NombreContacto = ""; ErrorCampo(() => e.SuplidorServicio.Guardar(s), "NombreContacto"); });
        Prueba("Errores de contacto llegan por campo", () => { var e = new Escenario(); var s = Escenario.SuplidorValido(); s.Id = 1; s.Telefono = "123"; s.Correo = "mal"; s.SitioWeb = "mal"; ErrorCampo(() => e.SuplidorServicio.Guardar(s), "Telefono"); ErrorCampo(() => e.SuplidorServicio.Guardar(s), "Correo"); ErrorCampo(() => e.SuplidorServicio.Guardar(s), "SitioWeb"); });
        foreach (decimal precio in new[] { 0m, -1m, 12.345m, 10000000000000000m }) Prueba("Precio inválido " + precio, () => { var e = new Escenario(); ErrorCampo(() => e.ProductoServicio.Guardar(Escenario.ProductoValido(precio)), "PrecioUnitario"); });
        foreach (decimal precio in new[] { 0.01m, 12.30m, 9999999999999999.99m }) Prueba("Precio válido " + precio, () => { var e = new Escenario(); e.ProductoServicio.Guardar(Escenario.ProductoValido(precio)); });
        Prueba("Entrada no redondea", () => Exigir(PrecioEntrada.Leer("12.345", CultureInfo.InvariantCulture) == 12.345m));
        Prueba("Entrada rechaza texto", () => ErrorCampo(() => PrecioEntrada.Leer("abc", CultureInfo.InvariantCulture), "PrecioUnitario"));
        Prueba("Entrada rechaza miles", () => ErrorCampo(() => PrecioEntrada.Leer("1,000.00", CultureInfo.InvariantCulture), "PrecioUnitario"));
        Prueba("Producto exige nombre", () => { var e = new Escenario(); var p = Escenario.ProductoValido(); p.Nombre = ""; ErrorCampo(() => e.ProductoServicio.Guardar(p), "Nombre"); });
        Prueba("Producto exige categoría existente", () => { var e = new Escenario(); var p = Escenario.ProductoValido(); p.CategoriaId = 999; ErrorCampo(() => e.ProductoServicio.Guardar(p), "CategoriaId"); });
        Prueba("Producto exige suplidor existente", () => { var e = new Escenario(); var p = Escenario.ProductoValido(); p.SuplidorId = 0; ErrorCampo(() => e.ProductoServicio.Guardar(p), "SuplidorId"); });
        Prueba("Producto exige categoría activa", () => { var e = new Escenario(); e.CategoriaServicio.Eliminar(1); ErrorCampo(() => e.ProductoServicio.Guardar(Escenario.ProductoValido()), "CategoriaId"); });
        Prueba("Producto exige suplidor activo", () => { var e = new Escenario(); e.SuplidorServicio.Eliminar(1); ErrorCampo(() => e.ProductoServicio.Guardar(Escenario.ProductoValido()), "SuplidorId"); });
        Prueba("Borrado categoría protegido", () => { var e = new Escenario(); e.ProductoServicio.Guardar(Escenario.ProductoValido()); ErrorOperacion(() => e.CategoriaServicio.Eliminar(1)); Exigir(e.Categorias.Obtener(1)!.Activo); });
        Prueba("Borrado suplidor protegido", () => { var e = new Escenario(); e.ProductoServicio.Guardar(Escenario.ProductoValido()); ErrorOperacion(() => e.SuplidorServicio.Eliminar(1)); });
        Prueba("Eliminado no aparece activo", () => { var e = new Escenario(); e.ProductoServicio.Guardar(Escenario.ProductoValido()); e.ProductoServicio.Eliminar(1); Exigir(e.ProductoServicio.Listar().Count == 0 && e.ProductoServicio.Listar(false).Count == 1); });
        Prueba("No edita eliminado", () => { var e = new Escenario(); var p = Escenario.ProductoValido(); e.ProductoServicio.Guardar(p); e.ProductoServicio.Eliminar(p.Id); ErrorOperacion(() => e.ProductoServicio.Guardar(p)); });
        Prueba("Restauración exige categoría", () => { var e = new Escenario(); e.ProductoServicio.Guardar(Escenario.ProductoValido()); e.ProductoServicio.Eliminar(1); e.CategoriaServicio.Eliminar(1); ErrorCampo(() => e.ProductoServicio.Restaurar(1), "CategoriaId"); });
        Prueba("Restauración exige suplidor", () => { var e = new Escenario(); e.ProductoServicio.Guardar(Escenario.ProductoValido()); e.ProductoServicio.Eliminar(1); e.SuplidorServicio.Eliminar(1); ErrorCampo(() => e.ProductoServicio.Restaurar(1), "SuplidorId"); });
        Prueba("Ciclo completo de restauración", () => { var e = new Escenario(); e.ProductoServicio.Guardar(Escenario.ProductoValido()); e.ProductoServicio.Eliminar(1); e.CategoriaServicio.Eliminar(1); e.SuplidorServicio.Eliminar(1); e.CategoriaServicio.Restaurar(1); e.SuplidorServicio.Restaurar(1); e.ProductoServicio.Restaurar(1); Exigir(e.ProductoServicio.Listar().Count == 1); });
        Prueba("Registro inexistente", () => { var e = new Escenario(); ErrorOperacion(() => e.CategoriaServicio.Eliminar(999)); });
        Prueba("PNG firma válida", () => Exigir(Validador.Foto(new FotoCategoria { NombreArchivo = "foto.PNG", Contenido = new byte[] { 137,80,78,71,13,10,26,10 } }) == null));
        Prueba("JPEG firma válida", () => Exigir(Validador.Foto(new FotoCategoria { NombreArchivo = "foto.jpeg", Contenido = new byte[] { 255,216,255,1,255,217 } }) == null));
        Prueba("Foto rechaza extensión", () => Exigir(Validador.Foto(new FotoCategoria { NombreArchivo = "foto.gif", Contenido = new byte[] { 1 } }) != null));
        Prueba("Foto rechaza archivo vacío", () => Exigir(Validador.Foto(new FotoCategoria { NombreArchivo = "foto.png" }) != null));
        Prueba("Foto rechaza firma falsa", () => Exigir(Validador.Foto(new FotoCategoria { NombreArchivo = "foto.jpg", Contenido = new byte[] { 1,2,3,4 } }) != null));
        Prueba("Foto rechaza más de 2 MB", () => Exigir(Validador.Foto(new FotoCategoria { NombreArchivo = "foto.png", Contenido = new byte[2097153] }) != null));
        foreach (int cantidad in new[] { 0, 5, 6 }) Prueba("Límite de fotos " + cantidad, () => { var e = new Escenario(); var c = new Categoria { Nombre = "Imágenes", Descripcion = "Prueba", Fotos = Enumerable.Range(1,cantidad).Select(i => new FotoCategoria { NombreArchivo = $"{i}.png", Contenido = new byte[] { 137,80,78,71,13,10,26,10 } }).ToList() }; if (cantidad>5) ErrorCampo(() => e.CategoriaServicio.Guardar(c), "Fotos"); else e.CategoriaServicio.Guardar(c); });
        Prueba("Búsqueda genérica ignora mayúsculas", () => Exigir(Buscador.Filtrar(new[] { "Teclado", "MOUSE" }, "mou", x => x).Single() == "MOUSE"));
        var cs = new[] { new Categoria { Id=1, Nombre="Equipos" }, new Categoria { Id=2, Nombre="Vacía" } };
        var ss = new[] { new Suplidor { Id=1, NombreEmpresa="Caribe" }, new Suplidor { Id=2, NombreEmpresa="Este" } };
        var ps = new[] { new Producto { Id=1, Nombre="Laptop", CategoriaId=1, SuplidorId=1, PrecioUnitario=100 }, new Producto { Id=2, Nombre="Mouse", CategoriaId=1, SuplidorId=2, PrecioUnitario=20 }, new Producto { Id=3, Nombre="Teclado", CategoriaId=1, SuplidorId=1, PrecioUnitario=30 } };
        var vistas = ConsultasInventario.Proyectar(ps,cs,ss);
        Prueba("Joins conservan nombres", () => Exigir(vistas.Single(x=>x.Id==1).Suplidor == "Caribe"));
        Prueba("Filtro combinado e inclusivo", () => Exigir(ConsultasInventario.Filtrar(vistas,"mOu",1,2,20,20).Single().Id==2));
        Prueba("Filtro fuera de rango vacío", () => Exigir(ConsultasInventario.Filtrar(vistas,"",null,null,101,200).Count==0));
        Prueba("Rango invertido rechaza", () => { try { ConsultasInventario.Filtrar(vistas,"",null,null,50,20); } catch(ArgumentException) { return; } throw new Exception("No rechazó el rango"); });
        Prueba("Promedio y conteo por categoría", () => { var x=ConsultasInventario.PorCategoria(ps,cs).Single(x=>x.Categoria=="Equipos"); Exigir(x.Cantidad==3 && x.Promedio==50); });
        Prueba("Categoría vacía conservada", () => { var x=ConsultasInventario.PorCategoria(ps,cs).Single(x=>x.Categoria=="Vacía"); Exigir(x.Cantidad==0 && x.Promedio==0); });
        Prueba("Ranking de suplidores", () => Exigir(ConsultasInventario.TopSuplidores(ps,ss).First()==new ResumenSuplidor("Caribe",2)));
        Prueba("Sobre promedio global", () => Exigir(ConsultasInventario.SobrePromedio(vistas).Single().Id==1));
        Prueba("Informe vacío no divide por cero", () => Exigir(ConsultasInventario.SobrePromedio(Array.Empty<ProductoVista>()).Count==0));
        Prueba("Ranking excluye eliminados", () => { var lista=ps.Select(x=>new Producto { Id=x.Id, CategoriaId=x.CategoriaId, SuplidorId=x.SuplidorId, PrecioUnitario=x.PrecioUnitario, Activo=false }); Exigir(ConsultasInventario.TopSuplidores(lista,ss).Count==0); });
        Prueba("Ranking limita a cinco", () => { var suplidores=Enumerable.Range(1,7).Select(i=>new Suplidor { Id=i,NombreEmpresa=$"Suplidor {i}" }); var productos=Enumerable.Range(1,7).Select(i=>new Producto { SuplidorId=i }); Exigir(ConsultasInventario.TopSuplidores(productos,suplidores).Count==5); });
        Console.WriteLine($"RESULTADO: {aprobadas} aprobadas, {fallidas} fallidas.");
        return fallidas == 0 ? 0 : 1;
    }
}
