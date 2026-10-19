# Registro de uso de inteligencia artificial

**Proyecto:** SistemaInventario  
**Asignatura:** ISW-124 — Programación Avanzada  
**Tecnologías:** C#, Windows Forms y SQL Server

## Herramientas

- **ChatGPT / Codex:** lo utilicé para explicar el código, preparar la documentación y consultar los problemas encontrados durante la configuración y revisión de la interfaz.
- **Visual Studio:** lo utilicé para abrir la solución, revisar los archivos, consultar los errores de compilación y administrar las referencias NuGet.
- **NuGet**
- **SQL Server y SSMS** 
## Qué generó la IA

Los aportes de la IA que destaco en este proyecto son:

- **Diseño de formularios:** propuso la distribución del menú, tablas, botones y campos de entrada. Los archivos incluyen `PrincipalForm.cs`, `EditorForm.cs`, `CategoriaForm.cs`, `SuplidorForm.cs`, `ProductoForm.cs` y `Tema.cs`.
- **Scripts SQL:** generó `schema.sql` para las tablas, claves foráneas, restricciones y triggers; `datos.sql` con los registros de muestra; y scripts adicionales de comprobación.
- **Consultas LINQ:** generó funciones de `ConsultasInventario.cs`, como `Filtrar`, `PorCategoria`, `TopSuplidores` y `SobrePromedio`, además del buscador genérico `Buscador.Filtrar<T>`.
## Qué cambié yo

Mi intervención documentada hasta esta versión ha sido revisar la propuesta en Visual Studio y señalar problemas concretos durante su preparación:

- Revisé los errores `NU1101`, `CS0234` y `CS0246`. Se identificó que Visual Studio buscaba `Microsoft.Data.SqlClient` en el origen de paquetes sin conexión. La corrección indicada fue habilitar `nuget.org` y restaurar las dependencias existentes.
- Consulté cómo configurar el servidor en `conexion.txt` y cómo agregar una conexión desde Visual Studio. Se aclaró que la conexión del Explorador de servidores y la que utiliza la aplicación se configuran por separado.
- **Eliminé labels innecesarios e incorrectos.** Quité etiquetas que no aportaban información útil y otras que no estaban bien planteadas, con el objetivo de simplificar la interfaz y presentar mejor la información.
- **Corregí el Dock del menú lateral.** Ajusté la implementación de las propiedades `Dock` para corregir la ubicación y distribución del menú dentro de la ventana principal.
- **Cambié los tamaños de los formularios.** Modifiqué sus dimensiones para mejorar el espacio disponible para los controles y la presentación de los datos.
- **Corregí las dependencias de acceso a datos.** Ajusté el origen de paquetes NuGet y la restauración de `Microsoft.Data.SqlClient`, necesario para utilizar las clases de conexión a SQL Server.
- **Adapté la conexión al servidor.** Modifiqué `conexion.txt` para utilizar la instancia y el tipo de autenticación correspondientes a mi equipo.
- **Ajusté la entrada del teléfono.** Corregí la presentación y el tratamiento del formato dominicano en el formulario de suplidores, manteniendo los guiones que exige la validación.
- **Corregí el tratamiento del precio.** Ajusté el manejo de la entrada y su validación para rechazar valores no válidos o mayores a dos decimales, sin redondearlos silenciosamente antes de validarlos.
- **Ajusté la búsqueda y los filtros.** Corregí el comportamiento del listado de productos al combinar texto, categoría, suplidor y rango de precios, así como el restablecimiento del listado al limpiar los filtros.

## Qué no domino aún

Como punto de estudio para profundizar, selecciono la protección de las relaciones cuando ocurren operaciones simultáneas: cómo se complementan la transacción `Serializable`, el bloqueo `sp_getapplock` y los triggers que impiden dejar productos activos con categorías o suplidores inactivos. y las funciones linq
