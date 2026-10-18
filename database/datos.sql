-- Datos ficticios de prueba. Se puede repetir sin duplicar los nombres de esta muestra.
USE SistemaInventario;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
INSERT dbo.Categorias(Nombre,Descripcion)
SELECT v.Nombre,v.Descripcion FROM (VALUES
(N'Computadoras',N'Equipos de escritorio y portátiles.'),
(N'Periféricos',N'Dispositivos de entrada y accesorios.'),
(N'Almacenamiento',N'Unidades internas, externas y memorias.'),
(N'Redes',N'Conectividad y redes de datos.'),
(N'Oficina',N'Equipos y suministros de oficina.')
) v(Nombre,Descripcion) WHERE NOT EXISTS(SELECT 1 FROM dbo.Categorias c WHERE c.Nombre=v.Nombre);
INSERT dbo.Suplidores(NombreEmpresa,NombreContacto,Telefono,Correo,SitioWeb)
SELECT v.* FROM (VALUES
(N'Tecno Caribe',N'Ana Pérez','809-555-0101',N'ventas@tecnocaribe.example',N'https://tecnocaribe.example'),
(N'Digital Quisqueya',N'Luis Santos','829-555-0102',N'contacto@digitalquisqueya.example',N'https://digitalquisqueya.example'),
(N'Soluciones del Este',N'María Díaz','849-555-0103',N'ventas@solucioneseste.example',N'https://solucioneseste.example'),
(N'Redes Dominicanas',N'José Ramírez','809-555-0104',N'info@redesdominicanas.example',NULL),
(N'Ofimática Nacional',N'Laura Gómez','829-555-0105',N'ventas@ofimatica.example',NULL)
) v(Empresa,Contacto,Telefono,Correo,Web) WHERE NOT EXISTS(SELECT 1 FROM dbo.Suplidores s WHERE s.NombreEmpresa=v.Empresa);
INSERT dbo.Productos(Nombre,PrecioUnitario,CategoriaId,SuplidorId)
SELECT v.Nombre,v.Precio,c.Id,s.Id FROM (VALUES
(N'Laptop estudio 14',32500.00,N'Computadoras',N'Tecno Caribe'),
(N'Laptop profesional 15',48500.00,N'Computadoras',N'Digital Quisqueya'),
(N'PC escritorio oficina',27000.00,N'Computadoras',N'Soluciones del Este'),
(N'Mini PC compacto',21500.00,N'Computadoras',N'Tecno Caribe'),
(N'Teclado inalámbrico',1250.00,N'Periféricos',N'Digital Quisqueya'),
(N'Mouse óptico',450.00,N'Periféricos',N'Tecno Caribe'),
(N'Webcam HD',2200.00,N'Periféricos',N'Soluciones del Este'),
(N'Audífonos USB',1800.00,N'Periféricos',N'Ofimática Nacional'),
(N'SSD 500 GB',2900.00,N'Almacenamiento',N'Tecno Caribe'),
(N'SSD 1 TB',4800.00,N'Almacenamiento',N'Digital Quisqueya'),
(N'Memoria USB 64 GB',650.00,N'Almacenamiento',N'Ofimática Nacional'),
(N'Disco externo 2 TB',5200.00,N'Almacenamiento',N'Soluciones del Este'),
(N'Router doble banda',3500.00,N'Redes',N'Redes Dominicanas'),
(N'Switch 8 puertos',1850.00,N'Redes',N'Redes Dominicanas'),
(N'Cable de red 3 metros',250.00,N'Redes',N'Digital Quisqueya'),
(N'Adaptador WiFi USB',950.00,N'Redes',N'Redes Dominicanas'),
(N'Impresora multifuncional',12500.00,N'Oficina',N'Ofimática Nacional'),
(N'UPS 750 VA',4200.00,N'Oficina',N'Soluciones del Este'),
(N'Escáner documental',7800.00,N'Oficina',N'Ofimática Nacional'),
(N'Calculadora de escritorio',850.00,N'Oficina',N'Redes Dominicanas')
) v(Nombre,Precio,Categoria,Suplidor)
JOIN dbo.Categorias c ON c.Nombre=v.Categoria AND c.Activo=1
JOIN dbo.Suplidores s ON s.NombreEmpresa=v.Suplidor AND s.Activo=1
WHERE NOT EXISTS(SELECT 1 FROM dbo.Productos p WHERE p.Nombre=v.Nombre);
COMMIT;
SELECT 'Categorias' Tabla,COUNT(*) Cantidad FROM dbo.Categorias
UNION ALL SELECT 'Suplidores',COUNT(*) FROM dbo.Suplidores
UNION ALL SELECT 'Productos',COUNT(*) FROM dbo.Productos;
GO
