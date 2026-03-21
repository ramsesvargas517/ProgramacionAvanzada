-----------------------------
--Creacion de SP

-----------------------------




USE Frijolito;
GO

-- ----------------------------------------------------------------
-- SP_IniciarSesion
-- Valida usuario y contraseña. Retorna datos del usuario si OK.
-- Parámetros: @Username, @PasswordHash
-- ----------------------------------------------------------------
CREATE OR ALTER PROCEDURE IniciarSesion
    @Username     VARCHAR(50),
    @PasswordHash VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        usuario_id,
        username,
        nombre,
        apellido,
        email
    FROM Usuario
    WHERE username      = @Username
      AND password_hash = @PasswordHash;
END
GO

-- ----------------------------------------------------------------
-- SP_RegistrarUsuario
-- Inserta nuevo usuario. Retorna filas afectadas (1=éxito, 0=fallo)
-- Parámetros: @Username, @PasswordHash, @Nombre, @Apellido, @Email
-- ----------------------------------------------------------------
CREATE OR ALTER PROCEDURE RegistrarUsuario
    @Username     VARCHAR(50),
    @PasswordHash VARCHAR(255),
    @Nombre       VARCHAR(100),
    @Apellido     VARCHAR(100),
    @Email        VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Usuario WHERE username = @Username)
    BEGIN
        SELECT 0 AS FilasAfectadas;
        RETURN;
    END

    INSERT INTO Usuario (username, password_hash, nombre, apellido, email)
    VALUES (@Username, @PasswordHash, @Nombre, @Apellido, @Email);

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

-- ----------------------------------------------------------------
-- SP_ObtenerProductos
-- Lista todos los productos activos con su categoría
-- ----------------------------------------------------------------
CREATE OR ALTER PROCEDURE ObtenerProductos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.producto_id,
        p.sku,
        p.nombre,
        p.descripcion,
        p.precio,
        p.stock,
        p.categoria_id,
        c.nombre AS categoria_nombre
    FROM Producto p
    INNER JOIN Categoria c ON p.categoria_id = c.categoria_id
    ORDER BY p.nombre;
END
GO

-- ----------------------------------------------------------------
-- SP_ObtenerProductoPorId
-- Obtiene un producto por su ID
-- ----------------------------------------------------------------
CREATE OR ALTER PROCEDURE ObtenerProductoPorId
    @ProductoId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.producto_id,
        p.sku,
        p.nombre,
        p.descripcion,
        p.precio,
        p.stock,
        p.categoria_id,
        c.nombre AS categoria_nombre
    FROM Producto p
    INNER JOIN Categoria c ON p.categoria_id = c.categoria_id
    WHERE p.producto_id = @ProductoId;
END
GO

-- ----------------------------------------------------------------
-- SP_ObtenerCategorias
-- Lista todas las categorías disponibles
-- ----------------------------------------------------------------
CREATE OR ALTER PROCEDURE ObtenerCategorias
AS
BEGIN
    SET NOCOUNT ON;

    SELECT categoria_id, nombre, descripcion
    FROM Categoria
    ORDER BY nombre;
END
GO

-------------------------
-- ============================================================
-- ACTUALIZACIÓN BD Frijolito
-- Ejecutar sobre la BD existente (NO la borra)
-- Agrega: rol en Usuario, imagen en Producto y Categoria,
--         datos de ejemplo, stored procedures nuevos
-- ============================================================

USE [Frijolito];
GO

-- ============================================================
-- 1. TABLA Rol (nueva)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Rol')
BEGIN
    CREATE TABLE Rol (
        rol_id  INT IDENTITY(1,1) PRIMARY KEY,
        nombre  VARCHAR(50) NOT NULL
    );

    INSERT INTO Rol (nombre) VALUES ('Administrador'), ('Cliente');
    PRINT 'Tabla Rol creada.';
END
GO

-- ============================================================
-- 2. COLUMNA rol_id en Usuario (si no existe)
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Usuario') AND name = 'rol_id'
)
BEGIN
    ALTER TABLE Usuario ADD rol_id INT NOT NULL DEFAULT 2;

    ALTER TABLE Usuario ADD CONSTRAINT FK_Usuario_Rol
        FOREIGN KEY (rol_id) REFERENCES Rol(rol_id);

    PRINT 'Columna rol_id agregada a Usuario.';
