-- ================================================================
-- ASIGNAR IMÁGENES A PLATOS Y BEBIDAS
-- Ejecutar en SSMS sobre la base de datos restaurante_monster
-- ================================================================

USE restaurante_monster;
GO

-- ================================================================
-- PLATOS - ENTRADA
-- ================================================================
UPDATE plato SET imagen = 'resources/productos/bolon-de-verde.jpg'
WHERE nombre LIKE '%Bol%n de verde%' OR nombre LIKE '%Bolon de verde%';

UPDATE plato SET imagen = 'resources/productos/ceviche-de-camaron.jpg'
WHERE nombre LIKE '%Ceviche%camaron%' OR nombre LIKE '%Ceviche%camar%n%';

UPDATE plato SET imagen = 'resources/productos/empanada-de-morocho.jpg'
WHERE nombre LIKE '%Empanada%morocho%';

UPDATE plato SET imagen = 'resources/productos/llapingachos.jpg'
WHERE nombre LIKE '%Llapingacho%';

UPDATE plato SET imagen = 'resources/productos/patacones-con-queso.jpg'
WHERE nombre LIKE '%Patacone%queso%';

UPDATE plato SET imagen = 'resources/productos/tostadas-con-aguacate.jpg'
WHERE nombre LIKE '%Tostada%aguacate%';

-- ================================================================
-- PLATOS - PLATO FUERTE
-- ================================================================
UPDATE plato SET imagen = 'resources/productos/apanado.webp'
WHERE nombre LIKE '%Apanado%';

UPDATE plato SET imagen = 'resources/productos/arroz-marinero.jpg'
WHERE nombre LIKE '%Arroz marinero%';

UPDATE plato SET imagen = 'resources/productos/churrasco.jpg'
WHERE nombre LIKE '%Churrasco%';

UPDATE plato SET imagen = 'resources/productos/corvina-al-ajillo.jpg'
WHERE nombre LIKE '%Corvina%ajillo%';

UPDATE plato SET imagen = 'resources/productos/encebollado-de-atun.jpg'
WHERE nombre LIKE '%Encebollado%at%n%' OR nombre LIKE '%Encebollado%atun%';

UPDATE plato SET imagen = 'resources/productos/fritada.avif'
WHERE nombre LIKE '%Fritada%';

UPDATE plato SET imagen = 'resources/productos/menestra-con-carne.jpg'
WHERE nombre LIKE '%Menestra%carne%';

UPDATE plato SET imagen = 'resources/productos/seco-de-chivo.avif'
WHERE nombre LIKE '%Seco%chivo%';

UPDATE plato SET imagen = 'resources/productos/seco-de-pollo.webp'
WHERE nombre LIKE '%Seco%pollo%';

UPDATE plato SET imagen = 'resources/productos/tilapia-frita.jpg'
WHERE nombre LIKE '%Tilapia%' OR nombre LIKE '%tilapia%';

-- ================================================================
-- PLATOS - POSTRES
-- ================================================================
UPDATE plato SET imagen = 'resources/productos/dulce-de-higos.webp'
WHERE nombre LIKE '%Dulce%higos%';

UPDATE plato SET imagen = 'resources/productos/espumilla.jpg'
WHERE nombre LIKE '%Espumilla%';

UPDATE plato SET imagen = 'resources/productos/helado-de-paila.jpg'
WHERE nombre LIKE '%Helado%paila%' OR nombre LIKE '%Helado%';

-- ================================================================
-- PLATOS - SOPAS
-- ================================================================
UPDATE plato SET imagen = 'resources/productos/caldo-bolas-verde.jpg'
WHERE nombre LIKE '%Caldo%bola%verde%';

UPDATE plato SET imagen = 'resources/productos/caldo-de-gallina.webp'
WHERE nombre LIKE '%Caldo%gallina%';

UPDATE plato SET imagen = 'resources/productos/caldo-de-patas.png'
WHERE nombre LIKE '%Caldo%patas%';

UPDATE plato SET imagen = 'resources/productos/fanesca.jpg'
WHERE nombre LIKE '%Fanesca%';

UPDATE plato SET imagen = 'resources/productos/locro-de-papa.jpg'
WHERE nombre LIKE '%Locro%papa%';

UPDATE plato SET imagen = 'resources/productos/sopa-de-lentejas.jpg'
WHERE nombre LIKE '%Sopa%lenteja%' OR nombre LIKE '%lenteja%';

-- ================================================================
-- BEBIDAS
-- ================================================================
UPDATE bebida SET imagen = 'resources/productos/morocho.jpg'
WHERE nombre LIKE '%Morocho%';

UPDATE bebida SET imagen = 'resources/productos/agua-mineral.jpg'
WHERE nombre LIKE '%Agua%mineral%' OR nombre LIKE '%agua%';

UPDATE bebida SET imagen = 'resources/productos/cerveza-artesanal.jpg'
WHERE nombre LIKE '%Cerveza%';

UPDATE bebida SET imagen = 'resources/productos/chicha-de-jora.webp'
WHERE nombre LIKE '%Chicha%jora%' OR nombre LIKE '%chicha%';

UPDATE bebida SET imagen = 'resources/productos/coca-cola.png'
WHERE nombre LIKE '%Coca%Cola%' OR nombre LIKE '%Coca-Cola%';

UPDATE bebida SET imagen = 'resources/productos/jugo-de-guanabana.jpg'
WHERE nombre LIKE '%Guanábana%' OR nombre LIKE '%guanabana%';

UPDATE bebida SET imagen = 'resources/productos/jugo-de-maracuya.webp'
WHERE nombre LIKE '%Maracuy%' OR nombre LIKE '%maracuya%';

UPDATE bebida SET imagen = 'resources/productos/colada-morada.jpg'
WHERE nombre LIKE '%Colada morada%' OR nombre LIKE '%colada%';

UPDATE bebida SET imagen = 'resources/productos/canelazo.webp'
WHERE nombre LIKE '%Canelazo%';

UPDATE bebida SET imagen = 'resources/productos/te-de-jamaica.jpg'
WHERE nombre LIKE '%Jamaica%' OR nombre LIKE '%jamaica%';

UPDATE bebida SET imagen = 'resources/productos/jugo-de-naranja.webp'
WHERE nombre LIKE '%naranja%' OR nombre LIKE '%Naranja%';

UPDATE bebida SET imagen = 'resources/productos/jugo-de-tomate-de-arbol.jpg'
WHERE nombre LIKE '%tomate%rbol%' OR nombre LIKE '%tomate de%rbol%';

-- ================================================================
-- VERIFICAR RESULTADOS
-- ================================================================
SELECT 'PLATOS' AS tabla, nombre, imagen FROM plato ORDER BY nombre;
SELECT 'BEBIDAS' AS tabla, nombre, imagen FROM bebida ORDER BY nombre;
GO
