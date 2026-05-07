# Vertical Slice Architecture — .NET Backend Rules

> Documento de referencia para proyectos .NET que sigan esta arquitectura.
> Creado como base reutilizable para nuevos proyectos.

---

## 1. Principios Fundamentales

| # | Principio | Descripción |
|---|-----------|-------------|
| 1 | **Feature = Directorio** | Cada caso de uso vive en su propio directorio con handler, DTOs, validación |
| 2 | **Entry Point único** | Cada feature expone un solo método público `Map(IEndpointRouteBuilder)` |
| 3 | **Mínimo acoplamiento entre slices** | Máximo acoplamiento dentro de un slice |
| 4 | **Sin abstracciones prematuras** | No crear repositorios/servicios genéricos hasta que haya duplicación real en 3+ slices |
| 5 | **CQRS nativo** | Commands (escrituras) separados de Queries (lecturas) por naturaleza del slice |

---

## 2. Stack Tecnológico

| Componente | Tecnología | Uso |
|------------|------------|-----|
| **Framework** | .NET 10+ / Minimal APIs | Endpoints HTTP |
| **ORM (Escritura)** | Entity Framework Core | INSERT, UPDATE, DELETE |
| **Micro-ORM (Lectura)** | Dapper | SELECT con SQL raw |
| **Validación** | FluentValidation | Validación de request bodies |
| **Identidad** | ASP.NET Core Identity + EF Core | Autenticación y autorización |
| **Base de datos** | PostgreSQL | Persistencia |
| **Bus de mensajes** | Ninguno (sin MediatR) | Handlers resueltos directamente por DI |

---

## 3. Estructura del Proyecto

```
{ProjectName}/
│
├── Domain/
│   └── Entities/                         # Entidades del dominio (POCOs)
│       ├── Doctor.cs
│       └── MedicalCenter.cs
│
├── Features/                             # ⭐ Corazón de la arquitectura
│   ├── {DomainPlural}/                   # Agrupado por dominio (Doctors, MedicalCenters)
│   │   ├── Create{Domain}/               # Un directorio por operación
│   │   │   ├── Create{Domain}Endpoint.cs # Minimal API endpoint (entry point)
│   │   │   ├── Create{Domain}Command.cs  # Command record + CommandHandler
│   │   │   └── Create{Domain}Validator.cs# FluentValidation rules
│   │   ├── Get{Domain}/
│   │   │   ├── Get{Domain}Endpoint.cs
│   │   │   └── Get{Domain}Query.cs       # Response record + QueryHandler (Dapper)
│   │   ├── List{DomainPlural}/
│   │   │   ├── List{Domain}Endpoint.cs
│   │   │   └── List{Domain}Query.cs
│   │   ├── Update{Domain}/
│   │   │   ├── Update{Domain}Endpoint.cs
│   │   │   ├── Update{Domain}Command.cs
│   │   │   └── Update{Domain}Validator.cs
│   │   └── Delete{Domain}/
│   │       ├── Delete{Domain}Endpoint.cs
│   │       └── Delete{Domain}Command.cs
│   └── ...
│
├── Infrastructure/                       # Cross-cutting concerns
│   ├── Persistence/
│   │   ├── AppDbContext.cs               # EF Core (IdentityDbContext)
│   │   ├── DapperConnectionFactory.cs    # Factory para IDbConnection
│   │   └── Migrations/                   # Migraciones generadas por EF Core
│   ├── Middleware/
│   │   └── ValidationFilter.cs           # Endpoint filter genérico FluentValidation
│   └── Extensions/
│       ├── ServiceCollectionExtensions.cs # Registro de infraestructura en DI
│       └── EndpointExtensions.cs         # Registro de endpoints + handlers
│
├── Program.cs                            # Composition Root
├── appsettings.json
└── {ProjectName}.csproj
```

---

## 4. Patrones por Tipo de Operación

### 4.1 Command (Escritura) — EF Core

