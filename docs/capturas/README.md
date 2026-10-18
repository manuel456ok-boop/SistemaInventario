# Capturas reales de la aplicación

Estado: pendientes de generar en Windows. Las imágenes de diagramas no sustituyen las capturas del programa.

1. Ejecute los scripts SQL, configure la conexión y abra el programa.
2. Mantenga la ventana visible y completa.
3. Desde PowerShell ejecute `./docs/capturas/capturar.ps1` desde la raíz del proyecto.
4. El script le pedirá abrir cada módulo y volver a PowerShell para pulsar Enter. Captura únicamente la ventana del inventario por su identificador, mediante `PrintWindow`.
5. Revise los PNG. En algunos escritorios remotos `PrintWindow` puede producir imágenes negras; en ese caso use Recortes de Windows manualmente.
6. Inserte las rutas de los PNG en el README antes de entregar. Incluya también un formulario con ErrorProvider y una categoría con una foto.

Nombres esperados: `01-productos.png`, `02-categorias.png`, `03-suplidores.png`, `04-papelera.png`, `05-informes.png`.
