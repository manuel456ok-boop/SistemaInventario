# Sistema de Inventario · ISW-124

Aplicación individual en **C# / Windows Forms / SQL Server** para categorías, suplidores y productos. Cuatro proyectos de aplicación, un proyecto adicional de pruebas y publicación **Release para Windows x64** incluida.

## Empezar en cinco pasos

1. Clone este repositorio desde GitHub o descárguelo mediante **Code → Download ZIP**. Si lo descarga como ZIP, extraiga su contenido. Abra la carpeta principal del proyecto, donde se encuentra este `README.md`.

2. En SQL Server Management Studio, conectado a su instancia de SQL Server, ejecute `database/schema.sql` **una vez**. Después ejecute `database/datos.sql`. La muestra contiene **5 categorías, 5 suplidores y 20 productos**; no incluye fotografías.

3. Abra `build/win-x64/conexion.txt` con Bloc de notas y cambie `Server` por el nombre de su instancia de SQL Server. La base de datos se crea por separado; la aplicación no ejecuta los scripts automáticamente.

4. Instale **.NET 8 Desktop Runtime x64** en Windows, si todavía no está instalado. Descárguelo desde la [página oficial de .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0), en la sección **.NET Desktop Runtime**.

5. Ejecute `ejecutar.cmd` desde la carpeta principal o abra `build/win-x64/SistemaInventario.Presentacion.exe`. Mantenga el ejecutable junto a sus DLL, archivos JSON y `conexion.txt`.
## Conexión

Predeterminado para SQL Server Express con autenticación de Windows:

```text
Server=.\SQLEXPRESS;Database=SistemaInventario;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=10;
```



## Abrir y compilar el código

Instale Visual Studio 2022 con **Desarrollo de escritorio de .NET** y compatibilidad con .NET 8. Abra `src/SistemaInventario.sln`, restaure NuGet y establezca **SistemaInventario.Presentacion** como proyecto de inicio. Presione F5. Los formularios están construidos por código; no requieren archivos `.Designer.cs` ni recursos generados para funcionar. Consulte `docs/Guia_Paso_a_Paso.md` para localizar cada control y evento.

Desde PowerShell, en la carpeta raíz, con el SDK .NET 8:

```powershell
.\compilar.ps1
```

Para generar una publicación que incluya el runtime y pueda ejecutarse sin instalarlo:

```powershell
.\compilar.ps1 -Autocontenido
```

La publicación incluida es **dependiente del runtime**, compilada en Release y orientada a `win-x64`. La opción autocontenida aumenta el tamaño y necesita descargar los paquetes de runtime al compilar.

## Estructura

```text
SistemaInventario/
  README.md
  compilar.ps1
  ejecutar.cmd
  src/
    SistemaInventario.sln
    SistemaInventario.Entidades/
    SistemaInventario.AccesoDatos/
    SistemaInventario.LogicaNegocio/
    SistemaInventario.Presentacion/
  database/
    schema.sql
    datos.sql
    verificar.sql
    pruebas.sql
  docs/
    Matriz_Requisitos.md
    Diagrama_Clases.svg
    Diagrama_ER.svg
    diagramas.md
    Pruebas_y_Limitaciones.md
    capturas/
  tests/SistemaInventario.Pruebas/
  build/win-x64/
```

## Funciones

- **Categorías:** alta, edición, listado, búsqueda por nombre/descripción, entre 0 y 5 fotos JPG/JPEG/PNG de máximo 2 MB cada una.
- **Suplidores:** empresa única, contacto, teléfono dominicano, correo y sitio web opcional HTTP/HTTPS.
- **Productos:** precio positivo con máximo dos decimales, categoría y suplidor activos; búsqueda por producto/categoría/suplidor, filtros combinados y rango inclusivo de precios.
- **Papelera:** búsqueda por tipo y nombre y restauración de los tres catálogos. Las fotos se conservan al eliminar y restaurar su categoría.
- **Informes LINQ:** conteo y promedio por categoría, top cinco suplidores y productos por encima del promedio global.
- **Auditoría:** fechas UTC en SQL y hora local en los listados. Todas las tablas tienen `Activo`, creación y modificación.

