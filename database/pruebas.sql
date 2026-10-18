-- PRUEBAS DE INTEGRACIÓN PARA UNA BASE DE PRUEBA recién inicializada.
-- Cada caso revierte sus cambios. Ejecutar todo en una conexión SSMS, después de datos.sql.
USE SistemaInventario;
GO
CREATE PROCEDURE #DebeRechazar @nombre NVARCHAR(100), @sql NVARCHAR(MAX), @esperado INT AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @fallo BIT=0;
    BEGIN TRANSACTION;
    BEGIN TRY
        EXEC sys.sp_executesql @sql;
    END TRY
    BEGIN CATCH
        SET @fallo=1;
        DECLARE @numero INT=ERROR_NUMBER();
        IF XACT_STATE()<>0 ROLLBACK;
        IF @numero<>@esperado THROW;
    END CATCH;
    IF XACT_STATE()<>0 ROLLBACK;
    IF @fallo=0 THROW 51200, 'La operación debió rechazarse y fue aceptada.', 1;
    PRINT CONCAT('OK: ',@nombre);
END;
GO
EXEC #DebeRechazar N'Categoría con productos activos',
N'UPDATE dbo.Categorias SET Activo=0 WHERE Id=(SELECT TOP(1) CategoriaId FROM dbo.Productos WHERE Activo=1);',51001;
EXEC #DebeRechazar N'Suplidor con productos activos',
N'UPDATE dbo.Suplidores SET Activo=0 WHERE Id=(SELECT TOP(1) SuplidorId FROM dbo.Productos WHERE Activo=1);',51002;
EXEC #DebeRechazar N'Unicidad sin distinguir mayúsculas',
N'INSERT dbo.Categorias(Nombre,Descripcion) SELECT UPPER(Nombre),N''Duplicado'' FROM dbo.Categorias WHERE Id=(SELECT MIN(Id) FROM dbo.Categorias);',2627;
EXEC #DebeRechazar N'Precio mayor a cero',
N'UPDATE dbo.Productos SET PrecioUnitario=0 WHERE Id=(SELECT MIN(Id) FROM dbo.Productos);',547;
EXEC #DebeRechazar N'Integridad referencial',
N'UPDATE dbo.Productos SET CategoriaId=-999 WHERE Id=(SELECT MIN(Id) FROM dbo.Productos);',547;
EXEC #DebeRechazar N'Categoría activa al crear producto',
N'INSERT dbo.Categorias(Nombre,Descripcion,Activo) VALUES(CONVERT(nvarchar(36),NEWID()),N''Prueba'',0); DECLARE @c int=CONVERT(int,SCOPE_IDENTITY()); INSERT dbo.Productos(Nombre,PrecioUnitario,CategoriaId,SuplidorId) SELECT N''Prueba temporal'',1,@c,MIN(Id) FROM dbo.Suplidores WHERE Activo=1;',51003;
EXEC #DebeRechazar N'Restauración con categoría inactiva',
N'DECLARE @p int=(SELECT MIN(Id) FROM dbo.Productos); DECLARE @c int=(SELECT CategoriaId FROM dbo.Productos WHERE Id=@p); UPDATE dbo.Productos SET Activo=0 WHERE CategoriaId=@c; UPDATE dbo.Categorias SET Activo=0 WHERE Id=@c; UPDATE dbo.Productos SET Activo=1 WHERE Id=@p;',51003;
EXEC #DebeRechazar N'Límite de cinco fotografías',
N'INSERT dbo.FotosCategoria(CategoriaId,NombreArchivo,Contenido) SELECT (SELECT MIN(Id) FROM dbo.Categorias),N''prueba.png'',0x89504E470D0A1A0A FROM (VALUES(1),(2),(3),(4),(5),(6)) v(n);',51004;
EXEC #DebeRechazar N'Tamaño de fotografía',
N'INSERT dbo.FotosCategoria(CategoriaId,NombreArchivo,Contenido) SELECT MIN(Id),N''vacia.png'',0x FROM dbo.Categorias;',547;
DROP PROCEDURE #DebeRechazar;
GO
