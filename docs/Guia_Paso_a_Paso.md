# Guía para entender y defender el proyecto

**Asignatura:** ISW-124 Programación Avanzada. **Tecnologías:** C#, Windows Forms, SQL Server y .NET 8.

Esta guía explica las decisiones y el recorrido completo de una operación. El archivo `Codigo_Explicado.html` acompaña la guía con el código fuente numerado y una explicación de cada línea. Los archivos `.cs` contienen comentarios cortos y documentación XML en sus métodos públicos. Para estudiar, siga el orden de esta guía y abra después cada archivo en el explorador HTML.

## 1. Qué resuelve la aplicación

Una categoría clasifica los productos. Un suplidor identifica la empresa que los vende. Un producto almacena su nombre, precio y las identidades de su categoría y suplidor. Una categoría puede contener muchos productos y un suplidor puede suministrar muchos productos; cada producto tiene exactamente una categoría y un suplidor.

Ejemplo: “Mouse óptico”, precio RD$450.00, categoría “Periféricos”, suplidor “Tecno Caribe”. El producto guarda las claves numéricas de esos dos registros. No repite el texto de categoría o empresa en su tabla. Por eso, al renombrar una categoría, los productos siguen asociados al mismo registro.

La papelera cambia `Activo` de `true` a `false`. El registro conserva su identidad y no se destruye. Restaurarlo vuelve a activar la misma fila. El historial de fechas permite conocer cuándo se creó y cuándo ocurrió la última modificación, que incluye eliminar y restaurar.

## 2. Preparación y primera ejecución

1. Instale SQL Server o SQL Server Express y SQL Server Management Studio. El primero es el motor que conserva los datos; el segundo sirve para administrarlo.
2. Conéctese desde SSMS. Copie el nombre del servidor que funcionó.
3. Abra y ejecute `database/schema.sql`. `GO` separa lotes para SSMS. Primero se crea la base si no existe; después las tablas, restricciones, índices y triggers.
4. Ejecute `database/datos.sql`. Inserta la muestra ficticia con cinco categorías, cinco suplidores y veinte productos. Resuelve las asociaciones por nombre; no depende de que las identidades comiencen en uno. Puede repetir este script sin duplicar los nombres de muestra. No reactiva registros que usted haya eliminado.
5. Cambie `Server` en el `conexion.txt` del ejecutable. Copie literalmente el valor que funcionó en SSMS. El archivo es texto plano: una barra se escribe una sola vez; el escape de literales C# no se aplica aquí.
6. Instale .NET 8 Desktop Runtime x64 y ejecute `build/win-x64/SistemaInventario.Presentacion.exe`. Conserve todos los archivos de esa carpeta.

Para trabajar con el código, abra `src/SistemaInventario.sln` en Visual Studio. Elija Presentación como proyecto de inicio. `Program.Main()` es el punto de entrada. Presione F5; Visual Studio compila y abre el formulario principal.

Si desea entregar un ejecutable sin instalar el runtime en otro equipo, compile en su Windows con `./compilar.ps1 -Autocontenido`. La publicación incluida necesita el runtime de escritorio; no es autocontenida.

## 3. Por qué existen cuatro proyectos

| Proyecto | Pregunta que responde | Ejemplo |
|---|---|---|
| Entidades | ¿Qué información y contratos maneja el sistema? | `Producto`, `Categoria`, `IRepositorio<T>` |
| AccesoDatos | ¿Cómo se guarda y se recupera? | `ProductoRepositorio`, SQL parametrizado |
| LogicaNegocio | ¿Qué se permite hacer? | No guardar un precio negativo |
| Presentacion | ¿Cómo interactúa el usuario? | Formularios, botones y `ErrorProvider` |

Esta separación permite cambiar el formulario sin rehacer las reglas o cambiar el repositorio sin alterar el formulario. Las pruebas sustituyen los repositorios SQL por repositorios en memoria; las mismas clases de negocio siguen funcionando. Eso demuestra una utilidad concreta de separar responsabilidades.

La presentación referencia Entidades y Negocio. Negocio conoce contratos y Datos, pero la creación de las implementaciones concretas se concentra en `Composicion`. Los servicios no hacen `new ProductoRepositorio`; lo reciben por constructor. `PrivateAssets="compile"` en la referencia de Negocio hacia Datos bloquea el uso de tipos de Datos desde Presentación y permite incluir sus dependencias de ejecución al publicar. Se comprobó que intentar crear `AccesoDatos.Logger` desde Presentación produce un error de compilación.