Usado para: `POST`, `PUT`, `DELETE`

```csharp
// ── {Operation}{Domain}Command.cs ───────────────────────────────

// Request DTO
public record Create{Domain}Command(string Name, string? OptionalField);

// Response DTO
public record Create{Domain}Response(Guid Id, string Name);

// Handler — usa EF Core para persistir
public class Create{Domain}CommandHandler
{
    private readonly AppDbContext _db;

    public Create{Domain}CommandHandler(AppDbContext db) => _db = db;

    public async Task<Create{Domain}Response> HandleAsync(
        Create{Domain}Command command, CancellationToken ct)
    {
        var entity = new {Domain}
        {
            Id = Guid.CreateVersion7(),
            Name = command.Name
        };

        _db.{DomainPlural}.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new Create{Domain}Response(entity.Id, entity.Name);
    }
}
```

### 4.2 Query (Lectura) — Dapper

Usado para: `GET` (individual y lista)

```csharp
// ── Get{Domain}Query.cs ─────────────────────────────────────────

// Response DTO (Dapper mapea directamente a este record)
public record Get{Domain}Response(Guid Id, string Name, string? OptionalField);

// Handler — usa Dapper con SQL raw
public class Get{Domain}QueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public Get{Domain}QueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<Get{Domain}Response?> HandleAsync(Guid id, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT id AS Id, name AS Name, optional_field AS OptionalField
            FROM {table_name}
            WHERE id = @Id
            """;

        return await connection.QueryFirstOrDefaultAsync<Get{Domain}Response>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }
}
```

### 4.3 Endpoint — Minimal API (Entry Point)

```csharp
// ── Create{Domain}Endpoint.cs ───────────────────────────────────

public static class Create{Domain}Endpoint
{
    // ⭐ ÚNICO método público — entry point del slice
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/{route}", Handle)
           .AddEndpointFilter<ValidationFilter<Create{Domain}Command>>()
           .WithTags("{DomainPlural}")
           .WithName("Create{Domain}")
           .Produces<Create{Domain}Response>(StatusCodes.Status201Created)
           .ProducesValidationProblem();

    // Método privado — resuelve handler desde DI
    private static async Task<IResult> Handle(
        Create{Domain}Command command,
        Create{Domain}CommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/{route}/{response.Id}", response);
    }
}
```

### 4.4 Validator — FluentValidation

```csharp
// ── Create{Domain}Validator.cs ──────────────────────────────────

public class Create{Domain}Validator : AbstractValidator<Create{Domain}Command>
{
    public Create{Domain}Validator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");
    }
}
```

---

## 5. Reglas del Composition Root (Program.cs)

```csharp
using {ProjectName}.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Infraestructura (EF Core, Identity, Dapper, FluentValidation, JWT)
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Feature handlers (Command/Query handlers como Scoped)
builder.Services.AddFeatureHandlers();

// 3. OpenAPI
builder.Services.AddOpenApi();

// 4. CORS
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseHttpsRedirection();

// ⚠️ Orden obligatorio: Authentication → Authorization → Endpoints
app.UseAuthentication();
app.UseAuthorization();

// 5. Registrar todos los endpoints
app.MapFeatureEndpoints();

app.Run();
```

---

## 6. Reglas de Infrastructure

### 6.1 AppDbContext (EF Core)

- Hereda de `IdentityDbContext` para Identity
- Registra `DbSet<T>` por cada entidad del dominio
- Mapea tablas y columnas a **snake_case** en `OnModelCreating`
- Se usa **SOLO** para Commands (INSERT, UPDATE, DELETE)

### 6.2 DapperConnectionFactory

- Registrado como **Singleton** en DI
- Almacena el connection string, NO una conexión abierta
- Crea `NpgsqlConnection` bajo demanda
- Se usa **SOLO** para Queries (SELECT)

### 6.3 ValidationFilter\<T\>