END
GO

-- ============================================================
-- 3. COLUMNA imagen en Categoria (si no existe)
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Categoria') AND name = 'imagen'
)
BEGIN
    ALTER TABLE Categoria ADD imagen VARCHAR(300) NULL;
    PRINT 'Columna imagen agregada a Categoria.';
END
GO

-- ============================================================
-- 4. COLUMNA imagen en Producto (si no existe)
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Producto') AND name = 'imagen'
)
BEGIN
    ALTER TABLE Producto ADD imagen VARCHAR(300) NULL;
    PRINT 'Columna imagen agregada a Producto.';
END
GO

-- ============================================================
-- 5. USUARIO ADMINISTRADOR POR DEFECTO
--    Username: admin | Password: Admin123*
--    SHA256 de "Admin123*" en mayúsculas
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE username = 'admin')
BEGIN
    INSERT INTO Usuario (username, password_hash, nombre, apellido, email, rol_id)
    VALUES (
        'admin',
        CONVERT(VARCHAR(256), HASHBYTES('SHA2_256', 'Admin123*'), 2),
        'Administrador',
        'Sistema',
        'admin@frijolito.com',
        1   -- rol Administrador
    );
    PRINT 'Usuario admin creado. Password: Admin123*';
END
GO

-- ============================================================
-- 6. CATEGORÍAS DE EJEMPLO (si la tabla está vacía)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Categoria)
BEGIN
    INSERT INTO Categoria (nombre, descripcion, imagen) VALUES
    ('Educativos',        'Juguetes que estimulan el aprendizaje',         '~/Content/Images/categorias/educativos.jpg'),
    ('Vehículos',         'Autos, camiones y vehículos a escala',          '~/Content/Images/categorias/vehiculos.jpg'),
    ('Muñecos y Figuras', 'Figuras de acción y personajes favoritos',      '~/Content/Images/categorias/munecas.jpg'),
    ('Juegos de Mesa',    'Juegos familiares para todas las edades',       '~/Content/Images/categorias/mesajuegos.jpg'),
    ('Arte y Manualidades','Kits creativos para pintar y construir',        '~/Content/Images/categorias/arte.jpg');

    PRINT 'Categorías de ejemplo insertadas.';
END
GO

-- ============================================================
-- 7. PRODUCTOS DE EJEMPLO (si la tabla está vacía)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Producto)
BEGIN
    INSERT INTO Producto (sku, nombre, descripcion, precio, stock, categoria_id, imagen) VALUES
    ('FRJ-1001', 'Set de Construcción 150 piezas', 'Juguete educativo con 150 piezas coloridas. Ideal 4-8 años.',   12999, 25, 1, '~/Content/Images/productos/construccion.jpg'),
    ('FRJ-1002', 'Auto a Control Remoto',          'Auto recargable con control remoto, velocidad ajustable.',        8500,  15, 2, '~/Content/Images/productos/auto-control.jpg'),
    ('FRJ-1003', 'Figura de Acción Héroe',         'Figura articulada 30cm con accesorios incluidos.',               5999,  30, 3, '~/Content/Images/productos/figura-accion.jpg'),
    ('FRJ-1004', 'Juego de Mesa Familiar',         'Para 2-6 jugadores, mayores de 6 años.',                         9990,  20, 4, '~/Content/Images/productos/juego-mesa.jpg'),
    ('FRJ-1005', 'Kit de Pintura Creativa',        'Set completo con pinturas, pinceles y lienzos.',                 7500,  18, 5, '~/Content/Images/productos/kit-pintura.jpg'),
    ('FRJ-1006', 'Carro de Juguete de Madera',     'Vehículo artesanal de madera para niños de 1-3 años.',           3999,  40, 2, '~/Content/Images/productos/carro-madera.jpg');

    PRINT 'Productos de ejemplo insertados.';
END
GO

-- ============================================================
-- 8. STORED PROCEDURE: IniciarSesion (REEMPLAZA el existente)
--    Ahora retorna también el rol
-- ============================================================
IF OBJECT_ID('IniciarSesion', 'P') IS NOT NULL
    DROP PROCEDURE IniciarSesion;
