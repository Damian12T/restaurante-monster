# Restaurante Monster · ASP.NET Core MVC + SQL Server

Sistema web para un restaurante con tres roles: el **cliente** pide desde un menú tipo quiosco, el **mesero** toma pedidos en mesa y el **administrador** gestiona el menú, el stock, los usuarios, las facturas y los reportes. Construido con **ASP.NET Core MVC (.NET 10)**, **SQL Server** y ADO.NET, con el patrón **MVC + DAO**.

![.NET](https://img.shields.io/badge/.NET-10-512BD4) ![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-5C2D91) ![SQL Server](https://img.shields.io/badge/SQL%20Server-ADO.NET-CC2927) ![Unitarias](https://img.shields.io/badge/xUnit-14%2F14-brightgreen) ![E2E](https://img.shields.io/badge/E2E-29%2F29%20%C2%B7%209%2F9%20seguridad-brightgreen) ![Licencia](https://img.shields.io/badge/licencia-MIT-lightgrey)

| | |
|---|---|
| **Problema** | En un restaurante, los pedidos, la facturación y el inventario suelen llevarse por separado: el stock no se descuenta al vender, las facturas se calculan a mano y no hay control de quién puede modificar el menú |
| **Objetivo** | Un sistema único en el que cada pedido (de cliente o de mesero) valida y descuenta stock, genera su factura con cargo de servicio e IVA, y en el que cada rol solo accede a lo suyo |
| **Estado** | ✅ Funcional. 14/14 pruebas unitarias y prueba E2E con 29/29 comprobaciones funcionales y 9/9 de seguridad (2026-10-28) |
| **Autor** | Edgar Damian Toscano Coro · Estudiante de Ingeniería de Software · Universidad de las Fuerzas Armadas ESPE |

## Contenido

[Capturas](#capturas) · [Funcionalidades](#funcionalidades) · [Tecnologías](#tecnologías) · [Arquitectura](#arquitectura) · [Modelo de datos](#modelo-de-datos) · [Seguridad](#seguridad) · [Instalación](#instalación-y-ejecución-local) · [Pruebas](#pruebas-y-validación) · [Limitaciones](#limitaciones-y-mejoras-futuras) · [Autoría](#autoría-y-créditos)

## Capturas

Capturas reales tomadas con Chrome sobre una base de datos recién creada con el script del repositorio: datos semilla y un cliente ficticio. Las fotografías de los platos no se incluyen en el repositorio (ver [Limitaciones](#limitaciones-y-mejoras-futuras)).

| Inicio de sesión | Registro de clientes |
|---|---|
| ![Login](docs/screenshots/01-login.png) | ![Registro](docs/screenshots/02-registro.png) |

**Cliente (quiosco)**

| Menú con filtros por categoría y buscador | Carrito |
|---|---|
| ![Menú](docs/screenshots/03-cliente-menu.png) | ![Carrito](docs/screenshots/04-cliente-carrito.png) |

| Factura con cargo de servicio e IVA | Vista móvil |
|---|---|
| ![Factura del cliente](docs/screenshots/05-cliente-factura.png) | <img src="docs/screenshots/17-cliente-movil.png" alt="Vista móvil" width="260"> |

**Mesero**

| Panel del mesero | Pedido en curso |
|---|---|
| ![Panel del mesero](docs/screenshots/06-mesero-dashboard.png) | ![Tomar pedido](docs/screenshots/07-mesero-tomar-pedido.png) |

| Factura del pedido en mesa | Acceso denegado a la administración |
|---|---|
| ![Factura del mesero](docs/screenshots/08-mesero-factura.png) | ![Sin permiso](docs/screenshots/16-mesero-sin-permiso.png) |

**Administrador**

| Panel con pedidos y ventas del día | Gestión de platos |
|---|---|
| ![Dashboard](docs/screenshots/09-admin-dashboard.png) | ![Platos](docs/screenshots/10-admin-platos.png) |

| Categorías | Empleados |
|---|---|
| ![Categorías](docs/screenshots/11-admin-categorias.png) | ![Empleados](docs/screenshots/12-admin-empleados.png) |

| Stock con mínimos | Facturas |
|---|---|
| ![Stock](docs/screenshots/13-admin-stock.png) | ![Facturas](docs/screenshots/14-admin-facturas.png) |

![Reportes](docs/screenshots/15-admin-reportes.png)

## Funcionalidades

| Rol | Funcionalidad | Resultado | Restricciones |
|---|---|---|---|
| Público | Registro de cliente | Crea la cuenta con rol `CLIENTE` | Correo único; contraseña de 6 caracteres como mínimo |
| Todos | Inicio y cierre de sesión | Redirige al panel de su rol | Cuentas desactivadas no entran |
| Cliente | Menú tipo quiosco | Filtros por categoría, búsqueda, botones + / − sin recargar la página | Solo platos y bebidas disponibles |
| Cliente | Carrito y pedido | Verifica stock, descuenta inventario y genera la factura | Rechaza el pedido si falta stock |
| Cliente | Factura | Subtotal, cargo de servicio (10 %), IVA y total; imprimible | Solo sus propias facturas |
| Mesero | Tomar pedido en mesa | Mismo flujo, con cliente opcional asignado | — |
| Mesero | Registrar cliente | Alta rápida desde el panel | — |
| Mesero | Alerta de stock | Lista de productos bajo el mínimo | — |
| Admin | Platos y bebidas | Crear, editar, activar o desactivar, con imagen JPEG/PNG/WEBP | Imagen de 2 MB como máximo |
| Admin | Categorías | Crear, editar y eliminar | No elimina categorías con platos |
| Admin | Clientes y empleados | Alta, edición, activación y desactivación | — |
| Admin | Stock | Cantidad y mínimo por fila o todo a la vez | — |
| Admin | Facturas y reportes | Listado, detalle y estadísticas del día | — |

## Tecnologías

| Categoría | Tecnología | Uso |
|---|---|---|
| Lenguaje | C# | Aplicación y pruebas |
| Backend | ASP.NET Core MVC (.NET 10) | Controladores, vistas Razor, sesión |
| Base de datos | SQL Server, `Microsoft.Data.SqlClient` 7 (ADO.NET) | Persistencia con consultas parametrizadas |
| Frontend | Razor, HTML, CSS propio y JavaScript sin frameworks | Interfaz de los tres roles |
| Seguridad | PBKDF2-SHA256 (`Rfc2898DeriveBytes`), antiforgery de ASP.NET Core, `JavaScriptEncoder` | Contraseñas, CSRF y XSS |
| Pruebas | xUnit; Puppeteer (`puppeteer-core`) + Chrome | Unitarias y de extremo a extremo |

## Arquitectura

```
restaurante-monster/
├── Program.cs                 Servicios, sesión, antiforgery global, cabeceras de seguridad, rutas
├── Controllers/               Seguridad, Admin, Mesero, Cliente, Carrito (+ BaseController: sesión y roles)
├── Data/                      Conexion y un DAO por entidad (ADO.NET, SqlCommand con parámetros)
├── Models/                    Usuario, Cliente, Empleado, Plato, Bebida, TipoPlato, Stock, Pedido, DetallePedido, Factura
├── Helpers/                   Encriptador (contraseñas), SubidaImagen, Js (codificación para JS), SessionExtensions
├── Views/                     Razor por rol + layouts
├── wwwroot/                   CSS, js/antiforgery.js, resources/productos (imágenes subidas, no versionadas)
├── database/                  Script de creación con datos semilla y asignación de imágenes
└── tests/
    ├── RestauranteMonster.Tests/   14 pruebas unitarias (xUnit)
    └── e2e/e2e-restaurante.mjs     Prueba E2E funcional y de seguridad
```

```mermaid
flowchart LR
    N[Navegador] -->|GET / POST + token| C[Controller<br/>CheckRole]
    C -->|sesión: usuario y carrito| S[(Sesión)]
    C --> D[DAO]
    D -->|SqlCommand parametrizado| BD[(SQL Server)]
    C --> V[Vista Razor]
    V --> N
```

**Flujo de un pedido:** el carrito vive en la sesión → al confirmar se verifica el stock de cada ítem → se crea el `pedido` y sus `detalle_pedido` → se descuenta el stock → se genera la `factura` (subtotal + 10 % de servicio + IVA) → el pedido pasa a `PAGADO`.

## Modelo de datos

**SGBD:** SQL Server. Script: [`database/restaurante_monster_sqlserver.sql`](database/restaurante_monster_sqlserver.sql) (10 tablas; datos semilla: 3 roles, 4 categorías, 29 platos, 12 bebidas, stock y 2 cuentas de demostración).

```mermaid
erDiagram
    ROL ||--o{ USUARIO : "tiene"
    USUARIO ||--o| CLIENTE : "perfil"
    USUARIO ||--o{ PEDIDO : "realiza (id_usuario)"
    USUARIO ||--o{ PEDIDO : "atiende (id_mesero)"
    TIPO_PLATO ||--o{ PLATO : "agrupa"
    PLATO ||--o| STOCK : "existencias"
    BEBIDA ||--o| STOCK : "existencias"
    PEDIDO ||--|{ DETALLE_PEDIDO : "contiene"
    PLATO ||--o{ DETALLE_PEDIDO : "se pide"
    BEBIDA ||--o{ DETALLE_PEDIDO : "se pide"
    PEDIDO ||--o| FACTURA : "genera"
    ROL {
        int id_rol PK
        nvarchar nombre
    }
    USUARIO {
        int id_usuario PK
        nvarchar correo UK
        nvarchar contrasena "PBKDF2"
        int id_rol FK
        bit estado
    }
    CLIENTE {
        int id_cliente PK
        int id_usuario FK
        nvarchar direccion
        date fecha_nacimiento
    }
    TIPO_PLATO {
        int id_tipo PK
        nvarchar nombre
    }
    PLATO {
        int id_plato PK
        nvarchar nombre
        decimal precio
        nvarchar imagen
        int id_tipo FK
        bit disponible
    }
    BEBIDA {
        int id_bebida PK
        nvarchar nombre
        decimal precio
        nvarchar imagen
        bit disponible
    }
    STOCK {
        int id_stock PK
        int id_plato FK
        int id_bebida FK
        int cantidad
        int stock_minimo
    }
    PEDIDO {
        int id_pedido PK
        int id_usuario FK
        int id_mesero FK
        datetime2 fecha
        nvarchar estado
        nvarchar tipo_pedido
    }
    DETALLE_PEDIDO {
        int id_detalle PK
        int id_pedido FK
        int id_plato FK
        int id_bebida FK
        int cantidad
        decimal precio_unitario
    }
    FACTURA {
        int id_factura PK
        int id_pedido FK "UNIQUE"
        decimal subtotal
        decimal cargo_servicio
        decimal iva
        decimal total
    }
```

`stock` y `detalle_pedido` referencian **un plato o una bebida** (la otra columna queda en `NULL`). `factura.id_pedido` es único: un pedido tiene como máximo una factura.

## Seguridad

### Medidas implementadas y comprobadas

| Riesgo | Medida | Comprobación |
|---|---|---|
| Contraseñas débiles en reposo | **PBKDF2-HMAC-SHA256**, sal aleatoria de 16 bytes, 600 000 iteraciones, comparación en tiempo constante. Los hashes SHA-256 antiguos se aceptan y **se migran automáticamente** en el primer inicio de sesión | Unitarias + E2E (S01, S02, F04, F21) |
| XSS almacenado | Datos insertados en JavaScript con `JavaScriptEncoder` (helper `Js.Cadena`); el resto con la codificación HTML de Razor | E2E (S09) con nombres maliciosos registrados por el público |
| CSRF | Filtro global `AutoValidateAntiforgeryToken`; token en todos los formularios y en las llamadas `fetch`; cookie de sesión `SameSite=Lax` y `HttpOnly` | E2E (S05: POST sin token → 400) |
| Cambios de estado por GET | Activar o desactivar y eliminar solo por POST | E2E (S03, S04) |
| Subida de archivos maliciosos | Tipo detectado por la firma binaria (JPEG/PNG/WEBP); extensión derivada del contenido; nombre aleatorio; 2 MB como máximo | Unitarias + E2E (S06, S07) |
| Acceso a facturas ajenas (IDOR) | El cliente solo ve facturas de sus propios pedidos | E2E (S08) |
| Control de acceso por rol | `CheckRole` en cada acción | E2E (F02, F19) |
| Inyección SQL | Todas las consultas usan `SqlCommand` con parámetros | Revisión de `Data/` |
| Filtración de información | Mensajes genéricos en login y registro; detalle en el log | Revisión |
| Cabeceras | `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy` | Revisión de `Program.cs` |
| Dependencias | `dotnet list package --vulnerable`: sin paquetes vulnerables | 2026-10-28 |

### Riesgos conocidos y recomendaciones pendientes

- **Sin límite de intentos de inicio de sesión** (fuerza bruta). Recomendado: el *rate limiter* de ASP.NET Core.
- La sesión no se regenera al iniciar sesión (la sesión de ASP.NET Core no permite rotar su identificador; habría que migrar a autenticación por cookies).
- Las cuentas de demostración del script tienen contraseñas conocidas: deben cambiarse fuera de un entorno de prueba.
- En el panel de administración, los errores de base de datos se muestran con su mensaje técnico (solo los ve el administrador).
- La comprobación de stock y su descuento no son atómicos: dos pedidos simultáneos podrían dejar el stock negativo.
- `UseHttpsRedirection` está activo, pero el perfil HTTP no define un puerto HTTPS; en producción debe servirse por HTTPS.

## Instalación y ejecución local

**Requisitos:** .NET SDK 10, SQL Server 2019+ (Express o Developer) con autenticación de Windows, y `sqlcmd` o SQL Server Management Studio.

```bash
# 1. Crear la base de datos con los datos semilla
sqlcmd -S localhost -E -C -i database/restaurante_monster_sqlserver.sql

# 2. Ejecutar
dotnet run --project RestauranteMonsterCSharp.csproj
```

Abrir la URL que indica la consola (perfil por defecto: <http://localhost:58620>).

> ⚠️ El script **elimina y vuelve a crear** la base `restaurante_monster` si ya existe.

| Cuenta de demostración | Correo | Contraseña |
|---|---|---|
| Administrador | `admin@monster.com` | `Admin#Monster2025` |
| Mesero | `mesero@monster.com` | `Mesero#Monster2025` |
| Cliente | Crear una desde **Registrarse** | — |

**Cadena de conexión** (`appsettings.json` → `ConnectionStrings:DefaultConnection`): por defecto `Server=localhost;Database=restaurante_monster;Trusted_Connection=True;TrustServerCertificate=True;`. Para otra instancia se puede sobrescribir sin tocar el archivo:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=.\SQLEXPRESS;Database=restaurante_monster;Trusted_Connection=True;TrustServerCertificate=True;"
```

| Problema | Solución |
|---|---|
| «No se pudo iniciar sesión…» | SQL Server no está iniciado o la cadena de conexión no corresponde a la instancia |
| Los platos aparecen sin foto | Normal: las imágenes no se versionan; súbelas desde **Admin → Platos/Bebidas** |
| `The certificate chain was issued by an authority that is not trusted` | Mantener `TrustServerCertificate=True` en local |

## Pruebas y validación

```bash
dotnet test RestauranteMonsterCSharp.sln          # 14 pruebas unitarias
```

| Tipo | Qué cubre | Resultado (2026-10-28) |
|---|---|---|
| Unitarias (xUnit) | Formato y sal del hash, verificación, compatibilidad con SHA-256 y migración, valores corruptos; firmas JPEG/PNG/WEBP y rechazo de HTML, GIF, WAVE y vacío | ✅ **14/14** |
| E2E funcional | Login y errores, 9 vistas de administración, CRUD de categorías, platos (con imagen), bebidas, empleados y clientes, stock (fila y total), pedido de mesero, registro y pedido de cliente, carrito, permisos por rol, cierre de sesión | ✅ **29/29** |
| E2E de seguridad | Migración de hashes, hash de usuarios nuevos, GET sin efectos, POST sin token, subida de `.html` y de PNG falso, IDOR de facturas, XSS en el panel | ✅ **9/9** |

**Antes y después** con la misma prueba E2E: el código original obtenía **28/29 funcionales y 0/9 de seguridad**. La comprobación funcional que fallaba era la edición de un cliente con apóstrofo en el nombre, que rompía el formulario.

**Ejecutar la E2E** (con la aplicación en marcha sobre una base **recién creada**):

```bash
npm install --no-save puppeteer-core@24
APP_URL=http://localhost:58620 node tests/e2e/e2e-restaurante.mjs e2e-resultados
```

Variables opcionales: `CHROME_PATH`, `SQLCMD`, `DB_SERVER`, `WWWROOT` (carpeta `wwwroot` de la app en ejecución) y `CAPTURAS=1`.

## Limitaciones y mejoras futuras

- **Imágenes de productos no incluidas:** las fotografías usadas durante el desarrollo no tienen una licencia de redistribución verificable, por lo que no se publican; la aplicación funciona sin ellas y permite subir imágenes propias.
- El IVA está fijado en el 12 % en el código (`DAOFactura`); en Ecuador la tarifa general vigente es otra y debería ser configurable.
- Pendientes de seguridad listados en [Seguridad](#riesgos-conocidos-y-recomendaciones-pendientes).
- Mejoras: transacciones en la creación de pedidos, paginación de facturas, reportes por rango de fechas y migración a autenticación por cookies con políticas de autorización.

## Autoría y créditos

**Desarrollador: Edgar Damian Toscano Coro**
Estudiante de Ingeniería de Software
Universidad de las Fuerzas Armadas ESPE

Proyecto académico individual. En octubre de 2026 se actualizó a .NET 10 y se corrigieron, con asistencia de una herramienta de IA (Claude Code), las vulnerabilidades descritas en [Seguridad](#seguridad); cada corrección está en su propio commit, y el primer commit conserva el código original.

Distribuido bajo la [licencia MIT](LICENSE).
