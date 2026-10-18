# Diagramas

Los SVG incluidos se pueden abrir directamente en el navegador. Estas fuentes Mermaid se renderizan en GitHub.

## Clases principales

```mermaid
classDiagram
    direction TB
    class EntidadBase {
        +int Id
        +bool Activo
        +DateTime FechaCreacion
        +DateTime FechaModificacion
    }
    class Categoria {
        +string Nombre
        +string Descripcion
        +List~FotoCategoria~ Fotos
    }
    class Suplidor {
        +string NombreEmpresa
        +string NombreContacto
        +string Telefono
        +string Correo
        +string SitioWeb
    }
    class Producto {
        +string Nombre
        +decimal PrecioUnitario
        +int CategoriaId
        +int SuplidorId
    }
    class FotoCategoria {
        +int CategoriaId
        +string NombreArchivo
        +byte[] Contenido
    }
    EntidadBase <|-- Categoria
    EntidadBase <|-- Suplidor
    EntidadBase <|-- Producto
    EntidadBase <|-- FotoCategoria
    Categoria "1" -- "0..*" Producto : clasifica
    Suplidor "1" -- "0..*" Producto : suministra
    Categoria "1" *-- "0..*" FotoCategoria : conserva
    note for Categoria "Máximo 5 fotos activas; historial inactivo conservado"
    class IRepositorio~T~ {
        <<interface>>
        +Listar(bool activos)
        +Obtener(int id)
        +Crear(T entidad)
        +Actualizar(T entidad)
        +Eliminar(int id)
        +Restaurar(int id)
    }
    class RepositorioSql~T~
    IRepositorio~T~ <|.. RepositorioSql~T~
    RepositorioSql~T~ <|-- CategoriaRepositorio
    RepositorioSql~T~ <|-- SuplidorRepositorio
    RepositorioSql~T~ <|-- ProductoRepositorio
    class ConexionSql {
        -ConexionSql()
        +Instancia
        +Configurar(string cadenaConexion)
        +CrearConexion()
    }
    RepositorioSql~T~ --> EjecutorSql
    EjecutorSql --> ConexionSql
    EjecutorSql --> ILogger
    ILogger <|.. Logger
    class ServicioBase~T~
    class IServicio~T~ {
        <<interface>>
    }
    IServicio~T~ <|.. ServicioBase~T~
    ServicioBase~T~ --> IRepositorio~T~
    ServicioBase~T~ <|-- CategoriaServicio
    ServicioBase~T~ <|-- SuplidorServicio
    ServicioBase~T~ <|-- ProductoServicio
    PrincipalForm --> ServiciosAplicacion
    ServiciosAplicacion --> IServicio~T~
    Composicion ..> ServiciosAplicacion : construye
```

## Modelo entidad-relación

```mermaid
erDiagram
    CATEGORIAS ||--o{ PRODUCTOS : clasifica
    SUPLIDORES ||--o{ PRODUCTOS : suministra
    CATEGORIAS ||--o{ FOTOS_CATEGORIA : conserva
    CATEGORIAS {
        int Id PK
        nvarchar100 Nombre UK
        nvarchar500 Descripcion
        bit Activo
        datetime2 FechaCreacion
        datetime2 FechaModificacion
    }
    SUPLIDORES {
        int Id PK
        nvarchar100 NombreEmpresa UK
        nvarchar100 NombreContacto
        varchar12 Telefono
        nvarchar254 Correo
        nvarchar500 SitioWeb "NULL permitido"
        bit Activo
        datetime2 FechaCreacion
        datetime2 FechaModificacion
    }
    PRODUCTOS {
        int Id PK
        nvarchar150 Nombre
        decimal18_2 PrecioUnitario
        int CategoriaId FK
        int SuplidorId FK
        bit Activo
        datetime2 FechaCreacion
        datetime2 FechaModificacion
    }
    FOTOS_CATEGORIA {
        int Id PK
        int CategoriaId FK
        nvarchar255 NombreArchivo
        varbinaryMAX Contenido
        bit Activo
        datetime2 FechaCreacion
        datetime2 FechaModificacion
    }
```

El máximo de cinco fotos aplica a las filas activas. La cardinalidad física es 0..N porque las versiones anteriores de fotografías se conservan inactivas. Cada fotografía tiene un único padre.

Los nombres de categoría y empresa son únicos en toda la tabla, incluso en la papelera. Las FK no utilizan borrado en cascada. Los triggers controlan el estado activo de asociaciones y el máximo de fotos. Las fechas se almacenan en UTC.
