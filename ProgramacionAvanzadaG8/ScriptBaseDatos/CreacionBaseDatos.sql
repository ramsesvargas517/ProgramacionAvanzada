CREATE DATABASE Frijolito;
GO

USE Frijolito;
GO

CREATE TABLE Usuario (
    usuario_id INT IDENTITY(1,1) PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL
);
GO


CREATE TABLE Cliente (
    cliente_id INT IDENTITY(1,1) PRIMARY KEY,
    identificacion VARCHAR(50) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    telefono VARCHAR(20),
    email VARCHAR(150),
    direccion VARCHAR(255),
    fecha_registro DATETIME DEFAULT GETDATE()
);
GO


CREATE TABLE Categoria (
    categoria_id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255)
);
GO


CREATE TABLE Producto (
    producto_id INT IDENTITY(1,1) PRIMARY KEY,
    sku VARCHAR(50) NOT NULL UNIQUE,
    nombre VARCHAR(150) NOT NULL,
    descripcion VARCHAR(255),
    precio DECIMAL(10,2) NOT NULL,
    stock INT NOT NULL,
    categoria_id INT NOT NULL,

    CONSTRAINT FK_Producto_Categoria
        FOREIGN KEY (categoria_id)
        REFERENCES Categoria(categoria_id)
);
GO


CREATE TABLE Venta (
    venta_id INT IDENTITY(1,1) PRIMARY KEY,
    fecha DATETIME DEFAULT GETDATE(),
    cliente_id INT NOT NULL,
    usuario_id INT NOT NULL,
    subtotal DECIMAL(10,2) NOT NULL,
    impuesto DECIMAL(10,2) NOT NULL,
    descuento DECIMAL(10,2) DEFAULT 0,
    total DECIMAL(10,2) NOT NULL,
    estado VARCHAR(50) NOT NULL,

    CONSTRAINT FK_Venta_Cliente
        FOREIGN KEY (cliente_id)
        REFERENCES Cliente(cliente_id),

    CONSTRAINT FK_Venta_Usuario
        FOREIGN KEY (usuario_id)
        REFERENCES Usuario(usuario_id)
);
GO

CREATE TABLE Detalle_Venta (
    detalle_venta_id INT IDENTITY(1,1) PRIMARY KEY,
    venta_id INT NOT NULL,
    producto_id INT NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL,
    descuento_linea DECIMAL(10,2) DEFAULT 0,
    total_linea DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_DetalleVenta_Venta
        FOREIGN KEY (venta_id)
        REFERENCES Venta(venta_id),

    CONSTRAINT FK_DetalleVenta_Producto
        FOREIGN KEY (producto_id)
        REFERENCES Producto(producto_id)
);
GO


CREATE TABLE Metodo_Pago (
    metodo_pago_id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL
);
GO


CREATE TABLE Pago (
    pago_id INT IDENTITY(1,1) PRIMARY KEY,
    venta_id INT NOT NULL,
    metodo_pago_id INT NOT NULL,
    fecha_pago DATETIME DEFAULT GETDATE(),
    monto DECIMAL(10,2) NOT NULL,
    referencia VARCHAR(100),

    CONSTRAINT FK_Pago_Venta
        FOREIGN KEY (venta_id)
        REFERENCES Venta(venta_id),

    CONSTRAINT FK_Pago_MetodoPago
        FOREIGN KEY (metodo_pago_id)
        REFERENCES Metodo_Pago(metodo_pago_id)
);
GO