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