- `IEndpointFilter` genérico que resuelve `IValidator<T>` desde DI
- Si no existe validator registrado, deja pasar la request
- Si la validación falla, retorna `Results.ValidationProblem()` (RFC 7807)
- Se aplica por endpoint: `.AddEndpointFilter<ValidationFilter<T>>()`

### 6.4 ServiceCollectionExtensions

Método `AddInfrastructure()` registra:
- `AppDbContext` con provider PostgreSQL (Npgsql)
- Identity con `IdentityUser`
- `DapperConnectionFactory` como Singleton
- FluentValidation validators via assembly scan

### 6.5 EndpointExtensions

- `MapFeatureEndpoints()` — invoca `Map()` de cada feature endpoint
- `AddFeatureHandlers()` — registra todos los Command/Query handlers como Scoped

### 6.6 JwtTokenService

- Registrado como **Singleton** en DI
- Lee `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationMinutes` de `appsettings.json`
- Genera tokens JWT firmados con `HmacSha256`
- Solo se inyecta en los handlers de Auth (`RegisterCommandHandler`, `LoginCommandHandler`)

---

## 7. Autorización JWT — Reglas

### 7.1 Configuración en appsettings.json

```json
{
  "Jwt": {
    "Key": "MinimoDe32CaracteresParaHmacSha256!",
    "Issuer": "{ProjectName}",
    "Audience": "{ProjectName}Clients",
    "ExpirationMinutes": "60"
  }
}
```

> ⚠️ **Nunca** versionar la `Key` en el repositorio. Usar **User Secrets** en desarrollo y variables de entorno en producción.

### 7.2 Middleware — Orden obligatorio en Program.cs

```csharp
app.UseCors();
app.UseHttpsRedirection();

// ⚠️ Orden obligatorio: Authentication → Authorization → Endpoints
app.UseAuthentication();   // Valida el JWT y establece ClaimsPrincipal
app.UseAuthorization();    // Evalúa las políticas de acceso

app.MapFeatureEndpoints();
```

### 7.3 Clasificación de Endpoints

| Tipo | Decorador | Cuándo usar |
|------|-----------|-------------|
| **Público** | `.AllowAnonymous()` | Login, Register, health checks |
| **Autenticado** | `.RequireAuthorization()` | Cualquier endpoint que requiera usuario logueado |
| **Por rol** | `.RequireAuthorization("Admin")` | Endpoints solo para roles específicos |

### 7.4 Endpoint Público (Auth)

```csharp
public static void Map(IEndpointRouteBuilder app) =>
    app.MapPost("/api/auth/login", Handle)
       .AddEndpointFilter<ValidationFilter<LoginCommand>>()
       .WithTags("Auth")
       .WithName("Login")
       .Produces<LoginResponse>()
       .Produces(StatusCodes.Status401Unauthorized)
       .AllowAnonymous();   // ← Sin token requerido
```

### 7.5 Endpoint Protegido (Requiere JWT)

```csharp
public static void Map(IEndpointRouteBuilder app) =>
    app.MapPost("/api/doctors", Handle)
       .AddEndpointFilter<ValidationFilter<CreateDoctorCommand>>()
       .WithTags("Doctors")
       .WithName("CreateDoctor")
       .Produces<CreateDoctorResponse>(StatusCodes.Status201Created)
       .ProducesValidationProblem()
       .Produces(StatusCodes.Status401Unauthorized)
       .RequireAuthorization();   // ← JWT obligatorio
```

### 7.6 Cómo consumir la API con JWT

**Paso 1 — Obtener token:**
```http
POST /api/auth/login
Content-Type: application/json

{ "email": "user@example.com", "password": "MyPassword123!" }
```

