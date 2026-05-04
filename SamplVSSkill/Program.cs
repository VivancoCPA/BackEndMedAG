using SamplVSSkill.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure (EF Core, Identity, Dapper, FluentValidation, JWT) ──
builder.Services.AddInfrastructure(builder.Configuration);

// ── Feature Handlers (CQRS Command/Query handlers) ──
builder.Services.AddFeatureHandlers();

// ── OpenAPI ──
builder.Services.AddOpenApi();

// ── JSON — Enums como strings ──
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ── Pipeline ──
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();

// ⚠️ Orden obligatorio: Authentication → Authorization → Endpoints
app.UseAuthentication();
app.UseAuthorization();

// ── Feature Endpoints (Minimal APIs) ──
app.MapFeatureEndpoints();

app.Run();

