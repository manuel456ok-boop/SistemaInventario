-- Ejecutar primero en SQL Server Management Studio (SSMS).
-- No elimina bases ni tablas existentes. Si ya se instaló, no repetir el esquema.
USE master;
GO
IF DB_ID(N'SistemaInventario') IS NULL
    CREATE DATABASE SistemaInventario;
GO
USE SistemaInventario;
GO
IF OBJECT_ID(N'dbo.Categorias', N'U') IS NOT NULL
    THROW 51000, 'El esquema ya existe. No repita schema.sql; ejecute datos.sql o use su base instalada.', 1;
GO
CREATE TABLE dbo.Categorias (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Categorias PRIMARY KEY,
    Nombre NVARCHAR(100) COLLATE Latin1_General_100_CI_AS NOT NULL,
    Descripcion NVARCHAR(500) NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Categorias_Activo DEFAULT 1,
    FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Categorias_Creacion DEFAULT SYSUTCDATETIME(),
    FechaModificacion DATETIME2(0) NOT NULL CONSTRAINT DF_Categorias_Modificacion DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Categorias_Nombre UNIQUE(Nombre),
    CONSTRAINT CK_Categorias_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) BETWEEN 3 AND 100),
    CONSTRAINT CK_Categorias_Descripcion CHECK (LEN(LTRIM(RTRIM(Descripcion))) BETWEEN 1 AND 500)
);
CREATE TABLE dbo.Suplidores (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Suplidores PRIMARY KEY,
    NombreEmpresa NVARCHAR(100) COLLATE Latin1_General_100_CI_AS NOT NULL,
    NombreContacto NVARCHAR(100) NOT NULL,
    Telefono VARCHAR(12) NOT NULL,
    Correo NVARCHAR(254) NOT NULL,
    SitioWeb NVARCHAR(500) NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Suplidores_Activo DEFAULT 1,
    FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Suplidores_Creacion DEFAULT SYSUTCDATETIME(),
    FechaModificacion DATETIME2(0) NOT NULL CONSTRAINT DF_Suplidores_Modificacion DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Suplidores_Empresa UNIQUE(NombreEmpresa),
    CONSTRAINT CK_Suplidores_Empresa CHECK (LEN(LTRIM(RTRIM(NombreEmpresa))) BETWEEN 3 AND 100),
    CONSTRAINT CK_Suplidores_Contacto CHECK (LEN(LTRIM(RTRIM(NombreContacto))) BETWEEN 3 AND 100),
    CONSTRAINT CK_Suplidores_Telefono CHECK (LEN(Telefono)=12 AND LEFT(Telefono,3) IN ('809','829','849') AND Telefono LIKE '[0-9][0-9][0-9]-[0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]')
);
CREATE TABLE dbo.Productos (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Productos PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    CategoriaId INT NOT NULL CONSTRAINT FK_Productos_Categorias REFERENCES dbo.Categorias(Id),
    SuplidorId INT NOT NULL CONSTRAINT FK_Productos_Suplidores REFERENCES dbo.Suplidores(Id),
    Activo BIT NOT NULL CONSTRAINT DF_Productos_Activo DEFAULT 1,
    FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Productos_Creacion DEFAULT SYSUTCDATETIME(),
    FechaModificacion DATETIME2(0) NOT NULL CONSTRAINT DF_Productos_Modificacion DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Productos_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) BETWEEN 3 AND 150),
    CONSTRAINT CK_Productos_Precio CHECK (PrecioUnitario > 0)
);
CREATE INDEX IX_Productos_Categoria_Activo ON dbo.Productos(CategoriaId,Activo);
CREATE INDEX IX_Productos_Suplidor_Activo ON dbo.Productos(SuplidorId,Activo);
CREATE TABLE dbo.FotosCategoria (
    Id INT IDENTITY(1,1) CONSTRAINT PK_Fotos PRIMARY KEY,
    CategoriaId INT NOT NULL CONSTRAINT FK_Fotos_Categorias REFERENCES dbo.Categorias(Id),
    NombreArchivo NVARCHAR(255) NOT NULL,
    Contenido VARBINARY(MAX) NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Fotos_Activo DEFAULT 1,
    FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Fotos_Creacion DEFAULT SYSUTCDATETIME(),
    FechaModificacion DATETIME2(0) NOT NULL CONSTRAINT DF_Fotos_Modificacion DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Fotos_Tamano CHECK (DATALENGTH(Contenido) BETWEEN 1 AND 2097152),
    CONSTRAINT CK_Fotos_Extension CHECK (LOWER(NombreArchivo) LIKE '%.jpg' OR LOWER(NombreArchivo) LIKE '%.jpeg' OR LOWER(NombreArchivo) LIKE '%.png')
);
CREATE INDEX IX_Fotos_Categoria_Activo ON dbo.FotosCategoria(CategoriaId,Activo);
GO
CREATE TRIGGER dbo.TR_Categorias_ProductosActivos ON dbo.Categorias AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.Productos p WITH (UPDLOCK,HOLDLOCK) ON p.CategoriaId=i.Id WHERE i.Activo=0 AND p.Activo=1)
        THROW 51001, 'No puede eliminar una categoría con productos activos.', 1;
END;
GO
CREATE TRIGGER dbo.TR_Suplidores_ProductosActivos ON dbo.Suplidores AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.Productos p WITH (UPDLOCK,HOLDLOCK) ON p.SuplidorId=i.Id WHERE i.Activo=0 AND p.Activo=1)
        THROW 51002, 'No puede eliminar un suplidor con productos activos.', 1;
END;
GO
CREATE TRIGGER dbo.TR_Productos_RelacionesActivas ON dbo.Productos AFTER INSERT, UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN dbo.Categorias c WITH (UPDLOCK,HOLDLOCK) ON c.Id=i.CategoriaId
        JOIN dbo.Suplidores s WITH (UPDLOCK,HOLDLOCK) ON s.Id=i.SuplidorId
        WHERE i.Activo=1 AND (c.Activo=0 OR s.Activo=0)
    ) THROW 51003, 'El producto necesita una categoría y un suplidor activos. Restaure sus asociaciones primero.', 1;
END;
GO
CREATE TRIGGER dbo.TR_Fotos_MaximoCinco ON dbo.FotosCategoria AFTER INSERT, UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT f.CategoriaId FROM dbo.FotosCategoria f WITH (UPDLOCK,HOLDLOCK)
        WHERE f.Activo=1 AND f.CategoriaId IN (SELECT CategoriaId FROM inserted UNION SELECT CategoriaId FROM deleted)
        GROUP BY f.CategoriaId HAVING COUNT(*)>5
    ) THROW 51004, 'Una categoría admite como máximo cinco fotos activas.', 1;
END;
GO