GO

CREATE PROCEDURE IniciarSesion
    @Username     VARCHAR(50),
    @PasswordHash VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.usuario_id,
        u.username,
        u.nombre,
        u.apellido,
        u.email,
        r.nombre AS rol
    FROM Usuario u
    INNER JOIN Rol r ON u.rol_id = r.rol_id
    WHERE u.username      = @Username
      AND u.password_hash = @PasswordHash;
END
GO

-- ============================================================
-- 9. SP: ObtenerCategorias (actualizado con imagen)
-- ============================================================
IF OBJECT_ID('ObtenerCategorias', 'P') IS NOT NULL
    DROP PROCEDURE ObtenerCategorias;
GO

CREATE PROCEDURE ObtenerCategorias
AS
BEGIN
    SET NOCOUNT ON;
    SELECT categoria_id, nombre, descripcion, imagen
    FROM Categoria
    ORDER BY nombre;
END
GO

-- ============================================================
-- 10. SP: ObtenerProductos (actualizado con imagen)
-- ============================================================
IF OBJECT_ID('ObtenerProductos', 'P') IS NOT NULL
    DROP PROCEDURE ObtenerProductos;
GO

CREATE PROCEDURE ObtenerProductos
    @CategoriaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.producto_id,
        p.sku,
        p.nombre,
        p.descripcion,
        p.precio,
        p.stock,
        p.categoria_id,
        p.imagen,
        c.nombre AS categoria_nombre
    FROM Producto p
    INNER JOIN Categoria c ON p.categoria_id = c.categoria_id
    WHERE (@CategoriaId IS NULL OR p.categoria_id = @CategoriaId)
    ORDER BY p.nombre;
END
GO

-- ============================================================
-- 11. SP: ObtenerProductoPorId (actualizado con imagen)
-- ============================================================
IF OBJECT_ID('ObtenerProductoPorId', 'P') IS NOT NULL
    DROP PROCEDURE ObtenerProductoPorId;
GO

CREATE PROCEDURE ObtenerProductoPorId
    @ProductoId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.producto_id,
        p.sku,
        p.nombre,
        p.descripcion,
        p.precio,
        p.stock,
        p.categoria_id,
        p.imagen,
        c.nombre AS categoria_nombre
    FROM Producto p
    INNER JOIN Categoria c ON p.categoria_id = c.categoria_id
    WHERE p.producto_id = @ProductoId;
END
GO

-- ============================================================
-- 12. SP: InsertarProducto (nuevo)
-- ============================================================
IF OBJECT_ID('InsertarProducto', 'P') IS NOT NULL
    DROP PROCEDURE InsertarProducto;
GO

CREATE PROCEDURE InsertarProducto
    @Sku         VARCHAR(50),
    @Nombre      VARCHAR(150),
    @Descripcion VARCHAR(255),
    @Precio      DECIMAL(10,2),
    @Stock       INT,
    @CategoriaId INT,
    @Imagen      VARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Producto WHERE sku = @Sku)
    BEGIN
        SELECT -1 AS ProductoId;
        RETURN;
    END

    INSERT INTO Producto (sku, nombre, descripcion, precio, stock, categoria_id, imagen)
    VALUES (@Sku, @Nombre, @Descripcion, @Precio, @Stock, @CategoriaId, @Imagen);

    SELECT SCOPE_IDENTITY() AS ProductoId;
END
GO

-- ============================================================
-- 13. SP: ActualizarProducto (nuevo)
-- ============================================================
IF OBJECT_ID('ActualizarProducto', 'P') IS NOT NULL
    DROP PROCEDURE ActualizarProducto;
GO

CREATE PROCEDURE ActualizarProducto
    @ProductoId  INT,
    @Sku         VARCHAR(50),
    @Nombre      VARCHAR(150),
    @Descripcion VARCHAR(255),
    @Precio      DECIMAL(10,2),
    @Stock       INT,
    @CategoriaId INT,
    @Imagen      VARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Producto
    SET sku         = @Sku,
        nombre      = @Nombre,
        descripcion = @Descripcion,
        precio      = @Precio,
        stock       = @Stock,
        categoria_id = @CategoriaId,
        imagen      = CASE WHEN @Imagen IS NULL OR @Imagen = '' THEN imagen ELSE @Imagen END
    WHERE producto_id = @ProductoId;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

