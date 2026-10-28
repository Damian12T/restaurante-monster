-- ================================================================
-- BASE DE DATOS: restaurante_monster (SQL Server)
-- Restaurante Monster | Grupo G06 | Web Avanzada
-- Convertido desde MySQL a SQL Server
-- Ejecutar en SQL Server Management Studio 22
-- ================================================================

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'restaurante_monster')
BEGIN
    ALTER DATABASE restaurante_monster SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE restaurante_monster;
END
GO

CREATE DATABASE restaurante_monster COLLATE SQL_Latin1_General_CP1_CI_AS;
GO

USE restaurante_monster;
GO

-- ===== ROLES =====
CREATE TABLE rol (
    id_rol   INT IDENTITY(1,1) PRIMARY KEY,
    nombre   NVARCHAR(50) NOT NULL
);
GO

-- ===== USUARIOS =====
CREATE TABLE usuario (
    id_usuario  INT IDENTITY(1,1) PRIMARY KEY,
    nombre      NVARCHAR(100),
    apellido    NVARCHAR(100),
    correo      NVARCHAR(150) UNIQUE,
    contrasena  NVARCHAR(255),
    telefono    NVARCHAR(20),
    id_rol      INT,
    estado      BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (id_rol) REFERENCES rol(id_rol)
);
GO

-- ===== TIPOS DE PLATO =====
CREATE TABLE tipo_plato (
    id_tipo INT IDENTITY(1,1) PRIMARY KEY,
    nombre  NVARCHAR(80) NOT NULL
);
GO

-- ===== PLATOS =====
CREATE TABLE plato (
    id_plato    INT IDENTITY(1,1) PRIMARY KEY,
    nombre      NVARCHAR(150) NOT NULL,
    descripcion NVARCHAR(MAX),
    precio      DECIMAL(8,2) NOT NULL,
    imagen      NVARCHAR(255),
    id_tipo     INT,
    disponible  BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (id_tipo) REFERENCES tipo_plato(id_tipo)
);
GO

-- ===== BEBIDAS =====
CREATE TABLE bebida (
    id_bebida   INT IDENTITY(1,1) PRIMARY KEY,
    nombre      NVARCHAR(150) NOT NULL,
    descripcion NVARCHAR(MAX),
    precio      DECIMAL(8,2) NOT NULL,
    imagen      NVARCHAR(255),
    disponible  BIT NOT NULL DEFAULT 1
);
GO

-- ===== STOCK =====
CREATE TABLE stock (
    id_stock    INT IDENTITY(1,1) PRIMARY KEY,
    id_plato    INT NULL,
    id_bebida   INT NULL,
    cantidad    INT NOT NULL DEFAULT 0,
    stock_minimo INT DEFAULT 5,
    FOREIGN KEY (id_plato)  REFERENCES plato(id_plato),
    FOREIGN KEY (id_bebida) REFERENCES bebida(id_bebida)
);
GO

-- ===== PEDIDOS =====
CREATE TABLE pedido (
    id_pedido   INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario  INT NULL,
    id_mesero   INT NULL,
    fecha       DATETIME2 DEFAULT GETDATE(),
    estado      NVARCHAR(30) DEFAULT 'PENDIENTE',
    tipo_pedido NVARCHAR(20) DEFAULT 'MESA',
    FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario),
    FOREIGN KEY (id_mesero)  REFERENCES usuario(id_usuario)
);
GO

-- ===== DETALLE PEDIDO =====
CREATE TABLE detalle_pedido (
    id_detalle      INT IDENTITY(1,1) PRIMARY KEY,
    id_pedido       INT,
    id_plato        INT NULL,
    id_bebida       INT NULL,
    cantidad        INT NOT NULL DEFAULT 1,
    precio_unitario DECIMAL(8,2),
    subtotal        DECIMAL(8,2),
    FOREIGN KEY (id_pedido)  REFERENCES pedido(id_pedido),
    FOREIGN KEY (id_plato)   REFERENCES plato(id_plato),
    FOREIGN KEY (id_bebida)  REFERENCES bebida(id_bebida)
);
GO

