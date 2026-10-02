# 🏗️ FIRMEZA - Sistema de Gestión y Despacho de Materiales de Construcción

> **Guía Oficial del Proyecto, Manual de Arquitectura & Guía de Despliegue**  
> Diseñado para comprender en profundidad la arquitectura de software, patrones de diseño, diagramas técnicos (ER y Clases), flujo de datos, seguridad y ejecución tanto en entorno local como en contenedores Docker.

---

## 📌 Tabla de Contenidos
1. [Visión General del Proyecto](#-visión-general-del-proyecto)
2. [Arquitectura de la Solución (Clean Architecture & SOLID)](#-arquitectura-de-la-solución-clean-architecture--solid)
3. [Estructura del Proyecto y Capas](#-estructura-del-proyecto-y-capas)
4. [Diagramas Técnicos de Arquitectura y Diseño](#-diagramas-técnicos-de-arquitectura-y-diseño)
   * [Diagrama de Clases de la Solución](#diagrama-de-clases-de-la-solución)
   * [Diagrama Entidad-Relación (ER) Completo](#diagrama-entidad-relación-er-completo)
   * [Diagrama de Secuencia (Flujo de Autenticación y Dashboard)](#diagrama-de-secuencia-flujo-de-autenticación-y-dashboard)
5. [Patrón Repository y Unit of Work (Acceso a Datos Desacoplado)](#-patrón-repository-y-unit-of-work-acceso-a-datos-desacoplado)
6. [Módulo de Gestión de Productos (CRUD, ViewModels y Filtrado)](#-módulo-de-gestión-de-productos-crud-viewmodels-y-filtrado)
7. [Módulo de Gestión de Clientes (CRUD, Validaciones y Búsqueda)](#-módulo-de-gestión-de-clientes-crud-validaciones-y-búsqueda)
8. [Manejo de Errores con Try-Catch y Validaciones de Entrada](#-manejo-de-errores-con-try-catch-y-validaciones-de-entrada)
9. [Seguridad y Control de Acceso (RBAC)](#-seguridad-y-control-de-acceso-rbac)
10. [Diseño Visual, UI/UX y Frontend (Header, Sidebar y Footer)](#-diseño-visual-uiux-y-frontend-header-sidebar-y-footer)
11. [Pruebas Unitarias Automatizadas (xUnit & Moq)](#-pruebas-unitarias-automatizadas-xunit--moq)
12. [Guía de Puesta en Marcha (Instalación y Ejecución Local)](#-guía-de-puesta-en-marcha-instalación-y-ejecución-local)
13. [Despliegue y Ejecución con Docker y Docker Compose](#-despliegue-y-ejecución-con-docker-y-docker-compose)
14. [Cuentas y Datos Semilla por Defecto (Seeders)](#-cuentas-y-datos-semilla-por-defecto-seeders)
15. [Banco de Preguntas Clave para Sustentación y Estudio](#-banco-de-preguntas-clave-para-sustentación-y-estudio)

---

## 🏢 Visión General del Proyecto

**Firmeza** es una plataforma integral desarrollada para empresas del sector de la construcción, distribuidores mayoristas de materiales pesados y ferreterías industriales. Permite administrar:
- **Catálogo de Materiales y Productos:** Control de precios unitarios, unidades de medida (Bolsa, M3, Millar, Unidad, Kg), stock en tiempo real y bajas lógicas para conservar balances históricos.
- **Directorio de Clientes:** Gestión de empresas constructoras, contratistas y clientes particulares con validación estricta de Documento/NIT y protección de integridad referencial.
- **Registro y Despacho de Ventas:** Trazabilidad de órdenes con estados de entrega (`Pendiente`, `En Ruta`, `Entregado`) y congelación de precios históricos de venta.
- **Panel Administrativo (Dashboard):** Métricas operativas en tiempo real (facturación total acumulada, órdenes por estado y alertas de inventario bajo `< 50` unidades).
- **Doble Experiencia de Usuario:** Interfaz web enriquecida con **ASP.NET Core Razor MVC** y cliente interactivo desacoplado en **Angular 22 (SPA)**.

---

## 🏛️ Arquitectura de la Solución (Clean Architecture & SOLID)

El proyecto sigue rigurosamente los principios de **Clean Architecture** (Arquitectura Limpia / Onion Architecture) y **Domain-Driven Design (DDD)** simplificado.

```mermaid
graph TD
    A["Capa de Presentación<br/>Firmeza.Web (MVC / API) + firmeza.client (Angular 22 SPA)"] --> B["Capa de Aplicación<br/>Firmeza.Application (DTOs, Interfaces, Repositorios, Validadores)"]
    A --> C["Capa de Infraestructura<br/>Firmeza.Infrastructure (EF Core, Repositorios, PostgreSQL, Identity)"]
    C --> B
    C --> D["Capa de Dominio<br/>Firmeza.Domain (Entidades, Reglas de Negocio, Constantes)"]
    B --> D
```

### 🧠 Principios y Ventajas de la Arquitectura:
1. **Independencia de Frameworks y Base de Datos:** Las reglas de negocio no conocen PostgreSQL ni ASP.NET; están aisladas en `Domain` y `Application`.
2. **Inversión de Dependencias (DIP):** Las capas internas definen las interfaces (contratos de repositorios y servicios) y las capas externas (`Infrastructure`) las implementan.
3. **Separación de Responsabilidades (SoC):** Cada proyecto resuelve una única preocupación técnica (Dominio, Casos de Uso, Persistencia o UI).
4. **Alta Testabilidad (Mocking):** Los servicios consumen `IUnitOfWork` e `IRepository`, facilitando pruebas unitarias sin tocar la base de datos real.

---

## 📂 Estructura del Proyecto y Capas

```text
Firmeza/
│
├── Dockerfile                   # Dockerfile Multi-stage (Node/Angular + .NET 10 Runtime)
├── docker-compose.yml           # Orquestación de Contenedores (Web + PostgreSQL 16)
├── Firmeza.slnx                 # Archivo de solución .NET
├── README.md                    # Documentación oficial y manual técnico
│
├── src/                         # Backend (.NET 10.0 / C# 13)
│   ├── Firmeza.Domain/          # Núcleo del Negocio (Cero dependencias externas)
│   │   ├── Entities/            # Cliente, Producto, Venta, VentaDetalle
│   │   ├── Constants/           # Roles (Administrador, Cliente)
│   │   └── Shared/              # BaseEntity (Id)
│   │
│   ├── Firmeza.Application/     # Casos de Uso, DTOs y Contratos
│   │   ├── DTOS/                # Clientes, Productos, Dashboard, Auth
│   │   ├── Interfaces/          # IAuthService, IDashboardService, IClienteService, IProductoService
│   │   │   └── Repositories/    # IBaseRepository, IClienteRepository, IProductoRepository, IVentaRepository, IUnitOfWork
│   │   └── Validators/          # ValidadorEdad (Validaciones con try-catch diferenciado)
│   │
│   ├── Firmeza.Infrastructure/  # Acceso a Datos, Repositorios y Servicios Externos
│   │   ├── Persistence/         # ApplicationDbContext, DataSeeder, Configuraciones EF Core
│   │   ├── Repositories/        # BaseRepository, ClienteRepository, ProductoRepository, VentaRepository, UnitOfWork
│   │   ├── Services/            # ClienteService, ProductoService, DashboardService (Usan IUnitOfWork)
│   │   ├── Identity/            # IdentitySeeder, AuthService (ASP.NET Core Identity & RBAC)
│   │   └── DependencyInjection.cs # Registro de dependencias en contenedor IoC
│   │
│   └── Firmeza.Web/             # Capa de Presentación Web (MVC y API REST)
│       ├── Controllers/         # ClientesController, ProductosController, HomeController, AccountController
│       ├── Controllers/Api/     # ClientesApiController, ProductosApiController, VentasApiController, DashboardApiController
│       ├── Views/               # Vistas Razor (Clientes, Productos, Home, Account)
│       ├── Models/              # ViewModels (ClienteIndexViewModel, ProductoIndexViewModel)
│       ├── wwwroot/             # Archivos estáticos, CSS/JS y compilado SPA Angular (/wwwroot/spa)
│       ├── appsettings.json     # Conexión a PostgreSQL e Identity
│       └── Program.cs           # Pipeline HTTP, CORS, Autenticación y Middleware
│
├── tests/                       # Pruebas Automatizadas
│   └── Firmeza.UnitTests/       # Proyecto de Pruebas Unitarias (xUnit & Moq)
│       ├── Domain/              # ProductoEntityTests, VentaDetalleEntityTests
│       ├── Application/         # ValidadorEdadTests
│       └── Services/            # ClienteServiceTests, ProductoServiceTests
│
└── firmeza.client/              # Frontend Desacoplado (Angular 22 SPA)
    ├── src/
    │   ├── app/
    │   │   ├── components/      # SidebarComponent, HeaderComponent, FooterComponent
    │   │   ├── pages/           # Dashboard, Productos, Clientes, Ventas
    │   │   ├── services/        # DashboardService (Cliente HTTP hacia APIs REST)
    │   │   └── app.routes.ts    # Enrutamiento de la SPA
    │   └── main.ts              # Bootstrap de Angular
    └── package.json             # Dependencias del cliente web
```

---

## 📊 Diagramas Técnicos de Arquitectura y Diseño

### Diagrama de Clases de la Solución

```mermaid
classDiagram
    class BaseEntity {
        <<abstract>>
        +Guid Id
    }

    class Cliente {
        +string DocumentoIdentidad
        +string RazonSocial
        +string Telefono
        +string Email
        +string DireccionEnvio
        +ICollection~Venta~ Ventas
    }

    class Producto {
        +string Nombre
        +string Descripcion
        +string UnidadMedida
        +decimal PrecioUnitario
        +int StockActual
        +bool Activo
        +ICollection~VentaDetalle~ DetallesVenta
    }

    class Venta {
        +Guid ClienteId
        +Cliente Cliente
        +DateTime FechaVenta
        +decimal Total
        +string EstadoDespacho
        +ICollection~VentaDetalle~ Detalles
    }

    class VentaDetalle {
        +Guid VentaId
        +Venta Venta
        +Guid ProductoId
        +Producto Producto
        +int Cantidad
        +decimal PrecioAplicado
        +decimal Subtotal
    }

    BaseEntity <|-- Cliente
    BaseEntity <|-- Producto
    BaseEntity <|-- Venta
    BaseEntity <|-- VentaDetalle

    class IUnitOfWork {
        <<interface>>
        +IClienteRepository Clientes
        +IProductoRepository Productos
        +IVentaRepository Ventas
        +Task~int~ SaveChangesAsync()
    }

    class IClienteRepository {
        <<interface>>
        +GetAllWithVentasAsync()
        +GetByIdWithVentasAsync()
        +DocumentoExistsAsync()
    }

    class IProductoRepository {
        <<interface>>
        +GetAllWithDetallesAsync()
        +GetByIdWithDetallesAsync()
        +GetUnidadesMedidaAsync()
    }

    class IVentaRepository {
        <<interface>>
        +GetAllWithDetailsAsync()
        +GetByIdWithDetailsAsync()
        +GetUltimasVentasAsync()
    }

    IUnitOfWork o-- IClienteRepository
    IUnitOfWork o-- IProductoRepository
    IUnitOfWork o-- IVentaRepository

    class IClienteService {
        <<interface>>
        +GetAllAsync()
        +GetByIdAsync()
        +CreateAsync()
        +UpdateAsync()
        +DeleteAsync()
    }

    class IProductoService {
        <<interface>>
        +GetAllAsync()
        +GetByIdAsync()
        +CreateAsync()
        +UpdateAsync()
        +DeleteAsync()
    }

    class ClienteService {
        -IUnitOfWork _unitOfWork
    }

    class ProductoService {
        -IUnitOfWork _unitOfWork
    }

    IClienteService <|.. ClienteService
    IProductoService <|.. ProductoService
    ClienteService --> IUnitOfWork
    ProductoService --> IUnitOfWork

    class ClientesController {
        -IClienteService _clienteService
        +Index()
        +Create()
        +Edit()
        +Delete()
    }

    class ProductosController {
        -IProductoService _productoService
        +Index()
        +Create()
        +Edit()
        +Delete()
    }

    ClientesController --> IClienteService
    ProductosController --> IProductoService
```

---

### Diagrama Entidad-Relación (ER) Completo

Incluye las tablas del dominio comercial y las tablas de seguridad de **ASP.NET Core Identity**:

```mermaid
erDiagram
    CLIENTES ||--o{ VENTAS : realiza
    VENTAS ||--|{ VENTA_DETALLES : contiene
    PRODUCTOS ||--o{ VENTA_DETALLES : referencia
    ASPNET_USERS ||--o{ ASPNET_USER_ROLES : asignado
    ASPNET_ROLES ||--o{ ASPNET_USER_ROLES : contiene

    CLIENTES {
        uuid Id PK
        string DocumentoIdentidad "UNIQUE"
        string RazonSocial
        string Telefono
        string Email
        string DireccionEnvio
    }

    PRODUCTOS {
        uuid Id PK
        string Nombre
        string Descripcion
        string UnidadMedida
        decimal PrecioUnitario
        int StockActual
        boolean Activo
    }

    VENTAS {
        uuid Id PK
        uuid ClienteId FK
        timestamp FechaVenta
        decimal Total
        string EstadoDespacho "Pendiente | En Ruta | Entregado"
    }

    VENTA_DETALLES {
        uuid Id PK
        uuid VentaId FK
        uuid ProductoId FK
        int Cantidad
        decimal PrecioAplicado
        decimal Subtotal
    }

    ASPNET_USERS {
        string Id PK
        string UserName
        string Email
        string PasswordHash
    }

    ASPNET_ROLES {
        string Id PK
        string Name "Administrador | Cliente"
    }

    ASPNET_USER_ROLES {
        string UserId PK,FK
        string RoleId PK,FK
    }
```

---

### Diagrama de Secuencia (Flujo de Autenticación y Dashboard)

```mermaid
sequenceDiagram
    autonumber
    actor Usuario as Administrador / Navegador
    participant Web as Firmeza.Web (MVC / API)
    participant Service as AuthService / DashboardService
    participant Repo as UnitOfWork & Repositories
    participant DB as PostgreSQL (ApplicationDbContext)

    Usuario->>Web: POST /Account/Login (Email, Password)
    Web->>Service: LoginAdminAsync(LoginDto)
    Service->>DB: Consultar usuario y roles (AspNetUsers)
    alt Es Rol Cliente
        Service-->>Web: Retorna IsClientBlockedFromAdmin = true
        Web-->>Usuario: Muestra mensaje: "Acceso denegado a panel administrativo"
    else Es Rol Administrador y Credenciales Válidas
        Service->>Web: Emite Cookie de Autenticación Segura
        Web-->>Usuario: Redirige a /Home/Dashboard
        Usuario->>Web: GET /api/dashboard/metrics
        Web->>Service: GetDashboardMetricsAsync()
        Service->>Repo: Consultar totales y alertas de stock
        Repo->>DB: Consultas LINQ optimizadas (AsNoTracking)
        DB-->>Repo: Entidades y Agregaciones
        Repo-->>Service: DashboardMetricsDto
        Service-->>Web: Métricas compiladas
        Web-->>Usuario: JSON con métricas en tiempo real
    end
```

---

## 🗄️ Patrón Repository y Unit of Work (Acceso a Datos Desacoplado)

### 📌 ¿Qué hace la carpeta `Repositories`?
La carpeta **`Repositories`** (ubicada en `Firmeza.Infrastructure/Repositories`) implementa el **Patrón Repositorio**, el cual actúa como mediador entre la persistencia (`ApplicationDbContext` / EF Core) y los servicios de aplicación (`Services`).

### 🎯 Beneficios Arquitectónicos:
1. **Desacoplamiento Absoluto:** Los servicios (`ClienteService`, `ProductoService`) y controladores no tienen referencias a LINQ ni al `DbContext` directamente.
2. **Inversión de Dependencias (DIP):** Los contratos se declaran en `Firmeza.Application/Interfaces/Repositories/` y las implementaciones concretas en `Firmeza.Infrastructure/Repositories/`.
3. **Unidad de Trabajo (Unit of Work):** Coordina múltiples repositorios asegurando que todas las operaciones de inserción, actualización o eliminación se ejecuten en una única transacción atómica con `SaveChangesAsync()`.

| Contrato (`Application`) | Implementación (`Infrastructure`) | Responsabilidad |
| :--- | :--- | :--- |
| `IBaseRepository<T>` | `BaseRepository<T>` | Operaciones CRUD genéricas (`GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `ExistsAsync`). |
| `IClienteRepository` | `ClienteRepository` | Consultas con historial de ventas, validación de unicidad de NIT y filtros avanzados. |
| `IProductoRepository` | `ProductoRepository` | Catálogo de materiales, unidades de medida y alertas de inventario. |
| `IVentaRepository` | `VentaRepository` | Órdenes históricas con desglose de productos y clientes asociados. |
| `IUnitOfWork` | `UnitOfWork` | Agrupa todos los repositorios y controla el guardado atómico transaccional. |

---

## 📦 Módulo de Gestión de Productos (CRUD, ViewModels y Filtrado)

* **Crear (`Create`):** Registro de nuevos materiales con validación de precios positivos, unidades de medida y stock inicial.
* **Consultar (`Index` y `Details`):** Vista de catálogo con filtros por unidad de medida, estado activo/inactivo, alerta de stock bajo (`< 50`) y ordenamiento multi-criterio.
* **Actualizar (`Edit`):** Modificación de especificaciones y precios de catálogo.
* **Eliminar / Desactivar (`Delete`):** Lógica de **integridad histórica**. Si el producto ya cuenta con ventas asociadas, se realiza un **Soft Delete** (`Activo = false`) para proteger los reportes contables del pasado; si no tiene ventas, se elimina físicamente.

---

## 👥 Módulo de Gestión de Clientes (CRUD, Validaciones y Búsqueda)

* **Crear (`Create`):** Registro de constructoras y clientes con validación estricta de formato y unicidad de Documento/NIT.
* **Consultar (`Index` y `Details`):** Listado con canales de contacto, dirección de despacho y acumulado histórico de facturación.
* **Actualizar (`Edit`):** Edición de información de contacto validando que no se duplique el NIT con otro cliente.
* **Eliminar (`Delete`):** Protección referencial. Si el cliente tiene ventas registradas (`totalCompras > 0`), el sistema **bloquea la eliminación** para salvaguardar la trazabilidad fiscal.

---

## 🛡️ Manejo de Errores con Try-Catch y Validaciones de Entrada

La capa de aplicación implementa manejo defensivo de excepciones mediante [`ValidadorEdad`](file:///home/cohorte-5/Escritorio/Firmeza/src/Firmeza.Application/Validators/ValidadorEdad.cs), aplicando bloques `try-catch` con captura de excepciones tipadas:
1. **`FormatException`:** Captura texto no numérico ingresado en campos enteros.
2. **`OverflowException`:** Captura valores fuera del rango permitido de un entero de 32 bits.
3. **`Exception` General:** Captura errores inesperados, retornando siempre mensajes claros y amigables al usuario que se reflejan en el `ModelState` y en las vistas Razor.

---

## 🔐 Seguridad y Control de Acceso (RBAC)

* **Rol `Administrador`:** Acceso exclusivo al panel administrativo Razor MVC (`/Home/Dashboard`, `/Productos`, `/Clientes`).
* **Rol `Cliente`:** Diseñado para compras desde la aplicación cliente. Si un usuario con rol `Cliente` intenta ingresar al panel administrativo, [`AuthService`](file:///home/cohorte-5/Escritorio/Firmeza/src/Firmeza.Infrastructure/Identity/AuthService.cs) activa la bandera `IsClientBlockedFromAdmin = true` y bloquea el acceso de inmediato.

---

## 🎨 Diseño Visual, UI/UX y Frontend (Header, Sidebar y Footer)

Tanto en **ASP.NET Core Razor MVC** como en **Angular SPA**, la experiencia de usuario mantiene un diseño profesional y coherente:
1. **Encabezado Superior (Header):** Identidad del sistema, estado en tiempo real del servidor (`Servidor / API Conectada`), avatar del usuario y menú desplegable de perfil.
2. **Navegación Lateral (Sidebar):** Menú lateral estilizado con fondo oscuro (`#0f172a`), íconos vectoriales modernos, enlaces activos en azul corporativo (`#2563eb`) y toggle responsivo para dispositivos móviles.
3. **Pie de Página (Footer):** Barra de cierre con información legal, políticas y versión del sistema.
4. **Paleta de Colores y Tipografía:**
   * Primario: `#2563eb` (Royal Blue)
   * Superficies Oscuras: `#0f172a` y `#1e293b`
   * Fondo de Contenido: `#f8fafc` (Slate 50)
   * Tipografía: `system-ui`, `-apple-system`, `Roboto`, `Helvetica Neue`.

---

## 🧪 Pruebas Unitarias Automatizadas (xUnit & Moq)

El proyecto cuenta con una suite de pruebas unitarias automatizadas en [`tests/Firmeza.UnitTests/`](file:///home/cohorte-5/Escritorio/Firmeza/tests/Firmeza.UnitTests) utilizando **xUnit** (framework de pruebas oficial y líder en el ecosistema .NET) y **Moq** (librería de aislamiento y dobles de prueba/mocking).

### 🎯 ¿Qué es xUnit y por qué se utiliza?
* **xUnit.net** es un framework de pruebas moderno, extensible y orientado a la programación orientada a objetos para C# y .NET.
* Permite validar de forma rápida y reproducible que las reglas de negocio y los componentes del sistema funcionen con exactitud sin necesidad de levantar un servidor web ni conectar una base de datos real.

### 📐 Estructura de las Pruebas: Patrón AAA (Arrange - Act - Assert)
Cada método de prueba sigue el estándar internacional **AAA**:
1. **Arrange (Organizar / Preparar):** Inicializa variables, entidades y configura los mocks de dependencias.
2. **Act (Actuar / Ejecutar):** Invoca el método, validador o servicio bajo prueba.
3. **Assert (Afirmar / Verificar):** Comprueba mediante aserciones (`Assert.Equal`, `Assert.True`, `Assert.ThrowsAsync`, etc.) que el resultado coincida exactamente con lo esperado.

### 🧩 Tipos de Pruebas en xUnit:
* **`[Fact]`:** Pruebas con condiciones e invariantes fijas que siempre deben cumplirse.
* **`[Theory]`:** Pruebas parametrizadas ejecutadas con múltiples conjuntos de datos (`[InlineData(...)]`) para verificar casos límite, datos válidos y datos erróneos en una sola prueba.

### 📦 Batería de Pruebas Implementadas:

| Proyecto / Capa | Archivo de Prueba | Escenarios Validados |
| :--- | :--- | :--- |
| **Dominio (`Domain`)** | [`ProductoEntityTests.cs`](file:///home/cohorte-5/Escritorio/Firmeza/tests/Firmeza.UnitTests/Domain/ProductoEntityTests.cs) | Inicialización correcta de propiedades de producto y evaluación del umbral de alerta de bajo stock (`Stock < 50`). |
| **Dominio (`Domain`)** | [`VentaDetalleEntityTests.cs`](file:///home/cohorte-5/Escritorio/Firmeza/tests/Firmeza.UnitTests/Domain/VentaDetalleEntityTests.cs) | Cálculo de subtotal histórico multiplicando `Cantidad * PrecioAplicado` congelado al momento de la venta. |
| **Aplicación (`Application`)** | [`ValidadorEdadTests.cs`](file:///home/cohorte-5/Escritorio/Firmeza/tests/Firmeza.UnitTests/Application/ValidadorEdadTests.cs) | Validación con `try-catch`, captura de `FormatException` (texto alfabético), `OverflowException` (números que exceden Int32) y rangos laborales. |
| **Servicios (`Infrastructure`)** | [`ClienteServiceTests.cs`](file:///home/cohorte-5/Escritorio/Firmeza/tests/Firmeza.UnitTests/Services/ClienteServiceTests.cs) | Aislamiento con `Mock<IUnitOfWork>`. Verifica que `DeleteAsync` arroje `InvalidOperationException` si el cliente tiene compras, y borre limpiamente si no las tiene. |
| **Servicios (`Infrastructure`)** | [`ProductoServiceTests.cs`](file:///home/cohorte-5/Escritorio/Firmeza/tests/Firmeza.UnitTests/Services/ProductoServiceTests.cs) | Aislamiento con `Mock<IUnitOfWork>`. Valida que al eliminar un producto con ventas asociadas se aplique **Soft Delete** (`Activo = false`), y que `CreateAsync` registre y confirme cambios. |

### ⚡ Comando para Ejecutar las Pruebas:
```bash
dotnet test
```
> Ejecuta todas las pruebas unitarias y genera el reporte de éxito en la terminal.

---

## 🚀 Guía de Puesta en Marcha (Instalación y Ejecución Local)

### Requisitos Previos:
- **.NET SDK 10.0** (o versión superior compatible).
- **Node.js (v18+)** y **npm**.
- **PostgreSQL (v14+)** en ejecución.

### 1. Configurar la Conexión a Base de Datos
Edita `src/Firmeza.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=firmeza_db;Username=postgres;Password=tu_password"
}
```

### 2. Ejecutar el Backend (.NET Web API & MVC)
```bash
cd src/Firmeza.Web
dotnet run
```
> **Nota:** Al iniciar, [`Program.cs`](file:///home/cohorte-5/Escritorio/Firmeza/src/Firmeza.Web/Program.cs) ejecuta automáticamente `dbContext.Database.MigrateAsync()` y si la base de datos está vacía, sembrará los roles, usuarios y el catálogo de prueba.

### 3. Ejecutar el Frontend Angular en Modo Desarrollo (Opcional)
```bash
cd firmeza.client
npm install
npm start
```
El cliente estará disponible en `http://localhost:4200/`.

---

## 🐳 Despliegue y Ejecución con Docker y Docker Compose

El proyecto incluye soporte nativo para contenedores mediante un **`Dockerfile` multi-stage** y un archivo **`docker-compose.yml`** que orquesta el backend web y la base de datos PostgreSQL en un entorno aislado.

### 1. Estructura de Contenedores
* **`firmeza-postgres-db`:** Servidor PostgreSQL 16 Alpine con volumen persistente (`postgres_data`) y healthcheck automático.
* **`firmeza-web-app`:** Contenedor .NET 10 en producción con la SPA de Angular precompilada y servida internamente en el puerto `8080` (mapeado a `5000` en el host).

### 2. Comandos de Despliegue con Docker Compose

```bash
# 1. Construir imágenes y levantar servicios en segundo plano
docker-compose up --build -d

# 2. Verificar el estado de los contenedores
docker-compose ps

# 3. Ver los logs en tiempo real del backend y la base de datos
docker-compose logs -f firmeza-web

# 4. Detener los servicios
docker-compose down
```

Una vez levantado, accede desde tu navegador a:
* **Panel Web Administrativo:** `http://localhost:5000`
* **Cliente SPA Angular:** `http://localhost:5000/spa/index.html`

---

## 👥 Cuentas y Datos Semilla por Defecto (Seeders)

### Usuarios de Prueba Preconfigurados:
| Rol | Correo Electrónico | Contraseña | Comportamiento en `/Account/Login` |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin@firmeza.com` | `Admin123*` | Inicia sesión exitosamente y accede al Dashboard |
| **Cliente** | `cliente@firmeza.com` | `Cliente123*` | Se rechaza con advertencia de bloqueo administrativo |

### Materiales Sembrados en Base de Datos:
* **Cemento Gris Tipo 1 x 50kg** (Bolsa - $32,000 COP)
* **Varilla Corrugada 1/2"** (Unidad - $45,000 COP)
* **Ladrillo Estructurado Arcilla 10x20x40** (Millar - $1,200,000 COP)
* **Arena Lavada de Río (M3)** (M3 - $85,000 COP)

---

## 🎓 Banco de Preguntas Clave para Sustentación y Estudio

1. **¿Qué patrón arquitectónico se utilizó y por qué?**
   * *Respuesta:* Clean Architecture en 4 capas (Domain, Application, Infrastructure, Presentation). Garantiza que el núcleo del negocio sea independiente de frameworks, bases de datos e interfaces de usuario.
2. **¿Qué rol cumplen la carpeta `Repositories` y `Unit of Work`?**
   * *Respuesta:* Desacoplan los servicios de Entity Framework Core. Las interfaces están en `Application` (Inversión de Dependencias) y las implementaciones en `Infrastructure/Repositories`. `Unit of Work` agrupa las operaciones asegurando confirmación atómica con `SaveChangesAsync()`.
3. **¿Cómo se protegen los registros históricos al eliminar productos o clientes?**
   * *Respuesta:* Para productos con ventas previas se aplica **Soft Delete** (`Activo = false`). Para clientes con facturas registradas, el sistema restringe el borrado físico para preservar la trazabilidad fiscal.
4. **¿Por qué `VentaDetalle` almacena `PrecioAplicado`?**
   * *Respuesta:* Para congelar el precio unitario del momento de la compra; si el precio de catálogo cambia a futuro, los balances históricos no se alteran.
5. **¿Cómo maneja el sistema la segregación de roles (RBAC)?**
   * *Respuesta:* Mediante ASP.NET Core Identity y el filtro `[Authorize(Roles = Roles.Administrador)]`. `AuthService` bloquea explícitamente el acceso de cuentas con rol `Cliente` al panel administrativo.
6. **¿Cómo se realiza el despliegue en Docker?**
   * *Respuesta:* Con un `Dockerfile` multi-stage que compila la SPA de Angular (Node.js) y la inyecta en el `wwwroot` de la aplicación .NET 10. `docker-compose.yml` orquesta la base de datos PostgreSQL con healthcheck y conecta el contenedor web en una red privada.
7. **¿Cómo se aplican las pruebas unitarias con xUnit y Moq en el proyecto?**
   * *Respuesta:* Mediante el proyecto `tests/Firmeza.UnitTests`, aplicando el patrón AAA (Arrange, Act, Assert). Se prueban entidades del dominio (cálculo de subtotales, reglas de stock), validadores de aplicación (`ValidadorEdad` con captura de excepciones) y servicios de infraestructura (`ClienteService`, `ProductoService`) aislando la base de datos con `Mock<IUnitOfWork>`.

---

*Desarrollado con arquitectura de software limpia, código tipado y documentación de nivel profesional.*