## 4. Paso uno: modelar las entidades y aplicar POO

Abra `EntidadBase.cs`:

```csharp
public abstract class EntidadBase
```

`public` permite usar el tipo desde otros proyectos. `abstract` indica que no se crean objetos de esa clase directamente; contiene lo común a otras entidades. `class` define un tipo de referencia.

`Id` identifica el registro. `Activo` marca si pertenece al catálogo o a la papelera. `FechaCreacion` y `FechaModificacion` almacenan la auditoría. `{ get; set; }` permite leer y asignar una propiedad. `= true` significa que un objeto nuevo nace activo.

Después abra `Producto.cs`. `Producto : EntidadBase` expresa **herencia**: obtiene las propiedades de auditoría sin repetirlas. `sealed` impide crear subclases de Producto. Agrega `Nombre`, `PrecioUnitario`, `CategoriaId` y `SuplidorId`. El precio es `decimal`, adecuado para importes con decimales; se evita `double`, cuya representación binaria puede introducir diferencias de precisión.

`Categoria` agrega una `List<FotoCategoria>`. Una lista es una colección de longitud variable. `new()` usa el tipo declarado a la izquierda y crea una lista vacía. Nunca se deja en `null`, lo que simplifica recorrerla y contarla.

`FotoCategoria.Contenido` es un arreglo de bytes. Guarda el contenido de la imagen, no su ubicación en el equipo. Por eso el archivo puede desaparecer de Descargas y la imagen seguirá existiendo en la base. `Array.Empty<byte>()` establece un arreglo vacío sin crear un arreglo independiente innecesario.

`Suplidor` contiene nombre de empresa, contacto, teléfono, correo y sitio web. El sitio opcional se representa con una cadena vacía en C# y con `NULL` en SQL. El repositorio convierte entre ambas representaciones.

Los principios de POO utilizados son:

- **Abstracción:** `IRepositorio<T>` expresa qué operaciones se necesitan y oculta cómo se realizan.
- **Encapsulación:** los repositorios privados del servicio no se manipulan desde el formulario; la UI pasa por operaciones públicas validadas.
- **Herencia:** entidades derivan de `EntidadBase`; repositorios de `RepositorioSql<T>`; editores de `EditorForm`.
- **Polimorfismo:** un `IRepositorio<Producto>` puede contener `ProductoRepositorio` o `RepositorioMemoria<Producto>`.

## 5. Paso dos: definir el repositorio genérico

Abra `IRepositorio.cs`:

```csharp
public interface IRepositorio<T> where T : EntidadBase
```

`interface` declara un contrato. El prefijo `I` sigue la convención solicitada. `T` representa un tipo que se decidirá al usar el contrato. La restricción `where T : EntidadBase` garantiza que ese tipo tiene `Id` y `Activo`.

`IRepositorio<Categoria>` trabaja con categorías; `IRepositorio<Producto>` trabaja con productos. El algoritmo común no se escribe tres veces.

Las operaciones son `Listar`, `Obtener`, `Crear`, `Actualizar`, `Eliminar` y `Restaurar`. `Listar(bool activos = true)` tiene un parámetro opcional: si no se especifica, devuelve activos. `T?` en `Obtener` reconoce que no encontrar una fila es un resultado posible. `Crear` devuelve un entero porque SQL Server asigna la identidad.

La clase genérica `RepositorioSql<T>` implementa el contrato. Tiene métodos abstractos de mapeo y propiedades de SQL que sus subclases deben completar. Por ejemplo, Producto usa `PrecioUnitario`, mientras Suplidor usa `Telefono`. El ciclo de conexión, ejecución y auditoría es común.

El segundo uso genérico propio es `Buscador.Filtrar<T>`. Recibe una colección, un texto y una función `Func<T,string>`. Esa función indica qué texto buscar en cada elemento. Para categorías se usa nombre y descripción; para suplidores, empresa, contacto, teléfono, correo y web.

## 6. Paso tres: conexión Singleton e inyección de dependencias

`ConexionSql` tiene un constructor privado. Fuera de la clase no se puede escribir `new ConexionSql()`.

```csharp
private static readonly Lazy<ConexionSql> instancia = new(() => new ConexionSql());
```

`static` comparte el campo entre todos los consumidores. `readonly` evita reemplazar la referencia después de inicializarla. `Lazy<T>` construye el objeto cuando se solicita por primera vez y permite una inicialización segura. La expresión lambda `() => ...` indica qué hacer cuando llegue ese momento.