-- ===== FACTURA =====
CREATE TABLE factura (
    id_factura      INT IDENTITY(1,1) PRIMARY KEY,
    id_pedido       INT UNIQUE,
    fecha           DATETIME2 DEFAULT GETDATE(),
    subtotal        DECIMAL(8,2),
    cargo_servicio  DECIMAL(8,2),
    iva             DECIMAL(8,2),
    total           DECIMAL(8,2),
    estado          NVARCHAR(20) DEFAULT 'EMITIDA',
    FOREIGN KEY (id_pedido) REFERENCES pedido(id_pedido)
);
GO

-- ===== CLIENTES (perfil extendido) =====
CREATE TABLE cliente (
    id_cliente      INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario      INT,
    direccion       NVARCHAR(200),
    fecha_nacimiento DATE,
    FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario)
);
GO

-- ================================================================
-- DATOS SEMILLA
-- ================================================================

INSERT INTO rol (nombre) VALUES ('ADMIN'), ('MESERO'), ('CLIENTE');

INSERT INTO tipo_plato (nombre) VALUES ('Entrada'), ('Sopa'), ('Plato Fuerte'), ('Postre');

-- Usuarios iniciales
-- Contrasenas: Admin#Monster2025 y Mesero#Monster2025
-- LOWER(CONVERT) genera el mismo hash SHA-256 que el codigo C# (UTF-8 ASCII)
INSERT INTO usuario (nombre, apellido, correo, contrasena, telefono, id_rol, estado) VALUES
('Admin', 'Monster', 'admin@monster.com',
 LOWER(CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', 'Admin#Monster2025'), 2)),
 '0999999999', 1, 1),
('Carlos', 'Mesero', 'mesero@monster.com',
 LOWER(CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', 'Mesero#Monster2025'), 2)),
 '0988888888', 2, 1);

-- ===== ENTRADAS (id_tipo=1) =====
INSERT INTO plato (nombre, descripcion, precio, imagen, id_tipo, disponible) VALUES
('Empanada de morocho','Masa de morocho rellena de carne, cebolla y aji',1.50,'resources/ENTRADA/empanada-de-morocho.jpg',1,1),
('Bolon de verde','Bola de platano verde con chicharron o queso',2.00,'resources/ENTRADA/bolon-de-verde.jpg',1,1),
('Ceviche de camaron','Camaron en curtido de limon, cebolla y cilantro',5.50,'resources/ENTRADA/ceviche-de-camaron.jpg',1,1),
('Patacones con queso','Rodajas de platano verde frito con queso blanco',2.50,'resources/foto.jpg',1,1),
('Llapingachos','Tortillas de papa con queso y chorizo',3.50,'resources/foto.jpg',1,1),
('Tostadas con aguacate','Rodajas de pan con aguacate y sal prieta',2.00,'resources/foto.jpg',1,1);

-- ===== SOPAS (id_tipo=2) =====
INSERT INTO plato (nombre, descripcion, precio, imagen, id_tipo, disponible) VALUES
('Caldo de bolas de verde','Caldo con bolones rellenos de carne y mani',4.50,'resources/SOPAS/caldo-bolas-verde.jpg',2,1),
('Caldo de gallina criolla','Gallina de campo en caldo aromatico con hierbas',5.00,'resources/SOPAS/caldo-de-gallina.webp',2,1),
('Caldo de patas','Pata de res con mote, mani y hierbas',4.00,'resources/SOPAS/caldo-de-patas.png',2,1),
('Sopa de lentejas','Lentejas con verduras, cuero y costilla',3.50,'resources/foto.jpg',2,1),
('Locro de papa','Sopa espesa de papa con queso y aguacate',4.00,'resources/foto.jpg',2,1),
('Fanesca (temporada)','Mezcla de 12 granos con bacalao (Semana Santa)',6.00,'resources/foto.jpg',2,1);

-- ===== PLATOS FUERTES (id_tipo=3) =====
INSERT INTO plato (nombre, descripcion, precio, imagen, id_tipo, disponible) VALUES
('Churrasco','Carne a la plancha con arroz, ensalada y papas fritas',8.50,'resources/PLATO FUERTE/churrasco.jpg',3,1),
('Apanado','Filete de pollo apanado con arroz marinero y ensalada',7.00,'resources/PLATO FUERTE/apanado.webp',3,1),
('Arroz marinero','Arroz con mariscos, camarones, almejas y calamar',9.00,'resources/PLATO FUERTE/arroz-marinero.jpg',3,1),
('Seco de pollo','Guiso de pollo en salsa de chicha y especias con arroz',7.50,'resources/foto.jpg',3,1),
('Seco de chivo','Guiso de chivo con arroz amarillo y maduro frito',8.00,'resources/foto.jpg',3,1),
('Fritada','Carne de cerdo frita con llapingachos, maiz y maduro',8.50,'resources/foto.jpg',3,1),
('Encebollado de atun','Sopa espesa de atun con yuca, cebolla y aji',5.50,'resources/foto.jpg',3,1),
('Corvina al ajillo','Filete de corvina en salsa de ajo y mantequilla',10.00,'resources/foto.jpg',3,1),
('Tilapia frita entera','Tilapia crujiente con patacones y ensalada',9.50,'resources/foto.jpg',3,1),
('Menestra con carne','Menestra de lenteja con carne asada y arroz',7.00,'resources/foto.jpg',3,1);

-- ===== POSTRES (id_tipo=4) =====
INSERT INTO plato (nombre, descripcion, precio, imagen, id_tipo, disponible) VALUES
('Dulce de higos','Higos en almibar con queso fresco',3.00,'resources/POSTRE/dulce-de-higos.webp',4,1),
('Espumilla','Merengue de guayaba con colores pastel',1.50,'resources/POSTRE/espumilla.jpg',4,1),
('Helado de paila','Helado artesanal batido a mano, sabores frutales',2.50,'resources/POSTRE/helado.jpg',4,1),
('Tres leches','Bizcocho empapado en tres leches, crema encima',3.50,'resources/foto.jpg',4,1),
('Arroz con leche','Arroz cremoso con canela y pasas',2.50,'resources/foto.jpg',4,1),
('Cake de chocolate','Porcion de pastel de chocolate artesanal',3.00,'resources/foto.jpg',4,1),
('Quimbolito','Envuelto dulce de maiz con pasas al vapor',2.00,'resources/foto.jpg',4,1);

-- ===== BEBIDAS =====
INSERT INTO bebida (nombre, descripcion, precio, imagen, disponible) VALUES
('Jugo de naranjilla','Jugo natural de naranjilla ecuatoriana',2.00,'resources/foto.jpg',1),
('Jugo de maracuya','Jugo natural de maracuya con azucar',2.00,'resources/foto.jpg',1),
('Jugo de tomate de arbol','Jugo de tomate de arbol fresco',2.00,'resources/foto.jpg',1),
('Jugo de guanabana','Jugo cremoso de guanabana natural',2.50,'resources/foto.jpg',1),
('Colada morada','Bebida tradicional de granos y frutas moradas',2.50,'resources/foto.jpg',1),
('Morocho con leche','Bebida caliente de morocho molido con leche',2.00,'resources/foto.jpg',1),
('Canelazo','Bebida caliente de canela con panela',2.00,'resources/foto.jpg',1),
('Agua de Jamaica','Infusion fria de flor de jamaica',1.50,'resources/foto.jpg',1),
('Coca-Cola','Bebida gaseosa 355ml',1.50,'resources/foto.jpg',1),
('Agua mineral','Agua con o sin gas 500ml',1.00,'resources/foto.jpg',1),
('Cerveza artesanal Monster','Cerveza rubia artesanal local',3.50,'resources/foto.jpg',1),
('Chicha de jora','Chicha tradicional fermentada levemente',2.00,'resources/foto.jpg',1);

-- ===== STOCK INICIAL =====
INSERT INTO stock (id_plato, cantidad, stock_minimo)
SELECT id_plato, 50, 5 FROM plato;

INSERT INTO stock (id_bebida, cantidad, stock_minimo)
SELECT id_bebida, 80, 10 FROM bebida;
GO