**Paso 2 — Usar el token en cada request:**
```http
GET /api/doctors
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 7.7 Reglas de Autorización en VSA

1. **Los slices de Auth son SIEMPRE públicos** — `Register` y `Login` llevan `.AllowAnonymous()`
2. **Los demás slices son SIEMPRE protegidos** — todos llevan `.RequireAuthorization()`
3. **`JwtTokenService` no va en slices de negocio** — solo en slices de Auth
4. **Los claims del usuario** se obtienen dentro del handler desde `HttpContext.User` si se necesitan
5. **No crear filtros de autorización personalizados** salvo que haya lógica de roles/permisos compleja

```csharp
// Acceder al usuario autenticado dentro de un handler
private static async Task<IResult> Handle(
    CreateDoctorCommand command,
    CreateDoctorCommandHandler handler,
    ClaimsPrincipal user,          // ← Inyectado automáticamente por Minimal APIs
    CancellationToken ct)
{
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // ...
}
```

---

## 8. Convenciones de Nomenclatura

| Elemento | Convención | Ejemplo |
|----------|-----------|---------|
| Directorio feature | `{Operación}{Dominio}/` | `CreateDoctor/` |
| Endpoint class | `{Operación}{Dominio}Endpoint` | `CreateDoctorEndpoint` |
| Command record | `{Operación}{Dominio}Command` | `CreateDoctorCommand` |
| Query handler | `{Operación}{Dominio}QueryHandler` | `GetDoctorQueryHandler` |
| Command handler | `{Operación}{Dominio}CommandHandler` | `CreateDoctorCommandHandler` |
| Response record | `{Operación}{Dominio}Response` | `CreateDoctorResponse` |
| Validator class | `{Operación}{Dominio}Validator` | `CreateDoctorValidator` |
| Entry point method | `Map` (estático) | `CreateDoctorEndpoint.Map(app)` |
| Tablas PostgreSQL | `snake_case` plural | `doctors`, `medical_centers` |
| Columnas PostgreSQL | `snake_case` | `is_vet`, `birth_date` |
| Rutas API | `kebab-case` plural | `/api/medical-centers` |

---

## 9. Reglas Estrictas

### ✅ SÍ hacer

1. **Un directorio = un caso de uso** — nunca mezclar operaciones
2. **Un `Map()` público por feature** — todo lo demás es privado
3. **EF Core solo para escrituras** — Commands (INSERT, UPDATE, DELETE)
4. **Dapper solo para lecturas** — Queries (SELECT con SQL raw)
5. **FluentValidation por Command** — cada command que reciba input tiene su validator
6. **Records para DTOs** — inmutables, concisos
7. **`Guid.CreateVersion7()`** — para generar IDs ordenados cronológicamente
8. **`CancellationToken`** — propagarlo siempre hasta la capa de datos
9. **Endpoint metadata** — `.WithTags()`, `.WithName()`, `.Produces<T>()` para OpenAPI

### ❌ NO hacer

1. **No usar MediatR** — los handlers se resuelven directamente por DI
2. **No crear repositorios genéricos** — hasta que haya duplicación real en 3+ slices
3. **No importar entre slices** — si necesitas compartir, extrae a `Domain/` o `Infrastructure/`
4. **No usar Controllers** — solo Minimal APIs
5. **No crear capas de servicio** — el handler ES la lógica del caso de uso
6. **No usar EF Core para SELECT** — usar Dapper con SQL raw
7. **No poner lógica de negocio en el Endpoint** — delegarla al handler
8. **No usar body en `MapDelete` ni `MapGet`** — Minimal APIs no permite leer el request body en estos métodos. Usar query string con `[AsParameters]`:

```csharp
// ❌ Falla en runtime: "Body was inferred but the method does not allow inferred body parameters"
app.MapDelete("/api/users/{userId}/claims", Handle);
private static Task<IResult> Handle(string userId, MyCommand command, ...) // command = body → ERROR

// ✅ Correcto: parámetros por query string
app.MapDelete("/api/users/{userId}/claims", Handle);
private static Task<IResult> Handle(
    string userId,
    [AsParameters] MyParams p,   // ?claimType=X&claimValue=Y
    MyHandler handler, ...) => ...