`ConexionSql.Instancia` expone el mismo objeto. `Configurar` valida que la cadena incluya servidor y base. `CrearConexion` construye una conexión corta por operación. El Singleton es la configuración/fábrica; compartir una única conexión abierta entre tareas causaría conflictos de lectores y transacciones. El pool de SqlClient se encarga de reutilizar conexiones físicas.

`Composicion.Crear` construye, en orden: conexión, logger, ejecutor, repositorios y servicios. Devuelve `ServiciosAplicacion`, que contiene únicamente operaciones de negocio y el contrato del logger. La UI no recibe una conexión ni un repositorio.

Ejemplo de inyección:

```csharp
public ProductoServicio(IRepositorio<Producto> repositorio,
    IRepositorio<Categoria> categorias, IRepositorio<Suplidor> suplidores)
```

El constructor recibe lo que necesita. Para probarlo, se pasan repositorios en memoria; para ejecutar, repositorios SQL. No hace falta un contenedor externo para aplicar inyección por constructor.

## 7. Paso cuatro: ejecutar SQL de forma consistente

`EjecutorSql.Ejecutar<T>` recibe un nombre de operación, si es escritura y una función que contiene el trabajo SQL.

1. Entra al `try` antes de abrir la conexión.
2. `using var cn` asegura que se cierre y libere incluso si ocurre una excepción.
3. `cn.Open()` abre la conexión.
4. Si hay escritura, inicia una transacción `Serializable`.
5. Obtiene un bloqueo de aplicación con `sp_getapplock`. Todas las escrituras realizadas por esta aplicación usan el mismo recurso. Así se ordenan y se evita que dos ventanas cambien relaciones al mismo tiempo. Si no obtiene el bloqueo en diez segundos, devuelve un error comprensible.
6. Ejecuta la función recibida.
7. `Commit()` confirma todos los cambios de la operación. Si no se alcanza ese punto, `Dispose` revierte la transacción.
8. `catch (SqlException)` registra el detalle y transforma el error en `PersistenciaException`.

`SqlException` se trata específicamente para que Datos pueda identificar restricciones únicas, claves foráneas, tiempo agotado y deadlocks. La UI no referencia ese tipo. `PersistenciaException` pertenece a Entidades y lleva un mensaje seguro junto a la causa interna.

El logger escribe en `%LOCALAPPDATA%\SistemaInventario\errores.log`. No usa la carpeta del ejecutable porque podría estar en una ubicación sin permisos de escritura. `lock` impide que dos tareas escriban simultáneamente en el archivo. Si falla el log por permisos o disco, se conserva el error original.

### SQL parametrizado

En `ProductoRepositorio.Parametros`, `@nombre`, `@precio`, `@categoria` y `@suplidor` se añaden a `SqlCommand.Parameters`. El texto del usuario nunca se concatena al SQL. Las tablas se interpolan desde constantes internas del repositorio, no desde una entrada del usuario.

Para el precio se especifican `Precision=18` y `Scale=2`. El servicio ya rechazó más de dos decimales antes de llegar a SQL. Los límites del servicio coinciden con `DECIMAL(18,2)`.

### Inserción y recuperación del Id

El INSERT utiliza `OUTPUT INSERTED.Id INTO @nueva` y luego `SELECT Id FROM @nueva`. Se necesita `INTO` porque las tablas tienen triggers; SQL Server no permite el `OUTPUT` directo al cliente en ese caso. El Id y las fotos se guardan dentro de la misma transacción. Si una foto falla, no queda una categoría guardada a medias.

### Fotografías

En una actualización de categoría, las fotos visibles previas se marcan inactivas y se insertan las fotos de la colección nueva. Este enfoque conserva versiones, aunque vuelve a guardar bytes de imágenes que no hayan cambiado. Es sencillo para esta práctica de hasta cinco fotos por categoría. El trigger cuenta únicamente fotos activas.

## 8. Paso cinco: validar en Negocio

`ServicioBase<T>.Guardar` distingue un objeto nuevo (`Id == 0`) de uno existente. Si existe y está eliminado, exige restaurarlo antes de editarlo. Después crea un diccionario de errores, ejecuta `Validar` y solo llama al repositorio si el diccionario queda vacío.

