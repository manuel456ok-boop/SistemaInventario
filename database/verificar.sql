-- Comprobaciones de integridad de SOLO LECTURA. Ejecutar después de schema.sql y datos.sql.
USE SistemaInventario;
GO
IF (SELECT COUNT(*) FROM dbo.Categorias) < 5 THROW 51200, 'Faltan categorías de prueba.', 1;
IF (SELECT COUNT(*) FROM dbo.Suplidores) < 5 THROW 51200, 'Faltan suplidores de prueba.', 1;
IF (SELECT COUNT(*) FROM dbo.Productos) < 20 THROW 51200, 'Faltan productos de prueba.', 1;
IF EXISTS (SELECT 1 FROM dbo.Productos p JOIN dbo.Categorias c ON c.Id=p.CategoriaId JOIN dbo.Suplidores s ON s.Id=p.SuplidorId WHERE p.Activo=1 AND (c.Activo=0 OR s.Activo=0))
    THROW 51200, 'Existen productos activos con asociaciones inactivas.', 1;
IF EXISTS (SELECT CategoriaId FROM dbo.FotosCategoria WHERE Activo=1 GROUP BY CategoriaId HAVING COUNT(*)>5)
    THROW 51200, 'Una categoría supera cinco fotos.', 1;
PRINT 'Integridad básica correcta. Esto no reemplaza las pruebas de interfaz.';
SELECT Id,Nombre,Activo FROM dbo.Categorias ORDER BY Id;
SELECT Id,NombreEmpresa,Activo FROM dbo.Suplidores ORDER BY Id;
SELECT Id,Nombre,PrecioUnitario,CategoriaId,SuplidorId,Activo FROM dbo.Productos ORDER BY Id;
GO