// Regla rápida por método HTTP:
// POST / PUT / PATCH → pueden leer body
// GET / DELETE       → solo route params y query string
```

9. **Usar `long` (no `int`) para resultados de agregados en Dapper** — PostgreSQL devuelve `COUNT()`, `SUM()` y `AVG()` como `bigint` (`Int64`). Dapper **no hace conversión implícita** al mapear al constructor de un `record`, y lanzará `InvalidOperationException` en runtime si el tipo no coincide exactamente:

```csharp
// ❌ Falla en runtime: PostgreSQL retorna bigint, el record espera int
public record SummaryResponse(int Total, int Active, int Inactive);

// ✅ Correcto: usar long para COUNT(*), SUM() y similares
public record SummaryResponse(long Total, long Active, long Inactive);

// Tabla de referencia — tipos PostgreSQL → C# en Dapper:
// COUNT(*) / SUM(int_col) → bigint  → long
// AVG(col)               → numeric → decimal
// MAX/MIN(int_col)       → int     → int  ← estos SÍ coinciden
```

---

## 9.5 Legibilidad y Mantenimiento — Reglas

> Estas reglas se derivan de la revisión de la arquitectura del proyecto.
> El objetivo es que **cualquier desarrollador nuevo pueda entender un slice en menos de 2 minutos**.

### 9.5.1 Organización de archivos por slice

Cada feature **siempre** tiene exactamente estos archivos (no más, no menos):

| Archivo | Contiene | Cuando crearlo |
|---------|----------|----------------|
| `{Op}{Domain}Command.cs` | Command record + Response record + Handler | Siempre |
| `{Op}{Domain}Endpoint.cs` | Clase estática con `Map()` + `Handle()` privado | Siempre |
| `{Op}{Domain}Validator.cs` | FluentValidation rules | Solo si el Command recibe input |

**No crear** archivos adicionales dentro del slice (interfaces, mappers, helpers propios del slice).

### 9.5.2 Regla del método público único

El **único** método público de un slice es `Map(IEndpointRouteBuilder)`.
Todo lo demás — `Handle`, helpers, constructors del handler — es `private` o `internal`.

```csharp
public static class CreateMedicalCenterEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => ...   // ← público: entry point

    private static async Task<IResult> Handle(...)             // ← privado: implementación
    {
        ...
    }
}
```

### 9.5.3 Handlers con lógica compleja — extraer métodos privados

Cuando un handler tiene lógica no trivial (SQL dinámico, cálculos, transformaciones),
**extrae métodos privados estáticos** con nombres descriptivos en lugar de comentarios inline.

```csharp
// ❌ Difícil de leer — lógica inline con comentarios
public async Task<PaginatedResult<T>> HandleAsync(Params p, CancellationToken ct)
{
    // build where
    var where = string.Empty;
    if (!string.IsNullOrWhiteSpace(p.Search)) { ... where = "WHERE ..."; }

    // build order
    var col = _whitelist.TryGetValue(...) ? col : "name";
    var orderBy = $"ORDER BY {col} {p.SortDesc ? "DESC" : "ASC"}";

    var sql = $"SELECT ... FROM table {where} {orderBy} LIMIT @PS OFFSET @O";
    ...
}

// ✅ Fácil de leer — métodos privados con nombres descriptivos
public async Task<PaginatedResult<T>> HandleAsync(Params p, CancellationToken ct)
{
    var parameters = BuildParameters(p, pageSize, offset);
    var where      = BuildWhereClause(p);
    var orderBy    = BuildOrderByClause(p);
    var sql        = BuildDataSql(where, orderBy);
    ...
}