Cada clave del diccionario coincide con una propiedad: `Nombre`, `Correo`, `CategoriaId`, etc. `nameof(entidad.Correo)` obtiene el nombre de la propiedad como texto sin escribir una cadena que podría quedarse desactualizada al renombrar el código.

`CategoriaServicio` recorta espacios, verifica nombre de 3–100 caracteres y descripción de 1–500, busca duplicados entre activos y eliminados y verifica fotos. La regla de nombre único también existe en SQL para resolver carreras entre aplicaciones.

`SuplidorServicio` valida empresa y contacto de 3–100 caracteres. El teléfono debe cumplir exactamente `^(809|829|849)-\d{3}-\d{4}$`: `^` inicia el texto, el grupo admite uno de los tres prefijos, `\d{3}` exige tres dígitos y `\d{4}` cuatro, con los guiones indicados. `$` termina el texto.

El correo usa regex y tiene un límite de 254 caracteres. La comprobación verifica formato; no confirma que la cuenta exista ni envía correos. El sitio web es opcional, máximo 500 caracteres, absoluto y HTTP o HTTPS. `Uri.TryCreate` comprueba que se pueda interpretar como URI; no necesita visitar el sitio.

`ProductoServicio` valida nombre de 3–150, precio positivo, máximo representable por SQL y dos decimales. La comparación `decimal.Round(precio,2) != precio` detecta si habría que cambiar el valor para dejarlo en dos decimales. `12.300` representa el mismo valor que `12.30` y se acepta; `12.345` se rechaza.

`PrecioEntrada.Leer` convierte la cadena usando la cultura del equipo, sin separadores de miles. La UI no fuerza un redondeo con `NumericUpDown`. Si el usuario introduce texto que no se puede convertir, se genera un error asociado a `PrecioUnitario`; si se puede convertir, el servicio valida el valor.

Para una foto se verifican nombre, extensión, tamaño y firma básica PNG/JPEG. La firma detecta archivos que se renombraron sin ser de ese tipo; no equivale a un análisis completo del contenido de la imagen. La vista previa intenta decodificar con `Image.FromStream` y avisa si no puede hacerlo.

### Regla de eliminación y restauración

`AntesEliminar` busca productos activos con `Any`. Si encuentra alguno asociado, rechaza el borrado. El trigger repite la protección en SQL. Las validaciones de Negocio permiten mensajes claros y los triggers protegen la integridad si otra instancia o un cliente SQL modifica los datos.

`AntesRestaurar` de Producto revisa ambas asociaciones. Una clave foránea solo garantiza que la fila padre exista; no garantiza que esté activa. Por eso hacen falta la regla de negocio y el trigger.

## 9. Paso seis: entender las consultas LINQ

LINQ permite consultar colecciones con tipos C# y sin SQL dentro de los formularios.

| Consulta | Operadores | Resultado visible |
|---|---|---|
| Proyección de productos | Dos `join`, `orderby`, `select` | Producto con nombre de categoría y empresa |
| Filtro combinado | `Where`, condiciones opcionales y `OrderBy` | Catálogo por texto, categoría, suplidor y precio |
| Resumen por categoría | `GroupJoin`, `Count`, `Average` | Conteo y promedio, incluyendo categorías vacías |
| Top cinco suplidores | `Where`, `GroupBy`, `Join`, `OrderByDescending`, `ThenBy`, `Take(5)` | Ranking de empresas |
| Sobre el promedio | `Select`, `DefaultIfEmpty`, `Average`, `Where`, `OrderByDescending` | Productos de precio superior al promedio global |
| Buscador genérico | `Where`, selector y `Contains` | Búsqueda sin distinguir mayúsculas |

El filtro de precio es inclusivo: de 100 a 200 incluye ambos extremos. Un campo vacío no limita ese extremo. Una categoría o suplidor con Id cero significa “todos”. La comparación de texto usa `StringComparison.OrdinalIgnoreCase`.

`GroupJoin` conserva las categorías vacías. En una categoría sin productos, el promedio se muestra como cero para evitar `Average` sobre una colección vacía. En el informe global, `DefaultIfEmpty(0m)` resuelve el mismo caso.

Estas consultas se ejecutan sobre una instantánea en memoria. La búsqueda no consulta SQL por cada pulsación. El botón **Actualizar** vuelve a leer la base, por ejemplo si otro usuario cambió un registro. Para catálogos mucho mayores sería apropiado paginar y filtrar desde SQL; no es necesario para la muestra de esta práctica.

