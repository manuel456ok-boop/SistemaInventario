# Verificación y comprobación pendiente

Fecha de preparación: **18 de octubre de 2026**.

## Ejecutado durante la preparación

| Comprobación | Resultado | Alcance |
|---|---|---|
| Compilación de solución Release | 0 errores, 0 advertencias | 4 proyectos de aplicación + pruebas, SDK 8.0.426 |
| Publicación Windows x64 | Correcta | EXE PE32+ de interfaz gráfica y dependencias |
| Pruebas automatizadas | **82 aprobadas, 0 fallidas** | Validaciones, borrado lógico, restauración, búsquedas y consultas LINQ |
| Sintaxis T-SQL | 0 errores en los 4 scripts | Microsoft ScriptDom 161.8901.0, parser SQL Server 2022 |
| Separación de capas | Correcta | Una referencia intencional a AccesoDatos desde Presentación no compila; el archivo de prueba se retiró |
| Dependencias publicadas | Correctas | Incluye SqlClient, SNI Windows y las cuatro DLL propias |
| Diagramas | Revisados tras rasterizar | SVG de clases y modelo ER, además de fuentes Mermaid |
| Código explicado | 61 archivos, 1,641 líneas | Contenido embebido, sin dependencias de Internet |

Los repositorios en memoria son dobles para probar negocio. No simulan resultados de una prueba real de conexión. El análisis sintáctico comprueba que los scripts se puedan analizar como T-SQL; no ejecuta constraints, triggers ni transacciones.

## Pendiente en Windows con SQL Server

- Ejecutar `schema.sql`, `datos.sql` y `verificar.sql` en el servidor real.
- Ejecutar las nueve pruebas de integración de `pruebas.sql` en una base de prueba recién inicializada. Cada caso revierte cambios. No usar una base de producción para las pruebas.
- Abrir y recorrer los formularios, comprobar distribución y respuesta a eventos con la configuración de pantalla del equipo.
- Generar capturas reales de los cinco módulos y añadirlas al README. El capturador de PowerShell está incluido, sin ejecución validada en este entorno.
- Subir el contenido a un repositorio GitHub. No se proporcionó un destino remoto y no se ha publicado uno.

## Lista manual de aceptación

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | Abrir la aplicación con conexión válida | Se carga Productos con los 20 productos de muestra |
| 2 | Escribir “mOuSe” | La búsqueda encuentra Mouse óptico |
| 3 | Combinar categoría, suplidor y mínimo/máximo | Solo quedan productos que cumplen todos los filtros |
| 4 | Escribir mínimo mayor al máximo | Se muestra error de rango y no se presentan resultados incorrectos |
| 5 | Crear categoría vacía | ErrorProvider en nombre y descripción |
| 6 | Crear categoría válida sin fotos | Se guarda y muestra el nuevo registro |
| 7 | Agregar cinco fotos válidas y guardar | Se guardan y se pueden previsualizar al editar |
| 8 | Intentar una sexta foto, extensión inválida o más de 2 MB | Se rechaza y se señala el campo Fotos |
| 9 | Crear suplidor con teléfono 800-123-4567 o correo incompleto | Se rechazan los campos correspondientes |
| 10 | Crear suplidor válido sin web | Se permite; opcional significa que puede quedar vacío |
| 11 | Guardar producto con precio 12.345 | Se rechaza sin redondear |
| 12 | Guardar producto con precio 12.34 y asociaciones activas | Se guarda con dos decimales |
| 13 | Editar y cancelar | No cambia la base ni el listado |
| 14 | Editar y guardar | Cambia la fecha de modificación; creación se conserva |
| 15 | Eliminar una categoría o suplidor de la muestra con productos activos | Se bloquea con explicación |
| 16 | Eliminar un producto | Desaparece del listado principal y aparece en Papelera |
| 17 | Buscarlo en Papelera y restaurarlo | Regresa al catálogo con el mismo Id |
| 18 | Eliminar un producto y luego su categoría sin otros productos activos | Ambos quedan en Papelera |
| 19 | Restaurar ese producto antes de la categoría | Se bloquea; no activa un producto inválido |
| 20 | Restaurar categoría y producto | Se recupera correctamente |
| 21 | Duplicar en mayúsculas el nombre de una categoría eliminada | Se rechaza y se indica restaurar el original |
| 22 | Abrir los tres informes | Conteos, ranking y promedios corresponden a activos |
| 23 | Cerrar y abrir la aplicación | Los cambios y las fotos siguen presentes en SQL |
| 24 | Usar servidor incorrecto y abrir la aplicación | Mensaje comprensible; detalle técnico en errores.log |
| 25 | Corregir conexion.txt y reiniciar | Se puede volver a cargar sin botones bloqueados por un error anterior |

Las fechas de creación y modificación tienen resolución de un segundo; dos operaciones dentro del mismo segundo pueden mostrar la misma hora.

## Límites técnicos conocidos

- La publicación incluida depende de .NET 8 Desktop Runtime x64. No está probada en Windows ARM64 ni sistemas de 32 bits.
- No hay control de edición optimista con `rowversion`: dos usuarios que editen el mismo registro pueden guardar por orden; el último guardado sobrescribe los campos anteriores. Sí se protegen las relaciones y se serializan las escrituras de esta aplicación.
- Las listas trabajan en memoria para la búsqueda inmediata. Para un volumen grande convendrían paginación y consultas filtradas en SQL. Las categorías cargan imágenes para mantener el ejemplo directo.
- El validador de fotos revisa extensión, tamaño y firma básica. La vista previa intenta decodificar la imagen; no se afirma que la firma por sí sola garantice una imagen íntegra.
- El formato de correo no confirma la existencia de la cuenta. La URL no necesita estar disponible en Internet.
- Editar fotos conserva versiones inactivas y vuelve a insertar la colección visible; simplifica la transacción y aumenta el uso de almacenamiento con el tiempo.

## Evidencia incluida

`resultado_compilacion.txt`, `resultado_pruebas.txt` y `resultado_sintaxis_sql.txt` contienen las salidas de las verificaciones ejecutadas. Las rutas absolutas de compilación pertenecen al entorno de preparación y no deben copiarse al equipo del usuario; use las rutas relativas del README.