private static string BuildWhereClause(Params p) { ... }
private static string BuildOrderByClause(Params p) { ... }
private static string BuildDataSql(string where, string orderBy) => $"...";
```

### 9.5.4 SQL dinámico — regla de seguridad obligatoria

Cuando el cliente controla el ORDER BY, **siempre usar whitelist**. Nunca interpolar directamente el valor del cliente en SQL.

```csharp
// ✅ Whitelist como campo estático del handler — visible y auditable
private static readonly Dictionary<string, string> AllowedSortColumns =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"]     = "name",
        ["type"]     = "type",
        ["address"]  = "address",
        ["isactive"] = "is_active"
    };

// Uso con fallback seguro
var column = AllowedSortColumns.GetValueOrDefault(queryParams.SortBy ?? "name", "name");
```

### 9.5.5 Registro explícito en EndpointExtensions — sin magia

Los endpoints y handlers se registran **manualmente y explícitamente** en `EndpointExtensions.cs`.
Esto es intencional: hace que el árbol de dependencias sea auditable sin reflexión ni convención implícita.

```csharp
// ── Medical Centers ──          ← grupo visual por dominio
CreateMedicalCenterEndpoint.Map(app);
GetMedicalCenterEndpoint.Map(app);
PagedMedicalCentersEndpoint.Map(app);
UpdateMedicalCenterEndpoint.Map(app);
DeleteMedicalCenterEndpoint.Map(app);   // ← soft delete / toggle
```

> **Regla:** Al agregar una nueva feature, actualizar `EndpointExtensions` en **dos lugares**:
> 1. `MapFeatureEndpoints()` — registrar el endpoint
> 2. `AddFeatureHandlers()` — registrar el handler en DI

### 9.5.6 Resumen — qué priorizar

| Prioridad | Principio |
|-----------|-----------|
| 🔴 Alta | Extraer métodos privados cuando `HandleAsync` supera ~20 líneas |
| 🔴 Alta | Whitelist obligatoria para cualquier valor del cliente en SQL |
| 🟡 Media | Un archivo por propósito (Command, Endpoint, Validator) |
| 🟡 Media | Nombres descriptivos en métodos privados — sin comentarios redundantes |
| 🟢 Baja | Agrupar visualmente los registros en `EndpointExtensions` por dominio |

---

## 10. Checklist para Agregar una Nueva Feature


```markdown
- [ ] Crear directorio: `Features/{DomainPlural}/{Operación}{Domain}/`
- [ ] Crear Command/Query record + Handler
- [ ] Crear Validator (si es Command con input)
- [ ] Crear Endpoint con método `Map()` estático
- [ ] Definir acceso: `.RequireAuthorization()` (protegido) o `.AllowAnonymous()` (público)
- [ ] Registrar handler en `EndpointExtensions.AddFeatureHandlers()`
- [ ] Registrar endpoint en `EndpointExtensions.MapFeatureEndpoints()`
- [ ] Si es nueva entidad:
  - [ ] Crear en `Domain/Entities/`
  - [ ] Agregar `DbSet<T>` en `AppDbContext`
  - [ ] Configurar mapping en `OnModelCreating`
  - [ ] Generar migración: `dotnet ef migrations add {Nombre}`
- [ ] Verificar: `dotnet build` sin errores
```

---

## 11. Paquetes NuGet Requeridos

```xml
<!-- EF Core + PostgreSQL -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.4" />

<!-- Identity + JWT -->
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.4" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.4" />

<!-- FluentValidation -->
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.0.0" />

<!-- Dapper + Npgsql -->
<PackageReference Include="Dapper" Version="2.1.66" />
<PackageReference Include="Npgsql" Version="9.0.3" />

<!-- OpenAPI -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
```

---

## 12. Comandos Útiles

```bash
# Compilar
dotnet build

# Crear migración
dotnet ef migrations add {NombreMigracion} --project {Project}.csproj --output-dir Infrastructure/Persistence/Migrations

# Aplicar migraciones a la base de datos
dotnet ef database update --project {Project}.csproj

# Revertir última migración (antes de aplicar)
dotnet ef migrations remove --project {Project}.csproj

# Ejecutar en desarrollo
dotnet run --project {Project}.csproj
```