## 10. Paso siete: construir los formularios

Los formularios están escritos por código para que se vea cómo se crea cada control. `Tema` concentra colores, tipografía, botones y tablas. El diseño usa paneles de distribución y `Dock`, que ajustan los controles al espacio disponible.

`PrincipalForm` crea el menú lateral y el área de contenido. Cada botón construye el control de su módulo, retira el anterior y lo libera con `Dispose`. El evento `Shown` abre Productos al iniciar.

`ListadoControl<T>` reutiliza barra de búsqueda, tabla y botones. `Load` inicia la carga. `Task.Run` ejecuta el servicio fuera del hilo de UI y `await` devuelve el control a la ventana mientras espera. Después del `await`, el enlace de datos ocurre en el hilo de UI. No se actualizan controles desde la tarea de fondo.

`SelectionChanged` habilita Editar y Eliminar cuando existe una fila. El Id de la fila identifica el objeto de la colección. `CellDoubleClick` también abre el editor. Si la tabla está vacía, Nuevo sigue disponible.

La propiedad `ocupado` evita operaciones repetidas. Durante una operación se deshabilita temporalmente el módulo; `finally` vuelve a habilitarlo aunque se produzca un error. Los botones no dependen de un estado que permanezca bloqueado después de un fallo.

`EditorForm` construye una cabecera, los campos y los botones. Cada editor añade sus propios controles y registra la relación entre propiedad y control en `controles`.

Al pulsar Guardar:

1. Limpia el ErrorProvider anterior.
2. Lee los valores de los controles en el hilo de UI.
3. Crea un objeto nuevo con esos valores y el Id original si se está editando.
4. Llama al servicio desde una tarea de fondo.
5. Si el servicio devuelve errores, asigna cada mensaje a su control con `errores.SetError`.
6. Si termina correctamente, devuelve `DialogResult.OK` y cierra.
7. El listado se recarga para mostrar el estado persistido.

El editor trabaja sobre una copia. Si modifica un nombre y pulsa Cancelar, no cambia la colección original ni se llama a SQL.

`CategoriaForm` incluye selector múltiple, lista y vista previa. Se limita la lectura de cada archivo a 2 MB + 1 byte: si es mayor que el máximo, el validador lo rechaza sin cargarlo completo. Quitar solo cambia la colección del editor hasta que se guarda. `Image` se libera al cambiar la foto y al cerrar para no acumular recursos GDI.

`ProductoForm` recibe únicamente categorías y suplidores activos, precargados antes de abrir el diálogo. La primera opción obliga a elegir una asociación. Si otra instancia desactiva una asociación después de cargar el combo, el servicio vuelve a validarla y el trigger protege el momento de guardar.

`PapeleraControl` reúne los tres catálogos eliminados en `RegistroPapelera`. El par Tipo + Id identifica el registro porque distintos catálogos pueden tener el mismo Id numérico. Al restaurar, usa el servicio correspondiente.

## 11. Recorrido de ejemplo de principio a fin

Suponga que se crea un producto llamado “Monitor 24” de RD$8500.50.

1. El usuario abre Productos y pulsa Nuevo.
2. Se leen categorías y suplidores activos y se abre `ProductoForm`.
3. Escribe el nombre/precio y selecciona ambas asociaciones.
4. Guardar llama a `PrecioEntrada.Leer` y construye un `Producto`.
5. `ProductoServicio.Guardar` recorta el nombre y valida nombre, precio y asociaciones.
6. El servicio invoca `IRepositorio<Producto>.Crear`.
7. La implementación SQL recibe el objeto y ejecuta el INSERT parametrizado dentro del ejecutor.
8. SQL valida las claves foráneas y el trigger confirma que ambos padres estén activos.
9. Se devuelve la identidad y se confirma la transacción.
10. El formulario cierra y el listado consulta la base nuevamente.
11. `ConsultasInventario.Proyectar` agrega nombres de categoría y suplidor mediante joins.
12. El DataGridView muestra el producto y las fechas en hora local.

Si el precio fuera `-1`, el recorrido se detendría en Negocio. Si la conexión fallara, Datos capturaría `SqlException`, registraría el error y la interfaz mostraría una explicación. Si el usuario cancelara, no se intentaría guardar.

## 12. Probar lo importante

Las 82 pruebas automatizadas verifican límites de longitud, unicidad, formato, precio, fotos, restricciones de borrado, restauración y resultados de LINQ. Usan repositorios de prueba, por lo que no necesitan un motor SQL. Ejecute:

```powershell
dotnet run --project tests/SistemaInventario.Pruebas -c Release
```

En SQL Server, ejecute `verificar.sql` y después `pruebas.sql` en una base de prueba recién creada. El segundo script contiene nueve operaciones que deben fallar por restricciones o triggers; cada una se revierte. No se declara que estas pruebas SQL hayan sido ejecutadas durante la preparación.

La comprobación manual de interfaz está en `Pruebas_y_Limitaciones.md`. Debe realizarla antes de presentar la práctica. Incluya crear, editar, cancelar, filtrar, eliminar y restaurar; no se limite a abrir las ventanas.

## 13. Guion corto para demostrarlo al profesor

1. Muestre los cuatro proyectos y sus referencias. Explique por qué Presentación no ejecuta SQL.
2. Cree una categoría sin fotos y otra con una foto. Intente una sexta para mostrar el error.
3. Cree un suplidor con teléfono incorrecto, muestre ErrorProvider y corrija.
4. Cree un producto; pruebe precio `12.345` y después `12.34`.
5. Combine categoría, suplidor y rango de precio en el catálogo.
6. Intente eliminar una categoría usada: debe bloquearse.
7. Elimine un producto y confirme que desaparece del listado principal y aparece en Papelera.
8. Elimine sus asociaciones si no tienen otros productos activos. Intente restaurar el producto: debe exigir restaurarlas primero.
9. Restaure asociaciones y producto. Abra Informes LINQ y explique cada resultado.
10. Muestre `IRepositorio<T>`, `ConexionSql`, `Composicion` y una prueba con repositorio en memoria.

## 14. Preguntas que debe poder responder

**¿Eliminar usa DELETE?** No. Ejecuta UPDATE sobre `Activo` y modifica la fecha. La fila y su identidad se conservan.

**¿Por qué la FK no basta?** Porque admite que la fila padre exista con `Activo=0`. El estado activo es una regla adicional.

**¿Para qué sirven los servicios?** Centralizan las reglas para que cualquier interfaz use la misma lógica antes de guardar.

**¿Dónde se aplica el Repository Pattern?** En el contrato `IRepositorio<T>`, la implementación común `RepositorioSql<T>` y los tres repositorios especializados.

**¿Dónde está el Singleton?** En `ConexionSql.Instancia`. Su constructor es privado y la instancia se crea con `Lazy`.

**¿Dónde se inyectan dependencias?** En los constructores de repositorios y servicios. `Composicion` decide las implementaciones concretas.

**¿Qué pasa si se repite el nombre de una categoría eliminada?** Se rechaza y se pide restaurar o editar el registro original.

**¿Por qué las fechas son UTC?** Para guardar una referencia consistente; la interfaz las convierte a la hora local del equipo.

**¿Qué comprueban las pruebas en memoria?** Las reglas y consultas. La conexión, los triggers y el funcionamiento visual requieren las pruebas de integración y la revisión en Windows.

**¿Cómo cambia la lógica si se sustituye SQL?** Las reglas se conservan si la nueva implementación cumple `IRepositorio<T>`; se cambia la composición y la implementación de persistencia.

**¿Qué verifica un `using`?** No verifica: administra la liberación de un objeto que implementa `IDisposable`, incluso si ocurre una excepción.

**¿`async` significa que todo corre en otro hilo?** No. `Task.Run` mueve el trabajo síncrono SQL a un hilo de trabajo; `await` espera sin bloquear la UI y continúa en su contexto.

## 15. Dónde modificar algo

| Cambio | Archivo principal |
|---|---|
| Colores, tipografía, estilo de tablas | `Tema.cs` |
| Navegación | `PrincipalForm.cs` |
| Regla de precio | `ProductoServicio.cs` |
| Formato de teléfono y correo | `Validador.cs` |
| Reglas de eliminación | `CategoriaServicio.cs`, `SuplidorServicio.cs` y triggers |
| Nuevo informe | `ConsultasInventario.cs` e `InformesControl.cs` |
| Campos de producto | Entidad, esquema, repositorio, servicio y formulario de Producto |
| Servidor de SQL | `conexion.txt` junto al ejecutable |
| Destino del log | `Logger.cs` |

Para añadir un campo, siga todas las capas en ese orden y actualice los diagramas, las pruebas y la documentación. Así el cambio conserva coherencia desde la pantalla hasta la base.