-- ============================================================
-- 14. SP: EliminarProducto (nuevo)
-- ============================================================
IF OBJECT_ID('EliminarProducto', 'P') IS NOT NULL
    DROP PROCEDURE EliminarProducto;
GO

CREATE PROCEDURE EliminarProducto
    @ProductoId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Producto WHERE producto_id = @ProductoId;
    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

-- ============================================================
-- 15. SP: InsertarCategoria (nuevo)
-- ============================================================
IF OBJECT_ID('InsertarCategoria', 'P') IS NOT NULL
    DROP PROCEDURE InsertarCategoria;
GO

CREATE PROCEDURE InsertarCategoria
    @Nombre      VARCHAR(100),
    @Descripcion VARCHAR(255),
    @Imagen      VARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Categoria (nombre, descripcion, imagen)
    VALUES (@Nombre, @Descripcion, @Imagen);
    SELECT SCOPE_IDENTITY() AS CategoriaId;
END
GO

-- ============================================================
-- 16. SP: ActualizarCategoria (nuevo)
-- ============================================================
IF OBJECT_ID('ActualizarCategoria', 'P') IS NOT NULL
    DROP PROCEDURE ActualizarCategoria;
GO

CREATE PROCEDURE ActualizarCategoria
    @CategoriaId INT,
    @Nombre      VARCHAR(100),
    @Descripcion VARCHAR(255),
    @Imagen      VARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Categoria
    SET nombre      = @Nombre,
        descripcion = @Descripcion,
        imagen      = CASE WHEN @Imagen IS NULL OR @Imagen = '' THEN imagen ELSE @Imagen END
    WHERE categoria_id = @CategoriaId;
    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

-- ============================================================
-- 17. SP: EliminarCategoria (nuevo)
-- ============================================================
IF OBJECT_ID('EliminarCategoria', 'P') IS NOT NULL
    DROP PROCEDURE EliminarCategoria;
GO

CREATE PROCEDURE EliminarCategoria
    @CategoriaId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Producto WHERE categoria_id = @CategoriaId)
    BEGIN
        SELECT -1 AS FilasAfectadas, 'Tiene productos asociados' AS Mensaje;
        RETURN;
    END

    DELETE FROM Categoria WHERE categoria_id = @CategoriaId;
    SELECT @@ROWCOUNT AS FilasAfectadas, 'Eliminado' AS Mensaje;
END
GO

-- ============================================================
-- 18. SP: ObtenerUsuarios (nuevo, para panel admin)
-- ============================================================
IF OBJECT_ID('ObtenerUsuarios', 'P') IS NOT NULL
    DROP PROCEDURE ObtenerUsuarios;
GO

CREATE PROCEDURE ObtenerUsuarios
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.usuario_id,
        u.username,
        u.nombre,
        u.apellido,
        u.email,
        r.nombre AS rol
    FROM Usuario u
    INNER JOIN Rol r ON u.rol_id = r.rol_id
    ORDER BY u.nombre;
END
GO

PRINT '====================================';
PRINT 'BD Frijolito actualizada con éxito.';
PRINT 'Admin: username=admin / pass=Admin123*';
PRINT '====================================';


----------------------------------------------------
-- ============================================================
-- Agrega columna "genero" a Producto
-- Valores: 'Nino' | 'Nina' | 'Unisex'
-- Ejecutar sobre la BD Frijolito existente
-- ============================================================

USE [Frijolito];
GO

-- 1. Agregar columna genero si no existe
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Producto') AND name = 'genero'
)
BEGIN
    ALTER TABLE Producto ADD genero VARCHAR(10) NOT NULL DEFAULT 'Unisex';
    PRINT 'Columna genero agregada a Producto.';
END
GO

-- 2. Actualizar SP ObtenerProductos → incluye imagen y genero, acepta filtros
IF OBJECT_ID('ObtenerProductos', 'P') IS NOT NULL
    DROP PROCEDURE ObtenerProductos;
GO

