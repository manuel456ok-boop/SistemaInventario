# Matriz de requisitos

“Implementado” describe el código entregado. No equivale a ejecución visual ni integración SQL verificada. Los límites de pruebas se indican al final.

| Requisito | Implementación | Evidencia / comprobación |
|---|---|---|
| Categorías CRUD y papelera | `CategoriaServicio`, `CategoriaRepositorio`, `CategoriasControl`, `CategoriaForm` | Pruebas de nombre, descripción, borrado y restauración |
| Nombre único 3–100 | Servicio + `UQ_Categorias_Nombre` y CHECK | Duplicados activos y eliminados; límites 3/100 |
| Descripción obligatoria ≤500 | Servicio + CHECK | Pruebas de vacío/500/501 |
| Fotos 0–5 | `FotoCategoria`, validación y trigger | Pruebas de límites; flujo visual pendiente |
| Fotos JPG/JPEG/PNG ≤2 MB | `Validador.Foto`; BLOB SQL | Firma básica, extensión, tamaño; vista previa en editor |
| Suplidores CRUD | Servicio, repositorio, control y editor de Suplidor | Reglas automatizadas y prueba manual pendiente |
| Empresa única y contacto | Servicio + UNIQUE/CHECK | Activos y papelera |
| Teléfono dominicano | Regex en Negocio + CHECK SQL | 809/829/849 y formatos inválidos |
| Correo válido | Regex con timeout en Negocio | Casos válidos e inválidos |
| Sitio opcional válido | `Uri.TryCreate`, HTTP/HTTPS en Negocio | Vacío, válido y esquemas rechazados |
| Productos CRUD | Servicio, repositorio, control y editor | Reglas automatizadas y prueba manual pendiente |
| Precio >0 y ≤2 decimales | `PrecioEntrada`, `ProductoServicio`, DECIMAL(18,2) | No redondeo silencioso; límites |
| Asociaciones activas | `ProductoServicio` + trigger | Guardar y restaurar |
| Todas las tablas con Activo | Esquema: categorías, suplidores, productos y fotos | BIT NOT NULL DEFAULT 1 |
| Exclusión de eliminados | `Listar(bool activos=true)` | Pruebas de catálogo/papelera |
| Pantalla Papelera | `PapeleraControl` | Tres tipos, búsqueda y restauración |
| No eliminar padres usados | Negocio + triggers | Pruebas de negocio y casos SQL entregados |
| Búsqueda inmediata | `TextChanged`, instantánea y LINQ | Cada listado y papelera |
| Filtros de producto | Categoría/suplidor/mínimo/máximo combinados | Filtro inclusivo, texto y rango inválido |
| Cuatro proyectos | `src/SistemaInventario.sln` | Cuatro de aplicación y uno de pruebas |
| Presentación sin referencia a Datos | Referencias de proyectos y `PrivateAssets="compile"` | Prueba negativa de compilación realizada |
| Repository genérico | `IRepositorio<T>`, `RepositorioSql<T>` | Tres implementaciones concretas |
| Singleton de conexión | `ConexionSql.Instancia` | `Lazy`, constructor privado, conexiones cortas |
| Inyección por constructor | Repositorios, servicios y contenedor de servicios | `Composicion.Crear` arma el grafo |
| Genérico adicional | `Buscador.Filtrar<T>` | Usado por categorías, suplidores y papelera |
| ≥5 LINQ no triviales | `ConsultasInventario` y `Buscador` | Joins, filtros, agrupación, ranking y promedios |
| Validaciones en Negocio | Servicios, `Validador`, `PrecioEntrada` | Diccionario por propiedad |
| ErrorProvider | `EditorForm`, errores de fotos | `SetError` por propiedad |
| SqlException específico | `EjecutorSql.Ejecutar<T>` | Todas las operaciones SQL dentro de ese try/catch |
| errores.log | `Logger` | En `%LOCALAPPDATA%\SistemaInventario` |
| Convenciones y XML | Archivos C# y métodos públicos | Un tipo/clase por archivo; comentarios directos |
| schema.sql + datos.sql | `database/` | 5 categorías, 5 suplidores, 20 productos |
| Claves foráneas | Productos y fotos | Integridad referencial explícita |
| README e instalación | `README.md`, `compilar.ps1`, `ejecutar.cmd` | Instrucciones completas |
| Capturas reales | Capturador y guía en `docs/capturas` | **Pendiente de ejecutar en Windows** |
| Diagrama de clases + ER | SVG y Mermaid en `docs/` | Fuente y visuales incluidos |
| Release compilado | `build/win-x64/SistemaInventario.Presentacion.exe` | PE Windows x64; dependencias incluidas; requiere Desktop Runtime 8 |
| Repositorio GitHub | Estructura e instrucciones listas | **Pendiente: falta URL de destino / publicación remota** |

Verificación realizada: compilación Release, publicación Windows, 82 pruebas de negocio/LINQ y sintaxis T-SQL con ScriptDom. Pendiente: ejecución en Windows, capturas, pruebas contra SQL Server y publicación a un repositorio remoto.
