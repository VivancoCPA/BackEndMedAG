# Walkthrough — Backend .NET 10 VSA + CQRS

## Qué se construyó

Backend completo para las entidades **`doctors`** y **`medical_centers`** siguiendo **Vertical Slice Architecture**, **CQRS sin MediatR**, y **Minimal APIs** en **.NET 10**.

---

## Estructura Final del Proyecto

```
SamplVSSkill/
├── Domain/
│   └── Entities/
│       ├── Doctor.cs
│       └── MedicalCenter.cs
├── Features/
│   ├── Doctors/
│   │   ├── CreateDoctor/   → POST   /api/doctors
│   │   ├── GetDoctor/      → GET    /api/doctors/{id}
│   │   ├── ListDoctors/    → GET    /api/doctors
│   │   ├── UpdateDoctor/   → PUT    /api/doctors/{id}
│   │   └── DeleteDoctor/   → DELETE /api/doctors/{id}
│   └── MedicalCenters/
│       ├── CreateMedicalCenter/  → POST   /api/medical-centers
│       ├── GetMedicalCenter/     → GET    /api/medical-centers/{id}
│       ├── ListMedicalCenters/   → GET    /api/medical-centers
│       ├── UpdateMedicalCenter/  → PUT    /api/medical-centers/{id}
│       └── DeleteMedicalCenter/  → DELETE /api/medical-centers/{id}
├── Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs                 ← EF Core (IdentityDbContext)
│   │   ├── DapperConnectionFactory.cs      ← Dapper
│   │   └── Migrations/                     ← EF Core migrations generadas
│   ├── Middleware/
│   │   └── ValidationFilter.cs             ← Generic endpoint filter
│   └── Extensions/
│       ├── ServiceCollectionExtensions.cs  ← DI registration
│       └── EndpointExtensions.cs           ← Endpoint + handler registration
└── Program.cs                              ← Composition root
```

---

## Decisiones de Diseño

### CQRS sin MediatR

Cada slice tiene:
- **Command** (record) + **CommandHandler** (clase con `HandleAsync`) para escrituras → **EF Core**
- **Query** (response record) + **QueryHandler** (clase con `HandleAsync`) para lecturas → **Dapper**

Los handlers se registran en DI como `Scoped` y se resuelven directamente en el endpoint (sin bus de mensajes).

```csharp
// Command handler — EF Core (INSERT/UPDATE/DELETE)
public class CreateDoctorCommandHandler {
    private readonly AppDbContext _db;
    public async Task<CreateDoctorResponse> HandleAsync(CreateDoctorCommand cmd, CancellationToken ct) { ... }
}

// Query handler — Dapper (SELECT)
public class GetDoctorQueryHandler {
    private readonly DapperConnectionFactory _connectionFactory;
    public async Task<GetDoctorResponse?> HandleAsync(Guid id, CancellationToken ct) { ... }
}
```

### VSA Entry Point — `Map()` estático

Cada feature expone **un único método público** `Map(IEndpointRouteBuilder)`:

```csharp
public static class CreateDoctorEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>   // ← Entry point
        app.MapPost("/api/doctors", Handle)
           .AddEndpointFilter<ValidationFilter<CreateDoctorCommand>>()
           .WithTags("Doctors");

    private static async Task<IResult> Handle(...) { ... } // ← privado
}
```

### FluentValidation — Endpoint Filter Genérico

`ValidationFilter<T>` resuelve `IValidator<T>` del DI, valida, y retorna `400 ValidationProblem` automáticamente:

```csharp
.AddEndpointFilter<ValidationFilter<CreateDoctorCommand>>()
```

### EF Core — Snake_case + Identity

- Tablas mapeadas explícitamente a snake_case PostgreSQL (`doctors`, `medical_centers`)
- `AppDbContext` hereda de `IdentityDbContext` para gestión de Identity

### Dapper — Raw SQL con alias

```sql
SELECT id AS Id, name AS Name, specialty AS Specialty, is_vet AS IsVet
FROM doctors WHERE id = @Id
```

---