CREATE PROCEDURE ObtenerProductos
    @CategoriaId INT     = NULL,
    @Genero      VARCHAR(10) = NULL   -- 'Nino' | 'Nina' | 'Unisex' | NULL = todos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.producto_id,
        p.sku,
        p.nombre,
        p.descripcion,
        p.precio,
        p.stock,
        p.categoria_id,
        p.imagen,
        p.genero,
        c.nombre AS categoria_nombre
    FROM Producto p
    INNER JOIN Categoria c ON p.categoria_id = c.categoria_id
    WHERE
        (@CategoriaId IS NULL OR p.categoria_id = @CategoriaId)
        AND (
            @Genero IS NULL
            OR p.genero = @Genero
            OR p.genero = 'Unisex'   -- Unisex aparece siempre en Niño y Niña
        )
    ORDER BY p.nombre;
END
GO

-- 3. Actualizar SP ObtenerProductoPorId → incluye imagen y genero
IF OBJECT_ID('ObtenerProductoPorId', 'P') IS NOT NULL
    DROP PROCEDURE ObtenerProductoPorId;
GO

CREATE PROCEDURE ObtenerProductoPorId
    @ProductoId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.producto_id,
        p.sku,
        p.nombre,
        p.descripcion,
        p.precio,
        p.stock,
        p.categoria_id,
        p.imagen,
        p.genero,
        c.nombre AS categoria_nombre
    FROM Producto p
    INNER JOIN Categoria c ON p.categoria_id = c.categoria_id
    WHERE p.producto_id = @ProductoId;
END
GO

-- 4. Actualizar SP InsertarProducto → acepta genero
IF OBJECT_ID('InsertarProducto', 'P') IS NOT NULL
    DROP PROCEDURE InsertarProducto;
GO

CREATE PROCEDURE InsertarProducto
    @Sku         VARCHAR(50),
    @Nombre      VARCHAR(150),
    @Descripcion VARCHAR(255),
    @Precio      DECIMAL(10,2),
    @Stock       INT,
    @CategoriaId INT,
    @Imagen      VARCHAR(300) = NULL,
    @Genero      VARCHAR(10)  = 'Unisex'
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Producto WHERE sku = @Sku)
    BEGIN
        SELECT -1 AS ProductoId;
        RETURN;
    END

    INSERT INTO Producto (sku, nombre, descripcion, precio, stock, categoria_id, imagen, genero)
    VALUES (@Sku, @Nombre, @Descripcion, @Precio, @Stock, @CategoriaId, @Imagen, @Genero);

    SELECT SCOPE_IDENTITY() AS ProductoId;
END
GO

-- 5. Actualizar SP ActualizarProducto → acepta genero
IF OBJECT_ID('ActualizarProducto', 'P') IS NOT NULL
    DROP PROCEDURE ActualizarProducto;
GO

CREATE PROCEDURE ActualizarProducto
    @ProductoId  INT,
    @Sku         VARCHAR(50),
    @Nombre      VARCHAR(150),
    @Descripcion VARCHAR(255),
    @Precio      DECIMAL(10,2),
    @Stock       INT,
    @CategoriaId INT,
    @Imagen      VARCHAR(300) = NULL,
    @Genero      VARCHAR(10)  = 'Unisex'
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Producto
    SET sku          = @Sku,
        nombre       = @Nombre,
        descripcion  = @Descripcion,
        precio       = @Precio,
        stock        = @Stock,
        categoria_id = @CategoriaId,
        genero       = @Genero,
        imagen       = CASE WHEN @Imagen IS NULL OR @Imagen = '' THEN imagen ELSE @Imagen END
    WHERE producto_id = @ProductoId;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

-- 6. Actualizar productos de ejemplo con género
UPDATE Producto SET genero = 'Nino'    WHERE sku IN ('FRJ-1002', 'FRJ-1003');
UPDATE Producto SET genero = 'Nina'    WHERE sku IN ('FRJ-1005');
UPDATE Producto SET genero = 'Unisex'  WHERE sku IN ('FRJ-1001', 'FRJ-1004', 'FRJ-1006');
GO

PRINT '====================================';
PRINT 'Columna genero y SPs actualizados.';
PRINT '====================================';
