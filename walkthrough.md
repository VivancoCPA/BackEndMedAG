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

---

## Próximos Pasos

1. **Aplicar migración a PostgreSQL**:
   ```bash
   dotnet ef database update --project SamplVSSkill/SamplVSSkill.csproj
   ```

2. **Agregar más entidades** del modelo (`users`, `pets`, `medical_incidents`, etc.) siguiendo el mismo patrón VSA.

3. **Autenticación JWT** — agregar slice `Login` / `Register` usando Identity.

4. **Swagger UI** — agregar `Swashbuckle` o usar el OpenAPI Explorer integrado en .NET 10.