Para editar o eliminar, seleccione una fila. Con una lista vacía, esos botones permanecen deshabilitados. El botón **Nuevo** permite crear el primer registro. Un doble clic en la fila también abre el editor. Si falla la conexión, corrija el archivo de conexión, reinicie la aplicación y pruebe de nuevo.

## Decisiones de negocio

1. La especificación de fotos menciona “una o más”, pero fija mínimo 0: se adopta el rango explícito **0–5**.
2. Categoría: nombre 3–100 y descripción 1–500 caracteres. Empresa/contacto: 3–100; producto: 3–150. Estas últimas longitudes concretan el requisito general de nombres dentro de longitud permitida.
3. La unicidad de categoría y empresa se conserva **incluyendo eliminados**. Así no se producen conflictos al restaurar. Se ignoran mayúsculas y se conservan diferencias de acentos en SQL.
4. No se puede desactivar una categoría o suplidor con productos activos. Debe reasignar o eliminar primero los productos.
5. Para restaurar un producto, su categoría y suplidor deben estar activos. Restaure esas asociaciones primero.
6. Las fotos se guardan en `VARBINARY(MAX)`, no en rutas del equipo. Al editar la colección, las versiones anteriores quedan inactivas; la papelera principal gestiona categorías, suplidores y productos, no el historial individual de fotos.
7. Los nombres de productos pueden repetirse: la consigna solo exige que no estén vacíos. No se agrega inventario de existencias, login ni facturación porque no forman parte de esta práctica.

## Arquitectura y patrones

`Presentacion → LogicaNegocio → AccesoDatos → SQL Server`; las capas comparten contratos y modelos de `Entidades`.

- `IRepositorio<T>`: contrato genérico de CRUD; `RepositorioSql<T>` implementa lo repetido.
- `ConexionSql`: Singleton de configuración y fábrica de conexiones de corta duración. No comparte una conexión abierta entre formularios.
- Inyección por constructor en servicios y repositorios. `Composicion.Crear` construye el grafo fuera de los formularios.
- `Buscador.Filtrar<T>`: método genérico adicional.
- La referencia de Negocio hacia Datos usa `PrivateAssets="compile"`: la presentación no puede compilar llamadas a Datos, pero las dependencias de ejecución sí se publican.
- Escrituras transaccionales, SQL parametrizado, integridad referencial y triggers para relaciones activas y límite de fotos. Las escrituras de la aplicación se serializan con un bloqueo de transacción.



## Capturas reales

**Pendientes de generar en Windows.** El proyecto incluye [instrucciones y un capturador](docs/capturas/README.md) para guardar fotografías reales de cada formulario. Ejecute `docs/capturas/capturar.ps1` y siga sus mensajes. Una vez creados los PNG, puede añadirlos al README con las rutas `docs/capturas/01-productos.png`, `02-categorias.png`, `03-suplidores.png`, `04-papelera.png` y `05-informes.png`.

![[Pasted image 20261018184519.png]]

![[{02AA3A3E-BB68-4EA4-A537-7C83E879A1F6}.png]]

![[{8170B9B0-A431-4445-BC86-B4604FCD0BE4}.png]]


![[{8C32476E-9F5C-4C31-9797-A4D511D0A97E}.png]]


![[{C0D45310-FF0D-4CDC-B292-E4202C773D1B}.png]]

Diagramas incluidos (documentación, no capturas de la interfaz):

![[Diagrama_Clases 1.svg]]

![[Diagrama_ER.svg]]



## Referencias técnicas oficiales

- Publicación: https://learn.microsoft.com/dotnet/core/tools/dotnet-publish
- Windows Forms: https://learn.microsoft.com/dotnet/desktop/winforms/overview/
- Microsoft.Data.SqlClient: https://www.nuget.org/packages/Microsoft.Data.SqlClient/6.1.7
- Triggers T-SQL: https://learn.microsoft.com/sql/t-sql/statements/create-trigger-transact-sql
- Bloqueos: https://learn.microsoft.com/sql/relational-databases/sql-server-transaction-locking-and-row-versioning-guide