## Paquetes NuGet Utilizados

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.4 | EF Core Provider PostgreSQL |
| `Microsoft.EntityFrameworkCore` | 9.0.4 | ORM para Commands |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 9.0.4 | Identity sobre EF Core |
| `FluentValidation.DependencyInjectionExtensions` | 12.0.0 | Validación + DI scan |
| `Dapper` | 2.1.66 | Micro-ORM para Queries |
| `Npgsql` | 9.0.3 | Conexión directa Dapper |

> **Nota**: `Npgsql.EFCore.PostgreSQL 10.x` aún no tiene release estable (solo preview). Se usa 9.x que es compatible con .NET 10.

---

## Validaciones

### Doctors
- `Name`: requerido, máx 200 chars
- `Specialty`: opcional, máx 200 chars

### MedicalCenters
- `Name`: requerido, máx 200 chars
- `Type`: debe ser `Hospital`, `Clínica`, `Veterinaria` o `Consultorio`
- `Latitude`: entre -90 y 90
- `Longitude`: entre -180 y 180

---

## Resultados de Verificación

| Check | Resultado |
|-------|-----------|
| `dotnet build` | ✅ 0 errores, 0 advertencias |
| `dotnet ef migrations add InitialCreate` | ✅ Migración generada |
| Tablas `doctors` y `medical_centers` en migración | ✅ Snake_case correcto |
| Tablas ASP.NET Identity en migración | ✅ Incluidas |
| Integración de `UserScope` en consultas/comandos | ✅ Completada y compilada exitosamente |

---

## Integración de UserScope (Ámbito de Usuario)

Se ha integrado el concepto de **`UserScope`** en el módulo de usuarios (`Auth`) para permitir la segmentación y protección de la información según el ámbito asignado a cada `Admin`.

### 1. Filtrado de Consultas por Scope (Dapper)
- **`ListUsers`** (`GET /api/users`): Filtra dinámicamente los registros de usuarios y sus aseguradoras asociadas si el usuario solicitante es un `Admin` estándar. Evade el filtro (bypass) si es `SuperAdmin`.
- **`PagedUsers`** (`GET /api/auth/users/paged`): Se modificó la consulta paginada en Dapper para inyectar y evaluar el filtro por `user_scope` de forma optimizada sobre la consulta principal y de conteo.

### 2. Validación y Seguridad de Ámbito (UserManager + Dapper)
Se inyectó verificación de ámbito en las siguientes operaciones individuales (retorna `403 Forbidden` si el usuario no pertenece al scope del administrador y no es el propio usuario operándose a sí mismo):
- **`GetUser`** (`GET /api/users/{userId}`)
- **`UpdateUser`** (`PUT /api/auth/users/{userId}`)
- **`ToggleUserStatus`** (`PATCH /api/users/{userId}/toggle-status`)

### 3. Creación y Asociación Automática
- **`CreateUser`** (`POST /api/auth/users`): Cuando un `Admin` crea un nuevo usuario, el handler registra de forma atómica una nueva relación en la tabla `user_scope` asociando al nuevo usuario al scope de su creador.

### 4. Nuevas Funcionalidades (Slices VSA)
Se agregaron tres nuevos casos de uso independientes para administrar manualmente los scopes:
- **`AddUserScope`** (`POST /api/users/{adminId}/scope/{userId}`): Asocia un usuario al scope de un admin.
- **`RemoveUserScope`** (`DELETE /api/users/{adminId}/scope/{userId}`): Elimina la asociación del scope.
- **`ListUserScopes`** (`GET /api/users/{adminId}/scopes`): Recupera e informa los usuarios asignados al ámbito de un admin específico.

---

## Próximos Pasos

1. **Ejecutar la base de datos y migraciones**:
   ```bash
   dotnet ef database update --project SamplVSSkill/SamplVSSkill.csproj
   ```

2. **Pruebas de Integración y de Roles**:
   Verificar el correcto rechazo con `403 Forbidden` al loguearse como un `Admin` de prueba e intentar obtener la información de un usuario registrado por otro administrador.

